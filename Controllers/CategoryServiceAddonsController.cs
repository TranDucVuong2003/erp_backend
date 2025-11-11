using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CategoryServiceAddonsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<CategoryServiceAddonsController> _logger;

		public CategoryServiceAddonsController(ApplicationDbContext context, ILogger<CategoryServiceAddonsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: api/CategoryServiceAddons
		[HttpGet]
		//[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetCategories()
		{
			var categories = await _context.CategoryServiceAddons
				.Include(c => c.Services)
				.Include(c => c.Addons)
				.ToListAsync();

			var response = categories.Select(c => new
			{
				c.Id,
				c.Name,
				c.CreatedAt,
				c.UpdatedAt,
				ServicesCount = c.Services?.Count ?? 0,
				AddonsCount = c.Addons?.Count ?? 0
			});

			return Ok(response);
		}

		// GET: api/CategoryServiceAddons/5
		[HttpGet("{id}")]
		//[Authorize]
		public async Task<ActionResult<object>> GetCategory(int id)
		{
			var category = await _context.CategoryServiceAddons
				.Include(c => c.Services)
					.ThenInclude(s => s.Tax)
				.Include(c => c.Addons)
					.ThenInclude(a => a.Tax)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (category == null)
			{
				return NotFound(new { message = "Không tìm th?y danh m?c" });
			}

			var response = new
			{
				category.Id,
				category.Name,
				category.CreatedAt,
				category.UpdatedAt,
				Services = category.Services?.Select(s => new
				{
					s.Id,
					s.Name,
					s.Price,
					s.IsActive,
					Tax = s.Tax != null ? new
					{
						s.Tax.Id,
						s.Tax.Rate,
						s.Tax.Notes
					} : null
				}).ToList(),
				Addons = category.Addons?.Select(a => new
				{
					a.Id,
					a.Name,
					a.Price,
					a.IsActive,
					Tax = a.Tax != null ? new
					{
						a.Tax.Id,
						a.Tax.Rate,
						a.Tax.Notes
					} : null
				}).ToList()
			};

			return Ok(response);
		}

		// GET: api/CategoryServiceAddons/{id}/services
		[HttpGet("{id}/services")]
		//[Authorize]
		public async Task<ActionResult<IEnumerable<Service>>> GetCategoryServices(int id)
		{
			var category = await _context.CategoryServiceAddons
				.Include(c => c.Services)
					.ThenInclude(s => s.Tax)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (category == null)
			{
				return NotFound(new { message = "Không tìm th?y danh m?c" });
			}

			return Ok(category.Services);
		}

		// GET: api/CategoryServiceAddons/{id}/addons
		[HttpGet("{id}/addons")]
		//[Authorize]
		public async Task<ActionResult<IEnumerable<Addon>>> GetCategoryAddons(int id)
		{
			var category = await _context.CategoryServiceAddons
				.Include(c => c.Addons)
					.ThenInclude(a => a.Tax)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (category == null)
			{
				return NotFound(new { message = "Không tìm th?y danh m?c" });
			}

			return Ok(category.Addons);
		}

		// POST: api/CategoryServiceAddons
		[HttpPost]
		//[Authorize]
		public async Task<ActionResult<Category_service_addons>> CreateCategory(Category_service_addons category)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Ki?m tra tên danh m?c ?ã t?n t?i ch?a
				var existingCategory = await _context.CategoryServiceAddons
					.FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

				if (existingCategory != null)
				{
					return BadRequest(new { message = "Tên danh m?c ?ã t?n t?i" });
				}

				category.CreatedAt = DateTime.UtcNow;
				_context.CategoryServiceAddons.Add(category);
				await _context.SaveChangesAsync();

				return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi t?o danh m?c m?i");
				return StatusCode(500, new { message = "L?i server khi t?o danh m?c", error = ex.Message });
			}
		}

		// PUT: api/CategoryServiceAddons/5
		[HttpPut("{id}")]
		//[Authorize]
		public async Task<ActionResult<Category_service_addons>> UpdateCategory(int id, Category_service_addons category)
		{
			try
			{
				if (id != category.Id)
				{
					return BadRequest(new { message = "ID không kh?p" });
				}

				// Ki?m tra category có t?n t?i không
				var existingCategory = await _context.CategoryServiceAddons.FindAsync(id);
				if (existingCategory == null)
				{
					return NotFound(new { message = "Không tìm th?y danh m?c" });
				}

				// Ki?m tra tên danh m?c ?ã t?n t?i ch?a (tr? danh m?c hi?n t?i)
				var duplicateCategory = await _context.CategoryServiceAddons
					.FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != id);

				if (duplicateCategory != null)
				{
					return BadRequest(new { message = "Tên danh m?c ?ã t?n t?i" });
				}

				// C?p nh?t các tr??ng
				existingCategory.Name = category.Name;
				existingCategory.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				return Ok(existingCategory);
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CategoryExists(id))
				{
					return NotFound(new { message = "Không tìm th?y danh m?c" });
				}
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi c?p nh?t danh m?c v?i ID: {CategoryId}", id);
				return StatusCode(500, new { message = "L?i server khi c?p nh?t danh m?c", error = ex.Message });
			}
		}

		// DELETE: api/CategoryServiceAddons/5
		[HttpDelete("{id}")]
		//[Authorize]
		public async Task<IActionResult> DeleteCategory(int id)
		{
			try
			{
				var category = await _context.CategoryServiceAddons
					.Include(c => c.Services)
					.Include(c => c.Addons)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (category == null)
				{
					return NotFound(new { message = "Không tìm th?y danh m?c" });
				}

				// Ki?m tra xem có services ho?c addons ?ang s? d?ng category này không
				if (category.Services?.Any() == true || category.Addons?.Any() == true)
				{
					return BadRequest(new 
					{ 
						message = "Không th? xóa danh m?c này vì ?ang có services ho?c addons liên k?t",
						servicesCount = category.Services?.Count ?? 0,
						addonsCount = category.Addons?.Count ?? 0
					});
				}

				_context.CategoryServiceAddons.Remove(category);
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi xóa danh m?c v?i ID: {CategoryId}", id);
				return StatusCode(500, new { message = "L?i server khi xóa danh m?c", error = ex.Message });
			}
		}

		private bool CategoryExists(int id)
		{
			return _context.CategoryServiceAddons.Any(e => e.Id == id);
		}
	}
}
