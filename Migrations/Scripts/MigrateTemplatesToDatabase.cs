using erp_backend.Data;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Migrations.Scripts
{
	public class MigrateTemplatesToDatabase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<MigrateTemplatesToDatabase> _logger;
		private readonly string _templatesPath;

		public MigrateTemplatesToDatabase(
			ApplicationDbContext context,
			ILogger<MigrateTemplatesToDatabase> logger)
		{
			_context = context;
			_logger = logger;
			_templatesPath = Path.Combine(
				Directory.GetCurrentDirectory(),
				"wwwroot",
				"Templates"
			);
		}

		public async Task MigrateAllTemplatesAsync()
		{
			_logger.LogInformation("=== BẮT ĐẦU MIGRATE TEMPLATES ===");

			// Document Templates
			await MigrateQuoteTemplateAsync();
			await MigrateSalaryReportTemplateAsync();
			await MigrateContractIndividualTemplateAsync();
			await MigrateContractBusinessTemplateAsync();

			// Email Templates
			await MigrateEmailAccountCreationTemplateAsync();
			await MigrateEmailPasswordResetOTPTemplateAsync();
			await MigrateEmailNotificationTemplateAsync();
			await MigrateEmailPaymentSuccessTemplateAsync();

			_logger.LogInformation("=== HOÀN THÀNH MIGRATE TEMPLATES ===");
		}

		private async Task MigrateQuoteTemplateAsync()
		{
			const string code = "QUOTE_DEFAULT";

			if (await _context.DocumentTemplates.AnyAsync(t => t.Code == code))
			{
				_logger.LogWarning("Template {Code} đã tồn tại, bỏ qua", code);
				return;
			}

			var filePath = Path.Combine(_templatesPath, "QuoteTemplate.html");
			if (!File.Exists(filePath))
			{
				_logger.LogError("Không tìm thấy file: {FilePath}", filePath);
				return;
			}

			var htmlContent = await File.ReadAllTextAsync(filePath);

			var template = new DocumentTemplate
			{
				Name = "Báo Giá Dịch Vụ (Mặc định)",
				TemplateType = "quote",
				Code = code,
				HtmlContent = htmlContent,
				Description = "Template báo giá dịch vụ mặc định cho khách hàng",
				AvailablePlaceholders = System.Text.Json.JsonSerializer.Serialize(new[]
				{
					"{{ngay_thang_nam}}",
					"{{ten_khach_hang}}",
					"{{dia_chi_khach_hang}}",
					"{{sdt_khach_hang}}",
					"{{email_khach_hang}}",
					"{{nguoi_tao_quote}}",
					"{{email_nguoi_tao}}",
					"{{sdt_nguoi_tao}}",
					"{{chuc_vu_nguoi_tao}}",
					"{{ten_category_service_addon}}",
					"{{SummaryItems}}",
					"{{DetailItems}}",
					"{{tong_thanh_toan}}",
					"{{tong_thanh_toan_chi_tiet}}"
				}),
				IsActive = true,
				IsDefault = true,
				CreatedAt = DateTime.UtcNow
			};

			_context.DocumentTemplates.Add(template);
			await _context.SaveChangesAsync();

			_logger.LogInformation("✔ Đã migrate template: {Code}", code);
		}

		private async Task MigrateSalaryReportTemplateAsync()
		{
			const string code = "SALARY_REPORT_DEFAULT";

			if (await _context.DocumentTemplates.AnyAsync(t => t.Code == code))
			{
				_logger.LogWarning("Template {Code} đã tồn tại, bỏ qua", code);
				return;
			}

			var filePath = Path.Combine(_templatesPath, "SalaryReportTemplate.html");
			if (!File.Exists(filePath))
			{
				_logger.LogError("Không tìm thấy file: {FilePath}", filePath);
				return;
			}

			var htmlContent = await File.ReadAllTextAsync(filePath);

			var template = new DocumentTemplate
			{
				Name = "Báo Cáo Thống Kê Lương (Mặc định)",
				TemplateType = "salary_report",
				Code = code,
				HtmlContent = htmlContent,
				Description = "Template báo cáo thống kê lương tháng",
				AvailablePlaceholders = System.Text.Json.JsonSerializer.Serialize(new[]
				{
					"{{PayPeriod}}",
					"{{ReportDate}}",
					"{{CreatedBy}}",
					"{{EmployeeRows}}",
					"{{TotalEmployees}}",
					"{{TotalBaseSalary}}",
					"{{TotalAllowance}}",
					"{{TotalBonus}}",
					"{{TotalDeduction}}",
					"{{TotalNetSalary}}",
					"{{GeneratedDateTime}}"
				}),
				IsActive = true,
				IsDefault = true,
				CreatedAt = DateTime.UtcNow
			};

			_context.DocumentTemplates.Add(template);
			await _context.SaveChangesAsync();

			_logger.LogInformation("✔ Đã migrate template: {Code}", code);
		}

		private async Task MigrateContractIndividualTemplateAsync()
		{
			const string code = "CONTRACT_INDIVIDUAL";

			if (await _context.DocumentTemplates.AnyAsync(t => t.Code == code))
			{
				_logger.LogWarning("Template {Code} đã tồn tại, bỏ qua", code);
				return;
			}

			var filePath = Path.Combine(_templatesPath, "generate_contract_individual.html");
			if (!File.Exists(filePath))
			{
				_logger.LogError("Không tìm thấy file: {FilePath}", filePath);
				return;
			}

			var htmlContent = await File.ReadAllTextAsync(filePath);

			var template = new DocumentTemplate
			{
				Name = "Hợp đồng Dịch Vụ (Khách hàng Cá nhân)",
				TemplateType = "contract",
				Code = code,
				HtmlContent = htmlContent,
				Description = "Template hợp đồng dành cho khách hàng cá nhân",
				AvailablePlaceholders = System.Text.Json.JsonSerializer.Serialize(new[]
				{
					"{{ContractNumber}}",
					"{{NumberContract}}",
					"{{ContractYear}}",
					"{{Day}}",
					"{{Month}}",
					"{{Year}}",
					"{{ContractDate}}",
					"{{ExpirationDate}}",
					"{{Location}}",
					"{{CompanyBName}}",
					"{{CompanyBAddress}}",
					"{{CompanyBPhone}}",
					"{{CompanyBEmail}}",
					"{{CustomerBirthDay}}",
					"{{CustomerBirthMonth}}",
					"{{CustomerBirthYear}}",
					"{{SubTotal}}",
					"{{TaxAmount}}",
					"{{TotalAmount}}",
					"{{AmountInWords}}",
					"{{PaymentMethod}}",
					"{{Items}}",
					"{{UserName}}",
					"{{UserPosition}}"
				}),
				IsActive = true,
				IsDefault = false,
				CreatedAt = DateTime.UtcNow
			};

			_context.DocumentTemplates.Add(template);
			await _context.SaveChangesAsync();

			_logger.LogInformation("✔ Đã migrate template: {Code}", code);
		}

		private async Task MigrateContractBusinessTemplateAsync()
		{
			const string code = "CONTRACT_BUSINESS";

			if (await _context.DocumentTemplates.AnyAsync(t => t.Code == code))
			{
				_logger.LogWarning("Template {Code} đã tồn tại, bỏ qua", code);
				return;
			}

			var filePath = Path.Combine(_templatesPath, "generate_contract_business.html");
			if (!File.Exists(filePath))
			{
				_logger.LogError("Không tìm thấy file: {FilePath}", filePath);
				return;
			}

			var htmlContent = await File.ReadAllTextAsync(filePath);

			var template = new DocumentTemplate
			{
				Name = "Hợp đồng Dịch Vụ (Khách hàng Doanh nghiệp)",
				TemplateType = "contract",
				Code = code,
				HtmlContent = htmlContent,
				Description = "Template hợp đồng dành cho khách hàng doanh nghiệp",
				AvailablePlaceholders = System.Text.Json.JsonSerializer.Serialize(new[]
				{
					"{{ContractNumber}}",
					"{{NumberContract}}",
					"{{ContractYear}}",
					"{{Day}}",
					"{{Month}}",
					"{{Year}}",
					"{{ContractDate}}",
					"{{ExpirationDate}}",
					"{{Location}}",
					"{{CompanyBName}}",
					"{{CompanyBAddress}}",
					"{{CompanyBTaxCode}}",
					"{{CompanyBRepName}}",
					"{{CompanyBRepPosition}}",
					"{{CompanyBRepID}}",
					"{{CompanyBPhone}}",
					"{{CompanyBEmail}}",
					"{{CompanyBEstablishedDate}}",
					"{{SubTotal}}",
					"{{TaxAmount}}",
					"{{TotalAmount}}",
					"{{AmountInWords}}",
					"{{PaymentMethod}}",
					"{{Items}}",
					"{{UserName}}",
					"{{UserPosition}}"
				}),
				IsActive = true,
				IsDefault = true,
				CreatedAt = DateTime.UtcNow
			};

			_context.DocumentTemplates.Add(template);
			await _context.SaveChangesAsync();

			_logger.LogInformation("✔ Đã migrate template: {Code}", code);
		}

		// ===================== EMAIL TEMPLATES =====================

		private async Task MigrateEmailAccountCreationTemplateAsync()
		{
			const string code = "EMAIL_ACCOUNT_CREATION";

			if (await _context.DocumentTemplates.AnyAsync(t => t.Code == code))
			{
				_logger.LogWarning("Template {Code} đã tồn tại, bỏ qua", code);
				return;
			}

			var filePath = Path.Combine(_templatesPath, "Email_AccountCreation.html");
			if (!File.Exists(filePath))
			{
				_logger.LogError("Không tìm thấy file: {FilePath}", filePath);
				return;
			}

			var htmlContent = await File.ReadAllTextAsync(filePath);

			var template = new DocumentTemplate
			{
				Name = "Email Tạo Tài Khoản",
				TemplateType = "email",
				Code = code,
				HtmlContent = htmlContent,
				Description = "Template email gửi khi tạo tài khoản mới cho nhân viên",
				AvailablePlaceholders = System.Text.Json.JsonSerializer.Serialize(new[]
				{
					"{{UserName}}",
					"{{UserEmail}}",
					"{{PlainPassword}}",
					"{{DepartmentName}}",
					"{{PositionName}}",
					"{{ActivationLink}}",
					"{{CurrentYear}}"
				}),
				IsActive = true,
				IsDefault = true,
				CreatedAt = DateTime.UtcNow
			};

			_context.DocumentTemplates.Add(template);
			await _context.SaveChangesAsync();

			_logger.LogInformation("✔ Đã migrate template: {Code}", code);
		}

		private async Task MigrateEmailPasswordResetOTPTemplateAsync()
		{
			const string code = "EMAIL_PASSWORD_RESET_OTP";

			if (await _context.DocumentTemplates.AnyAsync(t => t.Code == code))
			{
				_logger.LogWarning("Template {Code} đã tồn tại, bỏ qua", code);
				return;
			}

			var filePath = Path.Combine(_templatesPath, "Email_PasswordResetOTP.html");
			if (!File.Exists(filePath))
			{
				_logger.LogError("Không tìm thấy file: {FilePath}", filePath);
				return;
			}

			var htmlContent = await File.ReadAllTextAsync(filePath);

			var template = new DocumentTemplate
			{
				Name = "Email Mã OTP Đổi Mật Khẩu",
				TemplateType = "email",
				Code = code,
				HtmlContent = htmlContent,
				Description = "Template email gửi mã OTP để đổi mật khẩu",
				AvailablePlaceholders = System.Text.Json.JsonSerializer.Serialize(new[]
				{
					"{{UserName}}",
					"{{OtpCode}}",
					"{{ExpiryMinutes}}",
					"{{ExpiresAt}}",
					"{{CurrentYear}}"
				}),
				IsActive = true,
				IsDefault = true,
				CreatedAt = DateTime.UtcNow
			};

			_context.DocumentTemplates.Add(template);
			await _context.SaveChangesAsync();

			_logger.LogInformation("✔ Đã migrate template: {Code}", code);
		}

		private async Task MigrateEmailNotificationTemplateAsync()
		{
			const string code = "EMAIL_NOTIFICATION";

			if (await _context.DocumentTemplates.AnyAsync(t => t.Code == code))
			{
				_logger.LogWarning("Template {Code} đã tồn tại, bỏ qua", code);
				return;
			}

			var filePath = Path.Combine(_templatesPath, "Email_Notification.html");
			if (!File.Exists(filePath))
			{
				_logger.LogError("Không tìm thấy file: {FilePath}", filePath);
				return;
			}

			var htmlContent = await File.ReadAllTextAsync(filePath);

			var template = new DocumentTemplate
			{
				Name = "Email Thông Báo Chung",
				TemplateType = "email",
				Code = code,
				HtmlContent = htmlContent,
				Description = "Template email gửi thông báo chung cho người dùng",
				AvailablePlaceholders = System.Text.Json.JsonSerializer.Serialize(new[]
				{
					"{{RecipientName}}",
					"{{NotificationTitle}}",
					"{{NotificationContent}}",
					"{{CreatedAt}}",
					"{{NotificationUrl}}",
					"{{CurrentYear}}"
				}),
				IsActive = true,
				IsDefault = true,
				CreatedAt = DateTime.UtcNow
			};

			_context.DocumentTemplates.Add(template);
			await _context.SaveChangesAsync();

			_logger.LogInformation("✔ Đã migrate template: {Code}", code);
		}

		private async Task MigrateEmailPaymentSuccessTemplateAsync()
		{
			const string code = "EMAIL_PAYMENT_SUCCESS";

			if (await _context.DocumentTemplates.AnyAsync(t => t.Code == code))
			{
				_logger.LogWarning("Template {Code} đã tồn tại, bỏ qua", code);
				return;
			}

			var filePath = Path.Combine(_templatesPath, "Email_PaymentSuccess.html");
			if (!File.Exists(filePath))
			{
				_logger.LogError("Không tìm thấy file: {FilePath}", filePath);
				return;
			}

			var htmlContent = await File.ReadAllTextAsync(filePath);

			var template = new DocumentTemplate
			{
				Name = "Email Xác Nhận Thanh Toán",
				TemplateType = "email",
				Code = code,
				HtmlContent = htmlContent,
				Description = "Template email xác nhận thanh toán hợp đồng thành công",
				AvailablePlaceholders = System.Text.Json.JsonSerializer.Serialize(new[]
				{
					"{{Greeting}}",
					"{{MainMessage}}",
					"{{ContractNumber}}",
					"{{Amount}}",
					"{{PaymentType}}",
					"{{TransactionId}}",
					"{{TransactionDate}}",
					"{{CustomerInfo}}",
					"{{SaleInfo}}",
					"{{ContractUrl}}",
					"{{CurrentYear}}"
				}),
				IsActive = true,
				IsDefault = true,
				CreatedAt = DateTime.UtcNow
			};

			_context.DocumentTemplates.Add(template);
			await _context.SaveChangesAsync();

			_logger.LogInformation("✔ Đã migrate template: {Code}", code);
		}
	}
}
