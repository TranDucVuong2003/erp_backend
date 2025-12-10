using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using IronPdf;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class SaleOrdersController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<SaleOrdersController> _logger;
		
		public SaleOrdersController(ApplicationDbContext context, ILogger<SaleOrdersController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// L?y danh sách t?t c? sale orders
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetSaleOrders()
		{
			// Lấy role từ JWT token
			var roleClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
			var role = roleClaim?.Value;

			// Lấy UserId từ JWT token
			var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userid");
			
			var query = _context.SaleOrders
				.Include(so => so.SaleOrderServices)
					.ThenInclude(sos => sos.Service)
				.Include(so => so.SaleOrderAddons)
					.ThenInclude(soa => soa.Addon)
				.Include(so => so.Customer)
				.Include(so => so.CreatedByUser)
				.AsQueryable();

			// Nếu role là "user" thì chỉ lấy sale orders do user đó tạo
			if (role != null && role.ToLower() == "user")
			{
				if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
				{
					query = query.Where(so => so.CreatedByUserId == userId);
					_logger.LogInformation($"User role detected. Filtering sale orders for UserId: {userId}");
				}
				else
				{
					_logger.LogWarning("User role detected but UserId claim not found");
					return Forbid();
				}
			}
			else if (role != null && role.ToLower() == "admin")
			{
				// Admin có thể xem tất cả sale orders
				_logger.LogInformation("Admin role detected. Returning all sale orders");
			}
			else
			{
				// Nếu không có role hoặc role không hợp lệ
				_logger.LogWarning($"Invalid or missing role: {role}");
				return Forbid();
			}

			var saleOrders = await query.ToListAsync();

			// Map sang response có đầy đủ thông tin
			var response = saleOrders.Select(so => new
			{
				so.Id,
				so.Title,
				so.CustomerId,
				Customer = so.Customer != null ? new { 
					so.Customer.Id, 
					Name = so.Customer.CustomerType == "company" 
						? so.Customer.CompanyName 
						: so.Customer.Name,
					so.Customer.CustomerType
				} : null,
				so.Value,
				so.Probability,
				so.Status,
				so.Notes,
				so.CreatedAt,
				so.UpdatedAt,
				so.CreatedByUserId,
				CreatedByUser = so.CreatedByUser != null ? new { so.CreatedByUser.Id, so.CreatedByUser.Name } : null,
				SaleOrderServices = so.SaleOrderServices.Select(sos => new
				{
					sos.ServiceId,
					ServiceName = sos.Service?.Name,
					sos.UnitPrice,
					sos.duration,
					sos.template
				}).ToList(),
				SaleOrderAddons = so.SaleOrderAddons.Select(soa => new
				{
					soa.AddonId,
					AddonName = soa.Addon?.Name,
					soa.UnitPrice,
					soa.duration,
					soa.template
				}).ToList()
			});

			return Ok(response);
		}

		// L?y sale orders theo customer ID
		[HttpGet("by-customer/{customerId}")]
		//[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetSaleOrdersByCustomer(int customerId)
		{
			var saleOrders = await _context.SaleOrders
				.Include(so => so.SaleOrderServices)
					.ThenInclude(sos => sos.Service)
				.Include(so => so.SaleOrderAddons)
					.ThenInclude(soa => soa.Addon)
				.Include(so => so.Customer)
				.Where(d => d.CustomerId == customerId)
				.ToListAsync();

			// Map sang response có đầy đủ thông tin
			var response = saleOrders.Select(so => new
			{
				so.Id,
				so.Title,
				so.CustomerId,
				Customer = so.Customer != null ? new { 
					so.Customer.Id, 
					Name = so.Customer.CustomerType == "company" 
						? so.Customer.CompanyName 
						: so.Customer.Name,
					so.Customer.CustomerType
				} : null,
				so.Value,
				so.Probability,
				so.Status,
				so.Notes,
				so.CreatedAt,
				so.UpdatedAt,
				SaleOrderServices = so.SaleOrderServices.Select(sos => new
				{
					sos.ServiceId,
					ServiceName = sos.Service?.Name,
					sos.UnitPrice,
					sos.duration,
					sos.template
				}).ToList(),
				SaleOrderAddons = so.SaleOrderAddons.Select(soa => new
				{
					soa.AddonId,
					AddonName = soa.Addon?.Name,
					soa.UnitPrice,
					soa.duration,
					soa.template
				}).ToList()
			});

			return Ok(response);
		}

		// Th?ng kê sale orders
		[HttpGet("statistics")]
		//[Authorize]
		public async Task<ActionResult<object>> GetSaleOrderStatistics()
		{
			var totalSaleOrders = await _context.SaleOrders.CountAsync();
			var totalValue = await _context.SaleOrders.SumAsync(d => d.Value);
			var averageProbability = totalSaleOrders > 0 ? await _context.SaleOrders.AverageAsync(d => d.Probability) : 0;

			var saleOrders = await _context.SaleOrders.ToListAsync();
			var probabilityRanges = saleOrders
				.GroupBy(d => d.Probability switch
				{
					>= 0 and <= 25 => "Th?p (0-25%)",
					> 25 and <= 50 => "Trung bình (26-50%)",
					> 50 and <= 75 => "Cao (51-75%)",
					> 75 and <= 100 => "R?t cao (76-100%)",
					_ => "Không xác ??nh"
				})
				.Select(g => new
				{
					ProbabilityRange = g.Key,
					Count = g.Count(),
					TotalValue = g.Sum(d => d.Value)
				})
				.ToList();

			return Ok(new
			{
				TotalSaleOrders = totalSaleOrders,
				TotalValue = totalValue,
				AverageProbability = Math.Round(averageProbability, 2),
				ProbabilityRanges = probabilityRanges
			});
		}

		// L?y sale order theo ID
		[HttpGet("{id}")]
		//[Authorize]
		public async Task<ActionResult<object>> GetSaleOrder(int id)
		{
			var saleOrder = await _context.SaleOrders
				.Include(so => so.SaleOrderServices)
					.ThenInclude(sos => sos.Service)
				.Include(so => so.SaleOrderAddons)
					.ThenInclude(soa => soa.Addon)
				.Include(so => so.Customer)
				.Include(so => so.CreatedByUser)
				.FirstOrDefaultAsync(so => so.Id == id);

			if (saleOrder == null)
			{
				return NotFound(new { message = "Không tìm thấy sale order" });
			}

			// Map sang response có đầy đủ thông tin
			var response = new
			{
				saleOrder.Id,
				saleOrder.Title,
				saleOrder.CustomerId,
				Customer = saleOrder.Customer != null ? new { 
					saleOrder.Customer.Id, 
					Name = saleOrder.Customer.CustomerType == "company" 
						? saleOrder.Customer.CompanyName 
						: saleOrder.Customer.Name,
					saleOrder.Customer.CustomerType
				} : null,
				saleOrder.Value,
				saleOrder.Probability,
				saleOrder.Status,
				saleOrder.Notes,
				saleOrder.CreatedAt,
				saleOrder.UpdatedAt,
				saleOrder.CreatedByUserId,
				CreatedByUser = saleOrder.CreatedByUser != null ? new { saleOrder.CreatedByUser.Id, saleOrder.CreatedByUser.Name } : null,
				SaleOrderServices = saleOrder.SaleOrderServices.Select(sos => new
				{
					sos.ServiceId,
					ServiceName = sos.Service?.Name,
					sos.UnitPrice,
					sos.duration,
					sos.template
				}).ToList(),
				SaleOrderAddons = saleOrder.SaleOrderAddons.Select(soa => new
				{
					soa.AddonId,
					AddonName = soa.Addon?.Name,
					soa.UnitPrice,
					soa.duration,
					soa.template
				}).ToList()
			};

			return Ok(response);
		}

		// T?o sale order m?i v?i nhi?u services và addons
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<SaleOrderWithItemsResponse>> CreateSaleOrder([FromBody] CreateSaleOrderWithItemsRequest request)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			
			try
			{
				// Validate
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Ki?m tra customer t?n t?i
				var customerExists = await _context.Customers.AnyAsync(c => c.Id == request.CustomerId);
				if (!customerExists)
				{
					return BadRequest(new { message = "Customer không t?n t?i" });
				}

				// Ki?m tra và load services
				List<Service> services = new();
				if (request.Services.Any())
				{
					var serviceIds = request.Services.Select(s => s.ServiceId).ToList();
					services = await _context.Services
						.Where(s => serviceIds.Contains(s.Id))
						.ToListAsync();

					var missingServiceIds = serviceIds.Except(services.Select(s => s.Id)).ToList();
					if (missingServiceIds.Any())
					{
						return BadRequest(new { message = $"Service IDs không t?n t?i: {string.Join(", ", missingServiceIds)}" });
					}
				}

				// Ki?m tra và load addons
				List<Addon> addons = new();
				if (request.Addons.Any())
				{
					var addonIds = request.Addons.Select(a => a.AddonId).ToList();
					addons = await _context.Addons
						.Where(a => addonIds.Contains(a.Id))
						.ToListAsync();

					var missingAddonIds = addonIds.Except(addons.Select(a => a.Id)).ToList();
					if (missingAddonIds.Any())
					{
						return BadRequest(new { message = $"Addon IDs không t?n t?i: {string.Join(", ", missingAddonIds)}" });
					}
				}

				// Ki?m tra ph?i có ít nh?t 1 service ho?c addon
				if (!request.Services.Any() && !request.Addons.Any())
				{
					return BadRequest(new { message = "Ph?i có ít nh?t 1 service ho?c addon" });
				}

				// Tính t?ng giá tr? - Luôn l?y quantity t? Service/Addon
				decimal totalValue = 0;
				
				foreach (var serviceDto in request.Services)
				{
					var service = services.First(s => s.Id == serviceDto.ServiceId);
					var quantity = service.Quantity ?? 1; // Luôn l?y t? Service
					var unitPrice = serviceDto.UnitPrice ?? service.Price;
					totalValue += unitPrice * quantity;
				}

				foreach (var addonDto in request.Addons)
				{
					var addon = addons.First(a => a.Id == addonDto.AddonId);
					var quantity = addon.Quantity ?? 1; // Luôn l?y t? Addon
					var unitPrice = addonDto.UnitPrice ?? addon.Price;
					totalValue += unitPrice * quantity;
				}

				// T?o SaleOrder
				var saleOrder = new SaleOrder
				{
					Title = request.Title,
					CustomerId = request.CustomerId,
					Value = totalValue,
					Probability = request.Probability,
					Notes = request.Notes,
					Status = request.Status ?? "Draft",
					CreatedAt = DateTime.UtcNow
				};

				// Lấy UserId từ JWT token và gán vào CreatedByUserId
				var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userid");
				if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
				{
					// Kiểm tra user có tồn tại không
					var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
					if (userExists)
					{
						saleOrder.CreatedByUserId = userId;
						_logger.LogInformation($"Setting CreatedByUserId to {userId}");
					}
					else
					{
						_logger.LogWarning($"User with ID {userId} not found in database");
					}
				}
				else
				{
					_logger.LogWarning("UserId claim not found in token");
				}

				_context.SaleOrders.Add(saleOrder);
				await _context.SaveChangesAsync();

				// T?o SaleOrderServices - Luôn l?y quantity t? Service
				var saleOrderServices = new List<SaleOrderService>();
				foreach (var serviceDto in request.Services)
				{
					var service = services.First(s => s.Id == serviceDto.ServiceId);
					var saleOrderService = new SaleOrderService
					{
						SaleOrderId = saleOrder.Id,
						ServiceId = serviceDto.ServiceId,
						Quantity = service.Quantity, // Luôn l?y t? Service
						UnitPrice = serviceDto.UnitPrice ?? service.Price,
						Notes = serviceDto.Notes ?? service.Notes,
						duration = serviceDto.Duration ?? 0,
						template = serviceDto.Template ?? string.Empty,
						CreatedAt = DateTime.UtcNow
					};
					saleOrderServices.Add(saleOrderService);
					_context.SaleOrderServices.Add(saleOrderService);
				}

				// T?o SaleOrderAddons - Luôn l?y quantity t? Addon
				var saleOrderAddons = new List<SaleOrderAddon>();
				foreach (var addonDto in request.Addons)
				{
					var addon = addons.First(a => a.Id == addonDto.AddonId);
					var saleOrderAddon = new SaleOrderAddon
					{
						SaleOrderId = saleOrder.Id,
						AddonId = addonDto.AddonId,
						Quantity = addon.Quantity, // Luôn l?y t? Addon
						UnitPrice = addonDto.UnitPrice ?? addon.Price,
						Notes = addonDto.Notes ?? addon.Notes,
						duration = addonDto.Duration ?? 0,
						template = addonDto.Template ?? string.Empty,
						CreatedAt = DateTime.UtcNow
					};
					saleOrderAddons.Add(saleOrderAddon);
					_context.SaleOrderAddons.Add(saleOrderAddon);
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				// Reload sale order with CreatedByUser navigation property
				var savedSaleOrder = await _context.SaleOrders
					.Include(so => so.CreatedByUser)
					.FirstOrDefaultAsync(so => so.Id == saleOrder.Id);

				if (savedSaleOrder?.CreatedByUser == null && savedSaleOrder?.CreatedByUserId != null)
				{
					_logger.LogWarning($"CreatedByUser is null even though CreatedByUserId is {savedSaleOrder.CreatedByUserId}");
				}

				// Tạo response
				var response = new SaleOrderWithItemsResponse
				{
					Id = saleOrder.Id,
					Title = saleOrder.Title,
					CustomerId = saleOrder.CustomerId,
					Value = saleOrder.Value,
					Probability = saleOrder.Probability,
					Status = saleOrder.Status,
					CreatedAt = saleOrder.CreatedAt,
					Services = saleOrderServices.Select(sos => new SaleOrderServiceDetailDto
					{
						ServiceId = sos.ServiceId,
						ServiceName = services.First(s => s.Id == sos.ServiceId).Name,
						UnitPrice = sos.UnitPrice,
						Duration = sos.duration,
						Template = sos.template
					}).ToList(),
					Addons = saleOrderAddons.Select(soa => new SaleOrderAddonDetailDto
					{
						AddonId = soa.AddonId,
						AddonName = addons.First(a => a.Id == soa.AddonId).Name,
						UnitPrice = soa.UnitPrice,
						Duration = soa.duration,
						Template = soa.template
					}).ToList()
				};

				return CreatedAtAction(nameof(GetSaleOrder), new { id = saleOrder.Id }, response);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "L?i khi t?o sale order v?i nhi?u items");
				return StatusCode(500, new { message = "L?i server khi t?o sale order", error = ex.Message });
			}
		}

		// C?p nh?t sale order v?i ??y ?? thông tin (bao g?m Services và Addons)
		[HttpPut("{id}")]
		//[Authorize]
		public async Task<ActionResult<SaleOrderWithItemsResponse>> UpdateSaleOrder(int id, [FromBody] UpdateSaleOrderWithItemsRequest request)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			
			try
			{
				// Validate
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Ki?m tra t?n t?i
				var existingSaleOrder = await _context.SaleOrders
					.Include(so => so.SaleOrderServices)
					.Include(so => so.SaleOrderAddons)
					.FirstOrDefaultAsync(so => so.Id == id);

				if (existingSaleOrder == null)
				{
					return NotFound(new { message = "Không tìm th?y sale order" });
				}

				// C?p nh?t thông tin c? b?n
				if (!string.IsNullOrEmpty(request.Title))
				{
					existingSaleOrder.Title = request.Title;
				}

				if (request.CustomerId.HasValue)
				{
					var customerExists = await _context.Customers.AnyAsync(c => c.Id == request.CustomerId.Value);
					if (!customerExists)
					{
						return BadRequest(new { message = "Customer không t?n t?i" });
					}
					existingSaleOrder.CustomerId = request.CustomerId.Value;
				}

				if (request.Probability.HasValue)
				{
					existingSaleOrder.Probability = request.Probability.Value;
				}

				if (request.Notes != null)
				{
					existingSaleOrder.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes;
				}

				if (!string.IsNullOrEmpty(request.Status))
				{
					existingSaleOrder.Status = request.Status;
				}

				// C?p nh?t Services n?u có trong request
				List<Service> services = new();
				if (request.Services != null)
				{
					// Xóa các services c?
					_context.SaleOrderServices.RemoveRange(existingSaleOrder.SaleOrderServices);

					// Thêm services m?i (n?u có)
					if (request.Services.Any())
					{
						var serviceIds = request.Services.Select(s => s.ServiceId).ToList();
						services = await _context.Services
							.Where(s => serviceIds.Contains(s.Id))
							.ToListAsync();

						var missingServiceIds = serviceIds.Except(services.Select(s => s.Id)).ToList();
						if (missingServiceIds.Any())
						{
							await transaction.RollbackAsync();
							return BadRequest(new { message = $"Service IDs không t?n t?i: {string.Join(", ", missingServiceIds)}" });
						}

						foreach (var serviceDto in request.Services)
						{
							var service = services.First(s => s.Id == serviceDto.ServiceId);
							var saleOrderService = new SaleOrderService
							{
								SaleOrderId = existingSaleOrder.Id,
								ServiceId = serviceDto.ServiceId,
								Quantity = service.Quantity,
								UnitPrice = serviceDto.UnitPrice ?? service.Price,
								Notes = serviceDto.Notes ?? service.Notes,
								duration = serviceDto.Duration ?? 0,
								template = serviceDto.Template ?? string.Empty,
								CreatedAt = DateTime.UtcNow
							};
							_context.SaleOrderServices.Add(saleOrderService);
						}
					}
				}
				else
				{
					// N?u request.Services == null, gi? nguyên services hi?n t?i
					// Load l?i services ?? tính toán value
					var serviceIds = existingSaleOrder.SaleOrderServices.Select(s => s.ServiceId).ToList();
					if (serviceIds.Any())
					{
						services = await _context.Services
							.Where(s => serviceIds.Contains(s.Id))
							.ToListAsync();
					}
				}

				// C?p nh?t Addons n?u có trong request
				List<Addon> addons = new();
				if (request.Addons != null)
				{
					// Xóa các addons c?
					_context.SaleOrderAddons.RemoveRange(existingSaleOrder.SaleOrderAddons);

					// Thêm addons m?i (n?u có)
					if (request.Addons.Any())
					{
						var addonIds = request.Addons.Select(a => a.AddonId).ToList();
						addons = await _context.Addons
							.Where(a => addonIds.Contains(a.Id))
							.ToListAsync();

						var missingAddonIds = addonIds.Except(addons.Select(a => a.Id)).ToList();
						if (missingAddonIds.Any())
						{
							await transaction.RollbackAsync();
							return BadRequest(new { message = $"Addon IDs không t?n t?i: {string.Join(", ", missingAddonIds)}" });
						}

						foreach (var addonDto in request.Addons)
						{
							var addon = addons.First(a => a.Id == addonDto.AddonId);
							var saleOrderAddon = new SaleOrderAddon
							{
								SaleOrderId = existingSaleOrder.Id,
								AddonId = addonDto.AddonId,
								Quantity = addon.Quantity,
								UnitPrice = addonDto.UnitPrice ?? addon.Price,
								Notes = addonDto.Notes ?? addon.Notes,
								duration = addonDto.Duration ?? 0,
								template = addonDto.Template ?? string.Empty,
								CreatedAt = DateTime.UtcNow
							};
							_context.SaleOrderAddons.Add(saleOrderAddon);
						}
					}
				}
				else
				{
					// N?u request.Addons == null, gi? nguyên addons hi?n t?i
					// Load l?i addons ?? tính toán value
					var addonIds = existingSaleOrder.SaleOrderAddons.Select(a => a.AddonId).ToList();
					if (addonIds.Any())
					{
						addons = await _context.Addons
							.Where(a => addonIds.Contains(a.Id))
							.ToListAsync();
					}
				}

				// Tính l?i giá tr? t?ng
				decimal totalValue = 0;

				// Tính t? services
				if (request.Services != null && request.Services.Any())
				{
					foreach (var serviceDto in request.Services)
					{
						var service = services.First(s => s.Id == serviceDto.ServiceId);
						var quantity = service.Quantity ?? 1;
						var unitPrice = serviceDto.UnitPrice ?? service.Price;
						totalValue += unitPrice * quantity;
					}
				}
				else if (request.Services == null)
				{
					// Gi? nguyên services c?
					foreach (var sos in existingSaleOrder.SaleOrderServices)
					{
						var service = services.FirstOrDefault(s => s.Id == sos.ServiceId);
						var quantity = service?.Quantity ?? 1;
						totalValue += sos.UnitPrice * quantity;
					}
				}

				// Tính t? addons
				if (request.Addons != null && request.Addons.Any())
				{
					foreach (var addonDto in request.Addons)
					{
						var addon = addons.First(a => a.Id == addonDto.AddonId);
						var quantity = addon.Quantity ?? 1;
						var unitPrice = addonDto.UnitPrice ?? addon.Price;
						totalValue += unitPrice * quantity;
					}
				}
				else if (request.Addons == null)
				{
					// Gi? nguyên addons c?
					foreach (var soa in existingSaleOrder.SaleOrderAddons)
					{
						var addon = addons.FirstOrDefault(a => a.Id == soa.AddonId);
						var quantity = addon?.Quantity ?? 1;
						totalValue += soa.UnitPrice * quantity;
					}
				}

				existingSaleOrder.Value = totalValue;
				existingSaleOrder.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				// Load l?i ?? l?y ??y ?? thông tin
				var updatedSaleOrder = await _context.SaleOrders
					.Include(so => so.SaleOrderServices)
						.ThenInclude(sos => sos.Service)
					.Include(so => so.SaleOrderAddons)
						.ThenInclude(soa => soa.Addon)
					.FirstOrDefaultAsync(so => so.Id == id);

				// T?o response
				var response = new SaleOrderWithItemsResponse
				{
					Id = updatedSaleOrder!.Id,
					Title = updatedSaleOrder.Title,
					CustomerId = updatedSaleOrder.CustomerId,
					Value = updatedSaleOrder.Value,
					Probability = updatedSaleOrder.Probability,
					Notes = updatedSaleOrder.Notes,
					Status = updatedSaleOrder.Status,
					CreatedAt = updatedSaleOrder.CreatedAt,
					Services = updatedSaleOrder.SaleOrderServices.Select(sos => new SaleOrderServiceDetailDto
					{
						ServiceId = sos.ServiceId,
						ServiceName = sos.Service?.Name ?? string.Empty,
						UnitPrice = sos.UnitPrice,
						Duration = sos.duration,
						Template = sos.template
					}).ToList(),
					Addons = updatedSaleOrder.SaleOrderAddons.Select(soa => new SaleOrderAddonDetailDto
					{
						AddonId = soa.AddonId,
						AddonName = soa.Addon?.Name ?? string.Empty,
						UnitPrice = soa.UnitPrice,
						Duration = soa.duration,
						Template = soa.template
					}).ToList(),
					Message = "C?p nh?t sale order thành công"
				};

				return Ok(response);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "L?i khi c?p nh?t sale order v?i ID: {SaleOrderId}", id);
				return StatusCode(500, new { message = "L?i server khi c?p nh?t sale order", error = ex.Message });
			}
		}

		// C?p nh?t xác su?t sale order
		[HttpPatch("{id}/probability")]
		[Authorize]
		public async Task<IActionResult> UpdateSaleOrderProbability(int id, [FromBody] Dictionary<string, int> request)
		{
			try
			{
				if (!request.ContainsKey("probability"))
				{
					return BadRequest(new { message = "Thi?u tr??ng probability" });
				}

				var probability = request["probability"];

				if (probability < 0 || probability > 100)
				{
					return BadRequest(new { message = "Xác su?t ph?i t? 0-100%" });
				}

				var saleOrder = await _context.SaleOrders.FindAsync(id);
				if (saleOrder == null)
				{
					return NotFound(new { message = "Không tìm th?y sale order" });
				}

				saleOrder.Probability = probability;
				saleOrder.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				return Ok(new
				{
					message = "C?p nh?t xác su?t thành công",
					id = saleOrder.Id,
					probability = saleOrder.Probability,
					updatedAt = saleOrder.UpdatedAt
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi c?p nh?t xác su?t sale order v?i ID: {SaleOrderId}", id);
				return StatusCode(500, new { message = "L?i server khi c?p nh?t xác su?t", error = ex.Message });
			}
		}

		// Xóa sale order
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<ActionResult<DeleteSaleOrderResponse>> DeleteSaleOrder(int id)
		{
			try
			{
				var saleOrder = await _context.SaleOrders.FindAsync(id);
				if (saleOrder == null)
				{
					return NotFound(new { message = "Không tìm th?y sale order" });
				}

				var deletedSaleOrderInfo = new SaleOrderInfo
				{
					Id = saleOrder.Id,
					Title = saleOrder.Title,
					CustomerId = saleOrder.CustomerId,
					Value = saleOrder.Value,
					Probability = saleOrder.Probability,
					Notes = saleOrder.Notes,
					ServiceId = saleOrder.ServiceId,
					AddonId = saleOrder.AddonId,
					Status = saleOrder.Status
				};

				_context.SaleOrders.Remove(saleOrder);
				await _context.SaveChangesAsync();

				var response = new DeleteSaleOrderResponse
				{
					Message = "Xóa sale order thành công",
					DeletedSaleOrder = deletedSaleOrderInfo,
					DeletedAt = DateTime.UtcNow
				};

				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi xóa sale order v?i ID: {SaleOrderId}", id);
				return StatusCode(500, new { message = "L?i server khi xóa sale order", error = ex.Message });
			}
		}

		private bool SaleOrderExists(int id)
		{
			return _context.SaleOrders.Any(e => e.Id == id);
		}
	}
}
