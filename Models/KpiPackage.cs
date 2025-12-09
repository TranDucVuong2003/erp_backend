using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
	public class KpiPackage
	{
		public int Id { get; set; }

		[Required]
		[StringLength(200)]
		public string Name { get; set; } = string.Empty; // Tên gói (VD: "KPI Senior T10")

		[Required]
		[Range(1, 12)]
		public int Month { get; set; }

		[Required]
		[Range(2020, 2100)]
		public int Year { get; set; }

		[Required]
		[Range(0, double.MaxValue)]
		[Column(TypeName = "decimal(18,2)")]
		public decimal TargetAmount { get; set; } // Số tiền KPI gốc của gói này

		[StringLength(1000)]
		public string? Description { get; set; }

		public bool IsActive { get; set; } = true;

		public int CreatedByUserId { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation property
		[ForeignKey("CreatedByUserId")]
		public User? CreatedByUser { get; set; }
	}
}