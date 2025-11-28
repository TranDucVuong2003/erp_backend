using erp_backend.Data;
using erp_backend.Models;
using erp_backend.BackgroundJobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class KpiRecordsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly KpiCalculationJob _kpiJob;
		private readonly ILogger<KpiRecordsController> _logger;

		public KpiRecordsController(
			ApplicationDbContext context,
			KpiCalculationJob kpiJob,
			ILogger<KpiRecordsController> logger)
		{
			_context = context;
			_kpiJob = kpiJob;
			_logger = logger;
		}

		// GET: api/KpiRecords/calculate-and-get
		[HttpGet("calculate-and-get")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> CalculateAndGetKpiProgress([FromQuery] string period)
		{
			try
			{
				if (string.IsNullOrEmpty(period))
					return BadRequest(new { message = "Period không được để trống" });

				_logger.LogInformation("Admin yêu cầu tính toán và xem tiến độ KPI cho kỳ: {Period}", period);

				// Bước 1: Thực hiện tính toán KPI cho tất cả users
				await _kpiJob.CalculateMonthlyKpiAsync(period);

				// Bước 2: Lấy danh sách tiến độ KPI của Sales users
				var salesKpiData = await GetSaleKpiData(period);

				// Bước 3: Lấy thống kê tổng quan
				var summary = await GetKpiSummaryData(period);

				var response = new
				{
					message = $"Đã tính toán xong KPI cho kỳ {period}",
					period = period,
					calculatedAt = DateTime.UtcNow,
					summary = summary,
					salesProgress = salesKpiData
				};

				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tính toán và lấy tiến độ KPI");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		/// <summary>
		/// Lấy danh sách tiến độ KPI của tất cả users được giao KPI
		/// </summary>
		private async Task<List<object>> GetKpiProgressData(string period)
		{
			// Parse period để tạo date range
			var periodParts = period.Split('-');
			if (periodParts.Length != 2 || !int.TryParse(periodParts[0], out int year) || !int.TryParse(periodParts[1], out int month))
			{
				return new List<object>();
			}

			// ✅ Sửa: Chỉ định DateTimeKind.Utc cho PostgreSQL
			var periodStartDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
			var periodEndDate = periodStartDate.AddMonths(1).AddDays(-1);

			// Lấy tất cả user được giao KPI trong kỳ này
			var userAssignments = await _context.UserKpiAssignments
				.Include(a => a.Kpi).ThenInclude(k => k.Department)
				.Include(a => a.User)
				.Where(a => a.IsActive && 
				           a.Kpi.IsActive &&
				           (a.Kpi.StartDate >= periodStartDate && a.Kpi.StartDate <= periodEndDate ||
				            (a.Kpi.Period != null && a.Kpi.Period.Contains(period))))
				.AsNoTracking()
				.ToListAsync();

			if (!userAssignments.Any())
			{
				return new List<object>();
			}

			// Lấy KPI records đã tính cho kỳ này
			var kpiRecords = await _context.KpiRecords
				.Include(r => r.Kpi).ThenInclude(k => k.Department)
				.Include(r => r.User)
				.Include(r => r.Approver)
				.Where(r => r.Period == period)
				.AsNoTracking()
				.ToListAsync();

			// Group theo User để tạo danh sách tiến độ
			var userProgress = userAssignments
				.GroupBy(a => new { a.UserId, a.User.Name, a.User.Email })
				.Select(userGroup =>
				{
					var userId = userGroup.Key.UserId;
					var userName = userGroup.Key.Name;
					var userEmail = userGroup.Key.Email;

					// Lấy tất cả KPI assignments của user này
					var userKpiAssignments = userGroup.ToList();

					// Lấy KPI records của user này
					var userKpiRecords = kpiRecords.Where(r => r.UserId == userId).ToList();

					// Tính toán tiến độ cho từng KPI
					var kpiDetails = userKpiAssignments.Select(assignment =>
					{
						var kpiRecord = userKpiRecords.FirstOrDefault(r => r.KpiId == assignment.KpiId);
						
						// Xác định trạng thái KPI
						string kpiStatus;
						decimal achievementPercentage = 0;
						
						if (kpiRecord != null)
						{
							achievementPercentage = kpiRecord.AchievementPercentage;
							
							if (achievementPercentage >= 100)
								kpiStatus = "Đã đạt KPI";
							else if (achievementPercentage >= 50) // Nếu > 50% coi là đang hoàn thành
								kpiStatus = "Đang hoàn thành";
							else
								kpiStatus = "Không đạt KPI";
						}
						else
						{
							// Chưa có record = chưa tính toán
							kpiStatus = "Chưa tính toán";
						}

						return new
						{
							KpiId = assignment.KpiId,
							KpiName = assignment.Kpi?.Name,
							KpiType = assignment.Kpi?.KpiType,
							Department = assignment.Kpi?.Department?.Name,
							
							// Target information
							OriginalTargetValue = assignment.Kpi?.TargetValue ?? 0,
							CustomTargetValue = assignment.CustomTargetValue,
							ActualTargetValue = assignment.CustomTargetValue ?? assignment.Kpi?.TargetValue ?? 0,
							Weight = assignment.Weight,
							
							// Progress information
							ActualValue = kpiRecord?.ActualValue ?? 0,
							AchievementPercentage = Math.Round(achievementPercentage, 2),
							KpiStatus = kpiStatus,
							
							// Commission information
							CommissionAmount = kpiRecord?.CommissionAmount ?? 0,
							CommissionPercentage = kpiRecord?.CommissionPercentage,
							CommissionTierLevel = kpiRecord?.CommissionTierLevel,
							
							// Record status
							RecordStatus = kpiRecord?.Status ?? "Chưa tạo",
							
							// Detailed info by KPI type
							SalesInfo = assignment.Kpi?.KpiType == "Revenue" && kpiRecord != null ? new
							{
								Revenue = kpiRecord.ActualValue,
								Target = kpiRecord.TargetValue
							} : null,
							
							MarketingInfo = assignment.Kpi?.KpiType == "Leads" && kpiRecord != null ? new
							{
								ROI = kpiRecord.ROI,
								TotalLeads = kpiRecord.TotalLeads,
								ConvertedLeads = kpiRecord.ConvertedLeads,
								LeadConversionRate = kpiRecord.LeadConversionRate,
								ApprovedBudget = kpiRecord.ApprovedBudget,
								ActualSpending = kpiRecord.ActualSpending,
								CostPerLead = kpiRecord.CostPerLead,
								CostPerConversion = kpiRecord.CostPerConversion
							} : null,
							
							ITInfo = assignment.Kpi?.KpiType == "Tickets" && kpiRecord != null ? new
							{
								CompletionRate = kpiRecord.ActualValue,
								TotalTickets = kpiRecord.TotalTickets,
								CompletedTickets = kpiRecord.CompletedTickets,
								AverageResolutionTime = kpiRecord.AverageResolutionTime
							} : null,
							
							Notes = kpiRecord?.Notes,
							AssignedDate = assignment.AssignedDate,
							RecordDate = kpiRecord?.RecordDate,
							ApprovedAt = kpiRecord?.ApprovedAt,
							ApprovedBy = kpiRecord?.ApprovedBy
						};
					}).OrderBy(detail => detail.Department).ThenBy(detail => detail.KpiName).ToList();

					// Tính tổng hợp cho user
					var totalKpis = kpiDetails.Count;
					var calculatedKpis = kpiDetails.Count(k => k.RecordStatus != "Chưa tạo");
					var completedKpis = kpiDetails.Count(k => k.KpiStatus == "Đã đạt KPI");
					var inProgressKpis = kpiDetails.Count(k => k.KpiStatus == "Đang hoàn thành");
					var notAchievedKpis = kpiDetails.Count(k => k.KpiStatus == "Không đạt KPI");
					var notCalculatedKpis = kpiDetails.Count(k => k.KpiStatus == "Chưa tính toán");

					var completionRate = totalKpis > 0 
						? Math.Round((double)completedKpis / totalKpis * 100, 2) 
						: 0;

					var averageAchievement = calculatedKpis > 0 
						? Math.Round(kpiDetails.Where(k => k.RecordStatus != "Chưa tạo").Average(k => k.AchievementPercentage), 2)
						: 0;

					var totalCommission = kpiDetails.Sum(k => k.CommissionAmount);
					var pendingCommission = kpiDetails.Where(k => k.RecordStatus == "Pending").Sum(k => k.CommissionAmount);
					var approvedCommission = kpiDetails.Where(k => k.RecordStatus == "Approved").Sum(k => k.CommissionAmount);

					// Xác định trạng thái tổng quan của user
					string overallStatus;
					if (notCalculatedKpis == totalKpis)
						overallStatus = "Chưa tính toán";
					else if (completedKpis == totalKpis)
						overallStatus = "Hoàn thành xuất sắc";
					else if ((completedKpis + inProgressKpis) >= totalKpis * 0.7) // >= 70% là tốt
						overallStatus = "Đang hoàn thành tốt";
					else if (completedKpis > 0)
						overallStatus = "Đang hoàn thành";
					else
						overallStatus = "Cần cải thiện";

					return new
					{
						UserId = userId,
						UserName = userName,
						UserEmail = userEmail,
						OverallStatus = overallStatus,
						
						// KPI Statistics
						TotalKpis = totalKpis,
						CalculatedKpis = calculatedKpis,
						CompletedKpis = completedKpis,
						InProgressKpis = inProgressKpis,
						NotAchievedKpis = notAchievedKpis,
						NotCalculatedKpis = notCalculatedKpis,
						CompletionRate = completionRate,
						AverageAchievement = averageAchievement,
						
						// Commission Information
						TotalCommission = totalCommission,
						PendingCommission = pendingCommission,
						ApprovedCommission = approvedCommission,
						
						// Status Breakdown
						StatusCounts = new
						{
							Completed = completedKpis,
							InProgress = inProgressKpis,
							NotAchieved = notAchievedKpis,
							NotCalculated = notCalculatedKpis
						},
						
						RecordStatusCounts = new
						{
							Pending = kpiDetails.Count(k => k.RecordStatus == "Pending"),
							Approved = kpiDetails.Count(k => k.RecordStatus == "Approved"),
							Rejected = kpiDetails.Count(k => k.RecordStatus == "Rejected"),
							NotCreated = kpiDetails.Count(k => k.RecordStatus == "Chưa tạo")
						},
						
						// Detailed KPI Information
						KpiDetails = kpiDetails
					};
				})
				.OrderByDescending(u => u.AverageAchievement)
				.ThenByDescending(u => u.CompletionRate)
				.ToList<object>();

			return userProgress;
		}

		/// <summary>
		/// Lấy thống kê tổng quan KPI
		/// </summary>
		private async Task<object> GetKpiSummaryData(string period)
		{
			var query = _context.KpiRecords
				.Where(r => r.Period == period)
				.AsNoTracking();

			var records = await query.ToListAsync();

			if (!records.Any())
			{
				return new
				{
					TotalRecords = 0,
					TotalUsers = 0,
					Message = "Không có dữ liệu KPI cho kỳ này"
				};
			}

			return new
			{
				TotalRecords = records.Count,
				TotalUsers = records.Select(r => r.UserId).Distinct().Count(),
				
				StatusBreakdown = new
				{
					Pending = records.Count(r => r.Status == "Pending"),
					Approved = records.Count(r => r.Status == "Approved"),
					Rejected = records.Count(r => r.Status == "Rejected")
				},
				
				PerformanceStats = new
				{
					CompletedKpis = records.Count(r => r.AchievementPercentage >= 100),
					CompletionRate = Math.Round(records.Count(r => r.AchievementPercentage >= 100) * 100.0 / records.Count, 2),
					AverageAchievement = Math.Round(records.Average(r => r.AchievementPercentage), 2),
					MaxAchievement = Math.Round(records.Max(r => r.AchievementPercentage), 2),
					MinAchievement = Math.Round(records.Min(r => r.AchievementPercentage), 2)
				},
				
				CommissionStats = new
				{
					TotalCommission = records.Sum(r => r.CommissionAmount ?? 0),
					PendingCommission = records.Where(r => r.Status == "Pending").Sum(r => r.CommissionAmount ?? 0),
					ApprovedCommission = records.Where(r => r.Status == "Approved").Sum(r => r.CommissionAmount ?? 0),
					UsersWithCommission = records.Count(r => (r.CommissionAmount ?? 0) > 0)
				},
				
				DepartmentBreakdown = records
					.Where(r => r.Kpi?.Department != null)
					.GroupBy(r => r.Kpi.Department.Name)
					.Select(g => new
					{
						Department = g.Key,
						RecordCount = g.Count(),
						AverageAchievement = Math.Round(g.Average(r => r.AchievementPercentage), 2),
						TotalCommission = g.Sum(r => r.CommissionAmount ?? 0),
						CompletionRate = Math.Round(g.Count(r => r.AchievementPercentage >= 100) * 100.0 / g.Count(), 2)
					})
					.OrderByDescending(d => d.AverageAchievement)
					.ToList()
			};
		}

		// GET: api/KpiRecords
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<KpiRecord>>> GetKpiRecords(
			[FromQuery] string? period = null,
			[FromQuery] int? userId = null,
			[FromQuery] int? kpiId = null,
			[FromQuery] string? status = null)
		{
			try
			{
				var query = _context.KpiRecords
					.Include(r => r.Kpi).ThenInclude(k => k.Department)
					.Include(r => r.User)
					.Include(r => r.Approver)
					.AsNoTracking()
					.AsQueryable();

				var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
				int currentUserId = int.Parse(User.FindFirst("userid")?.Value ?? "0");

				if (role?.ToLower() == "user")
					query = query.Where(r => r.UserId == currentUserId);

				if (!string.IsNullOrEmpty(period))
					query = query.Where(r => r.Period == period);

				if (userId.HasValue)
					query = query.Where(r => r.UserId == userId.Value);

				if (kpiId.HasValue)
					query = query.Where(r => r.KpiId == kpiId.Value);

				if (!string.IsNullOrEmpty(status))
					query = query.Where(r => r.Status == status);

				var records = await query
					.OrderByDescending(r => r.RecordDate)
					.ToListAsync();

				return Ok(records);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách KpiRecord");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// GET: api/KpiRecords/5
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<KpiRecord>> GetKpiRecord(int id)
		{
			try
			{
				var record = await _context.KpiRecords
					.Include(r => r.Kpi).ThenInclude(k => k.CommissionTiers)
					.Include(r => r.User)
					.Include(r => r.Approver)
					.AsNoTracking()
					.FirstOrDefaultAsync(r => r.Id == id);

				if (record == null)
					return NotFound(new { message = "Không tìm thấy KpiRecord" });

				var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
				int currentUserId = int.Parse(User.FindFirst("userid")?.Value ?? "0");

				if (role?.ToLower() == "user" && record.UserId != currentUserId)
					return Forbid();

				return Ok(record);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy thông tin KpiRecord ID: {Id}", id);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// POST: api/KpiRecords/calculate
		[HttpPost("calculate")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ManualCalculate([FromQuery] string period)
		{
			try
			{
				if (string.IsNullOrEmpty(period))
					return BadRequest(new { message = "Period không được để trống" });

				_logger.LogInformation("Admin kích hoạt tính KPI thủ công cho kỳ: {Period}", period);

				await _kpiJob.CalculateMonthlyKpiAsync(period);

				return Ok(new { message = $"Đã tính KPI cho kỳ {period} thành công" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tính KPI thủ công");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// GET: api/KpiRecords/summary
		[HttpGet("summary")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> GetSummary([FromQuery] string? period = null)
		{
			try
			{
				var query = _context.KpiRecords.AsNoTracking().AsQueryable();

				if (!string.IsNullOrEmpty(period))
					query = query.Where(r => r.Period == period);

				var summary = await query
					.GroupBy(r => 1)
					.Select(g => new
					{
						TotalRecords = g.Count(),
						TotalUsers = g.Select(r => r.UserId).Distinct().Count(),

						PendingCount = g.Count(r => r.Status == "Pending"),
						ApprovedCount = g.Count(r => r.Status == "Approved"),
						RejectedCount = g.Count(r => r.Status == "Rejected"),

						CompletedCount = g.Count(r => r.AchievementPercentage >= 100),
						CompletionRate = g.Count() > 0
							? g.Count(r => r.AchievementPercentage >= 100) * 100.0 / g.Count()
							: 0,

						TotalCommission = g.Sum(r => r.CommissionAmount ?? 0),
						UsersWithCommission = g.Count(r => (r.CommissionAmount ?? 0) > 0),

						AverageAchievement = g.Average(r => r.AchievementPercentage),
						MaxAchievement = g.Max(r => r.AchievementPercentage),
						MinAchievement = g.Min(r => r.AchievementPercentage)
					})
					.FirstOrDefaultAsync();

				return Ok(summary);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy tổng quan KPI");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// POST: api/KpiRecords/5/approve
		[HttpPost("{id}/approve")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ApproveKpiRecord(int id)
		{
			try
			{
				var record = await _context.KpiRecords.FindAsync(id);
				if (record == null)
					return NotFound(new { message = "Không tìm thấy KpiRecord" });

				if (record.Status != "Pending")
					return BadRequest(new { message = "Chỉ có thể phê duyệt record đang chờ" });

				int approverId = int.Parse(User.FindFirst("userid")?.Value ?? "0");

				record.Status = "Approved";
				record.ApprovedBy = approverId;
				record.ApprovedAt = DateTime.UtcNow;
				record.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				return Ok(new { message = "Phê duyệt thành công" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi phê duyệt KpiRecord ID: {Id}", id);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// POST: api/KpiRecords/5/reject
		[HttpPost("{id}/reject")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> RejectKpiRecord(int id, [FromBody] string? reason)
		{
			try
			{
				var record = await _context.KpiRecords.FindAsync(id);
				if (record == null)
					return NotFound(new { message = "Không tìm thấy KpiRecord" });

				if (record.Status != "Pending")
					return BadRequest(new { message = "Chỉ có thể từ chối record đang chờ" });

				record.Status = "Rejected";
				record.Notes = $"{(record.Notes ?? "")}\n[REJECTED] {reason}";
				record.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				return Ok(new { message = "Đã từ chối KpiRecord" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi từ chối KpiRecord ID: {Id}", id);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// POST: api/KpiRecords/batch-approve
		[HttpPost("batch-approve")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> BatchApprove([FromBody] List<int> recordIds)
		{
			try
			{
				int approverId = int.Parse(User.FindFirst("userid")?.Value ?? "0");

				var records = await _context.KpiRecords
					.Where(r => recordIds.Contains(r.Id) && r.Status == "Pending")
					.ToListAsync();

				foreach (var r in records)
				{
					r.Status = "Approved";
					r.ApprovedBy = approverId;
					r.ApprovedAt = DateTime.UtcNow;
					r.UpdatedAt = DateTime.UtcNow;
				}

				await _context.SaveChangesAsync();

				return Ok(new { message = $"Đã phê duyệt {records.Count} record(s)", approvedCount = records.Count });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi batch approve");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// GET: api/KpiRecords/user/{userId}/summary
		[HttpGet("user/{userId}/summary")]
		[Authorize]
		public async Task<ActionResult> GetUserSummary(int userId, [FromQuery] string? period = null)
		{
			try
			{
				var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
				int currentUserId = int.Parse(User.FindFirst("userid")?.Value ?? "0");

				if (role?.ToLower() == "user" && userId != currentUserId)
					return Forbid();

				var query = _context.KpiRecords
					.Where(r => r.UserId == userId)
					.AsNoTracking();

				if (!string.IsNullOrEmpty(period))
					query = query.Where(r => r.Period == period);

				var summary = await query
					.GroupBy(r => 1)
					.Select(g => new
					{
						TotalKpis = g.Count(),
						CompletedKpis = g.Count(r => r.AchievementPercentage >= 100),
						CompletionRate = g.Count() > 0
							? g.Count(r => r.AchievementPercentage >= 100) * 100.0 / g.Count()
							: 0,

						AverageAchievement = g.Average(r => r.AchievementPercentage),

						TotalCommission = g.Sum(r => r.CommissionAmount ?? 0),
						PendingCommission = g.Where(r => r.Status == "Pending").Sum(r => r.CommissionAmount ?? 0),
						ApprovedCommission = g.Where(r => r.Status == "Approved").Sum(r => r.CommissionAmount ?? 0),

						PendingCount = g.Count(r => r.Status == "Pending"),
						ApprovedCount = g.Count(r => r.Status == "Approved"),
						RejectedCount = g.Count(r => r.Status == "Rejected")
					})
					.FirstOrDefaultAsync();

				return Ok(summary);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy summary KPI của user {UserId}", userId);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		/// <summary>
		/// Lấy danh sách tiến độ KPI của Sales users
		/// </summary>
		private async Task<List<object>> GetSaleKpiData(string period)
		{
			// Parse period để tạo date range
			var periodParts = period.Split('-');
			if (periodParts.Length != 2 || !int.TryParse(periodParts[0], out int year) || !int.TryParse(periodParts[1], out int month))
			{
				return new List<object>();
			}

			var periodStartDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
			var periodEndDate = periodStartDate.AddMonths(1).AddSeconds(-1);

			_logger.LogInformation("🔍 Debug - Period: {Period}, StartDate: {Start}, EndDate: {End}", 
				period, periodStartDate, periodEndDate);

			// Lấy Department Sales
			var salesDepartment = await _context.Departments
				.FirstOrDefaultAsync(d => d.Name.ToLower() == "phòng kinh doanh");

			if (salesDepartment == null)
			{
				_logger.LogWarning("Không tìm thấy phòng ban Sales");
				return new List<object>();
			}
			var departmentId = salesDepartment.Id;
			_logger.LogInformation("🔍 Debug - Sales Department ID: {DeptId}", departmentId);

			// Lấy tất cả users thuộc phòng Sales
			var salesUsers = await _context.Users
				.Where(u => u.DepartmentId == departmentId && u.Status == "active")
				.AsNoTracking()
				.ToListAsync();

			if (!salesUsers.Any())
			{
				_logger.LogInformation("Không có Sales user nào trong hệ thống");
				return new List<object>();
			}

			var salesUserIds = salesUsers.Select(u => u.Id).ToList();
			_logger.LogInformation("🔍 Debug - Sales User IDs: {UserIds}", string.Join(", ", salesUserIds));

			// BƯỚC 1: Lấy tất cả UserKpiAssignments (không filter gì cả)
			var allAssignments = await _context.UserKpiAssignments
				.AsNoTracking()
				.ToListAsync();
			_logger.LogInformation("🔍 Debug - Tổng số UserKpiAssignments trong DB: {Count}", allAssignments.Count);

			// BƯỚC 2: Filter theo IsActive
			var activeAssignments = allAssignments.Where(a => a.IsActive).ToList();
			_logger.LogInformation("🔍 Debug - Số assignments IsActive = true: {Count}", activeAssignments.Count);

			// BƯỚC 3: Filter theo UserId
			var userFilteredAssignments = activeAssignments.Where(a => salesUserIds.Contains(a.UserId)).ToList();
			_logger.LogInformation("🔍 Debug - Số assignments thuộc Sales users: {Count}", userFilteredAssignments.Count);

			if (!userFilteredAssignments.Any())
			{
				_logger.LogWarning("⚠️ Không có assignment nào thuộc sales users!");
				return new List<object>();
			}

			// BƯỚC 4: Lấy KPI IDs từ assignments
			var kpiIds = userFilteredAssignments.Select(a => a.KpiId).Distinct().ToList();
			_logger.LogInformation("🔍 Debug - KPI IDs cần check: {KpiIds}", string.Join(", ", kpiIds));

			// BƯỚC 5: Lấy KPI details
			var kpis = await _context.KPIs
				.Include(k => k.Department)
				.Include(k => k.CommissionTiers)
				.Where(k => kpiIds.Contains(k.Id))
				.AsNoTracking()
				.ToListAsync();

			_logger.LogInformation("🔍 Debug - Số KPIs tìm thấy: {Count}", kpis.Count);

			foreach (var kpi in kpis)
			{
				_logger.LogInformation("🔍 Debug - KPI {Id}: Name={Name}, IsActive={IsActive}, KpiType={Type}, StartDate={StartDate}, Period={Period}",
					kpi.Id, kpi.Name, kpi.IsActive, kpi.KpiType, kpi.StartDate, kpi.Period);
			}

			// BƯỚC 6: Filter KPI theo IsActive
			var activeKpiIds = kpis.Where(k => k.IsActive).Select(k => k.Id).ToList();
			_logger.LogInformation("🔍 Debug - KPI IDs IsActive: {Ids}", string.Join(", ", activeKpiIds));

			// BƯỚC 7: Filter KPI theo KpiType = "Revenue"
			var revenueKpiIds = kpis.Where(k => k.IsActive && k.KpiType == "Revenue").Select(k => k.Id).ToList();
			_logger.LogInformation("🔍 Debug - KPI IDs có type Revenue: {Ids}", string.Join(", ", revenueKpiIds));

			// BƯỚC 8: Filter KPI theo StartDate/EndDate hoặc Period
			// Logic: KPI phải đang active trong kỳ period
			var matchedKpis = kpis
				.Where(k =>
					k.KpiType == "Revenue" &&
					k.IsActive == true &&
					k.StartDate >= periodStartDate &&
					k.EndDate <= periodEndDate
				)
				.ToList();


			_logger.LogInformation("🔍 Debug - KPIs khớp tất cả điều kiện: {Count}", matchedKpis.Count);

			if (!matchedKpis.Any())
			{
				_logger.LogWarning("⚠️ Không có KPI nào khớp điều kiện Period hoặc StartDate/EndDate!");
				foreach (var kpi in kpis.Where(k => k.IsActive && k.KpiType == "Revenue"))
				{
					var periodCheck = kpi.Period != null && kpi.Period.Contains(period);
					var dateRangeCheck = kpi.StartDate <= periodEndDate && (kpi.EndDate == null || kpi.EndDate >= periodStartDate);
					_logger.LogWarning("KPI {Id}: Period='{Period}' (check={PeriodCheck}), StartDate={StartDate}, EndDate={EndDate} (check={DateCheck})",
						kpi.Id, kpi.Period ?? "NULL", periodCheck, kpi.StartDate, kpi.EndDate?.ToString() ?? "NULL", dateRangeCheck);
				}
				return new List<object>();
			}

			var matchedKpiIds = matchedKpis.Select(k => k.Id).ToList();

			// BƯỚC 9: Lấy assignments cuối cùng
			var finalAssignments = userFilteredAssignments
				.Where(a => matchedKpiIds.Contains(a.KpiId))
				.ToList();

			_logger.LogInformation("🔍 Debug - Số assignments cuối cùng: {Count}", finalAssignments.Count);

			if (!finalAssignments.Any())
			{
				_logger.LogInformation("Không có Sales user nào được giao KPI trong kỳ {Period}", period);
				return new List<object>();
			}

			// BƯỚC 10: Load User info
			var userIds = finalAssignments.Select(a => a.UserId).Distinct().ToList();
			var users = await _context.Users
				.Where(u => userIds.Contains(u.Id))
				.AsNoTracking()
				.ToListAsync();

			// Map KPI và User vào assignments
			foreach (var assignment in finalAssignments)
			{
				assignment.Kpi = matchedKpis.FirstOrDefault(k => k.Id == assignment.KpiId);
				assignment.User = users.FirstOrDefault(u => u.Id == assignment.UserId);
			}

			// Lấy KPI records đã tính cho kỳ này
			var kpiRecords = await _context.KpiRecords
				.Include(r => r.Kpi)
				.Where(r => r.Period == period && salesUserIds.Contains(r.UserId))
				.AsNoTracking()
				.ToListAsync();

			_logger.LogInformation("🔍 Debug - Số KPI Records tìm thấy: {Count}", kpiRecords.Count);

			// Lấy tất cả contracts đã paid trong kỳ
			var contracts = await _context.Contracts
				.Where(c => c.Status == "Paid" &&
				           c.CreatedAt >= periodStartDate &&
				           c.CreatedAt <= periodEndDate &&
				           salesUserIds.Contains(c.UserId))
				.AsNoTracking()
				.ToListAsync();

			_logger.LogInformation("🔍 Debug - Số Contracts Paid: {Count}", contracts.Count);

			// Xử lý từng user assignment
			var salesKpiData = new List<object>();

			foreach (var assignment in finalAssignments)
			{
				var userId = assignment.UserId;
				var user = assignment.User;
				var kpi = assignment.Kpi;

				// 1. Tính tổng doanh thu thực tế của user từ contracts đã paid
				var userContracts = contracts.Where(c => c.UserId == userId).ToList();
				var actualRevenue = userContracts.Sum(c => c.TotalAmount);

				// 2. Lấy target value
				var targetValue = assignment.CustomTargetValue ?? kpi.TargetValue;

				// 3. Tính % achievement
				decimal achievementPercentage = targetValue > 0 
					? (actualRevenue / targetValue) * 100 
					: 0;

				// 4. Xác định trạng thái KPI
				string kpiStatus;
				if (achievementPercentage >= 80)
					kpiStatus = "Hoàn thành KPI";
				else if (achievementPercentage >= 50)
					kpiStatus = "Đang hoàn thành";
				else
					kpiStatus = "Không đạt KPI";

				// 5. Tính hoa hồng
				decimal commissionAmount = 0;
				decimal? commissionPercentage = null;
				int? commissionTierLevel = null;
				string commissionNote = "";

				if (achievementPercentage >= 80 && achievementPercentage < 100)
				{
					// 80% - 99%: Không có hoa hồng
					commissionAmount = 0;
					commissionNote = "Đạt 80-99% KPI, chưa được hoa hồng";
				}
				else if (achievementPercentage >= 100)
				{
					// >= 100%: Tính hoa hồng theo tier
					var tiers = kpi.CommissionTiers?
						.Where(t => t.IsActive)
						.OrderBy(t => t.MinRevenue)
						.ToList();

					if (tiers != null && tiers.Any())
					{
						// Tìm tier phù hợp
						var matchedTier = tiers.FirstOrDefault(t =>
							actualRevenue >= t.MinRevenue &&
							(t.MaxRevenue == null || actualRevenue < t.MaxRevenue));

						if (matchedTier != null)
						{
							commissionPercentage = matchedTier.CommissionPercentage;
							commissionTierLevel = matchedTier.TierLevel;
							commissionAmount = actualRevenue * (commissionPercentage.Value / 100);
							commissionNote = $"Áp dụng bậc {commissionTierLevel} - Hoa hồng {commissionPercentage}%";
						}
						else
						{
							commissionNote = "Không tìm thấy tier hoa hồng phù hợp";
						}
					}
					else
					{
						commissionNote = "KPI chưa cấu hình bậc hoa hồng";
					}
				}

				// 6. Kiểm tra KPI record đã tồn tại
				var existingRecord = kpiRecords.FirstOrDefault(r => 
					r.KpiId == assignment.KpiId && r.UserId == userId);

				var recordStatus = existingRecord?.Status ?? "Chưa tạo";
				var recordDate = existingRecord?.RecordDate;
				var approvedAt = existingRecord?.ApprovedAt;
				var approvedBy = existingRecord?.ApprovedBy;

				// 7. Tạo response object
				salesKpiData.Add(new
				{
					UserId = userId,
					UserName = user?.Name,
					UserEmail = user?.Email,
					Department = "Sales",
					
					// KPI Information
					KpiId = kpi.Id,
					KpiName = kpi.Name,
					KpiType = kpi.KpiType,
					Period = period,
					
					// Target & Achievement
					TargetValue = targetValue,
					ActualRevenue = actualRevenue,
					AchievementPercentage = Math.Round(achievementPercentage, 2),
					KpiStatus = kpiStatus,
					
					// Contracts Detail
					TotalContracts = userContracts.Count,
					ContractIds = userContracts.Select(c => c.Id).ToList(),
					
					// Commission Information
					CommissionAmount = Math.Round(commissionAmount, 0),
					CommissionPercentage = commissionPercentage,
					CommissionTierLevel = commissionTierLevel,
					CommissionNote = commissionNote,
					
					// Record Status
					RecordStatus = recordStatus,
					RecordId = existingRecord?.Id,
					RecordDate = recordDate,
					ApprovedAt = approvedAt,
					ApprovedBy = approvedBy,
					
					// Assignment Info
					AssignedDate = assignment.AssignedDate,
					Weight = assignment.Weight,
					CustomTargetValue = assignment.CustomTargetValue,
					
					// Calculation Details
					CalculationDetails = new
					{
						ContractCount = userContracts.Count,
						TotalRevenue = actualRevenue,
						TargetRevenue = targetValue,
						AchievementRate = $"{Math.Round(achievementPercentage, 2)}%",
						IsKpiCompleted = achievementPercentage >= 80,
						CommissionEligible = achievementPercentage >= 100,
						Formula = $"Achievement = (Actual Revenue / Target Value) × 100 = ({actualRevenue} / {targetValue}) × 100 = {Math.Round(achievementPercentage, 2)}%"
					}
				});
			}

			// Sắp xếp theo achievement percentage
			return salesKpiData
				.OrderByDescending(x => ((dynamic)x).AchievementPercentage)
				.ToList<object>();
		}
	}
}
