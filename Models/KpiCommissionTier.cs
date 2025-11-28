using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class KpiCommissionTier
	{
		public int Id { get; set; }

		[Required]
		public int KpiId { get; set; }
		public KPI? Kpi { get; set; }

		// B?c thang (1, 2, 3, 4...)
		[Required]
		[Range(1, 100)]
		public int TierLevel { get; set; }

		// Doanh s? t?i thi?u
		[Required]
		public decimal MinRevenue { get; set; }

		// Doanh s? t?i ?a (null = không gi?i h?n)
		public decimal? MaxRevenue { get; set; }

		// % hoa h?ng
		[Required]
		[Range(0, 100)]
		public decimal CommissionPercentage { get; set; }

		[StringLength(500)]
		public string? Description { get; set; }

		public bool IsActive { get; set; } = true;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}
