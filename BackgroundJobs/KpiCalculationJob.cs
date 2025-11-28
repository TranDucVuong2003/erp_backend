using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Services;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.BackgroundJobs
{
    /// <summary>
    /// Job t? ??ng tính KpiRecord cu?i k? cho Sales, Marketing, IT
    /// </summary>
    public class KpiCalculationJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ICommissionCalculationService _commissionService;
        private readonly ILogger<KpiCalculationJob> _logger;

        public KpiCalculationJob(
            ApplicationDbContext context,
            ICommissionCalculationService commissionService,
            ILogger<KpiCalculationJob> logger)
        {
            _context = context;
            _commissionService = commissionService;
            _logger = logger;
        }

        /// <summary>
        /// Ki?m tra và tính KPI n?u là ngày cu?i tháng
        /// </summary>
        public async Task CheckAndCalculateMonthlyKpiAsync()
        {
            var today = DateTime.UtcNow;
            var tomorrow = today.AddDays(1);

            if (tomorrow.Day == 1)
            {
                var period = today.ToString("yyyy-MM");
                _logger.LogInformation("Hôm nay là ngày cu?i tháng, b?t ??u tính KPI cho k?: {Period}", period);
                await CalculateMonthlyKpiAsync(period);
            }
            else
            {
                _logger.LogInformation("Hôm nay không ph?i ngày cu?i tháng, b? qua");
            }
        }

        /// <summary>
        /// Tính toán KPI cho t?t c? users vào cu?i k?
        /// </summary>
        public async Task CalculateMonthlyKpiAsync(string period)
        {
            _logger.LogInformation("B?t ??u tính KPI cho k?: {Period}", period);

            try
            {
                var assignments = await _context.UserKpiAssignments
                    .Include(a => a.Kpi)
                        .ThenInclude(k => k.CommissionTiers)
                    .Include(a => a.Kpi)
                        .ThenInclude(k => k.Department)
                    .Include(a => a.User)
                    .Where(a => a.IsActive && 
                           a.Kpi.IsActive)
                    .ToListAsync();

                // L?c theo period (yyyy-MM)
                var filteredAssignments = assignments
                    .Where(a => a.Kpi.StartDate.ToString("yyyy-MM") == period ||
                               (a.Kpi.Period != null && a.Kpi.Period.Contains(period)))
                    .ToList();

                _logger.LogInformation("Tìm th?y {Count} assignments c?n tính", filteredAssignments.Count);

                foreach (var assignment in filteredAssignments)
                {
                    try
                    {
                        var kpiType = assignment.Kpi.KpiType;

                        switch (kpiType)
                        {
                            case "Revenue":
                                await CalculateSalesKpiAsync(assignment, period);
                                break;
                            case "Leads":
                                await CalculateMarketingKpiAsync(assignment, period);
                                break;
                            case "Tickets":
                                await CalculateITKpiAsync(assignment, period);
                                break;
                            default:
                                _logger.LogWarning("KpiType không h?p l?: {Type}", kpiType);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "L?i khi tính KPI cho User {UserId}, KPI {KpiId}", 
                            assignment.UserId, assignment.KpiId);
                    }
                }

                _logger.LogInformation("Hoàn thành tính KPI cho k?: {Period}", period);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi ch?y job tính KPI");
                throw;
            }
        }

        /// <summary>
        /// SALES: Tính KPI d?a trên doanh s? t? h?p ??ng ?ã thanh toán
        /// </summary>
        private async Task CalculateSalesKpiAsync(UserKpiAssignment assignment, string period)
        {
            var userId = assignment.UserId;
            var kpi = assignment.Kpi;

            _logger.LogInformation("Tính KPI Sales cho User {UserId}", userId);

            // Parse period
            var (startDate, endDate) = ParsePeriod(period);

            // 1. L?y m?c tiêu c? b?n (100% KPI) = MinRevenue c?a B?c 1
            var baseTarget = await _commissionService.GetBaseTargetValueAsync(kpi.Id);
            if (baseTarget == 0)
            {
                _logger.LogWarning("Không tìm th?y base target cho KPI {KpiId}", kpi.Id);
                return;
            }

            // 2. Tính doanh s? t? contracts ?ã paid
            var totalRevenue = await _context.Contracts
                .Where(c => c.UserId == userId &&
                           c.Status == "Paid" &&
                           c.CreatedAt >= startDate &&
                           c.CreatedAt <= endDate)
                .SumAsync(c => c.TotalAmount);

            // 3. Tính % KPI
            var achievementPercentage = baseTarget > 0 
                ? (totalRevenue / baseTarget) * 100 
                : 0;

            // 4. Tính hoa h?ng
            var commissionResult = await _commissionService.CalculateCommissionAsync(kpi.Id, totalRevenue);

            // 5. T?o KpiRecord
            await CreateKpiRecordAsync(new KpiRecord
            {
                KpiId = kpi.Id,
                UserId = userId,
                Period = period,
                ActualValue = totalRevenue,
                TargetValue = baseTarget,
                AchievementPercentage = achievementPercentage,
                CommissionAmount = commissionResult.CommissionAmount,
                CommissionPercentage = commissionResult.CommissionPercentage,
                CommissionTierLevel = commissionResult.TierLevel,
                Notes = commissionResult.Message,
                Status = "Pending",
                RecordDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Sales KPI - User {UserId}: Revenue={Revenue}, Achievement={Achievement}%, Commission={Commission}",
                userId, totalRevenue, achievementPercentage, commissionResult.CommissionAmount);
        }

        /// <summary>
        /// MARKETING: Tính KPI d?a trên ROI
        /// </summary>
        private async Task CalculateMarketingKpiAsync(UserKpiAssignment assignment, string period)
        {
            var userId = assignment.UserId;
            var kpi = assignment.Kpi;

            _logger.LogInformation("Tính KPI Marketing cho User {UserId}", userId);

            var (startDate, endDate) = ParsePeriod(period);

            // 1. L?y budget
            var budget = await _context.MarketingBudgets
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Period == period);

            if (budget == null)
            {
                _logger.LogWarning("Không tìm th?y budget cho User {UserId} k? {Period}", userId, period);
                return;
            }

            decimal approvedBudget = budget.ApprovedBudget;
            decimal actualSpending = budget.ActualSpending;
            decimal targetROI = budget.TargetROI;

            // 2. Tính doanh thu t? leads ?ã convert
            var totalRevenue = await _context.Leads
                .Where(l => l.CreatedByUserId == userId &&
                           l.IsConverted == true &&
                           l.ConvertedAt >= startDate &&
                           l.ConvertedAt <= endDate)
                .SumAsync(l => l.RevenueGenerated ?? 0);

            // 3. Tính ROI
            decimal actualROI = 0;
            if (actualSpending > 0)
            {
                actualROI = ((totalRevenue - actualSpending) / actualSpending) * 100;
            }

            // C?p nh?t budget
            budget.ActualROI = actualROI;
            await _context.SaveChangesAsync();

            // 4. Tính % KPI
            var achievementPercentage = targetROI > 0 
                ? (actualROI / targetROI) * 100 
                : 0;

            // 5. Tính hoa h?ng (d?a trên ROI)
            CommissionResult commissionResult;

            if (actualROI < targetROI)
            {
                commissionResult = new CommissionResult
                {
                    CommissionAmount = 0,
                    CommissionPercentage = 0,
                    TierLevel = null,
                    Message = $"ROI ch?a ??t m?c tiêu (ROI: {actualROI:F2}%, yêu c?u: {targetROI}%)"
                };
            }
            else
            {
                // Tìm tier d?a trên ROI
                var tiers = await _commissionService.GetCommissionTiersAsync(kpi.Id);
                var matchedTier = tiers.FirstOrDefault(t =>
                    actualROI >= t.MinRevenue &&
                    (t.MaxRevenue == null || actualROI < t.MaxRevenue)
                );

                if (matchedTier != null)
                {
                    // Hoa h?ng = % c?a approved budget
                    var commissionAmount = approvedBudget * matchedTier.CommissionPercentage / 100;
                    commissionResult = new CommissionResult
                    {
                        CommissionAmount = commissionAmount,
                        CommissionPercentage = matchedTier.CommissionPercentage,
                        TierLevel = matchedTier.TierLevel,
                        Message = $"ROI ??t {actualROI:F2}% - B?c {matchedTier.TierLevel}"
                    };
                }
                else
                {
                    commissionResult = new CommissionResult
                    {
                        CommissionAmount = 0,
                        CommissionPercentage = 0,
                        TierLevel = null,
                        Message = "Không tìm th?y tier phù h?p"
                    };
                }
            }

            // 6. L?y thông tin leads
            var totalLeads = await _context.Leads
                .Where(l => l.CreatedByUserId == userId &&
                           l.CreatedAt >= startDate && l.CreatedAt <= endDate)
                .CountAsync();

            var convertedLeads = await _context.Leads
                .Where(l => l.CreatedByUserId == userId &&
                           l.IsConverted == true &&
                           l.ConvertedAt >= startDate && l.ConvertedAt <= endDate)
                .CountAsync();

            var leadConversionRate = totalLeads > 0 
                ? (decimal)convertedLeads / totalLeads * 100 
                : 0;

            decimal costPerLead = totalLeads > 0 ? actualSpending / totalLeads : 0;
            decimal costPerConversion = convertedLeads > 0 ? actualSpending / convertedLeads : 0;

            // 7. T?o KpiRecord
            await CreateKpiRecordAsync(new KpiRecord
            {
                KpiId = kpi.Id,
                UserId = userId,
                Period = period,
                ActualValue = actualROI,  // ROI làm actual value
                TargetValue = targetROI,
                AchievementPercentage = achievementPercentage,
                CommissionAmount = commissionResult.CommissionAmount,
                CommissionPercentage = commissionResult.CommissionPercentage,
                CommissionTierLevel = commissionResult.TierLevel,
                
                // Thông tin leads
                TotalLeads = totalLeads,
                ConvertedLeads = convertedLeads,
                LeadConversionRate = leadConversionRate,
                
                // Thông tin budget
                ApprovedBudget = approvedBudget,
                ActualSpending = actualSpending,
                ROI = actualROI,
                CostPerLead = costPerLead,
                CostPerConversion = costPerConversion,
                
                Notes = $"Doanh thu: {totalRevenue:N0} VND | {commissionResult.Message}",
                Status = "Pending",
                RecordDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Marketing KPI - User {UserId}: ROI={ROI}%, Achievement={Achievement}%, Commission={Commission}",
                userId, actualROI, achievementPercentage, commissionResult.CommissionAmount);
        }

        /// <summary>
        /// IT: Tính KPI d?a trên % hoàn thành tickets
        /// </summary>
        private async Task CalculateITKpiAsync(UserKpiAssignment assignment, string period)
        {
            var userId = assignment.UserId;
            var kpi = assignment.Kpi;

            _logger.LogInformation("Tính KPI IT cho User {UserId}", userId);

            var (startDate, endDate) = ParsePeriod(period);

            // 1. Target = 80% (100% KPI)
            decimal targetValue = 80;

            // 2. ??m tickets
            var totalTickets = await _context.Tickets
                .Where(t => t.AssignedToId == userId &&
                           t.CreatedAt >= startDate &&
                           t.CreatedAt <= endDate)
                .CountAsync();

            var completedTickets = await _context.Tickets
                .Where(t => t.AssignedToId == userId &&
                           t.Status == "Closed" &&
                           t.CreatedAt >= startDate &&
                           t.CreatedAt <= endDate)
                .CountAsync();

            // 3. Tính % hoàn thành
            decimal completionRate = totalTickets > 0 
                ? (decimal)completedTickets / totalTickets * 100 
                : 0;

            // 4. Tính % KPI
            var achievementPercentage = targetValue > 0 
                ? (completionRate / targetValue) * 100 
                : 0;

            // 5. Tính th?i gian x? lý trung bình
            var tickets = await _context.Tickets
                .Where(t => t.AssignedToId == userId &&
                           t.Status == "Closed" &&
                           t.CreatedAt >= startDate &&
                           t.CreatedAt <= endDate &&
                           t.ClosedAt != null)
                .Select(t => new { t.CreatedAt, t.ClosedAt })
                .ToListAsync();

            decimal avgResolutionTime = 0;
            if (tickets.Any())
            {
                var totalHours = tickets.Sum(t => (t.ClosedAt!.Value - t.CreatedAt).TotalHours);
                avgResolutionTime = (decimal)(totalHours / tickets.Count);
            }

            // 6. T?o KpiRecord (IT không có hoa h?ng)
            await CreateKpiRecordAsync(new KpiRecord
            {
                KpiId = kpi.Id,
                UserId = userId,
                Period = period,
                ActualValue = completionRate,
                TargetValue = targetValue,
                AchievementPercentage = achievementPercentage,
                
                // IT không có hoa h?ng
                CommissionAmount = null,
                CommissionPercentage = null,
                CommissionTierLevel = null,
                
                // Thông tin tickets
                TotalTickets = totalTickets,
                CompletedTickets = completedTickets,
                AverageResolutionTime = avgResolutionTime,
                
                Notes = completionRate >= 80 
                    ? $"Hoàn thành KPI ({completionRate:F2}%)" 
                    : $"Ch?a ??t KPI ({completionRate:F2}%, yêu c?u 80%)",
                Status = "Pending",
                RecordDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "IT KPI - User {UserId}: Completion={Completion}%, Achievement={Achievement}%",
                userId, completionRate, achievementPercentage);
        }

        /// <summary>
        /// T?o ho?c update KpiRecord
        /// </summary>
        private async Task CreateKpiRecordAsync(KpiRecord record)
        {
            var existing = await _context.KpiRecords
                .FirstOrDefaultAsync(r => 
                    r.KpiId == record.KpiId && 
                    r.UserId == record.UserId && 
                    r.Period == record.Period);

            if (existing != null && existing.Status == "Approved")
            {
                _logger.LogInformation("KpiRecord ?ã t?n t?i và ?ã approved, b? qua");
                return;
            }

            if (existing != null)
            {
                // Update existing
                existing.ActualValue = record.ActualValue;
                existing.TargetValue = record.TargetValue;
                existing.AchievementPercentage = record.AchievementPercentage;
                existing.CommissionAmount = record.CommissionAmount;
                existing.CommissionPercentage = record.CommissionPercentage;
                existing.CommissionTierLevel = record.CommissionTierLevel;
                existing.TotalLeads = record.TotalLeads;
                existing.ConvertedLeads = record.ConvertedLeads;
                existing.LeadConversionRate = record.LeadConversionRate;
                existing.ApprovedBudget = record.ApprovedBudget;
                existing.ActualSpending = record.ActualSpending;
                existing.ROI = record.ROI;
                existing.CostPerLead = record.CostPerLead;
                existing.CostPerConversion = record.CostPerConversion;
                existing.TotalTickets = record.TotalTickets;
                existing.CompletedTickets = record.CompletedTickets;
                existing.AverageResolutionTime = record.AverageResolutionTime;
                existing.Notes = record.Notes;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                _context.KpiRecords.Add(record);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Parse period "2025-01" thành startDate và endDate
        /// </summary>
        private (DateTime startDate, DateTime endDate) ParsePeriod(string period)
        {
            var parts = period.Split('-');
            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddSeconds(-1);
            return (startDate, endDate);
        }
    }
}
