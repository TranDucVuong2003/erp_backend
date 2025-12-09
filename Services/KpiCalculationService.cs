using erp_backend.Data;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Services
{
    public interface IKpiCalculationService
    {
        /// <summary>
        /// Tính toán KPI cho m?t user trong tháng/n?m c? th?
        /// </summary>
        Task CalculateKpiForUserAsync(int userId, int month, int year);

        /// <summary>
        /// Tính toán KPI cho t?t c? users có target trong tháng/n?m
        /// </summary>
        Task CalculateKpiForAllUsersAsync(int month, int year);

        /// <summary>
        /// Tính toán KPI cho user d?a trên KpiTargetId
        /// </summary>
        Task CalculateKpiByTargetIdAsync(int kpiTargetId);
    }

    public class KpiCalculationService : IKpiCalculationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KpiCalculationService> _logger;

        public KpiCalculationService(ApplicationDbContext context, ILogger<KpiCalculationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tính toán KPI cho m?t user trong tháng/n?m c? th?
        /// </summary>
        public async Task CalculateKpiForUserAsync(int userId, int month, int year)
        {
            try
            {
                _logger.LogInformation("B?t ??u tính toán KPI cho User {UserId}, Tháng {Month}/{Year}", userId, month, year);

                // 1. L?y thông tin Target ?ã giao (T? b?ng SaleKpiTarget)
                var target = await _context.SaleKpiTargets
                    .Include(t => t.SaleUser)
                    .FirstOrDefaultAsync(t => t.UserId == userId 
                        && t.Month == month 
                        && t.Year == year 
                        && t.IsActive);

                if (target == null)
                {
                    _logger.LogWarning("User {UserId} ch?a ???c giao KPI cho tháng {Month}/{Year}", userId, month, year);
                    return;
                }

                // 2. Tính t?ng doanh thu th?c t? t? các h?p ??ng
                // ? FIXED: Thêm Status "Paid" vì trong ContractsController khi contract chuy?n sang "Paid" s? trigger KPI calculation
                var contractsQuery = _context.Contracts
                    .Include(c => c.SaleOrder)
                    .Where(c => c.SaleOrder!.CreatedByUserId == userId
                        && c.CreatedAt.Month == month
                        && c.CreatedAt.Year == year
                        && (c.Status == "Paid" || c.Status == "Completed" || c.Status == "Signed" || c.Status == "Active"));

                var contracts = await contractsQuery.ToListAsync();

                decimal totalPaidAmount = contracts.Sum(c => c.TotalAmount);
                int totalContracts = contracts.Count;

                _logger.LogInformation("User {UserId}: T?ng {Count} h?p ??ng, T?ng ti?n: {Amount:N0} VN?", 
                    userId, totalContracts, totalPaidAmount);

                // 3. Tìm m?c hoa h?ng (CommissionRate) áp d?ng
                // Logic: Tìm b?c cao nh?t mà doanh thu hi?n t?i ?ã v??t qua
                var commissionRate = await _context.CommissionRates
                    .Where(r => r.IsActive 
                        && totalPaidAmount >= r.MinAmount
                        && (r.MaxAmount == null || totalPaidAmount <= r.MaxAmount))
                    .OrderByDescending(r => r.MinAmount)
                    .FirstOrDefaultAsync();

                decimal commissionPercent = commissionRate?.CommissionPercentage ?? 0;
                int tierLevel = commissionRate?.TierLevel ?? 0;

                // Tính ti?n hoa h?ng
                decimal commissionAmount = (totalPaidAmount * commissionPercent) / 100;

                _logger.LogInformation("User {UserId}: Commission Tier {Tier}, Rate {Rate}%, Amount {Amount:N0} VN?", 
                    userId, tierLevel, commissionPercent, commissionAmount);

                // 4. Tính % hoàn thành KPI
                decimal achievementPercent = 0;
                if (target.TargetAmount > 0)
                {
                    achievementPercent = (totalPaidAmount / target.TargetAmount) * 100;
                }
                else if (totalPaidAmount > 0)
                {
                    achievementPercent = 100; // N?u target = 0 mà có doanh thu thì coi nh? 100%
                }

                bool isAchieved = totalPaidAmount >= target.TargetAmount;

                _logger.LogInformation("User {UserId}: ??t {Percent:N2}% KPI ({Paid:N0}/{Target:N0}), Hoàn thành: {Achieved}", 
                    userId, achievementPercent, totalPaidAmount, target.TargetAmount, isAchieved);

                // 5. C?p nh?t ho?c T?o m?i vào b?ng SaleKpiRecord
                var record = await _context.SaleKpiRecords
                    .FirstOrDefaultAsync(r => r.UserId == userId 
                        && r.Month == month 
                        && r.Year == year);

                if (record == null)
                {
                    record = new SaleKpiRecord
                    {
                        UserId = userId,
                        Month = month,
                        Year = year,
                        KpiTargetId = target.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.SaleKpiRecords.Add(record);
                    _logger.LogInformation("T?o m?i KPI Record cho User {UserId}", userId);
                }
                else
                {
                    _logger.LogInformation("C?p nh?t KPI Record ID {RecordId} cho User {UserId}", record.Id, userId);
                }

                // Update các ch? s? m?i nh?t
                record.TotalPaidAmount = totalPaidAmount;
                record.TotalContracts = totalContracts;
                record.TargetAmount = target.TargetAmount; // Luôn ??ng b? v?i target snapshot
                record.AchievementPercentage = Math.Round(achievementPercent, 2);
                record.IsKpiAchieved = isAchieved;
                record.CommissionPercentage = commissionPercent;
                record.CommissionTierLevel = tierLevel;
                record.CommissionAmount = commissionAmount;
                record.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("? Hoàn thành tính toán KPI cho User {UserId} - {UserName}", 
                    userId, target.SaleUser?.Name ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? L?i khi tính toán KPI cho User {UserId}, Tháng {Month}/{Year}", userId, month, year);
                throw;
            }
        }

        /// <summary>
        /// Tính toán KPI cho t?t c? users có target trong tháng/n?m
        /// </summary>
        public async Task CalculateKpiForAllUsersAsync(int month, int year)
        {
            try
            {
                _logger.LogInformation("========================================");
                _logger.LogInformation("B?t ??u tính toán KPI cho t?t c? users - Tháng {Month}/{Year}", month, year);
                _logger.LogInformation("========================================");

                // L?y t?t c? KPI targets c?a tháng ?ó
                var targets = await _context.SaleKpiTargets
                    .Where(t => t.Month == month && t.Year == year && t.IsActive)
                    .Include(t => t.SaleUser)
                    .ToListAsync();

                if (!targets.Any())
                {
                    _logger.LogWarning("Không có KPI Target nào cho tháng {Month}/{Year}", month, year);
                    return;
                }

                _logger.LogInformation("Tìm th?y {Count} users có KPI Target", targets.Count);

                int successCount = 0;
                int failedCount = 0;

                foreach (var target in targets)
                {
                    try
                    {
                        await CalculateKpiForUserAsync(target.UserId, month, year);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "L?i khi tính toán KPI cho User {UserId}", target.UserId);
                        failedCount++;
                    }
                }

                _logger.LogInformation("========================================");
                _logger.LogInformation("? Hoàn thành: {Success}/{Total} users", successCount, targets.Count);
                if (failedCount > 0)
                {
                    _logger.LogWarning("? Th?t b?i: {Failed} users", failedCount);
                }
                _logger.LogInformation("========================================");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i nghiêm tr?ng khi tính toán KPI cho t?t c? users");
                throw;
            }
        }

        /// <summary>
        /// Tính toán KPI cho user d?a trên KpiTargetId
        /// </summary>
        public async Task CalculateKpiByTargetIdAsync(int kpiTargetId)
        {
            try
            {
                var target = await _context.SaleKpiTargets
                    .FirstOrDefaultAsync(t => t.Id == kpiTargetId);

                if (target == null)
                {
                    _logger.LogWarning("Không tìm th?y KPI Target v?i ID {Id}", kpiTargetId);
                    return;
                }

                await CalculateKpiForUserAsync(target.UserId, target.Month, target.Year);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi tính toán KPI cho Target ID {Id}", kpiTargetId);
                throw;
            }
        }
    }
}
