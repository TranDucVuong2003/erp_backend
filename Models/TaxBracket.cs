using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class TaxBracket
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Thu nh?p t?i thi?u là b?t bu?c")]
		[Range(0, double.MaxValue, ErrorMessage = "Thu nh?p t?i thi?u ph?i l?n h?n ho?c b?ng 0")]
		public decimal MinIncome { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Thu nh?p t?i ?a ph?i l?n h?n ho?c b?ng 0")]
		public decimal? MaxIncome { get; set; } // Nullable ?? h? tr? tr??ng h?p "trên X tri?u"

		[Required(ErrorMessage = "Thu? su?t là b?t bu?c")]
		[Range(0, 100, ErrorMessage = "Thu? su?t ph?i t? 0-100 (ví d?: 5 = 5%)")]
		public float TaxRate { get; set; }

		[StringLength(2000)]
		public string? Notes { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }
	}
}
