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
	public class ResionsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<ResionsController> _logger;

		public ResionsController(ApplicationDbContext context, ILogger<ResionsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: api/Resions
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Resion>>> GetResions()
		{
			try
			{
				var resions = await _context.Resions
					.OrderBy(r => r.City)
					.ToListAsync();

				return Ok(resions);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách khu vực");
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách khu vực", error = ex.Message });
			}
		}

		// GET: api/Resions/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Resion>> GetResion(int id)
		{
			try
			{
				var resion = await _context.Resions.FindAsync(id);

				if (resion == null)
				{
					return NotFound(new { message = "Không tìm thấy khu vực" });
				}

				return Ok(resion);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy thông tin khu vực với ID: {ResionId}", id);
				return StatusCode(500, new { message = "Lỗi server khi lấy thông tin khu vực", error = ex.Message });
			}
		}

		// POST: api/Resions
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<Resion>> CreateResion(Resion resion)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Kiểm tra trùng tên thành phố
				var existingResion = await _context.Resions
					.FirstOrDefaultAsync(r => r.City.ToLower() == resion.City.ToLower());

				if (existingResion != null)
				{
					return BadRequest(new { message = "Khu vực với tên thành phố này đã tồn tại" });
				}

				resion.CreatedAt = DateTime.UtcNow;

				_context.Resions.Add(resion);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã tạo khu vực mới với ID: {ResionId}", resion.Id);

				return CreatedAtAction(nameof(GetResion), new { id = resion.Id }, resion);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo khu vực mới");
				return StatusCode(500, new { message = "Lỗi server khi tạo khu vực", error = ex.Message });
			}
		}

		// PUT: api/Resions/5
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateResion(int id, Resion resion)
		{
			if (id != resion.Id)
			{
				return BadRequest(new { message = "ID không khớp" });
			}

			try
			{
				var existingResion = await _context.Resions.FindAsync(id);
				if (existingResion == null)
				{
					return NotFound(new { message = "Không tìm thấy khu vực" });
				}

				// Kiểm tra trùng tên (ngoại trừ chính nó)
				var duplicateResion = await _context.Resions
					.FirstOrDefaultAsync(r => r.Id != id && r.City.ToLower() == resion.City.ToLower());

				if (duplicateResion != null)
				{
					return BadRequest(new { message = "Khu vực với tên thành phố này đã tồn tại" });
				}

				existingResion.City = resion.City;
				existingResion.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã cập nhật khu vực với ID: {ResionId}", id);

				return Ok(new { message = "Cập nhật khu vực thành công", resion = existingResion });
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ResionExists(id))
				{
					return NotFound(new { message = "Không tìm thấy khu vực" });
				}
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật khu vực với ID: {ResionId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật khu vực", error = ex.Message });
			}
		}

		// DELETE: api/Resions/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteResion(int id)
		{
			try
			{
				var resion = await _context.Resions.FindAsync(id);
				if (resion == null)
				{
					return NotFound(new { message = "Không tìm thấy khu vực" });
				}

				// Kiểm tra xem có phòng ban nào đang sử dụng khu vực này không
				var departmentsUsingResion = await _context.Departments
					.AnyAsync(d => d.ResionId == id);

				if (departmentsUsingResion)
				{
					return BadRequest(new { message = "Không thể xóa khu vực này vì đang có phòng ban sử dụng" });
				}

				_context.Resions.Remove(resion);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã xóa khu vực với ID: {ResionId}", id);

				return Ok(new { message = "Xóa khu vực thành công", deletedId = id });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa khu vực với ID: {ResionId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa khu vực", error = ex.Message });
			}
		}

		private bool ResionExists(int id)
		{
			return _context.Resions.Any(e => e.Id == id);
		}
	}
}
