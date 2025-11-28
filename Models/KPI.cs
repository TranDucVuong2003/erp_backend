using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class KPI
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Tên KPI là bắt buộc")]
		[StringLength(200)]
		public string Name { get; set; } = string.Empty;

		[StringLength(1000)]
		public string? Description { get; set; }

		// Thay đổi từ string sang foreign key
		[Required(ErrorMessage = "Phòng ban là bắt buộc")]
		public int DepartmentId { get; set; }
		public Departments? Department { get; set; }

		// Loại KPI
		[StringLength(50)]
		public string KpiType { get; set; } = string.Empty; // "Revenue", "Orders", "Leads", "Tickets", "Projects"

		// Đơn vị đo
		[StringLength(20)]
		public string MeasurementUnit { get; set; } = "Number"; // "VND", "%", "Number", "Hours"

		// Mục tiêu
		public decimal TargetValue { get; set; }

		// Công thức tính (JSON string)
		[StringLength(2000)]
		public string? CalculationFormula { get; set; } // JSON: {"type": "sum", "source": "SaleOrders", "field": "Value"}

		// Loại tính hoa hồng
		[StringLength(20)]
		public string CommissionType { get; set; } = "None"; // "None", "Tiered", "Fixed", "Percentage"

		// Kỳ đánh giá
		[StringLength(20)]
		public string Period { get; set; } = "Monthly"; // "Daily", "Weekly", "Monthly", "Quarterly", "Yearly"

		public DateTime StartDate { get; set; } = DateTime.UtcNow;
		public DateTime? EndDate { get; set; }

		// Trọng số
		[Range(0, 100)]
		public int Weight { get; set; } = 100;

		public bool IsActive { get; set; } = true;

		public int? CreatedBy { get; set; }
		public User? Creator { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public ICollection<UserKpiAssignment>? UserKpiAssignments { get; set; }
		public ICollection<KpiRecord>? KpiRecords { get; set; }
		public ICollection<KpiCommissionTier>? CommissionTiers { get; set; }
	}
}
