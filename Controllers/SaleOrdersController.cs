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
	public class SaleOrdersController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<SaleOrdersController> _logger;
		
		public SaleOrdersController(ApplicationDbContext context, ILogger<SaleOrdersController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// Lấy danh sách tất cả sale orders
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetSaleOrders()
		{
			var saleOrders = await _context.SaleOrders
				.Include(so => so.SaleOrderServices)
					.ThenInclude(sos => sos.Service)
				.Include(so => so.SaleOrderAddons)
					.ThenInclude(soa => soa.Addon)
				.Include(so => so.Customer)
				.Include(so => so.Tax)
				.ToListAsync();

			// Map sang response có đầy đủ thông tin
			var response = saleOrders.Select(so => new
			{
				so.Id,
				so.Title,
				so.CustomerId,
				Customer = so.Customer != null ? new { so.Customer.Id, so.Customer.Name } : null,
				so.Value,
				so.Probability,
				so.TaxId,
				Tax = so.Tax != null ? new { so.Tax.Id, so.Tax.Rate } : null,
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

		// Lấy sale orders theo customer ID
		[HttpGet("by-customer/{customerId}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetSaleOrdersByCustomer(int customerId)
		{
			var saleOrders = await _context.SaleOrders
				.Include(so => so.SaleOrderServices)
					.ThenInclude(sos => sos.Service)
				.Include(so => so.SaleOrderAddons)
					.ThenInclude(soa => soa.Addon)
				.Include(so => so.Customer)
				.Include(so => so.Tax)
				.Where(d => d.CustomerId == customerId)
				.ToListAsync();

			// Map sang response có đầy đủ thông tin
			var response = saleOrders.Select(so => new
			{
				so.Id,
				so.Title,
				so.CustomerId,
				Customer = so.Customer != null ? new { so.Customer.Id, so.Customer.Name } : null,
				so.Value,
				so.Probability,
				so.TaxId,
				Tax = so.Tax != null ? new { so.Tax.Id, so.Tax.Rate } : null,
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

		// Thống kê sale orders
		[HttpGet("statistics")]
		[Authorize]
		public async Task<ActionResult<object>> GetSaleOrderStatistics()
		{
			var totalSaleOrders = await _context.SaleOrders.CountAsync();
			var totalValue = await _context.SaleOrders.SumAsync(d => d.Value);
			var averageProbability = totalSaleOrders > 0 ? await _context.SaleOrders.AverageAsync(d => d.Probability) : 0;

			var saleOrders = await _context.SaleOrders.ToListAsync();
			var probabilityRanges = saleOrders
				.GroupBy(d => d.Probability switch
				{
					>= 0 and <= 25 => "Thấp (0-25%)",
					> 25 and <= 50 => "Trung bình (26-50%)",
					> 50 and <= 75 => "Cao (51-75%)",
					> 75 and <= 100 => "Rất cao (76-100%)",
					_ => "Không xác định"
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

		// Lấy sale order theo ID
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<object>> GetSaleOrder(int id)
		{
			var saleOrder = await _context.SaleOrders
				.Include(so => so.SaleOrderServices)
					.ThenInclude(sos => sos.Service)
				.Include(so => so.SaleOrderAddons)
					.ThenInclude(soa => soa.Addon)
				.Include(so => so.Customer)
				.Include(so => so.Tax)
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
				Customer = saleOrder.Customer != null ? new { saleOrder.Customer.Id, saleOrder.Customer.Name } : null,
				saleOrder.Value,
				saleOrder.Probability,
				saleOrder.TaxId,
				Tax = saleOrder.Tax != null ? new { saleOrder.Tax.Id, saleOrder.Tax.Rate } : null,
				saleOrder.CreatedAt,
				saleOrder.UpdatedAt,
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

		// Tạo sale order mới với nhiều services và addons
		[HttpPost]
		//[Authorize]
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

				// Kiểm tra customer tồn tại
				var customerExists = await _context.Customers.AnyAsync(c => c.Id == request.CustomerId);
				if (!customerExists)
				{
					return BadRequest(new { message = "Customer không tồn tại" });
				}

				// Kiểm tra và load services
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
						return BadRequest(new { message = $"Service IDs không tồn tại: {string.Join(", ", missingServiceIds)}" });
					}
				}

				// Kiểm tra và load addons
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
						return BadRequest(new { message = $"Addon IDs không tồn tại: {string.Join(", ", missingAddonIds)}" });
					}
				}

				// Kiểm tra phải có ít nhất 1 service hoặc addon
				if (!request.Services.Any() && !request.Addons.Any())
				{
					return BadRequest(new { message = "Phải có ít nhất 1 service hoặc addon" });
				}

				// Tính tổng giá trị - Luôn lấy quantity từ Service/Addon
				decimal totalValue = 0;
				
				foreach (var serviceDto in request.Services)
				{
					var service = services.First(s => s.Id == serviceDto.ServiceId);
					var quantity = service.Quantity ?? 1; // Luôn lấy từ Service
					var unitPrice = serviceDto.UnitPrice ?? service.Price;
					totalValue += unitPrice * quantity;
				}

				foreach (var addonDto in request.Addons)
				{
					var addon = addons.First(a => a.Id == addonDto.AddonId);
					var quantity = addon.Quantity ?? 1; // Luôn lấy từ Addon
					var unitPrice = addonDto.UnitPrice ?? addon.Price;
					totalValue += unitPrice * quantity;
				}

				// Tạo SaleOrder
				var saleOrder = new SaleOrder
				{
					Title = request.Title,
					CustomerId = request.CustomerId,
					Value = totalValue,
					Probability = request.Probability,
					Notes = request.Notes,
					TaxId = request.TaxId,
					CreatedAt = DateTime.UtcNow
				};

				_context.SaleOrders.Add(saleOrder);
				await _context.SaveChangesAsync();

				// Tạo SaleOrderServices - Luôn lấy quantity từ Service
				var saleOrderServices = new List<SaleOrderService>();
				foreach (var serviceDto in request.Services)
				{
					var service = services.First(s => s.Id == serviceDto.ServiceId);
					var saleOrderService = new SaleOrderService
					{
						SaleOrderId = saleOrder.Id,
						ServiceId = serviceDto.ServiceId,
						Quantity = service.Quantity, // Luôn lấy từ Service
						UnitPrice = serviceDto.UnitPrice ?? service.Price,
						Notes = serviceDto.Notes ?? service.Notes,
						duration = serviceDto.Duration ?? 0,
						template = serviceDto.Template ?? string.Empty,
						CreatedAt = DateTime.UtcNow
					};
					saleOrderServices.Add(saleOrderService);
					_context.SaleOrderServices.Add(saleOrderService);
				}

				// Tạo SaleOrderAddons - Luôn lấy quantity từ Addon
				var saleOrderAddons = new List<SaleOrderAddon>();
				foreach (var addonDto in request.Addons)
				{
					var addon = addons.First(a => a.Id == addonDto.AddonId);
					var saleOrderAddon = new SaleOrderAddon
					{
						SaleOrderId = saleOrder.Id,
						AddonId = addonDto.AddonId,
						Quantity = addon.Quantity, // Luôn lấy từ Addon
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

				// Tạo response
				var response = new SaleOrderWithItemsResponse
				{
					Id = saleOrder.Id,
					Title = saleOrder.Title,
					CustomerId = saleOrder.CustomerId,
					Value = saleOrder.Value,
					Probability = saleOrder.Probability,
					TaxId = saleOrder.TaxId,
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
				_logger.LogError(ex, "Lỗi khi tạo sale order với nhiều items");
				return StatusCode(500, new { message = "Lỗi server khi tạo sale order", error = ex.Message });
			}
		}

		// Cập nhật sale order
		[HttpPut("{id}")]
		[Authorize]
		public async Task<ActionResult<UpdateSaleOrderResponse>> UpdateSaleOrder(int id, [FromBody] Dictionary<string, object?> updateData)
		{
			try
			{
				// Kiểm tra tồn tại
				var existingSaleOrder = await _context.SaleOrders.FindAsync(id);
				if (existingSaleOrder == null)
				{
					return NotFound(new { message = "Không tìm thấy sale order" });
				}

				// Cập nhật các trường có trong request
				foreach (var kvp in updateData)
				{
					var propertyName = kvp.Key;
					var value = kvp.Value?.ToString();

					switch (propertyName.ToLower())
					{
						case "title":
							if (!string.IsNullOrEmpty(value))
							{
								if (value.Length > 255)
								{
									return BadRequest(new { message = "Tiêu đề không được vượt quá 255 ký tự" });
								}
								existingSaleOrder.Title = value;
							}
							break;

						case "customerid":
							if (kvp.Value != null && int.TryParse(kvp.Value.ToString(), out int customerId))
							{
								var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId);
								if (!customerExists)
								{
									return BadRequest(new { message = "Customer không tồn tại" });
								}
								existingSaleOrder.CustomerId = customerId;
							}
							else
							{
								return BadRequest(new { message = "Customer ID không hợp lệ" });
							}
							break;

						case "value":
							if (kvp.Value != null && decimal.TryParse(kvp.Value.ToString(), out decimal saleOrderValue))
							{
								if (saleOrderValue < 0)
								{
									return BadRequest(new { message = "Giá trị sale order phải lớn hơn hoặc bằng 0" });
								}
								existingSaleOrder.Value = saleOrderValue;
							}
							else
							{
								return BadRequest(new { message = "Giá trị sale order không hợp lệ" });
							}
							break;

						case "probability":
							if (kvp.Value != null && int.TryParse(kvp.Value.ToString(), out int probability))
							{
								if (probability < 0 || probability > 100)
								{
									return BadRequest(new { message = "Xác suất phải từ 0-100%" });
								}
								existingSaleOrder.Probability = probability;
							}
							else
							{
								return BadRequest(new { message = "Xác suất không hợp lệ" });
							}
							break;

						case "notes":
							if (value != null)
							{
								if (value.Length > 2000)
								{
									return BadRequest(new { message = "Ghi chú không được vượt quá 2000 ký tự" });
								}
								existingSaleOrder.Notes = string.IsNullOrWhiteSpace(value) ? null : value;
							}
							break;

						case "serviceid":
							if (kvp.Value != null && int.TryParse(kvp.Value.ToString(), out int serviceId))
							{
								if (serviceId <= 0)
								{
									return BadRequest(new { message = "Service ID phải lớn hơn 0" });
								}

								var serviceExists = await _context.Services.AnyAsync(s => s.Id == serviceId);
								if (!serviceExists)
								{
									return BadRequest(new { message = "Service không tồn tại" });
								}

								existingSaleOrder.ServiceId = serviceId;
							}
							else
							{
								return BadRequest(new { message = "Service ID không hợp lệ" });
							}
							break;

						case "addonid":
							if (kvp.Value != null && int.TryParse(kvp.Value.ToString(), out int addonId))
							{
								if (addonId > 0)
								{
									var addonExists = await _context.Addons.AnyAsync(a => a.Id == addonId);
									if (!addonExists)
									{
										return BadRequest(new { message = "Addon không tồn tại" });
									}
								}
								existingSaleOrder.AddonId = addonId > 0 ? addonId : null;
							}
							else
							{
								return BadRequest(new { message = "Addon ID không hợp lệ" });
							}
							break;

						case "id":
						case "createdat":
						case "updatedat":
							// Bỏ qua các trường hệ thống
							break;

						default:
							// Bỏ qua trường không hỗ trợ
							break;
					}
				}

				existingSaleOrder.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				var response = new UpdateSaleOrderResponse
				{
					Message = "Cập nhật thông tin sale order thành công",
					SaleOrder = new SaleOrderInfo
					{
						Id = existingSaleOrder.Id,
						Title = existingSaleOrder.Title,
						CustomerId = existingSaleOrder.CustomerId,
						Value = existingSaleOrder.Value,
						Probability = existingSaleOrder.Probability,
						Notes = existingSaleOrder.Notes,
						ServiceId = existingSaleOrder.ServiceId,
						AddonId = existingSaleOrder.AddonId
					},
					UpdatedAt = existingSaleOrder.UpdatedAt.Value
				};

				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật sale order với ID: {SaleOrderId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật sale order", error = ex.Message });
			}
		}

		// Cập nhật xác suất sale order
		[HttpPatch("{id}/probability")]
		[Authorize]
		public async Task<IActionResult> UpdateSaleOrderProbability(int id, [FromBody] Dictionary<string, int> request)
		{
			try
			{
				if (!request.ContainsKey("probability"))
				{
					return BadRequest(new { message = "Thiếu trường probability" });
				}

				var probability = request["probability"];

				if (probability < 0 || probability > 100)
				{
					return BadRequest(new { message = "Xác suất phải từ 0-100%" });
				}

				var saleOrder = await _context.SaleOrders.FindAsync(id);
				if (saleOrder == null)
				{
					return NotFound(new { message = "Không tìm thấy sale order" });
				}

				saleOrder.Probability = probability;
				saleOrder.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				return Ok(new
				{
					message = "Cập nhật xác suất thành công",
					id = saleOrder.Id,
					probability = saleOrder.Probability,
					updatedAt = saleOrder.UpdatedAt
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật xác suất sale order với ID: {SaleOrderId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật xác suất", error = ex.Message });
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
					return NotFound(new { message = "Không tìm thấy sale order" });
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
					AddonId = saleOrder.AddonId
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
				_logger.LogError(ex, "Lỗi khi xóa sale order với ID: {SaleOrderId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa sale order", error = ex.Message });
			}
		}

		// Xuất hợp đồng PDF
		[HttpGet("{id}/export-contract")]
		public async Task<IActionResult> ExportContract(int id)
		{
			try
			{
				// Bước 1: Lấy dữ liệu SaleOrder
				var saleOrder = await _context.SaleOrders.FindAsync(id);

				if (saleOrder == null)
				{
					return NotFound(new { message = "Không tìm thấy sale order" });
				}

				// Load Customer
				var customer = await _context.Customers.FindAsync(saleOrder.CustomerId);
				if (customer == null)
				{
					return NotFound(new { message = "Không tìm thấy thông tin customer" });
				}

				// Load Service (bắt buộc)
				var service = await _context.Services.FindAsync(saleOrder.ServiceId);
				if (service == null)
				{
					return NotFound(new { message = "Không tìm thấy thông tin service" });
				}

				// Load Addon nếu có
				Addon? addon = null;
				if (saleOrder.AddonId.HasValue)
				{
					addon = await _context.Addons.FindAsync(saleOrder.AddonId.Value);
				}

				// Bước 2: Đọc HTML template dựa vào loại khách hàng
				var templateFileName = customer.CustomerType?.ToLower() == "individual" 
					? "generate_contract_individual.html" 
					: "generate_contract_business.html";
				
				var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", templateFileName);
				
				if (!System.IO.File.Exists(templatePath))
				{
					return NotFound(new { message = $"Không tìm thấy template hợp đồng: {templateFileName}" });
				}

				var htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

				// Bước 3: Bind dữ liệu vào template
				var htmlContent = BindDataToTemplate(htmlTemplate, saleOrder, customer, service, addon);

				// Bước 4: Sử dụng IronPDF để convert HTML sang PDF
				var renderer = new ChromePdfRenderer();
				
				// Cấu hình renderer - loại bỏ header/footer
				renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
				renderer.RenderingOptions.MarginTop = 0;
				renderer.RenderingOptions.MarginBottom = 0;
				renderer.RenderingOptions.MarginLeft = 0;
				renderer.RenderingOptions.MarginRight = 0;
				renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;
				renderer.RenderingOptions.PrintHtmlBackgrounds = true;
				renderer.RenderingOptions.CreatePdfFormsFromHtml = false;
				renderer.RenderingOptions.EnableJavaScript = false;

				// Render HTML thành PDF
				var pdf = await Task.Run(() => renderer.RenderHtmlAsPdf(htmlContent));

				// Bước 5: Lấy PDF bytes
				var pdfBytes = pdf.BinaryData;

				// Bước 6: Trả về file PDF
				var fileName = $"HopDong_{saleOrder.Id}_{DateTime.Now:yyyyMMdd}.pdf";
				return File(pdfBytes, "application/pdf", fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xuất hợp đồng PDF cho SaleOrder ID: {SaleOrderId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xuất hợp đồng", error = ex.Message });
			}
		}

		// Helper method: Bind dữ liệu vào template
		private string BindDataToTemplate(string template, SaleOrder saleOrder, Customer customer, Service? service, Addon? addon)
		{
			var now = DateTime.Now;

			//// Convert ảnh thành Base64
			//var backgroundImageBase64 = GetImageAsBase64("wwwroot/Templates/assets/img/File_mau003.png");
			//var logoImageBase64 = GetImageAsBase64("wwwroot/Templates/assets/img/logo.webp");

			//// Thay thế đường dẫn ảnh bằng Base64
			//template = template
			//	.Replace("./assets/img/File_mau003.png", $"data:image/png;base64,{backgroundImageBase64}")
			//	.Replace("./assets/img/logo.webp", $"data:image/webp;base64,{logoImageBase64}");

			// Thông tin hợp đồng cơ bản
			template = template
				.Replace("{{ContractNumber}}", saleOrder.Id.ToString())
				.Replace("{{ContractYear}}", now.Year.ToString())
				.Replace("{{Day}}", now.Day.ToString())
				.Replace("{{Month}}", now.Month.ToString())
				.Replace("{{Year}}", now.Year.ToString())
				.Replace("{{Location}}", "Hà Nội");


			// Thông tin Bên A (Khách hàng)
			template = template
				.Replace("{{CompanyBName}}", customer.CompanyName ?? customer.Name ?? "")
				.Replace("{{CompanyBAddress}}", customer.CompanyAddress ?? customer.Address ?? "")
				.Replace("{{CompanyBTaxCode}}", customer.TaxCode ?? "")
				.Replace("{{CompanyBRepName}}", customer.RepresentativeName ?? customer.Name ?? "")
				.Replace("{{CompanyBRepPosition}}", customer.RepresentativePosition ?? "")
				.Replace("{{CompanyBRepID}}", customer.RepresentativeIdNumber ?? customer.IdNumber ?? "")
				.Replace("{{CompanyBPhone}}", customer.RepresentativePhone ?? customer.PhoneNumber ?? "")
				.Replace("{{CompanyBEmail}}", customer.RepresentativeEmail ?? customer.Email ?? "")
				.Replace("{{CustomerBirthDay}}", customer.BirthDate.Value.Day.ToString())
				.Replace("{{CustomerBirthMonth}}", customer.BirthDate.Value.Month.ToString())
				.Replace("{{CustomerBirthYear}}", customer.BirthDate.Value.Year.ToString());
			;

			// Thông tin giá trị
			var totalValue = saleOrder.Value;
			var vatRate = 10; // 10% VAT
			var vatAmount = totalValue * vatRate / 100;
			var netAmount = totalValue + vatAmount;

			template = template
				.Replace("{{SubTotal}}", totalValue.ToString("N0"))
				.Replace("{{Discount}}", "0")
				.Replace("{{VATRate}}", vatRate.ToString())
				.Replace("{{VATAmount}}", vatAmount.ToString("N0"))
				.Replace("{{NetAmount}}", netAmount.ToString("N0"))
				.Replace("{{AmountInWords}}", ConvertNumberToWords(netAmount))
				.Replace("{{PaymentMethod}}", "Chuyển khoản");

			// Tạo bảng items
			var itemsHtml = GenerateItemsTable(saleOrder, service, addon);
			template = template.Replace("{{Items}}", itemsHtml);

			return template;
		}

		// Helper method: Convert ảnh thành Base64
		private string GetImageAsBase64(string relativePath)
		{
			try
			{
				var imagePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
				if (System.IO.File.Exists(imagePath))
				{
					var imageBytes = System.IO.File.ReadAllBytes(imagePath);
					return Convert.ToBase64String(imageBytes);
				}
				_logger.LogWarning($"Không tìm thấy ảnh tại: {imagePath}");
				return string.Empty;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Lỗi khi đọc ảnh: {relativePath}");
				return string.Empty;
			}
		}

		// Helper method: Tạo bảng danh sách dịch vụ
		private string GenerateItemsTable(SaleOrder saleOrder, Service? service, Addon? addon)
		{
			var items = new System.Text.StringBuilder();
			var index = 1;

			// Thêm Service nếu có
			if (service != null)
			{
				items.AppendLine($@"
					<tr>
						<td style='text-align: center;'>{index++}</td>
						<td>{service.Name}</td>
						<td>{service.Description ?? ""}</td>
						<td style='text-align: center;'>1</td>
						<td style='text-align: right;'>{service.Price:N0}</td>
						<td style='text-align: right;'>{service.Price:N0}</td>
					</tr>");
			}

			// Thêm Addon nếu có
			if (addon != null)
			{
				items.AppendLine($@"
					<tr>
						<td style='text-align: center;'>{index++}</td>
						<td>{addon.Name}</td>
						<td>{addon.Description ?? ""}</td>
						<td style='text-align: center;'>1</td>
						<td style='text-align: right;'>{addon.Price:N0}</td>
						<td style='text-align: right;'>{addon.Price:N0}</td>
					</tr>");
			}

			// Nếu không có service/addon, hiển thị title
			if (service == null && addon == null)
			{
				items.AppendLine($@"
					<tr>
						<td style='text-align: center;'>1</td>
						<td>{saleOrder.Title}</td>
						<td>{saleOrder.Notes ?? ""}</td>
						<td style='text-align: center;'>1</td>
						<td style='text-align: right;'>{saleOrder.Value:N0}</td>
						<td style='text-align: right;'>{saleOrder.Value:N0}</td>
					</tr>");
			}

			return items.ToString();
		}

		// Helper method: Convert số thành chữ (tiếng Việt)
		private string ConvertNumberToWords(decimal number)
		{
			if (number == 0) return "Không đồng";

			var units = new[] { "", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
			var scales = new[] { "", "nghìn", "triệu", "tỷ" };

			var numStr = ((long)number).ToString();
			var result = new System.Text.StringBuilder();
			var scaleIndex = 0;

			while (numStr.Length > 0)
			{
				var groupSize = Math.Min(3, numStr.Length);
				var group = numStr.Substring(numStr.Length - groupSize);
				numStr = numStr.Substring(0, numStr.Length - groupSize);

				if (int.Parse(group) > 0)
				{
					var groupWords = ConvertGroupToWords(int.Parse(group), units);
					if (result.Length > 0)
						result.Insert(0, " ");
					result.Insert(0, scales[scaleIndex]);
					result.Insert(0, " ");
					result.Insert(0, groupWords);
				}

				scaleIndex++;
			}

			return char.ToUpper(result[0]) + result.ToString().Substring(1).Trim() + " đồng chẵn";
		}

		private string ConvertGroupToWords(int number, string[] units)
		{
			var hundred = number / 100;
			var ten = (number % 100) / 10;
			var unit = number % 10;

			var result = new System.Text.StringBuilder();

			if (hundred > 0)
			{
				result.Append(units[hundred]);
				result.Append(" trăm");
			}

			if (ten > 1)
			{
				if (result.Length > 0) result.Append(" ");
				result.Append(units[ten]);
				result.Append(" mươi");
			}
			else if (ten == 1)
			{
				if (result.Length > 0) result.Append(" ");
				result.Append("mười");
			}

			if (unit > 0)
			{
				if (result.Length > 0) result.Append(" ");
				result.Append(units[unit]);
			}

			return result.ToString();
		}

		private bool SaleOrderExists(int id)
		{
			return _context.SaleOrders.Any(e => e.Id == id);
		}
	}
}
