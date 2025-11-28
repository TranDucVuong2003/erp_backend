using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class MarketingExpense
	{
		public int Id { get; set; }

		[Required]
		public int MarketingBudgetId { get; set; }
		public MarketingBudget? MarketingBudget { get; set; }

		[Required]
		public int UserId { get; set; }
		public User? User { get; set; }

		// Lo?i chi phí
		[Required]
		[StringLength(50)]
		public string ExpenseType { get; set; } = string.Empty; // "GoogleAds", "FacebookAds", "SEO", "Content", "Other"

		[Required]
		[StringLength(200)]
		public string Description { get; set; } = string.Empty;

		[Required]
		[Range(0, double.MaxValue)]
		public decimal Amount { get; set; }

		// Ngày chi
		[Required]
		public DateTime ExpenseDate { get; set; }

		// K?t qu? mang l?i
		public int? LeadsGenerated { get; set; } = 0;
		public decimal? RevenueGenerated { get; set; } = 0;

		// Chi phí m?i lead
		public decimal? CostPerLead { get; set; } = 0;

		[StringLength(500)]
		public string? InvoiceNumber { get; set; }

		[StringLength(1000)]
		public string? AttachmentUrl { get; set; }

		[StringLength(1000)]
		public string? Notes { get; set; }

		[StringLength(20)]
		public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"

		public int? ApprovedBy { get; set; }
		public User? Approver { get; set; }

		public DateTime? ApprovedAt { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}
