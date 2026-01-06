using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
    /// <summary>
    /// Bảng ghi nhận kết quả KPI và hoa hồng thực tế của Sale
    /// Tự động tính toán dựa trên các hợp đồng có PaymentStatus = "Paid"
    /// </summary>
    public class SaleKpiRecord
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // Sale User ID

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2020, 2100)]
        public int Year { get; set; }

        public int? KpiTargetId { get; set; } // Link tới SaleKpiTarget

        /// <summary>
        /// Tổng tiền hợp đồng đã thanh toán (PaymentStatus = "Paid")
        /// </summary>
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPaidAmount { get; set; } = 0;

        /// <summary>
        /// KPI được giao (copy từ SaleKpiTarget)
        /// </summary>
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; } = 0;

        /// <summary>
        /// % hoàn thành KPI = (TotalPaidAmount / TargetAmount) * 100
        /// </summary>
        [Range(0, 1000)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal AchievementPercentage { get; set; } = 0;

        /// <summary>
        /// Đạt KPI hay không (TotalPaidAmount >= TargetAmount)
        /// </summary>
        public bool IsKpiAchieved { get; set; } = false;

        /// <summary>
        /// % hoa hồng được áp dụng (dựa vào CommissionRate)
        /// </summary>
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal CommissionPercentage { get; set; } = 0;

        /// <summary>
        /// Số tiền hoa hồng = TotalPaidAmount * CommissionPercentage / 100
        /// </summary>
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; } = 0;

        /// <summary>
        /// Bậc hoa hồng (Tier Level)
        /// </summary>
        public int? CommissionTierLevel { get; set; }

        /// <summary>
        /// Tổng số hợp đồng đã thanh toán trong tháng
        /// </summary>
        public int TotalContracts { get; set; } = 0;


        [StringLength(1000)]
        public string? Notes { get; set; }


        public int? ApprovedBy { get; set; } // Admin duyệt

        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User? SaleUser { get; set; }
        public SaleKpiTarget? KpiTarget { get; set; }
        public User? ApprovedByUser { get; set; }
    }
}
