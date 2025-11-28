using erp_backend.Data;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Services
{
    public interface ICommissionCalculationService
    {
        Task<CommissionResult> CalculateCommissionAsync(int kpiId, decimal actualValue);
        Task<List<KpiCommissionTier>> GetCommissionTiersAsync(int kpiId);
        Task<decimal> GetBaseTargetValueAsync(int kpiId);
    }

    public class CommissionResult
    {
        public decimal CommissionAmount { get; set; }
        public decimal CommissionPercentage { get; set; }
        public int? TierLevel { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CommissionCalculationService : ICommissionCalculationService
    {
        private readonly ApplicationDbContext _context;

        public CommissionCalculationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// L?y m?c tiêu c? b?n (100% KPI) = MinRevenue c?a B?c 1
        /// </summary>
        public async Task<decimal> GetBaseTargetValueAsync(int kpiId)
        {
            var baseTier = await _context.KpiCommissionTiers
                .Where(t => t.KpiId == kpiId && t.IsActive)
                .OrderBy(t => t.TierLevel)
                .FirstOrDefaultAsync();

            return baseTier?.MinRevenue ?? 0;
        }

        /// <summary>
        /// Tính hoa h?ng d?a trên actualValue và KPI
        /// Logic: 
        /// - Sales: actualValue = Doanh s? (VND)
        /// - Marketing: actualValue = ROI (%)
        /// - IT: Không có hoa h?ng
        /// </summary>
        public async Task<CommissionResult> CalculateCommissionAsync(int kpiId, decimal actualValue)
        {
            // 1. L?y t?t c? tiers c?a KPI
            var tiers = await _context.KpiCommissionTiers
                .Where(t => t.KpiId == kpiId && t.IsActive)
                .OrderBy(t => t.TierLevel)
                .ToListAsync();

            if (!tiers.Any())
            {
                return new CommissionResult
                {
                    CommissionAmount = 0,
                    CommissionPercentage = 0,
                    TierLevel = null,
                    Message = "Không có c?u hình hoa h?ng cho KPI này"
                };
            }

            // 2. L?y base target (MinRevenue c?a B?c 1)
            var baseTier = tiers.First();
            decimal baseTarget = baseTier.MinRevenue;

            // 3. Ki?m tra có ??t m?c tiêu c? b?n không (100% KPI)
            if (actualValue < baseTarget)
            {
                return new CommissionResult
                {
                    CommissionAmount = 0,
                    CommissionPercentage = 0,
                    TierLevel = null,
                    Message = $"Ch?a ??t m?c tiêu c? b?n (??t {actualValue:N0}, yêu c?u t?i thi?u {baseTarget:N0})"
                };
            }

            // 4. Tìm b?c phù h?p v?i actualValue
            KpiCommissionTier? matchedTier = null;

            foreach (var tier in tiers)
            {
                bool isInRange = actualValue >= tier.MinRevenue && 
                                 (tier.MaxRevenue == null || actualValue < tier.MaxRevenue);

                if (isInRange)
                {
                    matchedTier = tier;
                    break;
                }
            }

            // 5. N?u v??t quá b?c cao nh?t
            if (matchedTier == null)
            {
                var highestTier = tiers.Last();
                if (highestTier.MaxRevenue == null || actualValue >= highestTier.MaxRevenue)
                {
                    matchedTier = highestTier;
                }
            }

            if (matchedTier == null)
            {
                return new CommissionResult
                {
                    CommissionAmount = 0,
                    CommissionPercentage = 0,
                    TierLevel = null,
                    Message = "Không tìm th?y b?c hoa h?ng phù h?p"
                };
            }

            // 6. Tính hoa h?ng
            // ??i v?i Sales: commissionAmount = actualValue * %
            // ??i v?i Marketing: commissionAmount s? ???c tính riêng trong job
            decimal commissionAmount = actualValue * matchedTier.CommissionPercentage / 100;

            return new CommissionResult
            {
                CommissionAmount = commissionAmount,
                CommissionPercentage = matchedTier.CommissionPercentage,
                TierLevel = matchedTier.TierLevel,
                Message = $"Áp d?ng b?c {matchedTier.TierLevel}: {matchedTier.Description}"
            };
        }

        /// <summary>
        /// L?y danh sách các b?c hoa h?ng c?a KPI
        /// </summary>
        public async Task<List<KpiCommissionTier>> GetCommissionTiersAsync(int kpiId)
        {
            return await _context.KpiCommissionTiers
                .Where(t => t.KpiId == kpiId && t.IsActive)
                .OrderBy(t => t.TierLevel)
                .ToListAsync();
        }
    }
}
