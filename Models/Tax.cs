using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Tax
	{
		public int Id { get; set; }

		[Required]
		[Range(0, 100, ErrorMessage = "Tỷ lệ thuế phải từ 0-100%")]
		public float Rate { get; set; }

		[StringLength(2000)]
		public string? Notes { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }
	}
}
