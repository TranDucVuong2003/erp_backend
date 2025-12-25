using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
	/// <summary>
	/// DTO cho yêu c?u tính l??ng cho 1 nhân viên
	/// </summary>
	public class PayslipCalculateRequest
	{
		[Required(ErrorMessage = "UserId là b?t bu?c")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "Tháng là b?t bu?c")]
		[Range(1, 12, ErrorMessage = "Tháng ph?i t? 1-12")]
		public int Month { get; set; }

		[Required(ErrorMessage = "N?m là b?t bu?c")]
		[Range(2020, 2100, ErrorMessage = "N?m ph?i t? 2020-2100")]
		public int Year { get; set; }

		[Range(0, 1, ErrorMessage = "Thu? su?t ph?i t? 0-1 (ví d?: 0.1 = 10%)")]
		public decimal? TaxRate { get; set; } // M?c ??nh 10% n?u không truy?n
	}

	/// <summary>
	/// DTO cho yêu c?u tính l??ng hàng lo?t cho t?t c? nhân viên có ch?m công trong tháng
	/// </summary>
	public class PayslipCalculateBatchRequest
	{
		[Required(ErrorMessage = "Tháng là b?t bu?c")]
		[Range(1, 12, ErrorMessage = "Tháng ph?i t? 1-12")]
		public int Month { get; set; }

		[Required(ErrorMessage = "N?m là b?t bu?c")]
		[Range(2020, 2100, ErrorMessage = "N?m ph?i t? 2020-2100")]
		public int Year { get; set; }

		[Range(0, 1, ErrorMessage = "Thu? su?t ph?i t? 0-1 (ví d?: 0.1 = 10%)")]
		public decimal? TaxRate { get; set; }
	}
}
