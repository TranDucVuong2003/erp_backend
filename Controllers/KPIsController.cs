using erp_backend.Data;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class KPIsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<KPIsController> _logger;

		public KPIsController(ApplicationDbContext context, ILogger<KPIsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		//Lấy dữ liệu
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<KPI>>> GetKPIs()
		{
			try
			{
				// Lấy role từ JWT token
				var roleClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
				var role = roleClaim?.Value;

				// Admin có thể xem tất cả KPI
				if (role != null && role.ToLower() == "admin")
				{
					_logger.LogInformation("Admin role detected. Returning all KPIs");
					var kpis = await _context.KPIs
						.OrderByDescending(k => k.CreatedAt)
						.ToListAsync();
					return Ok(kpis);
				}
				else if (role != null && role.ToLower() == "user")
				{
					// User chỉ xem KPI của phòng ban mình (nếu cần)

					var userIdClaim = User.FindFirst("userid");
					var userId = int.Parse(userIdClaim.Value);


					if (userId == null)
					{
						_logger.LogWarning("Không tìm thấy UserID");
						return Forbid();
					}


					var user = await _context.Users
						.Include(u => u.Position)
						.Include(u => u.Department)
						.FirstOrDefaultAsync(u => u.Id == userId);

					if (user == null)
					{
						_logger.LogWarning("User not found with ID: {UserId}", userId);
						return NotFound(new { message = "Không tìm thấy thông tin người dùng" });
					}

					if (user.Department == null || string.IsNullOrWhiteSpace(user.Department.Name))
					{
						_logger.LogWarning("User {UserId} has no Department assigned", userId);
						return Ok(new List<KPI>()); // Trả về danh sách rỗng nếu không có Department
					}

					_logger.LogInformation("User role detected. UserId: {UserId}, Department: {Department}", userId, user.Department.Name);
					var kpis = await _context.KPIs
						.Where(k => k.Department.ToLower() == user.Department.Name.ToLower())
						.OrderByDescending(k => k.CreatedAt)
						.ToListAsync();

					_logger.LogInformation("Returning {Count} KPIs for department: {Department}", kpis.Count, user.Department.Name);
					return Ok(kpis);

					//_logger.LogInformation("User role detected. Returning all KPIs");
					//var kpis = await _context.KPIs
					//	.OrderByDescending(k => k.CreatedAt)
					//	.ToListAsync();
					//return Ok(kpis);
				}
				else
				{
					_logger.LogWarning($"Invalid or missing role: {role}");
					return Forbid();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách KPI");
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách KPI", error = ex.Message });
			}
		}


		// GET: api/KPIs/5
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<KPI>> GetKPI(int id)
		{
			try
			{
				var kpi = await _context.KPIs.FindAsync(id);

				if (kpi == null)
				{
					return NotFound(new { message = "Không tìm thấy KPI" });
				}

				return Ok(kpi);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy thông tin KPI với ID: {KpiId}", id);
				return StatusCode(500, new { message = "Lỗi server khi lấy thông tin KPI", error = ex.Message });
			}
		}


		// GET: api/KPIs/department/{department}
		[HttpGet("department/{department}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<KPI>>> GetKPIsByDepartment(string department)
		{
			try
			{
				var kpis = await _context.KPIs
					.Where(k => k.Department.ToLower() == department.ToLower())
					.OrderByDescending(k => k.CreatedAt)
					.ToListAsync();

				return Ok(kpis);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách KPI theo phòng ban: {Department}", department);
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách KPI", error = ex.Message });
			}
		}


		// POST: api/KPIs
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<KPI>> CreateKPI(KPI kpi)
		{
			try
			{
				// Kiểm tra model validation
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Kiểm tra trùng tên KPI (nếu cần)
				var existingKpi = await _context.KPIs
					.FirstOrDefaultAsync(k => k.Name.ToLower() == kpi.Name.ToLower()
						&& k.Department.ToLower() == kpi.Department.ToLower());

				if (existingKpi != null)
				{
					return BadRequest(new { message = "KPI với tên này đã tồn tại trong phòng ban" });
				}

				// Gán thời gian tạo UTC
				kpi.CreatedAt = DateTime.UtcNow;

				_context.KPIs.Add(kpi);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã tạo KPI mới với ID: {KpiId}", kpi.Id);

				return CreatedAtAction(nameof(GetKPI), new { id = kpi.Id }, kpi);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo KPI mới");
				return StatusCode(500, new { message = "Lỗi server khi tạo KPI", error = ex.Message });
			}
		}


		// PUT: api/KPIs/5
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateKPI(int id, KPI kpi)
		{
			if (id != kpi.Id)
			{
				return BadRequest(new { message = "ID không khớp" });
			}

			try
			{
				// Kiểm tra KPI có tồn tại không
				var existingKpi = await _context.KPIs.FindAsync(id);
				if (existingKpi == null)
				{
					return NotFound(new { message = "Không tìm thấy KPI" });
				}

				// Kiểm tra trùng tên (ngoại trừ chính nó)
				var duplicateKpi = await _context.KPIs
					.FirstOrDefaultAsync(k => k.Id != id
						&& k.Name.ToLower() == kpi.Name.ToLower()
						&& k.Department.ToLower() == kpi.Department.ToLower());

				if (duplicateKpi != null)
				{
					return BadRequest(new { message = "KPI với tên này đã tồn tại trong phòng ban" });
				}

				// Cập nhật các trường
				existingKpi.Name = kpi.Name;
				existingKpi.Description = kpi.Description;
				existingKpi.Department = kpi.Department;
				existingKpi.TargetValue = kpi.TargetValue;
				existingKpi.TargetPercentage = kpi.TargetPercentage;
				existingKpi.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã cập nhật KPI với ID: {KpiId}", id);

				return Ok(new { message = "Cập nhật KPI thành công", kpi = existingKpi });
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!KPIExists(id))
				{
					return NotFound(new { message = "Không tìm thấy KPI" });
				}
				else
				{
					throw;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật KPI với ID: {KpiId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật KPI", error = ex.Message });
			}
		}


		// DELETE: api/KPIs/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteKPI(int id)
		{
			try
			{
				var kpi = await _context.KPIs.FindAsync(id);
				if (kpi == null)
				{
					return NotFound(new { message = "Không tìm thấy KPI" });
				}

				_context.KPIs.Remove(kpi);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã xóa KPI với ID: {KpiId}", id);

				return Ok(new { message = "Xóa KPI thành công", deletedId = id });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa KPI với ID: {KpiId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa KPI", error = ex.Message });
			}
		}


		private bool KPIExists(int id)
		{
			return _context.KPIs.Any(e => e.Id == id);
		}
	}
}
