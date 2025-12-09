using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using erp_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace erp_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin")] // Chỉ Admin hoặc Manager mới được quản lý KPI
	public class KpiPackagesController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IKpiCalculationService _kpiCalculationService;
		private readonly ILogger<KpiPackagesController> _logger;

		public KpiPackagesController(
			ApplicationDbContext context, 
			IKpiCalculationService kpiCalculationService,
			ILogger<KpiPackagesController> logger)
		{
			_context = context;
			_kpiCalculationService = kpiCalculationService;
			_logger = logger;
		}

		// GET: api/KpiPackages
		// Lấy danh sách gói KPI với filter theo tháng/năm
		[HttpGet]
		public async Task<ActionResult<IEnumerable<KpiPackageResponseDto>>> GetKpiPackages(
			[FromQuery] int? month,
			[FromQuery] int? year,
			[FromQuery] bool? isActive)
		{
			try
			{
				var query = _context.KpiPackages.AsQueryable();

				// Apply filters
				if (month.HasValue)
					query = query.Where(x => x.Month == month.Value);

				if (year.HasValue)
					query = query.Where(x => x.Year == year.Value);

				if (isActive.HasValue)
					query = query.Where(x => x.IsActive == isActive.Value);

				// Get data with user info
				var packages = await query
					.Include(x => x.CreatedByUser)
					.OrderByDescending(x => x.Year)
					.ThenByDescending(x => x.Month)
					.ThenByDescending(x => x.CreatedAt)
					.Select(x => new KpiPackageResponseDto
					{
						Id = x.Id,
						Name = x.Name,
						Month = x.Month,
						Year = x.Year,
						TargetAmount = x.TargetAmount,
						Description = x.Description,
						IsActive = x.IsActive,
						CreatedByUserId = x.CreatedByUserId,
						CreatedByUserName = x.CreatedByUser != null ? x.CreatedByUser.Name : null,
						CreatedAt = x.CreatedAt,
						AssignedUsersCount = _context.SaleKpiTargets.Count(t => t.KpiPackageId == x.Id)
					})
					.ToListAsync();

				return Ok(new { success = true, data = packages });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách gói KPI", error = ex.Message });
			}
		}

		// GET: api/KpiPackages/5
		// Lấy chi tiết 1 gói KPI
		[HttpGet("{id}")]
		public async Task<ActionResult<KpiPackageResponseDto>> GetKpiPackage(int id)
		{
			try
			{
				var package = await _context.KpiPackages
					.Where(x => x.Id == id)
					.Select(x => new KpiPackageResponseDto
					{
						Id = x.Id,
						Name = x.Name,
						Month = x.Month,
						Year = x.Year,
						TargetAmount = x.TargetAmount,
						Description = x.Description,
						IsActive = x.IsActive,
						CreatedByUserId = x.CreatedByUserId,
						CreatedByUserName = x.CreatedByUser != null ? x.CreatedByUser.Name : null,
						CreatedAt = x.CreatedAt,
						AssignedUsersCount = _context.SaleKpiTargets.Count(t => t.KpiPackageId == x.Id)
					})
					.FirstOrDefaultAsync();

				if (package == null)
					return NotFound(new { success = false, message = "Không tìm thấy gói KPI" });

				return Ok(new { success = true, data = package });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin gói KPI", error = ex.Message });
			}
		}

		// POST: api/KpiPackages
		// Tạo gói KPI mới
		[HttpPost]
		public async Task<ActionResult<KpiPackage>> CreateKpiPackage([FromBody] CreateKpiPackageDto dto)
		{
			try
			{
				// Lấy ID người tạo từ token
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (string.IsNullOrEmpty(userIdClaim))
					return Unauthorized(new { success = false, message = "Không xác định được người dùng" });

				int userId = int.Parse(userIdClaim);

				// Kiểm tra user có tồn tại không
				var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
				if (!userExists)
					return BadRequest(new { success = false, message = "User không tồn tại" });

				// Kiểm tra trùng tên gói trong cùng tháng/năm
				var existingPackage = await _context.KpiPackages
					.AnyAsync(x => x.Name == dto.Name && x.Month == dto.Month && x.Year == dto.Year && x.IsActive);

				if (existingPackage)
					return BadRequest(new { success = false, message = "Đã tồn tại gói KPI với tên này trong tháng/năm được chọn" });

				var package = new KpiPackage
				{
					Name = dto.Name,
					Month = dto.Month,
					Year = dto.Year,
					TargetAmount = dto.TargetAmount,
					Description = dto.Description,
					IsActive = true,
					CreatedByUserId = userId,
					CreatedAt = DateTime.UtcNow
				};

				_context.KpiPackages.Add(package);
				await _context.SaveChangesAsync();

				return CreatedAtAction(
					nameof(GetKpiPackage),
					new { id = package.Id },
					new { success = true, message = "Tạo gói KPI thành công", data = package }
				);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Lỗi khi tạo gói KPI", error = ex.Message });
			}
		}

		// PUT: api/KpiPackages/5
		// Cập nhật gói KPI
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateKpiPackage(int id, [FromBody] UpdateKpiPackageDto dto)
		{
			try
			{
				var package = await _context.KpiPackages.FindAsync(id);
				if (package == null)
					return NotFound(new { success = false, message = "Không tìm thấy gói KPI" });

				// Kiểm tra trùng tên (ngoại trừ chính nó)
				var duplicateName = await _context.KpiPackages
					.AnyAsync(x => x.Id != id 
						&& x.Name == dto.Name 
						&& x.Month == dto.Month 
						&& x.Year == dto.Year 
						&& x.IsActive);

				if (duplicateName)
					return BadRequest(new { success = false, message = "Đã tồn tại gói KPI với tên này trong tháng/năm được chọn" });

				// Cập nhật thông tin
				package.Name = dto.Name;
				package.Month = dto.Month;
				package.Year = dto.Year;
				package.TargetAmount = dto.TargetAmount;
				package.Description = dto.Description;
				package.IsActive = dto.IsActive;

				// Lưu ý: Việc cập nhật gói ở đây KHÔNG tự động cập nhật TargetAmount của nhân viên đã gán
				// Điều này đảm bảo tính năng Snapshot (giữ lại giá trị lịch sử)
				// Nếu muốn cập nhật cho nhân viên, Admin phải gán lại

				await _context.SaveChangesAsync();

				return Ok(new { success = true, message = "Cập nhật gói KPI thành công", data = package });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật gói KPI", error = ex.Message });
			}
		}

		// DELETE: api/KpiPackages/5
		// Xóa gói KPI (Soft delete nếu đã được gán)
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteKpiPackage(int id)
		{
			try
			{
				var package = await _context.KpiPackages.FindAsync(id);
				if (package == null)
					return NotFound(new { success = false, message = "Không tìm thấy gói KPI" });

				// Kiểm tra xem gói này đã được gán cho ai chưa
				bool isAssigned = await _context.SaleKpiTargets.AnyAsync(x => x.KpiPackageId == id);

				if (isAssigned)
				{
					// Soft delete - chỉ đánh dấu IsActive = false
					package.IsActive = false;
					await _context.SaveChangesAsync();
					return Ok(new { success = true, message = "Gói đã được sử dụng nên chỉ chuyển sang trạng thái ngưng hoạt động" });
				}
				else
				{
					// Hard delete - xóa vĩnh viễn
					_context.KpiPackages.Remove(package);
					await _context.SaveChangesAsync();
					return Ok(new { success = true, message = "Đã xóa gói KPI vĩnh viễn" });
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Lỗi khi xóa gói KPI", error = ex.Message });
			}
		}

		// POST: api/KpiPackages/assign
		// Gán gói KPI cho danh sách nhân viên (QUAN TRỌNG)
		[HttpPost("assign")]
		public async Task<IActionResult> AssignKpiToUsers([FromBody] AssignKpiPackageDto request)
		{
			try
			{
				// Validate gói KPI
				var package = await _context.KpiPackages.FindAsync(request.KpiPackageId);
				if (package == null)
					return BadRequest(new { success = false, message = "Gói KPI không tồn tại" });

				if (!package.IsActive)
					return BadRequest(new { success = false, message = "Gói KPI này đang bị khóa" });

				// Lấy thông tin admin đang thực hiện gán
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (string.IsNullOrEmpty(userIdClaim))
					return Unauthorized(new { success = false, message = "Không xác định được người dùng" });

				int adminId = int.Parse(userIdClaim);

				// Validate danh sách users
				if (request.UserIds == null || !request.UserIds.Any())
					return BadRequest(new { success = false, message = "Danh sách user trống" });

				var results = new List<AssignKpiResultDto>();

				// Lấy thông tin users để hiển thị tên
				var users = await _context.Users
					.Where(u => request.UserIds.Contains(u.Id))
					.Select(u => new { u.Id, u.Name })
					.ToListAsync();

				foreach (var userId in request.UserIds)
				{
					var user = users.FirstOrDefault(u => u.Id == userId);
					var result = new AssignKpiResultDto
					{
						UserId = userId,
						UserName = user?.Name ?? "Unknown"
					};

					try
					{
						// Kiểm tra user có tồn tại không
						if (user == null)
						{
							result.Status = "Failed";
							result.Message = "User không tồn tại";
							results.Add(result);
							continue;
						}

						// Tìm xem user đã có KPI tháng đó chưa
						var existingTarget = await _context.SaleKpiTargets
							.FirstOrDefaultAsync(x => x.UserId == userId
											   && x.Month == package.Month
											   && x.Year == package.Year);

						if (existingTarget != null)
						{
							// Đã có -> Update lại theo gói mới
							existingTarget.KpiPackageId = package.Id;
							existingTarget.TargetAmount = package.TargetAmount; // Snapshot giá trị mới
							existingTarget.AssignedByUserId = adminId;
							existingTarget.AssignedAt = DateTime.UtcNow;
							existingTarget.UpdatedAt = DateTime.UtcNow;
							existingTarget.Notes = request.Notes ?? $"Cập nhật sang gói: {package.Name}";

							result.Status = "Updated";
							result.Message = $"Đã cập nhật KPI sang gói '{package.Name}'";
						}
						else
						{
							// Chưa có -> Tạo mới
							var newTarget = new SaleKpiTarget
							{
								UserId = userId,
								KpiPackageId = package.Id,
								Month = package.Month,
								Year = package.Year,
								TargetAmount = package.TargetAmount, // Snapshot giá trị
								AssignedByUserId = adminId,
								AssignedAt = DateTime.UtcNow,
								IsActive = true,
								CreatedAt = DateTime.UtcNow,
								Notes = request.Notes ?? $"Gán lần đầu gói: {package.Name}"
							};

							_context.SaleKpiTargets.Add(newTarget);

							result.Status = "Created";
							result.Message = $"Đã gán gói KPI '{package.Name}' thành công";
						}

						results.Add(result);
					}
					catch (Exception ex)
					{
						result.Status = "Failed";
						result.Message = $"Lỗi: {ex.Message}";
						results.Add(result);
					}
				}

				await _context.SaveChangesAsync();

				var summary = new
				{
					total = results.Count,
					created = results.Count(r => r.Status == "Created"),
					updated = results.Count(r => r.Status == "Updated"),
					failed = results.Count(r => r.Status == "Failed")
				};

				return Ok(new
				{
					success = true,
					message = "Hoàn tất quy trình gán KPI",
					summary = summary,
					details = results
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Lỗi khi gán KPI", error = ex.Message });
			}
		}

		// GET: api/KpiPackages/5/assigned-users
		// Lấy danh sách users đã được gán gói KPI này
		[HttpGet("{id}/assigned-users")]
		public async Task<IActionResult> GetAssignedUsers(int id)
		{
			try
			{
				var package = await _context.KpiPackages.FindAsync(id);
				if (package == null)
					return NotFound(new { success = false, message = "Không tìm thấy gói KPI" });

				var assignedUsers = await _context.SaleKpiTargets
					.Where(t => t.KpiPackageId == id)
					.Include(t => t.SaleUser)
					.Select(t => new
					{
						t.Id,
						t.UserId,
						UserName = t.SaleUser != null ? t.SaleUser.Name : null,
						UserEmail = t.SaleUser != null ? t.SaleUser.Email : null,
						t.TargetAmount,
						t.AssignedAt,
						t.IsActive,
						t.Notes
					})
					.ToListAsync();

				return Ok(new { success = true, data = assignedUsers });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách users", error = ex.Message });
			}
		}

		// ✅ POST: api/KpiPackages/calculate-kpi
		// Tính toán KPI Record cho tháng/năm cụ thể (trigger thủ công)
		[HttpPost("calculate-kpi")]
		public async Task<IActionResult> CalculateKpiRecords([FromQuery] int? month, [FromQuery] int? year)
		{
			try
			{
				var targetMonth = month ?? DateTime.UtcNow.Month;
				var targetYear = year ?? DateTime.UtcNow.Year;

				_logger.LogInformation("Admin trigger tính toán KPI cho tháng {Month}/{Year}", targetMonth, targetYear);

				// Gọi service để tính toán cho tất cả users
				await _kpiCalculationService.CalculateKpiForAllUsersAsync(targetMonth, targetYear);

				// Lấy kết quả để hiển thị
				var results = await _context.SaleKpiRecords
					.Include(r => r.SaleUser)
					.Where(r => r.Month == targetMonth && r.Year == targetYear)
					.OrderByDescending(r => r.TotalPaidAmount)
					.Select(r => new
					{
						userId = r.UserId,
						userName = r.SaleUser != null ? r.SaleUser.Name : "Unknown",
						targetAmount = r.TargetAmount,
						totalPaidAmount = r.TotalPaidAmount,
						achievementPercentage = r.AchievementPercentage,
						isKpiAchieved = r.IsKpiAchieved,
						commissionAmount = r.CommissionAmount,
						commissionPercentage = r.CommissionPercentage,
						tierLevel = r.CommissionTierLevel,
						totalContracts = r.TotalContracts
					})
					.ToListAsync();

				return Ok(new
				{
					success = true,
					message = $"Đã tính toán KPI Record cho {results.Count} users",
					month = targetMonth,
					year = targetYear,
					data = results
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tính toán KPI");
				return StatusCode(500, new { success = false, message = "Lỗi khi tính toán KPI", error = ex.Message });
			}
		}

		// ✅ POST: api/KpiPackages/calculate-kpi-user/{userId}
		// Tính toán KPI cho một user cụ thể
		[HttpPost("calculate-kpi-user/{userId}")]
		public async Task<IActionResult> CalculateKpiForUser(int userId, [FromQuery] int? month, [FromQuery] int? year)
		{
			try
			{
				var targetMonth = month ?? DateTime.UtcNow.Month;
				var targetYear = year ?? DateTime.UtcNow.Year;

				// Kiểm tra user tồn tại
				var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
				if (!userExists)
					return NotFound(new { success = false, message = "User không tồn tại" });

				_logger.LogInformation("Admin trigger tính toán KPI cho User {UserId}, Tháng {Month}/{Year}", 
					userId, targetMonth, targetYear);

				// Tính toán KPI
				await _kpiCalculationService.CalculateKpiForUserAsync(userId, targetMonth, targetYear);

				// Lấy kết quả
				var result = await _context.SaleKpiRecords
					.Include(r => r.SaleUser)
					.Include(r => r.KpiTarget)
					.Where(r => r.UserId == userId && r.Month == targetMonth && r.Year == targetYear)
					.Select(r => new
					{
						id = r.Id,
						userId = r.UserId,
						userName = r.SaleUser != null ? r.SaleUser.Name : "Unknown",
						month = r.Month,
						year = r.Year,
						targetAmount = r.TargetAmount,
						totalPaidAmount = r.TotalPaidAmount,
						achievementPercentage = r.AchievementPercentage,
						isKpiAchieved = r.IsKpiAchieved,
						commissionAmount = r.CommissionAmount,
						commissionPercentage = r.CommissionPercentage,
						tierLevel = r.CommissionTierLevel,
						totalContracts = r.TotalContracts,
						updatedAt = r.UpdatedAt
					})
					.FirstOrDefaultAsync();

				if (result == null)
				{
					return NotFound(new 
					{ 
						success = false, 
						message = $"User {userId} chưa được giao KPI cho tháng {targetMonth}/{targetYear}" 
					});
				}

				return Ok(new
				{
					success = true,
					message = "Đã tính toán KPI thành công",
					data = result
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tính toán KPI cho User {UserId}", userId);
				return StatusCode(500, new { success = false, message = "Lỗi khi tính toán KPI", error = ex.Message });
			}
		}
	}
}
