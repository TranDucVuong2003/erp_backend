using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using System.Text.Json;
using erp_backend.Services;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class QuotesController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<QuotesController> _logger;
		private readonly IPdfService _pdfService;

		public QuotesController(
			ApplicationDbContext context, 
			ILogger<QuotesController> logger,
			IPdfService pdfService)
		{
			_context = context;
			_logger = logger;
			_pdfService = pdfService;
		}

		// ✅ Helper method: Lấy User ID từ JWT token
		private int? GetCurrentUserId()
		{
			var userIdClaim = User.FindFirst("UserId")?.Value ?? 
							  User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (int.TryParse(userIdClaim, out var userId))
			{
				return userId;
			}
			return null;
		}

		// GET: api/Quotes
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetQuotes()
		{
			// Lấy role từ JWT token
			var roleClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
			var role = roleClaim?.Value;

			// Lấy UserId từ JWT token
			var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userid");
			
			var query = _context.Quotes
				.Include(q => q.Customer)
				.Include(q => q.CreatedByUser)
					.ThenInclude(u => u.Position)
				.Include(q => q.CategoryServiceAddon)
				.Include(q => q.QuoteServices)
					.ThenInclude(qs => qs.Service)
						.ThenInclude(s => s!.Tax)
				.Include(q => q.QuoteServices)
					.ThenInclude(qs => qs.Service)
						.ThenInclude(s => s!.CategoryServiceAddons)
				.Include(q => q.QuoteAddons)
					.ThenInclude(qa => qa.Addon)
				.AsQueryable();

			// Nếu role là "user" thì chỉ lấy báo giá do user đó tạo
			if (role != null && role.ToLower() == "user")
			{
				if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
				{
					// Lọc theo CreatedByUserId
					query = query.Where(q => q.CreatedByUserId == userId);
					_logger.LogInformation($"User role detected. Filtering quotes for UserId: {userId}");
				}
				else
				{
					_logger.LogWarning("User role detected but UserId claim not found");
					return Forbid();
				}
			}
			else if (role != null && role.ToLower() == "admin")
			{
				// Admin có thể xem tất cả báo giá
				_logger.LogInformation("Admin role detected. Returning all quotes");
			}
			else
			{
				// Nếu không có role hoặc role không hợp lệ
				_logger.LogWarning($"Invalid or missing role: {role}");
				return Forbid();
			}

			var quotes = await query.ToListAsync();

			var response = quotes.Select(q => new
			{		
				q.Id,
				q.CustomerId,
				Customer = q.Customer != null ? new { q.Customer.Id, Name = q.Customer.Name ?? q.Customer.CompanyName } : null,
				CreatedByUserId = q.CreatedByUserId,
				CreatedByUser = q.CreatedByUser != null ? new 
				{ 
					q.CreatedByUser.Id, 
					q.CreatedByUser.Name, 
					q.CreatedByUser.Email,
					q.CreatedByUser.PhoneNumber,
					q.CreatedByUser.Position
				} : null,
				CategoryServiceAddonId = q.CategoryServiceAddonId,
				CategoryServiceAddon = q.CategoryServiceAddon != null ? new
				{
					q.CategoryServiceAddon.Id,
					q.CategoryServiceAddon.Name
				} : null,
				Services = q.QuoteServices.Select(qs => new
				{
					qs.Id,
					qs.ServiceId,
					ServiceName = qs.Service?.Name,
					UnitPrice = qs.UnitPrice,
					qs.Quantity,
					qs.Notes,
					Service = qs.Service != null ? new
					{
						qs.Service.Id,
						qs.Service.Name,
						qs.Service.Description,
						qs.Service.Price,
						qs.Service.IsActive,
						Tax = qs.Service.Tax != null ? new
						{
							qs.Service.Tax.Id,
							qs.Service.Tax.Rate
						} : null
					} : null
				}).ToList(),
				Addons = q.QuoteAddons.Select(qa => new
				{
					qa.Id,
					qa.AddonId,
					AddonName = qa.Addon?.Name,
					UnitPrice = qa.UnitPrice,
					qa.Quantity,
					qa.Notes
				}).ToList(),
				q.CustomService,
				q.FilePath,
				q.Amount,
				q.CreatedAt,
				q.UpdatedAt
			});

			return Ok(response);
		}

		// GET: api/Quotes/5
		[HttpGet("{id}")]
		//[Authorize]
		public async Task<ActionResult<object>> GetQuote(int id)
		{
			var quote = await _context.Quotes
				.Include(q => q.Customer)
				.Include(q => q.CreatedByUser)
					.ThenInclude(u => u.Position)
				.Include(q => q.CategoryServiceAddon)
				.Include(q => q.QuoteServices)
					.ThenInclude(qs => qs.Service)
				.Include(q => q.QuoteAddons)
					.ThenInclude(qa => qa.Addon)
				.FirstOrDefaultAsync(q => q.Id == id);

			if (quote == null)
			{
				return NotFound(new { message = "Không tìm thấy báo giá" });
			}

			var response = new
			{
				quote.Id,
				quote.CustomerId,
				Customer = quote.Customer != null ? new
				{
					quote.Customer.Id,
					Name = quote.Customer.Name ?? quote.Customer.CompanyName,
					quote.Customer.Email,
					quote.Customer.PhoneNumber,
					quote.Customer.CustomerType
				} : null,
				CreatedByUserId = quote.CreatedByUserId,
				CreatedByUser = quote.CreatedByUser != null ? new 
				{ 
					quote.CreatedByUser.Id, 
					quote.CreatedByUser.Name, 
					quote.CreatedByUser.Email,
					quote.CreatedByUser.PhoneNumber,
					quote.CreatedByUser.Position
				} : null,
				CategoryServiceAddonId = quote.CategoryServiceAddonId,
				CategoryServiceAddon = quote.CategoryServiceAddon != null ? new
				{
					quote.CategoryServiceAddon.Id,
					quote.CategoryServiceAddon.Name
				} : null,
				Services = quote.QuoteServices.Select(qs => new
				{
					qs.Id,
					qs.ServiceId,
					Service = qs.Service != null ? new
					{
						qs.Service.Id,
						qs.Service.Name,
						qs.Service.Description,
						qs.Service.Price
					} : null,
					UnitPrice = qs.Service?.Price ?? 0
				}).ToList(),
				Addons = quote.QuoteAddons.Select(qa => new
				{
					qa.Id,
					qa.AddonId,
					Addon = qa.Addon != null ? new
					{
						qa.Addon.Id,
						qa.Addon.Name,
						qa.Addon.Description,
						qa.Addon.Price
					} : null,
					UnitPrice = qa.Addon?.Price ?? 0
				}).ToList(),
				quote.CustomService,
				quote.FilePath,
				quote.Amount,
				quote.CreatedAt,
				quote.UpdatedAt
			};

			return Ok(response);
		}

		// GET: api/Quotes/by-customer/{customerId}
		[HttpGet("by-customer/{customerId}")]
		//[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetQuotesByCustomer(int customerId)
		{
			var quotes = await _context.Quotes
				.Include(q => q.QuoteServices)
					.ThenInclude(qs => qs.Service)
				.Include(q => q.QuoteAddons)
					.ThenInclude(qa => qa.Addon)
				.Where(q => q.CustomerId == customerId)
				.ToListAsync();

			var response = quotes.Select(q => new
			{
				q.Id,
				Services = q.QuoteServices.Select(qs => new
				{
					qs.Id,
					qs.ServiceId,
					ServiceName = qs.Service?.Name,
					UnitPrice = qs.Service?.Price ?? 0
				}).ToList(),
				Addons = q.QuoteAddons.Select(qa => new
				{
					qa.Id,
					qa.AddonId,
					AddonName = qa.Addon?.Name,
					UnitPrice = qa.Addon?.Price ?? 0
				}).ToList(),
				q.CustomService,
				q.FilePath,
				q.Amount,
				q.CreatedAt,
				q.UpdatedAt
			});

			return Ok(response);
		}

		// POST: api/Quotes
		[HttpPost]
		//[Authorize]
		public async Task<ActionResult<Quote>> CreateQuote([FromBody] CreateQuoteDto dto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// ✅ Lấy User ID từ JWT hoặc DTO
				var currentUserId = GetCurrentUserId() ?? dto.CreatedByUserId;

				// Kiểm tra user exists nếu có UserId
				if (currentUserId.HasValue)
				{
					var userExists = await _context.Users.AnyAsync(u => u.Id == currentUserId.Value);
					if (!userExists)
					{
						return BadRequest(new { message = "User không tồn tại" });
					}
				}

				// Kiểm tra customer exists nếu có CustomerId
				if (dto.CustomerId.HasValue)
				{
					var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId);
					if (!customerExists)
					{
						return BadRequest(new { message = "Khách hàng không tồn tại" });
					}
				}

				// Kiểm tra category exists nếu có
				if (dto.CategoryServiceAddonId.HasValue)
				{
					var categoryExists = await _context.CategoryServiceAddons.AnyAsync(c => c.Id == dto.CategoryServiceAddonId);
					if (!categoryExists)
					{
						return BadRequest(new { message = "Category không tồn tại" });
					}
				}

				// Kiểm tra Services exist
				if (dto.Services != null && dto.Services.Any())
				{
					var serviceIds = dto.Services.Select(s => s.ServiceId).ToList();
					var existingServiceIds = await _context.Services
						.Where(s => serviceIds.Contains(s.Id))
						.Select(s => s.Id)
						.ToListAsync();

					var missingServiceIds = serviceIds.Except(existingServiceIds).ToList();
					if (missingServiceIds.Any())
					{
						return BadRequest(new { message = $"Dịch vụ không tồn tại: {string.Join(", ", missingServiceIds)}" });
					}
				}

				// Kiểm tra Addons exist
				if (dto.Addons != null && dto.Addons.Any())
				{
					var addonIds = dto.Addons.Select(a => a.AddonId).ToList();
					var existingAddonIds = await _context.Addons
						.Where(a => addonIds.Contains(a.Id))
						.Select(a => a.Id)
						.ToListAsync();

					var missingAddonIds = addonIds.Except(existingAddonIds).ToList();
					if (missingAddonIds.Any())
					{
						return BadRequest(new { message = $"Addon không tồn tại: {string.Join(", ", missingAddonIds)}" });
					}
				}

				// ✅ Tính toán Amount tự động CHỈ từ Services + Addons (BAO GỒM VAT)
				decimal calculatedAmount = 0;

				// Tính tổng Services (BAO GỒM VAT)
				if (dto.Services != null && dto.Services.Any())
				{
					var serviceIds = dto.Services.Select(s => s.ServiceId).ToList();
					var services = await _context.Services
						.Include(s => s.Tax)
						.Where(s => serviceIds.Contains(s.Id))
						.ToDictionaryAsync(s => s.Id, s => s);

					foreach (var svc in dto.Services)
					{
						if (services.TryGetValue(svc.ServiceId, out var service))
						{
							var unitPrice = svc.UnitPrice > 0 ? svc.UnitPrice : service.Price;
							var lineTotal = unitPrice * svc.Quantity;
							// ✅ Get tax rate from Service.Tax, default to 0 if not set
							var vatRate = service.Tax?.Rate ?? 0f;
							calculatedAmount += lineTotal * (1 + (decimal)vatRate / 100);
						}
					}
				}

				// Tính tổng Addons (BAO GỒM VAT)
				if (dto.Addons != null && dto.Addons.Any())
				{
					var addonIds = dto.Addons.Select(a => a.AddonId).ToList();
					var addons = await _context.Addons
						.Include(a => a.Tax)
						.Where(a => addonIds.Contains(a.Id))
						.ToDictionaryAsync(a => a.Id, a => a);

					foreach (var addonDto in dto.Addons)
					{
						if (addons.TryGetValue(addonDto.AddonId, out var addon))
						{
							var unitPrice = addonDto.UnitPrice > 0 ? addonDto.UnitPrice : addon.Price;
							var lineTotal = unitPrice * addonDto.Quantity;
							// ✅ Get tax rate from Addon.Tax, default to 0 if not set
							var vatRate = addon.Tax?.Rate ?? 0f;
							calculatedAmount += lineTotal * (1 + (decimal)vatRate / 100);
						}
					}
				}

				// ✅ Amount KHÔNG bao gồm CustomService - CustomService chỉ là thông tin chi tiết breakdown

				// Tạo Quote với Amount tự động tính toán
				var quote = new Quote
				{
					CustomerId = dto.CustomerId,
					CreatedByUserId = currentUserId,
					CategoryServiceAddonId = dto.CategoryServiceAddonId,
					CustomServiceJson = dto.CustomService != null 
						? JsonSerializer.Serialize(dto.CustomService) 
						: null,
					FilePath = dto.FilePath,
					Amount = calculatedAmount, // ✅ CHỈ tính từ Services + Addons (BAO GỒM VAT)
					CreatedAt = DateTime.UtcNow
				};

				_context.Quotes.Add(quote);
				await _context.SaveChangesAsync();

				// Thêm Services
				if (dto.Services != null && dto.Services.Any())
				{
					var serviceIds = dto.Services.Select(s => s.ServiceId).ToList();
					var services = await _context.Services
						.Where(s => serviceIds.Contains(s.Id))
						.ToDictionaryAsync(s => s.Id, s => s);

					foreach (var svc in dto.Services)
					{
						decimal finalUnitPrice = svc.UnitPrice;
						if (finalUnitPrice <= 0 && services.TryGetValue(svc.ServiceId, out var service))
						{
							finalUnitPrice = service.Price;
						}

						var quoteService = new QuoteService
						{
							QuoteId = quote.Id,
							ServiceId = svc.ServiceId,
							Quantity = svc.Quantity,
							UnitPrice = finalUnitPrice,
							Notes = svc.Notes,
							CreatedAt = DateTime.UtcNow
						};
						_context.QuoteServices.Add(quoteService);
					}
				}

				// Thêm Addons
				if (dto.Addons != null && dto.Addons.Any())
				{
					var addonIds = dto.Addons.Select(a => a.AddonId).ToList();
					var addons = await _context.Addons
						.Where(a => addonIds.Contains(a.Id))
						.ToDictionaryAsync(a => a.Id, a => a);

					foreach (var addonDto in dto.Addons)
					{
						decimal finalUnitPrice = addonDto.UnitPrice;
						if (finalUnitPrice <= 0 && addons.TryGetValue(addonDto.AddonId, out var addon))
						{
							finalUnitPrice = addon.Price;
						}

						var quoteAddon = new QuoteAddon
						{
							QuoteId = quote.Id,
							AddonId = addonDto.AddonId,
							Quantity = addonDto.Quantity,
							UnitPrice = finalUnitPrice,
							Notes = addonDto.Notes,
							CreatedAt = DateTime.UtcNow
						};
						_context.QuoteAddons.Add(quoteAddon);
					}
				}

				await _context.SaveChangesAsync();

				// Load navigation properties để trả về
				await _context.Entry(quote).Reference(q => q.Customer).LoadAsync();
				await _context.Entry(quote).Reference(q => q.CreatedByUser).LoadAsync();
				await _context.Entry(quote).Reference(q => q.CategoryServiceAddon).LoadAsync();
				await _context.Entry(quote).Collection(q => q.QuoteServices).LoadAsync();
				await _context.Entry(quote).Collection(q => q.QuoteAddons).LoadAsync();

				// Load Service và Addon details
				foreach (var qs in quote.QuoteServices)
				{
					await _context.Entry(qs).Reference(x => x.Service).LoadAsync();
				}
				foreach (var qa in quote.QuoteAddons)
				{
					await _context.Entry(qa).Reference(x => x.Addon).LoadAsync();
				}

				return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quote);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo báo giá mới");
				return StatusCode(500, new { message = "Lỗi server khi tạo báo giá", error = ex.Message });
			}
		}

		// PUT: api/Quotes/5
		[HttpPut("{id}")]
		//[Authorize]
		public async Task<ActionResult<Quote>> UpdateQuote(int id, [FromBody] CreateQuoteDto dto)
		{
			try
			{
				var existingQuote = await _context.Quotes
					.Include(q => q.QuoteServices)
					.Include(q => q.QuoteAddons)
					.FirstOrDefaultAsync(q => q.Id == id);

				if (existingQuote == null)
				{
					return NotFound(new { message = "Không tìm thấy báo giá" });
				}

				if (dto.CustomerId.HasValue)
				{
					var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId);
					if (!customerExists)
					{
						return BadRequest(new { message = "Khách hàng không tồn tại" });
					}
				}

				if (dto.CreatedByUserId.HasValue)
				{
					var userExists = await _context.Users.AnyAsync(u => u.Id == dto.CreatedByUserId);
					if (!userExists)
					{
						return BadRequest(new { message = "User không tồn tại" });
					}
				}

				if (dto.CategoryServiceAddonId.HasValue)
				{
					var categoryExists = await _context.CategoryServiceAddons.AnyAsync(c => c.Id == dto.CategoryServiceAddonId);
					if (!categoryExists)
					{
						return BadRequest(new { message = "Category không tồn tại" });
					}
				}

				if (dto.Services != null && dto.Services.Any())
				{
					var serviceIds = dto.Services.Select(s => s.ServiceId).ToList();
					var existingServiceIds = await _context.Services
						.Where(s => serviceIds.Contains(s.Id))
						.Select(s => s.Id)
						.ToListAsync();

					var missingServiceIds = serviceIds.Except(existingServiceIds).ToList();
					if (missingServiceIds.Any())
					{
						return BadRequest(new { message = $"Dịch vụ không tồn tại: {string.Join(", ", missingServiceIds)}" });
					}
				}

				if (dto.Addons != null && dto.Addons.Any())
				{
					var addonIds = dto.Addons.Select(a => a.AddonId).ToList();
					var existingAddonIds = await _context.Addons
						.Where(a => addonIds.Contains(a.Id))
						.Select(a => a.Id)
						.ToListAsync();

					var missingAddonIds = addonIds.Except(existingAddonIds).ToList();
					if (missingAddonIds.Any())
					{
						return BadRequest(new { message = $"Addon không tồn tại: {string.Join(", ", missingAddonIds)}" });
					}
				}

				// ✅ Tính toán Amount tự động CHỈ từ Services + Addons (BAO GỒM VAT)
				decimal calculatedAmount = 0;

				// Tính tổng Services (BAO GỒM VAT)
				if (dto.Services != null && dto.Services.Any())
				{
					var serviceIds = dto.Services.Select(s => s.ServiceId).ToList();
					var services = await _context.Services
						.Include(s => s.Tax)
						.Where(s => serviceIds.Contains(s.Id))
						.ToDictionaryAsync(s => s.Id, s => s);

					foreach (var svc in dto.Services)
					{
						if (services.TryGetValue(svc.ServiceId, out var service))
						{
							var unitPrice = svc.UnitPrice > 0 ? svc.UnitPrice : service.Price;
							var lineTotal = unitPrice * svc.Quantity;
							// ✅ Get tax rate from Service.Tax, default to 0 if not set
							var vatRate = service.Tax?.Rate ?? 0f;
							calculatedAmount += lineTotal * (1 + (decimal)vatRate / 100);
						}
					}
				}

				// Tính tổng Addons (BAO GỒM VAT)
				if (dto.Addons != null && dto.Addons.Any())
				{
					var addonIds = dto.Addons.Select(a => a.AddonId).ToList();
					var addons = await _context.Addons
						.Include(a => a.Tax)
						.Where(a => addonIds.Contains(a.Id))
						.ToDictionaryAsync(a => a.Id, a => a);

					foreach (var addonDto in dto.Addons)
					{
						if (addons.TryGetValue(addonDto.AddonId, out var addon))
						{
							var unitPrice = addonDto.UnitPrice > 0 ? addonDto.UnitPrice : addon.Price;
							var lineTotal = unitPrice * addonDto.Quantity;
							// ✅ Get tax rate from Addon.Tax, default to 0 if not set
							var vatRate = addon.Tax?.Rate ?? 0f;
							calculatedAmount += lineTotal * (1 + (decimal)vatRate / 100);
						}
					}
				}

				// ✅ Invalidate PDF cache nếu có thay đổi quan trọng
				bool needsRegenerate = dto.CustomerId != existingQuote.CustomerId ||
							  dto.CreatedByUserId != existingQuote.CreatedByUserId ||
							  dto.CategoryServiceAddonId != existingQuote.CategoryServiceAddonId ||
							  calculatedAmount != existingQuote.Amount ||
							  (dto.Services?.Count ?? 0) != existingQuote.QuoteServices.Count ||
							  (dto.Addons?.Count ?? 0) != existingQuote.QuoteAddons.Count;

				if (needsRegenerate && !string.IsNullOrEmpty(existingQuote.FilePath))
				{
					// Xóa file PDF cũ
					var relativePath = existingQuote.FilePath.TrimStart('/');
					var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
					
					if (System.IO.File.Exists(oldFilePath))
					{
						try
						{
							System.IO.File.Delete(oldFilePath);
							_logger.LogInformation("Đã xóa file PDF cũ do cập nhật Quote: {FilePath}", existingQuote.FilePath);
						}
						catch (Exception ex)
						{
							_logger.LogWarning(ex, "Không thể xóa file PDF cũ: {FilePath}", existingQuote.FilePath);
						}
					}

					// Reset thông tin PDF
					existingQuote.FilePath = null;
				}

				existingQuote.CustomerId = dto.CustomerId;
				existingQuote.CustomServiceJson = dto.CustomService != null 
					? JsonSerializer.Serialize(dto.CustomService) 
					: null;
				existingQuote.Amount = calculatedAmount; // ✅ CHỈ tính từ Services + Addons (BAO GỒM VAT)
				existingQuote.CreatedByUserId = dto.CreatedByUserId;
				existingQuote.CategoryServiceAddonId = dto.CategoryServiceAddonId;
				existingQuote.UpdatedAt = DateTime.UtcNow;

				_context.QuoteServices.RemoveRange(existingQuote.QuoteServices);

				if (dto.Services != null && dto.Services.Any())
				{
					// Thêm Services mới
					// ✅ Tối ưu: Load tất cả services 1 lần
					var serviceIds = dto.Services.Select(s => s.ServiceId).ToList();
					var services = await _context.Services
						.Where(s => serviceIds.Contains(s.Id))
						.ToDictionaryAsync(s => s.Id, s => s);

					foreach (var svc in dto.Services)
					{
						// ✅ Lấy giá từ database nếu unitPrice = 0 hoặc null
						decimal finalUnitPrice = svc.UnitPrice;
						if (finalUnitPrice <= 0 && services.TryGetValue(svc.ServiceId, out var service))
						{
							finalUnitPrice = service.Price;
						}

						var quoteService = new QuoteService
						{
							QuoteId = existingQuote.Id,
							ServiceId = svc.ServiceId,
							Quantity = svc.Quantity,
							UnitPrice = finalUnitPrice, // ✅ Sử dụng giá đã xử lý
							Notes = svc.Notes,
							CreatedAt = DateTime.UtcNow
						};
						_context.QuoteServices.Add(quoteService);
					}
				}

				_context.QuoteAddons.RemoveRange(existingQuote.QuoteAddons);

				if (dto.Addons != null && dto.Addons.Any())
				{
					// Thêm Addons mới
					// ✅ Tối ưu: Load tất cả addons 1 lần
					var addonIds = dto.Addons.Select(a => a.AddonId).ToList();
					var addons = await _context.Addons
						.Where(a => addonIds.Contains(a.Id))
						.ToDictionaryAsync(a => a.Id, a => a);

					foreach (var addonDto in dto.Addons)
					{
						// ✅ Lấy giá từ database nếu unitPrice = 0 hoặc null
						decimal finalUnitPrice = addonDto.UnitPrice;
						if (finalUnitPrice <= 0 && addons.TryGetValue(addonDto.AddonId, out var addon))
						{
							finalUnitPrice = addon.Price;
						}

						var quoteAddon = new QuoteAddon
						{
							QuoteId = existingQuote.Id,
							AddonId = addonDto.AddonId,
							Quantity = addonDto.Quantity,
							UnitPrice = finalUnitPrice, // ✅ Sử dụng giá đã xử lý
							Notes = addonDto.Notes,
							CreatedAt = DateTime.UtcNow
						};
						_context.QuoteAddons.Add(quoteAddon);
					}
				}

				await _context.SaveChangesAsync();

				await _context.Entry(existingQuote).Reference(q => q.Customer).LoadAsync();
				await _context.Entry(existingQuote).Reference(q => q.CreatedByUser).LoadAsync();
				await _context.Entry(existingQuote).Reference(q => q.CategoryServiceAddon).LoadAsync();
				await _context.Entry(existingQuote).Collection(q => q.QuoteServices).LoadAsync();
				await _context.Entry(existingQuote).Collection(q => q.QuoteAddons).LoadAsync();

				foreach (var qs in existingQuote.QuoteServices)
				{
					await _context.Entry(qs).Reference(x => x.Service).LoadAsync();
				}
				foreach (var qa in existingQuote.QuoteAddons)
				{
					await _context.Entry(qa).Reference(x => x.Addon).LoadAsync();
				}

				return Ok(existingQuote);
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!QuoteExists(id))
				{
					return NotFound(new { message = "Không tìm thấy báo giá" });
				}
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật báo giá với ID: {QuoteId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật báo giá", error = ex.Message });
			}
		}

		// DELETE: api/Quotes/5
		[HttpDelete("{id}")]
		//[Authorize]
		public async Task<IActionResult> DeleteQuote(int id)
		{
			try
			{
				var quote = await _context.Quotes.FindAsync(id);
				if (quote == null)
				{
					return NotFound(new { message = "Không tìm thấy báo giá" });
				}

				// ✅ Xóa file PDF nếu tồn tại
				if (!string.IsNullOrEmpty(quote.FilePath))
				{
					// Remove leading slash if exists
					var relativePath = quote.FilePath.TrimStart('/');
					var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
					
					if (System.IO.File.Exists(filePath))
					{
						try
						{
							System.IO.File.Delete(filePath);
							_logger.LogInformation("Đã xóa file PDF báo giá: {FilePath}", quote.FilePath);
						}
						catch (Exception ex)
						{
							_logger.LogWarning(ex, "Không thể xóa file PDF báo giá: {FilePath}", quote.FilePath);
							// Tiếp tục xóa record trong database dù file không xóa được
						}
					}
				}

				_context.Quotes.Remove(quote);
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa báo giá với ID: {QuoteId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa báo giá", error = ex.Message });
			}
		}

		private bool QuoteExists(int id)
		{
			return _context.Quotes.Any(e => e.Id == id);
		}

		// ==================== PREVIEW QUOTE HTML ====================
		
		// GET: api/Quotes/5/preview
		[HttpGet("{id}/preview")]
		public async Task<IActionResult> PreviewQuote(int id)
		{
			try
			{
				var quote = await _context.Quotes
					.Include(q => q.Customer)
					.Include(q => q.CreatedByUser)
						.ThenInclude(u => u.Position)
					.Include(q => q.CategoryServiceAddon)
					.Include(q => q.QuoteServices)
						.ThenInclude(qs => qs.Service)
							.ThenInclude(s => s!.Tax)
					.Include(q => q.QuoteAddons)
						.ThenInclude(qa => qa.Addon)
							.ThenInclude(a => a!.Tax)
					.FirstOrDefaultAsync(q => q.Id == id);

				if (quote == null)
					return NotFound(new { message = "Không tìm thấy báo giá" });

				if (quote.Customer == null)
					return BadRequest(new { message = "Customer không tồn tại" });

				// ✅ Lấy template từ database thay vì file
				var template = await _context.DocumentTemplates
					.Where(t => t.Code == "QUOTE_DEFAULT" && t.IsActive)
					.FirstOrDefaultAsync();

				if (template == null)
					return NotFound(new { message = "Không tìm thấy template báo giá trong database" });

				var htmlContent = BindQuoteDataToTemplate(template.HtmlContent, quote);

				return Content(htmlContent, "text/html");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi preview báo giá ID: {QuoteId}", id);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// ==================== EXPORT QUOTE PDF ====================

		// GET: api/Quotes/5/export-pdf
		[HttpGet("{id}/export-pdf")]
		public async Task<IActionResult> ExportQuotePdf(int id)
		{
			try
			{
				var quote = await _context.Quotes
					.Include(q => q.Customer)
					.Include(q => q.CreatedByUser)
						.ThenInclude(u => u.Position)
					.Include(q => q.CategoryServiceAddon)
					.Include(q => q.QuoteServices)
						.ThenInclude(qs => qs.Service)
							.ThenInclude(s => s!.Tax)
					.Include(q => q.QuoteAddons)
						.ThenInclude(qa => qa.Addon)
							.ThenInclude(a => a!.Tax)
					.FirstOrDefaultAsync(q => q.Id == id);

				if (quote == null)
					return NotFound(new { message = "Không tìm thấy báo giá" });

				if (quote.Customer == null)
					return BadRequest(new { message = "Customer không tồn tại" });

				// ✅ Lấy template từ database thay vì file
				var template = await _context.DocumentTemplates
					.Where(t => t.Code == "QUOTE_DEFAULT" && t.IsActive)
					.FirstOrDefaultAsync();

				if (template == null)
					return NotFound(new { message = "Không tìm thấy template báo giá trong database" });

				var htmlContent = BindQuoteDataToTemplate(template.HtmlContent, quote);

				// ✅ Use PuppeteerSharp through IPdfService
				var pdfBytes = await _pdfService.ConvertHtmlToPdfAsync(htmlContent);

				var fileName = $"BaoGia_{quote.Id}_{DateTime.Now:yyyyMMdd}.pdf";
				var quotesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Quotes");
				
				if (!Directory.Exists(quotesFolder))
					Directory.CreateDirectory(quotesFolder);

				var filePath = Path.Combine(quotesFolder, fileName);
				await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

				quote.FilePath = $"/Quotes/{fileName}";
				quote.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				return File(pdfBytes, "application/pdf", fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xuất báo giá PDF cho Quote ID: {QuoteId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xuất báo giá", error = ex.Message });
			}
		}

		// ==================== HELPER METHODS ====================

		private string BindQuoteDataToTemplate(string template, Quote quote)
		{
			// ✅ Decode escape characters từ database
			template = System.Text.RegularExpressions.Regex.Unescape(template);

			var customer = quote.Customer!;
			var now = DateTime.Now;

			template = template
				.Replace("{{ngay_thang_nam}}", now.ToString("dd/MM/yyyy"))
				.Replace("{{ten_khach_hang}}", customer.Name ?? customer.CompanyName ?? "")
				.Replace("{{dia_chi_khach_hang}}", customer.Address ?? customer.CompanyAddress ?? "")
				.Replace("{{sdt_khach_hang}}", customer.PhoneNumber ?? customer.RepresentativePhone ?? "")
				.Replace("{{email_khach_hang}}", customer.Email ?? customer.RepresentativeEmail ?? "");

			var createdByUser = quote.CreatedByUser;
			if (createdByUser != null)
			{
				template = template
					.Replace("{{nguoi_tao_quote}}", createdByUser.Name ?? "")
					.Replace("{{email_nguoi_tao}}", createdByUser.Email ?? "")
					.Replace("{{sdt_nguoi_tao}}", createdByUser.PhoneNumber ?? "")
					.Replace("{{chuc_vu_nguoi_tao}}", createdByUser.Position?.PositionName ?? "");
			}
			else
			{
				template = template
					.Replace("{{nguoi_tao_quote}}", "")
					.Replace("{{email_nguoi_tao}}", "")
					.Replace("{{sdt_nguoi_tao}}", "")
					.Replace("{{chuc_vu_nguoi_tao}}", "");
			}

			var categoryServiceAddon = quote.CategoryServiceAddon;
			if (categoryServiceAddon != null)
			{
				template = template.Replace("{{ten_category_service_addon}}", categoryServiceAddon.Name ?? "");
			}
			else
			{
				template = template.Replace("{{ten_category_service_addon}}", "");
			}

			// ✅ Generate dynamic summary items table (ONLY Services and Addons)
			var summaryItemsHtml = GenerateSummaryItemsTable(quote);
			template = template.Replace("{{SummaryItems}}", summaryItemsHtml);

			// Calculate totals for summary section
			decimal grandTotal = CalculateGrandTotal(quote);
			template = template.Replace("{{tong_thanh_toan}}", grandTotal.ToString("N0"));

			// Generate detailed items table (ALL: Services + Addons + CustomService)
			var detailItemsHtml = GenerateDetailItemsTable(quote);
			template = template.Replace("{{DetailItems}}", detailItemsHtml);
			template = template.Replace("{{tong_thanh_toan_chi_tiet}}", grandTotal.ToString("N0"));

			return template;
		}

		// ✅ Generate Summary Items Table - ONLY Services and Addons (NO CustomService)
		private string GenerateSummaryItemsTable(Quote quote)
		{
			var items = new System.Text.StringBuilder();
			var index = 1;

			// Process Services ONLY
			foreach (var qs in quote.QuoteServices)
			{
				var service = qs.Service;
				if (service == null) continue;

				var lineTotal = qs.UnitPrice * qs.Quantity;
				// ✅ Get tax rate from Service.Tax, default to 0 if not set
				var vatRate = service.Tax?.Rate ?? 0f;
				var totalWithVat = lineTotal * (1 + (decimal)vatRate / 100);

				items.AppendLine($@"
                <tr>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center; vertical-align: top; font-size: 11px;'>
                        {index++}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: left; vertical-align: top; font-size: 11px;'>
                        <strong>{service.Name}</strong>
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right; vertical-align: top; font-size: 11px;'>
                        {qs.UnitPrice:N0}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center; vertical-align: top; font-size: 11px;'>
                        {qs.Quantity}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right; vertical-align: top; font-size: 11px;'>
                        {vatRate:N2}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right; vertical-align: top; font-size: 11px;'>
                        {totalWithVat:N0}
                    </td>
                </tr>");
			}

			// Process Addons ONLY
			foreach (var qa in quote.QuoteAddons)
			{
				var addon = qa.Addon;
				if (addon == null) continue;

				var lineTotal = qa.UnitPrice * qa.Quantity;
				// ✅ Get tax rate from Addon.Tax, default to 0 if not set
				var vatRate = addon.Tax?.Rate ?? 0f;
				var totalWithVat = lineTotal * (1 + (decimal)vatRate / 100);

				items.AppendLine($@"
                <tr>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center; vertical-align: top; font-size: 11px;'>
                        {index++}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: left; vertical-align: top; font-size: 11px;'>
                        <strong>{addon.Name}</strong>
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right; vertical-align: top; font-size: 11px;'>
                        {qa.UnitPrice:N0}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center; vertical-align: top; font-size: 11px;'>
                        {qa.Quantity}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right; vertical-align: top; font-size: 11px;'>
                        {vatRate:N2}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right; vertical-align: top; font-size: 11px;'>
                        {totalWithVat:N0}
                    </td>
                </tr>");
			}

			// ❌ DO NOT include CustomService in Summary Table

			return items.ToString();
		}

		// ✅ Generate Detail Items Table - ONLY CustomService (NO Services/Addons)
		private string GenerateDetailItemsTable(Quote quote)
		{
			var items = new System.Text.StringBuilder();
			var index = 1;

			// ❌ DO NOT Process Services in Detail Table
			// ❌ DO NOT Process Addons in Detail Table

			// ✅ Process Custom Services ONLY - This is the ONLY section in Detail Table
			if (quote.CustomService != null && quote.CustomService.Any())
			{
				// ✅ Create a lookup dictionary for services to get their tax rates
				var serviceTaxLookup = quote.QuoteServices
					.Where(qs => qs.Service != null)
					.ToDictionary(
						qs => qs.Service!.Name, 
						qs => qs.Service!.Tax?.Rate ?? 0f
					);

				foreach (var customService in quote.CustomService)
				{
					var categoryName = quote.CategoryServiceAddon?.Name ?? 
						(!string.IsNullOrEmpty(customService.RelatedService) ? customService.RelatedService : "Dịch vụ tùy chỉnh");
					
					var lineTotal = customService.UnitPrice * customService.Quantity;
					
					// ✅ Get tax rate from the custom service, or try to match with related service
					float vatRate = customService.Tax;
					
					// If tax is 0 and there's a related service, try to get tax from that service
					if (vatRate == 0 && !string.IsNullOrEmpty(customService.RelatedService))
					{
						if (serviceTaxLookup.TryGetValue(customService.RelatedService, out var relatedTax))
						{
							vatRate = relatedTax;
						}
					}
					
					var totalWithVat = lineTotal * (1 + (decimal)vatRate / 100);

					items.AppendLine($@"
                    <tr>
                        <td style='border: 1px solid #ddd; padding: 8px; text-align: center; vertical-align: top; font-size: 11px;'>
                            {index++}
                        </td>
                        <td style='border: 1px solid #ddd; padding: 8px; text-align: left; vertical-align: top; font-size: 11px;'>
                            <strong style='color: #000'>{customService.ServiceName}</strong>
                        </td>
                        <td style='border: 1px solid #ddd; padding: 8px; text-align: left; vertical-align: top; font-size: 11px;'>
                            {categoryName}
                        </td>
                        <td style='border: 1px solid #ddd; padding: 8px; text-align: right; vertical-align: top; font-size: 11px;'>
                            {customService.UnitPrice:N0}
                        </td>
                        <td style='border: 1px solid #ddd; padding: 8px; text-align: center; vertical-align: top; font-size: 11px;'>
                            {customService.Quantity}
                        </td>
                        <td style='border: 1px solid #ddd; padding: 8px; text-align: right; vertical-align: top; font-size: 11px;'>
                            {vatRate:N2}
                        </td>
                        <td style='border: 1px solid #ddd; padding: 8px; text-align: right; vertical-align: top; font-size: 11px;'>
                            {totalWithVat:N0}
                        </td>
                    </tr>");
				}
			}

			return items.ToString();
		}

		// ✅ Calculate Grand Total (ONLY Services + Addons, NO CustomService)
		private decimal CalculateGrandTotal(Quote quote)
		{
			decimal total = 0;

			// Process Services ONLY
			foreach (var qs in quote.QuoteServices)
			{
				var service = qs.Service;
				if (service == null) continue;

				var lineTotal = qs.UnitPrice * qs.Quantity;
				// ✅ Get tax rate from Service.Tax, default to 0 if not set
				var vatRate = service.Tax?.Rate ?? 0f;
				total += lineTotal * (1 + (decimal)vatRate / 100);
			}

			// Process Addons ONLY
			foreach (var qa in quote.QuoteAddons)
			{
				var addon = qa.Addon;
				if (addon == null) continue;

				var lineTotal = qa.UnitPrice * qa.Quantity;
				// ✅ Get tax rate from Addon.Tax, default to 0 if not set
				var vatRate = addon.Tax?.Rate ?? 0f;
				total += lineTotal * (1 + (decimal)vatRate / 100);
			}

			// ❌ DO NOT include CustomService in grand total

			return total;
		}
	}
}
