using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;

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
		public async Task<ActionResult<IEnumerable<SaleOrder>>> GetSaleOrders()
		{
			return await _context.SaleOrders.ToListAsync();
		}

		// Lấy sale orders theo customer ID
		[HttpGet("by-customer/{customerId}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<SaleOrder>>> GetSaleOrdersByCustomer(int customerId)
		{
			return await _context.SaleOrders
				.Where(d => d.CustomerId == customerId)
				.ToListAsync();
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
		public async Task<ActionResult<SaleOrder>> GetSaleOrder(int id)
		{
			var saleOrder = await _context.SaleOrders.FindAsync(id);

			if (saleOrder == null)
			{
				return NotFound(new { message = "Không tìm thấy sale order" });
			}

			return saleOrder;
		}

		// Tạo sale order mới
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<SaleOrder>> CreateSaleOrder(SaleOrder saleOrder)
		{
			try
			{
				// Kiểm tra model validation
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Kiểm tra customer tồn tại
				var customerExists = await _context.Customers.AnyAsync(c => c.Id == saleOrder.CustomerId);
				if (!customerExists)
				{
					return BadRequest(new { message = "Customer không tồn tại" });
				}

				// Validate giá trị sale order
				if (saleOrder.Value < 0)
				{
					return BadRequest(new { message = "Giá trị sale order phải lớn hơn hoặc bằng 0" });
				}

				// Validate xác suất
				if (saleOrder.Probability < 0 || saleOrder.Probability > 100)
				{
					return BadRequest(new { message = "Xác suất phải từ 0-100%" });
				}

				// Kiểm tra service nếu có
				if (saleOrder.ServiceId.HasValue && saleOrder.ServiceId > 0)
				{
					var serviceExists = await _context.Services.AnyAsync(s => s.Id == saleOrder.ServiceId);
					if (!serviceExists)
					{
						return BadRequest(new { message = "Service không tồn tại" });
					}
				}

				// Kiểm tra addon nếu có
				if (saleOrder.AddonId.HasValue && saleOrder.AddonId > 0)
				{
					var addonExists = await _context.Addons.AnyAsync(a => a.Id == saleOrder.AddonId);
					if (!addonExists)
					{
						return BadRequest(new { message = "Addon không tồn tại" });
					}
				}

				saleOrder.CreatedAt = DateTime.UtcNow;
				_context.SaleOrders.Add(saleOrder);
				await _context.SaveChangesAsync();

				return CreatedAtAction(nameof(GetSaleOrder), new { id = saleOrder.Id }, saleOrder);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo sale order mới");
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
								if (serviceId > 0)
								{
									var serviceExists = await _context.Services.AnyAsync(s => s.Id == serviceId);
									if (!serviceExists)
									{
										return BadRequest(new { message = "Service không tồn tại" });
									}
								}
								existingSaleOrder.ServiceId = serviceId > 0 ? serviceId : null;
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

		private bool SaleOrderExists(int id)
		{
			return _context.SaleOrders.Any(e => e.Id == id);
		}
	}
}
