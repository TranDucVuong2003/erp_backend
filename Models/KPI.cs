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

		[Required(ErrorMessage = "Phòng ban là bắt buộc")]
		[StringLength(50)]
		public string Department { get; set; } = string.Empty; // "Sale", "IT", "Admin"

		public float TargetValue { get; set; }

		[Range(0, 100, ErrorMessage = "Tỷ lệ mục tiêu phải từ 0-100%")]
		public int TargetPercentage { get; set; } = 0;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}
