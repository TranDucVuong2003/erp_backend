using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
	/// <summary>
	/// DTO cho m?t b?n ch?m công trong request batch
	/// </summary>
	public class MonthlyAttendanceItem
	{
		[Required(ErrorMessage = "UserId là b?t bu?c")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "S? ngày công là b?t bu?c")]
		[Range(0, 31, ErrorMessage = "S? ngày công ph?i t? 0-31")]
		public float ActualWorkDays { get; set; }
	}

	/// <summary>
	/// DTO cho yêu c?u t?o ch?m công hàng lo?t
	/// </summary>
	public class MonthlyAttendanceBatchRequest
	{
		[Required(ErrorMessage = "Tháng là b?t bu?c")]
		[Range(1, 12, ErrorMessage = "Tháng ph?i t? 1-12")]
		public int Month { get; set; }

		[Required(ErrorMessage = "N?m là b?t bu?c")]
		[Range(2020, 2100, ErrorMessage = "N?m ph?i t? 2020-2100")]
		public int Year { get; set; }

		[Required(ErrorMessage = "Danh sách ch?m công là b?t bu?c")]
		[MinLength(1, ErrorMessage = "Ph?i có ít nh?t 1 b?n ch?m công")]
		public List<MonthlyAttendanceItem> Attendances { get; set; } = new();
	}
}
