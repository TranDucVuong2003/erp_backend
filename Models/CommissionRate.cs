using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
    /// <summary>
    /// B?ng c?u hình t? l? hoa h?ng theo b?c doanh s?
    /// 15tr - 30tr -> 5%, 30tr - 60tr -> 7%, 60tr - 100tr -> 8%, >100tr -> 10%
    /// </summary>
    public class CommissionRate
    {
        public int Id { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinAmount { get; set; } // S? ti?n t?i thi?u (VD: 15,000,000)

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxAmount { get; set; } // S? ti?n t?i ?a (VD: 30,000,000), null = không gi?i h?n

        [Required]
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal CommissionPercentage { get; set; } // T? l? % (VD: 5.00)

        public int TierLevel { get; set; } = 1; // B?c (1, 2, 3, 4...)

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
