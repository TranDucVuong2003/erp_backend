using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
    /// <summary>
    /// B?ng l?u KPI ???c giao cho t?ng Sale theo tháng
    /// Admin s? giao KPI ??u m?i tháng
    /// </summary>
    public class SaleKpiTarget
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // Sale User ID

        [Required]
        [Range(1, 12)]
        public int Month { get; set; } // Tháng (1-12)

        [Required]
        [Range(2020, 2100)]
        public int Year { get; set; } // N?m

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "KPI ph?i l?n h?n 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; } // S? ti?n KPI (VD: 20,000,000 VND)

        [Required]
        public int AssignedByUserId { get; set; } // Admin giao KPI

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User? SaleUser { get; set; }
        public User? AssignedByUser { get; set; }
    }
}
