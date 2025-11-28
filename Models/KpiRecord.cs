using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class KpiRecord
	{
		public int Id { get; set; }

		[Required]
		public int KpiId { get; set; }
		public KPI? Kpi { get; set; }

		public int UserId { get; set; }
		public User? User { get; set; }

		[Required]
		public decimal ActualValue { get; set; } 

		public decimal TargetValue { get; set; }

		public decimal AchievementPercentage { get; set; }

		// ===== THÔNG TIN CHO SALES =====
		public decimal? CommissionAmount { get; set; }
		public decimal? CommissionPercentage { get; set; }
		public int? CommissionTierLevel { get; set; }

		// ===== THÔNG TIN CHO IT =====
		public int? TotalTickets { get; set; }
		public int? CompletedTickets { get; set; }
		public decimal? AverageResolutionTime { get; set; } // Hours

		// ===== THÔNG TIN CHO MARKETING =====
		// Leads
		public int? TotalLeads { get; set; }
		public int? QualifiedLeads { get; set; }
		public int? ConvertedLeads { get; set; }
		public decimal? LeadConversionRate { get; set; }
		public decimal? LeadsScore { get; set; } // 0-100

		// Budget
		public decimal? ApprovedBudget { get; set; }
		public decimal? ActualSpending { get; set; }
		public decimal? BudgetUsagePercentage { get; set; }
		public bool? IsOverBudget { get; set; }
		public decimal? BudgetScore { get; set; } // 0-100

		// Tổng điểm Marketing (50% Leads + 50% Budget)
		public decimal? MarketingTotalScore { get; set; }

		// Cost metrics
		public decimal? CostPerLead { get; set; }
		public decimal? CostPerConversion { get; set; }
		public decimal? ROI { get; set; }

		[Required]
		[StringLength(20)]
		public string Period { get; set; } = string.Empty;

		[Required]
		public DateTime RecordDate { get; set; } = DateTime.UtcNow;

		[StringLength(20)]
		public string Status { get; set; } = "Pending";

		[StringLength(2000)]
		public string? Notes { get; set; }

		public int? ApprovedBy { get; set; }
		public User? Approver { get; set; }

		public DateTime? ApprovedAt { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}
