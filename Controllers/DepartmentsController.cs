using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class DepartmentsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<DepartmentsController> _logger;

		public DepartmentsController(ApplicationDbContext context, ILogger<DepartmentsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: api/Departments
		[HttpGet]
		public async Task<ActionResult<IEnumerable<object>>> GetDepartments()
		{
			try
			{
				var departments = await _context.Departments
					.Include(d => d.Resion)
					.OrderBy(d => d.Name)
					.Select(d => new
					{
						d.Id,
						d.Name,
						d.ResionId,
						Resion = d.Resion != null ? new
						{
							d.Resion.Id,
							d.Resion.City
						} : null,
						d.CreatedAt,
						d.UpdatedAt
					})
					.ToListAsync();

				return Ok(departments);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách phòng ban");
				return StatusCode(500, new { message = "L?i server khi l?y danh sách phòng ban", error = ex.Message });
			}
		}

		// GET: api/Departments/5
		[HttpGet("{id}")]
		public async Task<ActionResult<object>> GetDepartment(int id)
		{
			try
			{
				var department = await _context.Departments
					.Include(d => d.Resion)
					.Where(d => d.Id == id)
					.Select(d => new
					{
						d.Id,
						d.Name,
						d.ResionId,
						Resion = d.Resion != null ? new
						{
							d.Resion.Id,
							d.Resion.City
						} : null,
						d.CreatedAt,
						d.UpdatedAt
					})
					.FirstOrDefaultAsync();

				if (department == null)
				{
					return NotFound(new { message = "Không tìm th?y phòng ban" });
				}

				return Ok(department);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y thông tin phòng ban v?i ID: {DepartmentId}", id);
				return StatusCode(500, new { message = "L?i server khi l?y thông tin phòng ban", error = ex.Message });
			}
		}

		// GET: api/Departments/resion/5
		[HttpGet("resion/{resionId}")]
		public async Task<ActionResult<IEnumerable<object>>> GetDepartmentsByResion(int resionId)
		{
			try
			{
				var departments = await _context.Departments
					.Include(d => d.Resion)
					.Where(d => d.ResionId == resionId)
					.OrderBy(d => d.Name)
					.Select(d => new
					{
						d.Id,
						d.Name,
						d.ResionId,
						Resion = d.Resion != null ? new
						{
							d.Resion.Id,
							d.Resion.City
						} : null,
						d.CreatedAt,
						d.UpdatedAt
					})
					.ToListAsync();

				return Ok(departments);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách phòng ban theo khu v?c: {ResionId}", resionId);
				return StatusCode(500, new { message = "L?i server khi l?y danh sách phòng ban", error = ex.Message });
			}
		}

		// POST: api/Departments
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<Departments>> CreateDepartment(Departments department)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Ki?m tra Resion có t?n t?i không
				var resionExists = await _context.Resions.AnyAsync(r => r.Id == department.ResionId);
				if (!resionExists)
				{
					return BadRequest(new { message = "Khu v?c không t?n t?i" });
				}

				// Ki?m tra trùng tên phòng ban
				var existingDepartment = await _context.Departments
					.FirstOrDefaultAsync(d => d.Name.ToLower() == department.Name.ToLower());

				if (existingDepartment != null)
				{
					return BadRequest(new { message = "Phòng ban v?i tên này ?ã t?n t?i" });
				}

				department.CreatedAt = DateTime.UtcNow;

				_context.Departments.Add(department);
				await _context.SaveChangesAsync();

				// Load l?i v?i navigation property
				var createdDepartment = await _context.Departments
					.Include(d => d.Resion)
					.FirstAsync(d => d.Id == department.Id);

				_logger.LogInformation("?ã t?o phòng ban m?i v?i ID: {DepartmentId}", department.Id);

				return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, new
				{
					createdDepartment.Id,
					createdDepartment.Name,
					createdDepartment.ResionId,
					Resion = createdDepartment.Resion != null ? new
					{
						createdDepartment.Resion.Id,
						createdDepartment.Resion.City
					} : null,
					createdDepartment.CreatedAt,
					createdDepartment.UpdatedAt
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi t?o phòng ban m?i");
				return StatusCode(500, new { message = "L?i server khi t?o phòng ban", error = ex.Message });
			}
		}

		// PUT: api/Departments/5
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateDepartment(int id, Departments department)
		{
			if (id != department.Id)
			{
				return BadRequest(new { message = "ID không kh?p" });
			}

			try
			{
				var existingDepartment = await _context.Departments.FindAsync(id);
				if (existingDepartment == null)
				{
					return NotFound(new { message = "Không tìm th?y phòng ban" });
				}

				// Ki?m tra Resion có t?n t?i không
				var resionExists = await _context.Resions.AnyAsync(r => r.Id == department.ResionId);
				if (!resionExists)
				{
					return BadRequest(new { message = "Khu v?c không t?n t?i" });
				}

				// Ki?m tra trùng tên (ngo?i tr? chính nó)
				var duplicateDepartment = await _context.Departments
					.FirstOrDefaultAsync(d => d.Id != id && d.Name.ToLower() == department.Name.ToLower());

				if (duplicateDepartment != null)
				{
					return BadRequest(new { message = "Phòng ban v?i tên này ?ã t?n t?i" });
				}

				existingDepartment.Name = department.Name;
				existingDepartment.ResionId = department.ResionId;
				existingDepartment.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				// Load l?i v?i navigation property
				var updatedDepartment = await _context.Departments
					.Include(d => d.Resion)
					.FirstAsync(d => d.Id == id);

				_logger.LogInformation("?ã c?p nh?t phòng ban v?i ID: {DepartmentId}", id);

				return Ok(new
				{
					message = "C?p nh?t phòng ban thành công",
					department = new
					{
						updatedDepartment.Id,
						updatedDepartment.Name,
						updatedDepartment.ResionId,
						Resion = updatedDepartment.Resion != null ? new
						{
							updatedDepartment.Resion.Id,
							updatedDepartment.Resion.City
						} : null,
						updatedDepartment.CreatedAt,
						updatedDepartment.UpdatedAt
					}
				});
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!DepartmentExists(id))
				{
					return NotFound(new { message = "Không tìm th?y phòng ban" });
				}
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi c?p nh?t phòng ban v?i ID: {DepartmentId}", id);
				return StatusCode(500, new { message = "L?i server khi c?p nh?t phòng ban", error = ex.Message });
			}
		}

		// DELETE: api/Departments/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteDepartment(int id)
		{
			try
			{
				var department = await _context.Departments.FindAsync(id);
				if (department == null)
				{
					return NotFound(new { message = "Không tìm th?y phòng ban" });
				}

				// Ki?m tra xem có user nào ?ang s? d?ng phòng ban này không
				var usersUsingDepartment = await _context.Users
					.AnyAsync(u => u.DepartmentId == id);

				if (usersUsingDepartment)
				{
					return BadRequest(new { message = "Không th? xóa phòng ban này vì ?ang có ng??i dùng s? d?ng" });
				}

				_context.Departments.Remove(department);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã xóa phòng ban v?i ID: {DepartmentId}", id);

				return Ok(new { message = "Xóa phòng ban thành công", deletedId = id });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi xóa phòng ban v?i ID: {DepartmentId}", id);
				return StatusCode(500, new { message = "L?i server khi xóa phòng ban", error = ex.Message });
			}
		}

		private bool DepartmentExists(int id)
		{
			return _context.Departments.Any(e => e.Id == id);
		}
	}
}
