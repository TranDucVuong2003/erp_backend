using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
	public class SaleKpiTarget
	{
		public int Id { get; set; }

		[Required]
		public int UserId { get; set; } // Sale được gán

		// --- PHẦN THÊM MỚI ---
		[Required]
		public int KpiPackageId { get; set; } // Khóa ngoại trỏ về bảng MonthlyKpiPackage
											  // ---------------------

		[Required]
		[Range(1, 12)]
		public int Month { get; set; }

		[Required]
		[Range(2020, 2100)]
		public int Year { get; set; }

		// VẪN GIỮ LẠI CỘT NÀY (Để snapshot giá trị tiền tại thời điểm gán)
		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal TargetAmount { get; set; }

		[Required]
		public int AssignedByUserId { get; set; }

		public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

		[StringLength(1000)]
		public string? Notes { get; set; }

		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public User? SaleUser { get; set; }
		public User? AssignedByUser { get; set; }

		// --- NAVIGATION MỚI ---
		[ForeignKey("KpiPackageId")]
		public KpiPackage? KpiPackage { get; set; }
	}
}