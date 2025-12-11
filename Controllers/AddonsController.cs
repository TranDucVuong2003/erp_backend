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
	public class AddonsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<AddonsController> _logger;

		public AddonsController(ApplicationDbContext context, ILogger<AddonsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// Lấy danh sách tất cả addons
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Addon>>> GetAddons()
		{
			return await _context.Addons
				.Include(a => a.Tax)
				.Include(a => a.CategoryServiceAddons)
				.ToListAsync();
		}

		// Lấy addons đang hoạt động
		[HttpGet("active")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Addon>>> GetActiveAddons()
		{
			return await _context.Addons
				.Include(a => a.Tax)
				.Include(a => a.CategoryServiceAddons)
				.Where(a => a.IsActive)
				.ToListAsync();
		}

		// Lấy addons theo categoryId
		[HttpGet("by-category/{categoryId}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Addon>>> GetAddonsByCategoryId(int categoryId)
		{
			return await _context.Addons
				.Include(a => a.Tax)
				.Include(a => a.CategoryServiceAddons)
				.Where(a => a.CategoryId == categoryId)
				.ToListAsync();
		}

		// Lấy addon theo ID
		[HttpGet("{id}")]
		//[Authorize]
		public async Task<ActionResult<Addon>> GetAddon(int id)
		{
			var addon = await _context.Addons
				.Include(a => a.Tax)
				.Include(a => a.CategoryServiceAddons)
				.FirstOrDefaultAsync(a => a.Id == id);

			if (addon == null)
			{
				return NotFound(new { message = "Không tìm thấy addon" });
			}

			return addon;
		}

		// Tạo addon mới
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<Addon>> CreateAddon(Addon addon)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Validate price
				if (addon.Price < 0)
				{
					return BadRequest(new { message = "Giá addon phải lớn hơn hoặc bằng 0" });
				}

				// Validate quantity
				if (addon.Quantity.HasValue && addon.Quantity.Value < 1)
				{
					return BadRequest(new { message = "Số lượng phải lớn hơn 0" });
				}

				addon.CreatedAt = DateTime.UtcNow;
				_context.Addons.Add(addon);
				await _context.SaveChangesAsync();

				return CreatedAtAction(nameof(GetAddon), new { id = addon.Id }, addon);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo addon mới");
				return StatusCode(500, new { message = "Lỗi server khi tạo addon", error = ex.Message });
			}
		}

		// Cập nhật addon
		[HttpPut("{id}")]
		[Authorize]
		public async Task<ActionResult> UpdateAddon(int id, [FromBody] Dictionary<string, object?> updateData)
		{
			try
			{
				var existingAddon = await _context.Addons.FindAsync(id);
				if (existingAddon == null)
				{
					return NotFound(new { message = "Không tìm thấy addon" });
				}

				foreach (var kvp in updateData)
				{
					var propertyName = kvp.Key;
					var value = kvp.Value?.ToString();

					switch (propertyName.ToLower())
					{
						case "name":
							if (!string.IsNullOrEmpty(value))
							{
								if (value.Length > 200)
								{
									return BadRequest(new { message = "Tên addon không được vượt quá 200 ký tự" });
								}
								existingAddon.Name = value;
							}
							break;

						case "description":
							if (value != null)
							{
								if (!string.IsNullOrWhiteSpace(value) && value.Length > 1000)
								{
									return BadRequest(new { message = "Mô tả không được vượt quá 1000 ký tự" });
								}
								existingAddon.Description = string.IsNullOrWhiteSpace(value) ? null : value;
							}
							break;

						case "price":
							if (kvp.Value != null)
							{
								if (decimal.TryParse(kvp.Value.ToString(), out decimal price))
								{
									if (price < 0)
									{
										return BadRequest(new { message = "Giá addon phải lớn hơn hoặc bằng 0" });
									}
									existingAddon.Price = price;
								}
								else
								{
									return BadRequest(new { message = "Giá addon không hợp lệ" });
								}
							}
							break;

						case "quantity":
							if (kvp.Value != null)
							{
								if (int.TryParse(kvp.Value.ToString(), out int quantity))
								{
									if (quantity < 1)
									{
										return BadRequest(new { message = "Số lượng phải lớn hơn 0" });
									}
									existingAddon.Quantity = quantity;
								}
								else
								{
									return BadRequest(new { message = "Số lượng không hợp lệ" });
								}
							}
							break;

						case "categoryid":
							if (kvp.Value != null)
							{
								if (int.TryParse(kvp.Value.ToString(), out int categoryId))
								{
									// Verify category exists
									var categoryExists = await _context.CategoryServiceAddons.AnyAsync(c => c.Id == categoryId);
									if (!categoryExists)
									{
										return BadRequest(new { message = "Category không tồn tại" });
									}
									existingAddon.CategoryId = categoryId;
								}
								else
								{
									return BadRequest(new { message = "CategoryId không hợp lệ" });
								}
							}
							else
							{
								// Cho phép set CategoryId = null
								existingAddon.CategoryId = null;
							}
							break;

						case "taxid":
							if (kvp.Value != null)
							{
								if (int.TryParse(kvp.Value.ToString(), out int taxId))
								{
									// Verify tax exists
									var taxExists = await _context.Taxes.AnyAsync(t => t.Id == taxId);
									if (!taxExists)
									{
										return BadRequest(new { message = "Tax không tồn tại" });
									}
									existingAddon.TaxId = taxId;
								}
								else
								{
									return BadRequest(new { message = "TaxId không hợp lệ" });
								}
							}
							else
							{
								// Cho phép set TaxId = null
								existingAddon.TaxId = null;
							}
							break;

						case "isactive":
							if (kvp.Value != null)
							{
								if (bool.TryParse(kvp.Value.ToString(), out bool isActive))
								{
									existingAddon.IsActive = isActive;
								}
								else
								{
									return BadRequest(new { message = "Giá trị IsActive phải là true hoặc false" });
								}
							}
							break;

						case "notes":
							if (value != null)
							{
								if (!string.IsNullOrWhiteSpace(value) && value.Length > 2000)
								{
									return BadRequest(new { message = "Ghi chú không được vượt quá 2000 ký tự" });
								}
								existingAddon.Notes = string.IsNullOrWhiteSpace(value) ? null : value;
							}
							break;

						case "id":
						case "createdat":
						case "updatedat":
						case "type": // Ignore old type field
							// Bỏ qua các trường này
							break;

						default:
							// Bỏ qua các trường không được hỗ trợ
							break;
					}
				}

				existingAddon.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				var response = new
				{
					Message = "Cập nhật addon thành công",
					Addon = new
					{
						Id = existingAddon.Id,
						Name = existingAddon.Name,
						Description = existingAddon.Description,
						Price = existingAddon.Price,
						Quantity = existingAddon.Quantity,
						CategoryId = existingAddon.CategoryId,
						TaxId = existingAddon.TaxId,
						IsActive = existingAddon.IsActive,
						Notes = existingAddon.Notes
					},
					UpdatedAt = existingAddon.UpdatedAt.Value
				};

				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật addon với ID: {AddonId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật addon", error = ex.Message });
			}
		}

		// Xóa addon
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<ActionResult> DeleteAddon(int id)
		{
			try
			{
				var addon = await _context.Addons.FindAsync(id);
				if (addon == null)
				{
					return NotFound(new { message = "Không tìm thấy addon" });
				}

				var response = new
				{
					Message = "Xóa addon thành công",
					DeletedAddon = new
					{
						Id = addon.Id,
						Name = addon.Name,
						Description = addon.Description,
						Price = addon.Price,
						Quantity = addon.Quantity,
						CategoryId = addon.CategoryId,
						TaxId = addon.TaxId,
						IsActive = addon.IsActive,
						Notes = addon.Notes
					},
					DeletedAt = DateTime.UtcNow
				};

				_context.Addons.Remove(addon);
				await _context.SaveChangesAsync();

				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa addon với ID: {AddonId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa addon", error = ex.Message });
			}
		}

		private bool AddonExists(int id)
		{
			return _context.Addons.Any(e => e.Id == id);
		}
	}
}
