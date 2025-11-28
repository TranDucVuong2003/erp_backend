using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class UserKpiAssignment
	{
		public int Id { get; set; }

		[Required]
		public int UserId { get; set; }
		public User? User { get; set; }

		[Required]
		public int KpiId { get; set; }
		public KPI? Kpi { get; set; }

		// Target riêng cho user này (n?u khác v?i KPI chung)
		public decimal? CustomTargetValue { get; set; }

		// Tr?ng s? KPI cho user (%)
		[Range(0, 100)]
		public int Weight { get; set; } = 100;

		public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		public bool IsActive { get; set; } = true;

		public int? AssignedBy { get; set; }
		public User? Assigner { get; set; }

		[StringLength(1000)]
		public string? Notes { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}
