namespace erp_backend.Models.DTOs
{
	// DTO cho d? li?u báo cáo l??ng
	public class SalaryReportDto
	{
		public string PayPeriod { get; set; } = string.Empty; // "Tháng 12/2024"
		public string Department { get; set; } = "T?t c? phòng ban";
		public string ReportDate { get; set; } = string.Empty; // "01/01/2025"
		public string CreatedBy { get; set; } = string.Empty;
		public string GeneratedDateTime { get; set; } = string.Empty;

		public List<SalaryReportEmployeeDto> Employees { get; set; } = new();

		public int TotalEmployees { get; set; }
		public decimal TotalBaseSalary { get; set; }
		public decimal TotalAllowance { get; set; }
		public decimal TotalBonus { get; set; }
		public decimal TotalDeduction { get; set; }
		public decimal TotalNetSalary { get; set; }
	}

	// DTO cho t?ng nhân viên trong báo cáo
	public class SalaryReportEmployeeDto
	{
		public string FullName { get; set; } = string.Empty;
		public decimal BaseSalary { get; set; }
		public decimal Allowance { get; set; }
		public decimal Bonus { get; set; }
		public decimal Deduction { get; set; }
		public decimal NetSalary { get; set; }
	}

	// Request DTO
	public class GenerateSalaryReportRequest
	{
		public int Month { get; set; }
		public int Year { get; set; }
		public int? DepartmentId { get; set; } // Null = t?t c? phòng ban
		public string CreatedByName { get; set; } = "Qu?n tr? viên";
	}
}
