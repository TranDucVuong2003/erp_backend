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
	public class RolesController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<RolesController> _logger;

		public RolesController(ApplicationDbContext context, ILogger<RolesController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: api/Roles
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Roles>>> GetRoles()
		{
			try
			{
				var roles = await _context.Roles
					.OrderBy(r => r.Name)
					.ToListAsync();

				return Ok(roles);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách vai trò");
				return StatusCode(500, new { message = "L?i server khi l?y danh sách vai trò", error = ex.Message });
			}
		}

		// GET: api/Roles/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Roles>> GetRole(int id)
		{
			try
			{
				var role = await _context.Roles.FindAsync(id);

				if (role == null)
				{
					return NotFound(new { message = "Không tìm th?y vai trò" });
				}

				return Ok(role);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y thông tin vai trò v?i ID: {RoleId}", id);
				return StatusCode(500, new { message = "L?i server khi l?y thông tin vai trò", error = ex.Message });
			}
		}

		// POST: api/Roles
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<Roles>> CreateRole(Roles role)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Ki?m tra trùng tên vai trò
				var existingRole = await _context.Roles
					.FirstOrDefaultAsync(r => r.Name.ToLower() == role.Name.ToLower());

				if (existingRole != null)
				{
					return BadRequest(new { message = "Vai trò v?i tên này ?ã t?n t?i" });
				}

				role.CreatedAt = DateTime.UtcNow;

				_context.Roles.Add(role);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã t?o vai trò m?i v?i ID: {RoleId}", role.Id);

				return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi t?o vai trò m?i");
				return StatusCode(500, new { message = "L?i server khi t?o vai trò", error = ex.Message });
			}
		}

		// PUT: api/Roles/5
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateRole(int id, Roles role)
		{
			if (id != role.Id)
			{
				return BadRequest(new { message = "ID không kh?p" });
			}

			try
			{
				var existingRole = await _context.Roles.FindAsync(id);
				if (existingRole == null)
				{
					return NotFound(new { message = "Không tìm th?y vai trò" });
				}

				// Ki?m tra trùng tên (ngo?i tr? chính nó)
				var duplicateRole = await _context.Roles
					.FirstOrDefaultAsync(r => r.Id != id && r.Name.ToLower() == role.Name.ToLower());

				if (duplicateRole != null)
				{
					return BadRequest(new { message = "Vai trò v?i tên này ?ã t?n t?i" });
				}

				existingRole.Name = role.Name;
				existingRole.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã c?p nh?t vai trò v?i ID: {RoleId}", id);

				return Ok(new { message = "C?p nh?t vai trò thành công", role = existingRole });
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!RoleExists(id))
				{
					return NotFound(new { message = "Không tìm th?y vai trò" });
				}
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi c?p nh?t vai trò v?i ID: {RoleId}", id);
				return StatusCode(500, new { message = "L?i server khi c?p nh?t vai trò", error = ex.Message });
			}
		}

		// DELETE: api/Roles/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteRole(int id)
		{
			try
			{
				var role = await _context.Roles.FindAsync(id);
				if (role == null)
				{
					return NotFound(new { message = "Không tìm th?y vai trò" });
				}

				// Ki?m tra xem có user nào ?ang s? d?ng vai trò này không
				var usersUsingRole = await _context.Users
					.AnyAsync(u => u.RoleId == id);

				if (usersUsingRole)
				{
					return BadRequest(new { message = "Không th? xóa vai trò này vì ?ang có ng??i dùng s? d?ng" });
				}

				_context.Roles.Remove(role);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã xóa vai trò v?i ID: {RoleId}", id);

				return Ok(new { message = "Xóa vai trò thành công", deletedId = id });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi xóa vai trò v?i ID: {RoleId}", id);
				return StatusCode(500, new { message = "L?i server khi xóa vai trò", error = ex.Message });
			}
		}

		private bool RoleExists(int id)
		{
			return _context.Roles.Any(e => e.Id == id);
		}
	}
}
