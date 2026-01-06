using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace erp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SaleKpiRecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SaleKpiRecordsController> _logger;

        public SaleKpiRecordsController(ApplicationDbContext context, ILogger<SaleKpiRecordsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// L?y danh sách t?t c? KPI Records (có filter)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetSaleKpiRecords(
            [FromQuery] int? month, 
            [FromQuery] int? year,
            [FromQuery] int? userId,
            [FromQuery] bool? isKpiAchieved)
        {
            try
            {
                var query = _context.SaleKpiRecords
                    .Include(r => r.SaleUser)
                    .Include(r => r.KpiTarget)
                    .AsQueryable();

                // Apply filters
                if (month.HasValue)
                    query = query.Where(r => r.Month == month.Value);

                if (year.HasValue)
                    query = query.Where(r => r.Year == year.Value);

                if (userId.HasValue)
                    query = query.Where(r => r.UserId == userId.Value);

                if (isKpiAchieved.HasValue)
                    query = query.Where(r => r.IsKpiAchieved == isKpiAchieved.Value);

                var records = await query
                    .OrderByDescending(r => r.Year)
                    .ThenByDescending(r => r.Month)
                    .ThenByDescending(r => r.TotalPaidAmount)
                    .Select(r => new
                    {
                        id = r.Id,
                        userId = r.UserId,
                        userName = r.SaleUser != null ? r.SaleUser.Name : null,
                        userEmail = r.SaleUser != null ? r.SaleUser.Email : null,
                        month = r.Month,
                        year = r.Year,
                        targetAmount = r.TargetAmount,
                        totalPaidAmount = r.TotalPaidAmount,
                        achievementPercentage = r.AchievementPercentage,
                        isKpiAchieved = r.IsKpiAchieved,
                        commissionPercentage = r.CommissionPercentage,
                        commissionAmount = r.CommissionAmount,
                        commissionTierLevel = r.CommissionTierLevel,
                        totalContracts = r.TotalContracts,
                        notes = r.Notes,
                        createdAt = r.CreatedAt,
                        updatedAt = r.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y danh sách KPI Records");
                return StatusCode(500, new { success = false, message = "?ã x?y ra l?i khi l?y danh sách KPI Records" });
            }
        }

        /// <summary>
        /// L?y chi ti?t m?t KPI Record theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetSaleKpiRecord(int id)
        {
            try
            {
                var record = await _context.SaleKpiRecords
                    .Include(r => r.SaleUser)
                        .ThenInclude(u => u!.Department)
                    .Include(r => r.SaleUser)
                        .ThenInclude(u => u!.Position)
                    .Include(r => r.KpiTarget)
                        .ThenInclude(t => t!.KpiPackage)
                    .Include(r => r.ApprovedByUser)
                    .Where(r => r.Id == id)
                    .Select(r => new
                    {
                        id = r.Id,
                        userId = r.UserId,
                        saleUser = r.SaleUser != null ? new
                        {
                            id = r.SaleUser.Id,
                            name = r.SaleUser.Name,
                            email = r.SaleUser.Email,
                            phoneNumber = r.SaleUser.PhoneNumber,
                            departmentName = r.SaleUser.Department != null ? r.SaleUser.Department.Name : null,
                            positionName = r.SaleUser.Position != null ? r.SaleUser.Position.PositionName : null
                        } : null,
                        month = r.Month,
                        year = r.Year,
                        kpiTargetId = r.KpiTargetId,
                        kpiPackageName = r.KpiTarget != null && r.KpiTarget.KpiPackage != null 
                            ? r.KpiTarget.KpiPackage.Name 
                            : null,
                        targetAmount = r.TargetAmount,
                        totalPaidAmount = r.TotalPaidAmount,
                        achievementPercentage = r.AchievementPercentage,
                        isKpiAchieved = r.IsKpiAchieved,
                        commissionPercentage = r.CommissionPercentage,
                        commissionAmount = r.CommissionAmount,
                        commissionTierLevel = r.CommissionTierLevel,
                        totalContracts = r.TotalContracts,
                        notes = r.Notes,
                        approvedBy = r.ApprovedBy,
                        approvedByUserName = r.ApprovedByUser != null ? r.ApprovedByUser.Name : null,
                        approvedAt = r.ApprovedAt,
                        createdAt = r.CreatedAt,
                        updatedAt = r.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (record == null)
                {
                    return NotFound(new { success = false, message = $"Không tìm th?y KPI Record v?i ID {id}" });
                }

                return Ok(new { success = true, data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y KPI Record ID {Id}", id);
                return StatusCode(500, new { success = false, message = "?ã x?y ra l?i khi l?y thông tin KPI Record" });
            }
        }

        /// <summary>
        /// L?y KPI Records c?a m?t user c? th?
        /// </summary>
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult> GetKpiRecordsByUser(int userId)
        {
            try
            {
                var records = await _context.SaleKpiRecords
                    .Include(r => r.SaleUser)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.Year)
                    .ThenByDescending(r => r.Month)
                    .Select(r => new
                    {
                        id = r.Id,
                        month = r.Month,
                        year = r.Year,
                        targetAmount = r.TargetAmount,
                        totalPaidAmount = r.TotalPaidAmount,
                        achievementPercentage = r.AchievementPercentage,
                        isKpiAchieved = r.IsKpiAchieved,
                        commissionAmount = r.CommissionAmount,
                        commissionPercentage = r.CommissionPercentage,
                        totalContracts = r.TotalContracts,
                        updatedAt = r.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y KPI Records c?a User {UserId}", userId);
                return StatusCode(500, new { success = false, message = "?ã x?y ra l?i khi l?y KPI Records" });
            }
        }

        /// <summary>
        /// L?y KPI Records theo tháng/n?m
        /// </summary>
        [HttpGet("by-period")]
        public async Task<ActionResult> GetKpiRecordsByPeriod([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new { success = false, message = "Tháng ph?i t? 1 ??n 12" });
                }

                var records = await _context.SaleKpiRecords
                    .Include(r => r.SaleUser)
                    .Where(r => r.Month == month && r.Year == year)
                    .OrderByDescending(r => r.TotalPaidAmount)
                    .Select(r => new
                    {
                        id = r.Id,
                        userId = r.UserId,
                        userName = r.SaleUser != null ? r.SaleUser.Name : null,
                        targetAmount = r.TargetAmount,
                        totalPaidAmount = r.TotalPaidAmount,
                        achievementPercentage = r.AchievementPercentage,
                        isKpiAchieved = r.IsKpiAchieved,
                        commissionAmount = r.CommissionAmount,
                        commissionPercentage = r.CommissionPercentage,
                        totalContracts = r.TotalContracts
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y KPI Records theo Period");
                return StatusCode(500, new { success = false, message = "?ã x?y ra l?i khi l?y KPI Records" });
            }
        }

        /// <summary>
        /// L?y b?ng x?p h?ng Top performers
        /// </summary>
        [HttpGet("leaderboard")]
        public async Task<ActionResult> GetLeaderboard([FromQuery] int? month, [FromQuery] int? year)
        {
            try
            {
                var targetMonth = month ?? DateTime.UtcNow.Month;
                var targetYear = year ?? DateTime.UtcNow.Year;

                // ? FIX: Query database tr??c, không dùng index trong Select
                var records = await _context.SaleKpiRecords
                    .Include(r => r.SaleUser)
                    .Where(r => r.Month == targetMonth && r.Year == targetYear)
                    .OrderByDescending(r => r.AchievementPercentage)
                    .ThenByDescending(r => r.TotalPaidAmount)
                    .Select(r => new
                    {
                        userId = r.UserId,
                        userName = r.SaleUser != null ? r.SaleUser.Name : "Unknown",
                        targetAmount = r.TargetAmount,
                        totalPaidAmount = r.TotalPaidAmount,
                        achievementPercentage = r.AchievementPercentage,
                        isKpiAchieved = r.IsKpiAchieved,
                        commissionAmount = r.CommissionAmount,
                        totalContracts = r.TotalContracts
                    })
                    .ToListAsync();

                // ? Thêm rank sau khi ?ã có data (trong memory)
                var leaderboard = records.Select((r, index) => new
                {
                    rank = index + 1,
                    r.userId,
                    r.userName,
                    r.targetAmount,
                    r.totalPaidAmount,
                    r.achievementPercentage,
                    r.isKpiAchieved,
                    r.commissionAmount,
                    r.totalContracts
                }).ToList();

                return Ok(new 
                { 
                    success = true, 
                    month = targetMonth,
                    year = targetYear,
                    data = leaderboard 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y b?ng x?p h?ng");
                return StatusCode(500, new { success = false, message = "?ã x?y ra l?i khi l?y b?ng x?p h?ng" });
            }
        }

        /// <summary>
        /// Th?ng kê t?ng quan KPI
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetStatistics([FromQuery] int? month, [FromQuery] int? year)
        {
            try
            {
                var targetMonth = month ?? DateTime.UtcNow.Month;
                var targetYear = year ?? DateTime.UtcNow.Year;

                var records = await _context.SaleKpiRecords
                    .Where(r => r.Month == targetMonth && r.Year == targetYear)
                    .ToListAsync();

                var statistics = new
                {
                    month = targetMonth,
                    year = targetYear,
                    totalUsers = records.Count,
                    usersAchievedKpi = records.Count(r => r.IsKpiAchieved),
                    usersNotAchievedKpi = records.Count(r => !r.IsKpiAchieved),
                    achievementRate = records.Count > 0 
                        ? Math.Round((double)records.Count(r => r.IsKpiAchieved) / records.Count * 100, 2) 
                        : 0,
                    totalTargetAmount = records.Sum(r => r.TargetAmount),
                    totalPaidAmount = records.Sum(r => r.TotalPaidAmount),
                    totalCommissionAmount = records.Sum(r => r.CommissionAmount),
                    totalContracts = records.Sum(r => r.TotalContracts),
                    averageAchievementPercentage = records.Count > 0 
                        ? Math.Round(records.Average(r => (double)r.AchievementPercentage), 2) 
                        : 0
                };

                return Ok(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y th?ng kê KPI");
                return StatusCode(500, new { success = false, message = "?ã x?y ra l?i khi l?y th?ng kê" });
            }
        }

        /// <summary>
        /// C?p nh?t ghi chú cho KPI Record (Admin only)
        /// </summary>
        [HttpPut("{id}/notes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateNotes(int id, [FromBody] UpdateKpiNotesRequest request)
        {
            try
            {
                var record = await _context.SaleKpiRecords.FindAsync(id);
                if (record == null)
                {
                    return NotFound(new { success = false, message = $"Không tìm th?y KPI Record v?i ID {id}" });
                }

                record.Notes = request.Notes;
                record.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "?ã c?p nh?t ghi chú" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi c?p nh?t ghi chú KPI Record ID {Id}", id);
                return StatusCode(500, new { success = false, message = "?ã x?y ra l?i khi c?p nh?t ghi chú" });
            }
        }
    }

    // DTO for update notes
    public class UpdateKpiNotesRequest
    {
        public string? Notes { get; set; }
    }
}
