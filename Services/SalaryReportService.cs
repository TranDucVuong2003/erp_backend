using erp_backend.Data;
using erp_backend.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace erp_backend.Services
{
	public interface ISalaryReportService
	{
		Task<byte[]> GenerateSalaryReportPdfAsync(GenerateSalaryReportRequest request);
		Task<string> GenerateSalaryReportPreviewHtmlAsync(GenerateSalaryReportRequest request);
	}

	public class SalaryReportService : ISalaryReportService
	{
		private readonly ApplicationDbContext _context;
		private readonly IPdfService _pdfService;
		private readonly IWebHostEnvironment _env;
		private readonly ILogger<SalaryReportService> _logger;

		public SalaryReportService(
			ApplicationDbContext context,
			IPdfService pdfService,
			IWebHostEnvironment env,
			ILogger<SalaryReportService> logger)
		{
			_context = context;
			_pdfService = pdfService;
			_env = env;
			_logger = logger;
		}

		public async Task<byte[]> GenerateSalaryReportPdfAsync(GenerateSalaryReportRequest request)
		{
			try
			{
				// Validate input
				if (request.Month < 1 || request.Month > 12)
					throw new ArgumentException("Tháng ph?i t? 1-12");

				if (request.Year < 2020 || request.Year > 2100)
					throw new ArgumentException("N?m không h?p l?");

				// 1. Build report data from database
				var reportData = await BuildReportDataAsync(request);

				// 2. Generate PDF from HTML template using PuppeteerSharp
				_logger.LogInformation("Using HTML template with PuppeteerSharp for PDF generation");
				var htmlContent = await GenerateHtmlFromTemplateAsync(reportData);
				var pdfBytes = await _pdfService.ConvertHtmlToPdfAsync(htmlContent);

				_logger.LogInformation(
					"?ã t?o báo cáo l??ng PDF cho tháng {Month}/{Year}. Size: {Size} bytes",
					request.Month,
					request.Year,
					pdfBytes.Length
				);

				return pdfBytes;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi t?o báo cáo l??ng PDF tháng {Month}/{Year}", request.Month, request.Year);
				throw;
			}
		}

		public async Task<string> GenerateSalaryReportPreviewHtmlAsync(GenerateSalaryReportRequest request)
		{
			try
			{
				// Validate input
				if (request.Month < 1 || request.Month > 12)
					throw new ArgumentException("Tháng ph?i t? 1-12");

				if (request.Year < 2020 || request.Year > 2100)
					throw new ArgumentException("N?m không h?p l?");

				// 1. Build report data from database
				var reportData = await BuildReportDataAsync(request);

				// 2. Generate HTML from template
				var html = await GenerateHtmlFromTemplateAsync(reportData);

				_logger.LogInformation(
					"?ã t?o HTML preview báo cáo l??ng cho tháng {Month}/{Year}",
					request.Month,
					request.Year
				);

				return html;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi t?o HTML preview báo cáo l??ng tháng {Month}/{Year}", request.Month, request.Year);
				throw;
			}
		}

		private async Task<string> GenerateHtmlFromTemplateAsync(SalaryReportDto data)
		{
			// Load template HTML
			var templatePath = Path.Combine(_env.WebRootPath, "Templates", "SalaryReportTemplate.html");
			if (!File.Exists(templatePath))
			{
				throw new FileNotFoundException($"Template không tìm th?y: {templatePath}");
			}

			var htmlTemplate = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);

			// Replace placeholders
			var htmlContent = ReplaceTemplatePlaceholders(htmlTemplate, data);

			return htmlContent;
		}

		private string ReplaceTemplatePlaceholders(string htmlTemplate, SalaryReportDto data)
		{
			// Replace header info
			var html = htmlTemplate
				.Replace("{{PayPeriod}}", data.PayPeriod)
				.Replace("{{Department}}", data.Department)
				.Replace("{{ReportDate}}", data.ReportDate)
				.Replace("{{CreatedBy}}", data.CreatedBy)
				.Replace("{{GeneratedDateTime}}", data.GeneratedDateTime);

			// Build employee rows
			var rowsBuilder = new StringBuilder();
			int index = 1;

			foreach (var emp in data.Employees)
			{
				rowsBuilder.AppendLine($@"
          <tr>
            <td class=""text-center"">{index}</td>
            <td>{emp.FullName}</td>
            <td class=""text-right number-format"">{FormatCurrency(emp.BaseSalary)}</td>
            <td class=""text-right number-format"">{FormatCurrency(emp.Allowance)}</td>
            <td class=""text-right number-format"">{FormatCurrency(emp.Bonus)}</td>
            <td class=""text-right number-format"">{FormatCurrency(emp.Deduction)}</td>
            <td class=""text-right number-format"">{FormatCurrency(emp.NetSalary)}</td>
          </tr>");
				index++;
			}

			html = html.Replace("{{EmployeeRows}}", rowsBuilder.ToString());

			// Replace summary totals
			html = html
				.Replace("{{TotalEmployees}}", data.TotalEmployees.ToString())
				.Replace("{{TotalBaseSalary}}", FormatCurrency(data.TotalBaseSalary))
				.Replace("{{TotalAllowance}}", FormatCurrency(data.TotalAllowance))
				.Replace("{{TotalBonus}}", FormatCurrency(data.TotalBonus))
				.Replace("{{TotalDeduction}}", FormatCurrency(data.TotalDeduction))
				.Replace("{{TotalNetSalary}}", FormatCurrency(data.TotalNetSalary));

			return html;
		}

		private string FormatCurrency(decimal amount)
		{
			return amount.ToString("#,##0", new CultureInfo("vi-VN"));
		}

		private async Task<SalaryReportDto> BuildReportDataAsync(GenerateSalaryReportRequest request)
		{
			// Load payslips v?i thông tin user
			var query = _context.Payslips
				.Include(p => p.User)
					.ThenInclude(u => u!.Department)
				.Where(p => p.Month == request.Month && p.Year == request.Year);

			// Filter theo phòng ban n?u có
			if (request.DepartmentId.HasValue)
			{
				query = query.Where(p => p.User!.DepartmentId == request.DepartmentId.Value);
			}

			var payslips = await query.OrderBy(p => p.User!.Name).ToListAsync();

			if (payslips.Count == 0)
			{
				throw new InvalidOperationException($"Không có dữ liệu lương tháng {request.Month}/{request.Year}");
			}

			// Load SalaryContracts ?? l?y BaseSalary
			var userIds = payslips.Select(p => p.UserId).Distinct().ToList();
			var contracts = await _context.SalaryContracts
				.Where(c => userIds.Contains(c.UserId))
				.ToDictionaryAsync(c => c.UserId, c => c);

			// Load SalaryComponents ?? tính ph? c?p, th??ng, kh?u tr?
			var components = await _context.SalaryComponents
				.Where(c => userIds.Contains(c.UserId)
					&& c.Month == request.Month
					&& c.Year == request.Year)
				.ToListAsync();

			var componentsByUser = components
				.GroupBy(c => c.UserId)
				.ToDictionary(
					g => g.Key,
					g => g.ToList()
				);

			// Build employee list
			var employees = new List<SalaryReportEmployeeDto>();

			foreach (var payslip in payslips)
			{
				var baseSalary = contracts.ContainsKey(payslip.UserId)
					? contracts[payslip.UserId].BaseSalary
					: 0;

				var userComponents = componentsByUser.ContainsKey(payslip.UserId)
					? componentsByUser[payslip.UserId]
					: new List<Models.SalaryComponent>();

				// Tính Ph? c?p (các kho?n IN lo?i Allowance)
				var allowance = userComponents
					.Where(c => c.Type.ToLower() == "in" && c.Reason.ToLower().Contains("ph? c?p"))
					.Sum(c => c.Amount);

				// Tính Th??ng (các kho?n IN không ph?i ph? c?p)
				var bonus = userComponents
					.Where(c => c.Type.ToLower() == "in" && !c.Reason.ToLower().Contains("ph? c?p"))
					.Sum(c => c.Amount);

				// Tính Kh?u tr? (các kho?n OUT + b?o hi?m + thu?)
				var deduction = userComponents
					.Where(c => c.Type.ToLower() == "out")
					.Sum(c => c.Amount)
					+ payslip.InsuranceDeduction
					+ payslip.TaxAmount;

				employees.Add(new SalaryReportEmployeeDto
				{
					FullName = payslip.User?.Name ?? "N/A",
					BaseSalary = baseSalary,
					Allowance = allowance,
					Bonus = bonus,
					Deduction = deduction,
					NetSalary = payslip.NetSalary
				});
			}

			// L?y tên phòng ban
			string departmentName = "T?t c? phòng ban";
			if (request.DepartmentId.HasValue)
			{
				var dept = await _context.Departments.FindAsync(request.DepartmentId.Value);
				departmentName = dept?.Name ?? "N/A";
			}

			// Build report DTO
			var report = new SalaryReportDto
			{
				PayPeriod = $"Tháng {request.Month:00}/{request.Year}",
				Department = departmentName,
				ReportDate = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
				CreatedBy = request.CreatedByName,
				GeneratedDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
				Employees = employees,
				TotalEmployees = employees.Count,
				TotalBaseSalary = employees.Sum(e => e.BaseSalary),
				TotalAllowance = employees.Sum(e => e.Allowance),
				TotalBonus = employees.Sum(e => e.Bonus),
				TotalDeduction = employees.Sum(e => e.Deduction),
				TotalNetSalary = employees.Sum(e => e.NetSalary)
			};

			return report;
		}
	}
}
