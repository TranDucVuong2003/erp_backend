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
	public class PositionsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<PositionsController> _logger;

		public PositionsController(ApplicationDbContext context, ILogger<PositionsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: api/Positions
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Positions>>> GetPositions()
		{
			try
			{
				var positions = await _context.Positions
					.OrderBy(p => p.Level)
					.ThenBy(p => p.PositionName)
					.ToListAsync();

				return Ok(positions);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách ch?c v?");
				return StatusCode(500, new { message = "L?i server khi l?y danh sách ch?c v?", error = ex.Message });
			}
		}

		// GET: api/Positions/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Positions>> GetPosition(int id)
		{
			try
			{
				var position = await _context.Positions.FindAsync(id);

				if (position == null)
				{
					return NotFound(new { message = "Không tìm th?y ch?c v?" });
				}

				return Ok(position);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y thông tin ch?c v? v?i ID: {PositionId}", id);
				return StatusCode(500, new { message = "L?i server khi l?y thông tin ch?c v?", error = ex.Message });
			}
		}

		// GET: api/Positions/level/{level}
		[HttpGet("level/{level}")]
		public async Task<ActionResult<IEnumerable<Positions>>> GetPositionsByLevel(int level)
		{
			try
			{
				var positions = await _context.Positions
					.Where(p => p.Level == level)
					.OrderBy(p => p.PositionName)
					.ToListAsync();

				return Ok(positions);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách ch?c v? theo c?p b?c: {Level}", level);
				return StatusCode(500, new { message = "L?i server khi l?y danh sách ch?c v?", error = ex.Message });
			}
		}

		// POST: api/Positions
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<Positions>> CreatePosition(Positions position)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Ki?m tra trùng tên ch?c v?
				var existingPosition = await _context.Positions
					.FirstOrDefaultAsync(p => p.PositionName.ToLower() == position.PositionName.ToLower());

				if (existingPosition != null)
				{
					return BadRequest(new { message = "Ch?c v? v?i tên này ?ã t?n t?i" });
				}

				position.CreatedAt = DateTime.UtcNow;

				_context.Positions.Add(position);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã t?o ch?c v? m?i v?i ID: {PositionId}", position.Id);

				return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, position);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi t?o ch?c v? m?i");
				return StatusCode(500, new { message = "L?i server khi t?o ch?c v?", error = ex.Message });
			}
		}

		// PUT: api/Positions/5
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdatePosition(int id, Positions position)
		{
			if (id != position.Id)
			{
				return BadRequest(new { message = "ID không kh?p" });
			}

			try
			{
				var existingPosition = await _context.Positions.FindAsync(id);
				if (existingPosition == null)
				{
					return NotFound(new { message = "Không tìm th?y ch?c v?" });
				}

				// Ki?m tra trùng tên (ngo?i tr? chính nó)
				var duplicatePosition = await _context.Positions
					.FirstOrDefaultAsync(p => p.Id != id && p.PositionName.ToLower() == position.PositionName.ToLower());

				if (duplicatePosition != null)
				{
					return BadRequest(new { message = "Ch?c v? v?i tên này ?ã t?n t?i" });
				}

				existingPosition.PositionName = position.PositionName;
				existingPosition.Level = position.Level;
				existingPosition.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã c?p nh?t ch?c v? v?i ID: {PositionId}", id);

				return Ok(new { message = "C?p nh?t ch?c v? thành công", position = existingPosition });
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!PositionExists(id))
				{
					return NotFound(new { message = "Không tìm th?y ch?c v?" });
				}
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi c?p nh?t ch?c v? v?i ID: {PositionId}", id);
				return StatusCode(500, new { message = "L?i server khi c?p nh?t ch?c v?", error = ex.Message });
			}
		}

		// DELETE: api/Positions/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeletePosition(int id)
		{
			try
			{
				var position = await _context.Positions.FindAsync(id);
				if (position == null)
				{
					return NotFound(new { message = "Không tìm th?y ch?c v?" });
				}

				// Ki?m tra xem có user nào ?ang s? d?ng ch?c v? này không
				var usersUsingPosition = await _context.Users
					.AnyAsync(u => u.PositionId == id);

				if (usersUsingPosition)
				{
					return BadRequest(new { message = "Không th? xóa ch?c v? này vì ?ang có ng??i dùng s? d?ng" });
				}

				_context.Positions.Remove(position);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã xóa ch?c v? v?i ID: {PositionId}", id);

				return Ok(new { message = "Xóa ch?c v? thành công", deletedId = id });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi xóa ch?c v? v?i ID: {PositionId}", id);
				return StatusCode(500, new { message = "L?i server khi xóa ch?c v?", error = ex.Message });
			}
		}

		private bool PositionExists(int id)
		{
			return _context.Positions.Any(e => e.Id == id);
		}
	}
}
