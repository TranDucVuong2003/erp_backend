using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class CustomServiceItem
	{
		[StringLength(200)]
		public string ServiceName { get; set; } = string.Empty;

		[Range(0, double.MaxValue, ErrorMessage = "??n giá ph?i l?n h?n ho?c b?ng 0")]
		public decimal UnitPrice { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "S? l??ng ph?i l?n h?n 0")]
		public int Quantity { get; set; } = 1;

		[Range(0, 100, ErrorMessage = "Thu? ph?i t? 0-100%")]
		public decimal Tax { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "T?ng ti?n ph?i l?n h?n ho?c b?ng 0")]
		public decimal Total { get; set; }

		[StringLength(200)]
		public string? RelatedService { get; set; }
	}
}
