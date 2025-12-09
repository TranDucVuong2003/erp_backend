using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
	// DTO dùng ?? t?o m?i gói KPI
	public class CreateKpiPackageDto
	{
		[Required(ErrorMessage = "Tên gói là b?t bu?c")]
		[StringLength(200, ErrorMessage = "Tên gói không ???c v??t quá 200 ký t?")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "Tháng là b?t bu?c")]
		[Range(1, 12, ErrorMessage = "Tháng ph?i t? 1-12")]
		public int Month { get; set; }

		[Required(ErrorMessage = "N?m là b?t bu?c")]
		[Range(2020, 2100, ErrorMessage = "N?m ph?i t? 2020-2100")]
		public int Year { get; set; }

		[Required(ErrorMessage = "S? ti?n KPI là b?t bu?c")]
		[Range(0, double.MaxValue, ErrorMessage = "S? ti?n KPI ph?i >= 0")]
		public decimal TargetAmount { get; set; }

		[StringLength(1000, ErrorMessage = "Mô t? không ???c v??t quá 1000 ký t?")]
		public string? Description { get; set; }
	}

	// DTO dùng ?? c?p nh?t gói KPI
	public class UpdateKpiPackageDto
	{
		[Required(ErrorMessage = "Tên gói là b?t bu?c")]
		[StringLength(200, ErrorMessage = "Tên gói không ???c v??t quá 200 ký t?")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "Tháng là b?t bu?c")]
		[Range(1, 12, ErrorMessage = "Tháng ph?i t? 1-12")]
		public int Month { get; set; }

		[Required(ErrorMessage = "N?m là b?t bu?c")]
		[Range(2020, 2100, ErrorMessage = "N?m ph?i t? 2020-2100")]
		public int Year { get; set; }

		[Required(ErrorMessage = "S? ti?n KPI là b?t bu?c")]
		[Range(0, double.MaxValue, ErrorMessage = "S? ti?n KPI ph?i >= 0")]
		public decimal TargetAmount { get; set; }

		[StringLength(1000, ErrorMessage = "Mô t? không ???c v??t quá 1000 ký t?")]
		public string? Description { get; set; }

		public bool IsActive { get; set; } = true;
	}

	// DTO dùng ?? gán gói KPI cho danh sách nhân viên
	public class AssignKpiPackageDto
	{
		[Required(ErrorMessage = "ID gói KPI là b?t bu?c")]
		public int KpiPackageId { get; set; }

		[Required(ErrorMessage = "Danh sách User ID là b?t bu?c")]
		[MinLength(1, ErrorMessage = "Ph?i có ít nh?t 1 user")]
		public List<int> UserIds { get; set; } = new List<int>();

		[StringLength(1000, ErrorMessage = "Ghi chú không ???c v??t quá 1000 ký t?")]
		public string? Notes { get; set; }
	}

	// DTO response cho KpiPackage
	public class KpiPackageResponseDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public int Month { get; set; }
		public int Year { get; set; }
		public decimal TargetAmount { get; set; }
		public string? Description { get; set; }
		public bool IsActive { get; set; }
		public int CreatedByUserId { get; set; }
		public string? CreatedByUserName { get; set; }
		public DateTime CreatedAt { get; set; }
		public int AssignedUsersCount { get; set; } // S? user ?ã ???c gán
	}

	// DTO response cho vi?c gán KPI
	public class AssignKpiResultDto
	{
		public int UserId { get; set; }
		public string? UserName { get; set; }
		public string Status { get; set; } = string.Empty; // "Created", "Updated", "Failed"
		public string? Message { get; set; }
	}
}
