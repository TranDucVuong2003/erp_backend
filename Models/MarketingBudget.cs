using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class MarketingBudget
	{
		public int Id { get; set; }

		[Required]
		public int UserId { get; set; }
		public User? User { get; set; }

		// Tháng/n?m (2024-11)
		[Required]
		[StringLength(7)]
		public string Period { get; set; } = string.Empty;

		// Ngân sách ???c phê duy?t
		[Required]
		[Range(0, double.MaxValue)]
		public decimal ApprovedBudget { get; set; }

		// Chi phí th?c t?
		[Range(0, double.MaxValue)]
		public decimal ActualSpending { get; set; } = 0;

		// % ?ã s? d?ng
		public decimal UsagePercentage { get; set; } = 0;

		// V??t ngân sách?
		public bool IsOverBudget { get; set; } = false;

		// S? ti?n v??t
		public decimal OverBudgetAmount { get; set; } = 0;

		// ? THÊM M?I: Target ROI (m?c ??nh 200%)
		[Range(0, 10000)]
		public decimal TargetROI { get; set; } = 200;

		// ? THÊM M?I: Actual ROI (tính t? ??ng)
		public decimal ActualROI { get; set; } = 0;

		[StringLength(1000)]
		public string? Notes { get; set; }

		[StringLength(20)]
		public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"

		public int? ApprovedBy { get; set; }
		public User? Approver { get; set; }

		public DateTime? ApprovedAt { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }

		// Navigation
		public ICollection<MarketingExpense>? Expenses { get; set; }
	}
}
