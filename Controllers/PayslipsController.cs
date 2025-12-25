using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	//[Authorize]
	public class PayslipsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<PayslipsController> _logger;

		public PayslipsController(ApplicationDbContext context, ILogger<PayslipsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// Helper method: Tính số công chuẩn (C) - trừ Thứ 7 và Chủ nhật
		private int CalculateStandardWorkDays(int month, int year)
		{
			int standardDays = 0;
			int daysInMonth = DateTime.DaysInMonth(year, month);

			for (int day = 1; day <= daysInMonth; day++)
			{
				var date = new DateTime(year, month, day);
				// Nếu không phải Thứ 7 (Saturday) hoặc Chủ nhật (Sunday)
				if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
				{
					standardDays++;
				}
			}

			return standardDays;
		}

		// Helper method: Lấy giá trị PayrollConfig an toàn
		private decimal GetPayrollConfigValue(Dictionary<string, string> configs, string key, string defaultValue, bool isRequired = false)
		{
			if (configs.TryGetValue(key, out var value))
			{
				if (decimal.TryParse(value, out var result))
				{
					_logger.LogDebug("Loaded config {Key} = {Value} from database", key, result);
					return result;
				}
				else
				{
					_logger.LogWarning("Invalid decimal value for config {Key}: {Value}. Using default: {DefaultValue}", 
						key, value, defaultValue);
				}
			}
			else if (isRequired)
			{
				_logger.LogError("Required config {Key} not found in PayrollConfig table. Using default: {DefaultValue}", 
					key, defaultValue);
			}
			else
			{
				_logger.LogWarning("Config {Key} not found in PayrollConfig table. Using default: {DefaultValue}", 
					key, defaultValue);
			}

			return decimal.Parse(defaultValue);
		}

		// POST: api/Payslips/calculate
		// API tự động tính lương cho user trong tháng
		[HttpPost("calculate")]
		[Authorize]
		public async Task<ActionResult<object>> CalculatePayslip([FromBody] PayslipCalculateRequest request)
		{
			try
			{
				// --- BƯỚC 1: LẤY DỮ LIỆU CẦN THIẾT ---

				// 1.1 Lấy Hợp đồng (SalaryContract)
				var contract = await _context.SalaryContracts
					.FirstOrDefaultAsync(s => s.UserId == request.UserId);
				if (contract == null) 
					return BadRequest(new { message = "Nhân viên chưa được cấu hình lương" });

				// 1.2 Lấy Chấm công
				var attendance = await _context.MonthlyAttendances
					.FirstOrDefaultAsync(a => a.UserId == request.UserId 
						&& a.Month == request.Month 
						&& a.Year == request.Year);
				if (attendance == null) 
					return BadRequest(new { message = $"Chưa có chấm công tháng {request.Month}/{request.Year}" });

				// 1.3 Lấy Cấu hình (PayrollConfig) - ✅ ĐÃ CẢI THIỆN
				var configs = await _context.PayrollConfigs.ToDictionaryAsync(k => k.Key, v => v.Value);
				
				// Load các config với validation
				decimal personalDeduction = GetPayrollConfigValue(configs, "PERSONAL_DEDUCTION", "0", isRequired: true);
				decimal dependentDeduction = GetPayrollConfigValue(configs, "DEPENDENT_DEDUCTION", "0", isRequired: true);
				decimal govBaseSalary = GetPayrollConfigValue(configs, "GOV_BASE_SALARY", "2340000", isRequired: true);
				decimal regionMinWage = GetPayrollConfigValue(configs, "MIN_WAGE_REGION_1_2026", "5310000", isRequired: true);

				// --- BƯỚC 2: TÍNH CÔNG & THU NHẬP GROSS ---

				int standardDays = CalculateStandardWorkDays(request.Month, request.Year);
				decimal salaryByWorkDays = (contract.BaseSalary / standardDays) * (decimal)attendance.ActualWorkDays;

				// Lấy Thưởng/Phạt
				var components = await _context.SalaryComponents
					.Where(c => c.UserId == request.UserId 
						&& c.Month == request.Month 
						&& c.Year == request.Year)
					.ToListAsync();
				decimal totalBonus = components.Where(c => c.Type.ToLower() == "in").Sum(c => c.Amount);
				decimal totalPenalty = components.Where(c => c.Type.ToLower() == "out").Sum(c => c.Amount);

				decimal grossIncome = salaryByWorkDays + totalBonus - totalPenalty;

				// --- BƯỚC 3: TÍNH BẢO HIỂM (Logic Trần/Sàn) ---

				decimal totalInsuranceDeduction = 0;

				if (contract.ContractType == "OFFICIAL")
				{
					var policies = await _context.InsurancePolicy.ToListAsync();
					decimal insuranceSalary = contract.InsuranceSalary; // Lấy từ Hợp đồng

					foreach (var policy in policies)
					{
						decimal salaryForCalc = insuranceSalary;
						decimal maxCap = 0;

						// Check Trần (Capping)
						if (policy.CapBaseType == "GOV_BASE") 
							maxCap = govBaseSalary * 20;
						else if (policy.CapBaseType == "REGION_MIN") 
							maxCap = regionMinWage * 20;

						if (salaryForCalc > maxCap && maxCap > 0) 
							salaryForCalc = maxCap;

						// Cộng dồn tiền nhân viên phải đóng
						totalInsuranceDeduction += salaryForCalc * ((decimal)policy.EmployeeRate / 100);
					}
				}

				// --- BƯỚC 4: TÍNH THUẾ TNCN (Logic Lũy tiến / Vẳng lai) ---

				decimal taxAmount = 0;
				decimal taxableIncome = grossIncome - totalInsuranceDeduction; // Thu nhập chịu thuế
				decimal familyDeduction = 0;
				decimal assessableIncome = 0; // Thu nhập tính thuế

				if (contract.ContractType == "OFFICIAL")
				{
					// Case 1: Nhân viên chính thức (Lũy tiến)
					familyDeduction = personalDeduction + (contract.DependentsCount * dependentDeduction);
					assessableIncome = taxableIncome - familyDeduction;

					if (assessableIncome > 0)
					{
						// Gọi Logic tính thuế 5 bậc (Lấy từ TaxBrackets)
						var brackets = await _context.TaxBrackets.OrderBy(t => t.MinIncome).ToListAsync();

						foreach (var bracket in brackets)
						{
							decimal taxableInBracket = 0;

							if (bracket.MaxIncome.HasValue)
							{
								// Bậc có giới hạn trên
								if (assessableIncome > bracket.MinIncome)
								{
									decimal maxInBracket = Math.Min(assessableIncome, bracket.MaxIncome.Value);
									taxableInBracket = maxInBracket - bracket.MinIncome;
								}
							}
							else
							{
								// Bậc cao nhất (không giới hạn)
								if (assessableIncome > bracket.MinIncome)
									taxableInBracket = assessableIncome - bracket.MinIncome;
							}

							if (taxableInBracket > 0)
								taxAmount += taxableInBracket * ((decimal)bracket.TaxRate / 100);
						}
					}
				}
				else
				{
					// Case 2: Vãng lai / Thử việc (10% phẳng)
					// Kiểm tra Cam kết 08
					if (!contract.HasCommitment08)
					{
						decimal threshold = decimal.Parse(configs.GetValueOrDefault("FLAT_TAX_THRESHOLD", "2000000"));
						if (grossIncome >= threshold)
						{
							taxAmount = grossIncome * 0.10m; // 10%
						}
					}
				}

				// --- BƯỚC 5: TÍNH THỰC LĨNH & LƯU DB ---

				decimal netSalary = grossIncome - totalInsuranceDeduction - taxAmount;

				// Lưu vào Payslip Model (UPSERT)
				var existingPayslip = await _context.Payslips
					.FirstOrDefaultAsync(p => p.UserId == request.UserId 
						&& p.Month == request.Month 
						&& p.Year == request.Year);

				Payslip payslip;
				bool isUpdate = false;

				if (existingPayslip != null)
				{
					// UPDATE
					existingPayslip.StandardWorkDays = standardDays;
					existingPayslip.GrossSalary = Math.Round(grossIncome, 2);
					existingPayslip.InsuranceDeduction = Math.Round(totalInsuranceDeduction, 2);
					existingPayslip.FamilyDeduction = Math.Round(familyDeduction, 2);
					existingPayslip.AssessableIncome = Math.Round(assessableIncome, 2);
					existingPayslip.TaxAmount = Math.Round(taxAmount, 2);
					existingPayslip.NetSalary = Math.Round(netSalary, 2);
					existingPayslip.UpdatedAt = DateTime.UtcNow;

					payslip = existingPayslip;
					isUpdate = true;
				}
				else
				{
					// INSERT
					payslip = new Payslip
					{
						UserId = request.UserId,
						Month = request.Month,
						Year = request.Year,
						StandardWorkDays = standardDays,
						GrossSalary = Math.Round(grossIncome, 2),
						InsuranceDeduction = Math.Round(totalInsuranceDeduction, 2),
						FamilyDeduction = Math.Round(familyDeduction, 2),
						AssessableIncome = Math.Round(assessableIncome, 2),
						TaxAmount = Math.Round(taxAmount, 2),
						NetSalary = Math.Round(netSalary, 2),
						Status = "DRAFT",
						CreatedAt = DateTime.UtcNow
					};

					_context.Payslips.Add(payslip);
				}

				await _context.SaveChangesAsync();

				_logger.LogInformation(
					"{Action} phiếu lương cho User ID: {UserId} tháng {Month}/{Year}. NetSalary: {NetSalary}",
					isUpdate ? "Cập nhật" : "Tạo mới",
					request.UserId,
					request.Month,
					request.Year,
					netSalary
				);

				// Load thông tin đầy đủ để trả về
				var result = await _context.Payslips
					.Include(p => p.User)
					.Where(p => p.Id == payslip.Id)
					.Select(p => new
					{
						p.Id,
						p.UserId,
						UserName = p.User != null ? p.User.Name : null,
						UserEmail = p.User != null ? p.User.Email : null,
						p.Month,
						p.Year,
						p.StandardWorkDays,
						p.GrossSalary,
						p.InsuranceDeduction,
						p.FamilyDeduction,
						p.AssessableIncome,
						p.TaxAmount,
						p.NetSalary,
						p.Status,
						p.CreatedAt,
						p.UpdatedAt,
						p.PaidAt
					})
					.FirstOrDefaultAsync();

				return Ok(new
				{
					message = isUpdate ? "Tính lại và cập nhật phiếu lương thành công" : "Tính lương và tạo phiếu lương thành công",
					calculation = new
					{
						step1_standardWorkDays = standardDays,
						step2_baseSalary = contract.BaseSalary,
						step2_actualWorkDays = attendance.ActualWorkDays,
						step3_salaryByWorkDays = Math.Round(salaryByWorkDays, 2),
						step4_totalBonus = totalBonus,
						step4_totalPenalty = totalPenalty,
						step4_grossIncome = Math.Round(grossIncome, 2),
						step5_insuranceDeduction = Math.Round(totalInsuranceDeduction, 2),
						step6_familyDeduction = Math.Round(familyDeduction, 2),
						step6_assessableIncome = Math.Round(assessableIncome, 2),
						step7_taxAmount = Math.Round(taxAmount, 2),
						step8_netSalary = Math.Round(netSalary, 2)
					},
					payslip = result
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tính lương cho User ID: {UserId} tháng {Month}/{Year}", 
					request.UserId, request.Month, request.Year);
				return StatusCode(500, new { message = "Lỗi server khi tính lương", error = ex.Message });
			}
		}

		// POST: api/Payslips/calculate-batch
		// API tính lương hàng loạt cho tất cả user có chấm công trong tháng
		[HttpPost("calculate-batch")]
		public async Task<ActionResult<object>> CalculatePayslipBatch([FromBody] PayslipCalculateBatchRequest request)
		{
			try
			{
				// Validate input
				if (request.Month < 1 || request.Month > 12)
				{
					return BadRequest(new { message = "Tháng phải từ 1-12" });
				}

				if (request.Year < 2020 || request.Year > 2100)
				{
					return BadRequest(new { message = "Năm không hợp lệ" });
				}

				// Lấy danh sách user có chấm công trong tháng
				var attendances = await _context.MonthlyAttendances
					.Where(a => a.Month == request.Month && a.Year == request.Year)
					.ToListAsync();

				if (attendances.Count == 0)
				{
					return BadRequest(new { message = $"Không có dữ liệu chấm công nào trong tháng {request.Month}/{request.Year}" });
				}

				// Load cấu hình 1 lần cho tất cả
				var configs = await _context.PayrollConfigs.ToDictionaryAsync(k => k.Key, v => v.Value);
				decimal personalDeduction = GetPayrollConfigValue(configs, "PERSONAL_DEDUCTION", "0", isRequired: true);
				decimal dependentDeduction = GetPayrollConfigValue(configs, "DEPENDENT_DEDUCTION", "0", isRequired: true);
				decimal govBaseSalary = GetPayrollConfigValue(configs, "GOV_BASE_SALARY", "2340000", isRequired: true);
				decimal regionMinWage = GetPayrollConfigValue(configs, "MIN_WAGE_REGION_1_2026", "5310000", isRequired: true);
				decimal flatTaxThreshold = GetPayrollConfigValue(configs, "FLAT_TAX_THRESHOLD", "2000000");

				// Load các bảng thuế và bảo hiểm 1 lần
				var policies = await _context.InsurancePolicy.ToListAsync();
				var brackets = await _context.TaxBrackets.OrderBy(t => t.MinIncome).ToListAsync();

				int standardDays = CalculateStandardWorkDays(request.Month, request.Year);

				var results = new List<object>();
				var errors = new List<object>();

				foreach (var attendance in attendances)
				{
					try
					{
						// 1. Lấy Hợp đồng
						var contract = await _context.SalaryContracts
							.FirstOrDefaultAsync(s => s.UserId == attendance.UserId);
						
						if (contract == null)
						{
							errors.Add(new
							{
								userId = attendance.UserId,
								error = "Chưa có cấu hình hợp đồng lương"
							});
							continue;
						}

						// 2. Tính lương theo công
						decimal salaryByWorkDays = (contract.BaseSalary / standardDays) * (decimal)attendance.ActualWorkDays;

						// 3. Lấy thưởng phạt
						var components = await _context.SalaryComponents
							.Where(c => c.UserId == attendance.UserId 
								&& c.Month == request.Month 
								&& c.Year == request.Year)
							.ToListAsync();

						decimal totalBonus = components.Where(c => c.Type.ToLower() == "in").Sum(c => c.Amount);
						decimal totalPenalty = components.Where(c => c.Type.ToLower() == "out").Sum(c => c.Amount);
						decimal grossIncome = salaryByWorkDays + totalBonus - totalPenalty;

						// 4. Tính bảo hiểm
						decimal totalInsuranceDeduction = 0;

						if (contract.ContractType == "OFFICIAL")
						{
							decimal insuranceSalary = contract.InsuranceSalary;

							foreach (var policy in policies)
							{
								decimal salaryForCalc = insuranceSalary;
								decimal maxCap = 0;

								if (policy.CapBaseType == "GOV_BASE")
									maxCap = govBaseSalary * 20;
								else if (policy.CapBaseType == "REGION_MIN")
									maxCap = regionMinWage * 20;

								if (salaryForCalc > maxCap && maxCap > 0)
									salaryForCalc = maxCap;

								totalInsuranceDeduction += salaryForCalc * ((decimal)policy.EmployeeRate / 100);
							}
						}

						// 5. Tính thuế
						decimal taxAmount = 0;
						decimal taxableIncome = grossIncome - totalInsuranceDeduction;
						decimal familyDeduction = 0;
						decimal assessableIncome = 0;

						if (contract.ContractType == "OFFICIAL")
						{
							familyDeduction = personalDeduction + (contract.DependentsCount * dependentDeduction);
							assessableIncome = taxableIncome - familyDeduction;

							if (assessableIncome > 0)
							{
								foreach (var bracket in brackets)
								{
									decimal taxableInBracket = 0;

									if (bracket.MaxIncome.HasValue)
									{
										if (assessableIncome > bracket.MinIncome)
										{
											decimal maxInBracket = Math.Min(assessableIncome, bracket.MaxIncome.Value);
											taxableInBracket = maxInBracket - bracket.MinIncome;
										}
									}
									else
									{
										if (assessableIncome > bracket.MinIncome)
											taxableInBracket = assessableIncome - bracket.MinIncome;
									}

									if (taxableInBracket > 0)
										taxAmount += taxableInBracket * ((decimal)bracket.TaxRate / 100);
								}
							}
						}
						else
						{
							if (!contract.HasCommitment08 && grossIncome >= flatTaxThreshold)
							{
								taxAmount = grossIncome * 0.10m;
							}
						}

						// 6. Tính thực lĩnh
						decimal netSalary = grossIncome - totalInsuranceDeduction - taxAmount;

						// 7. UPSERT
						var existingPayslip = await _context.Payslips
							.FirstOrDefaultAsync(p => p.UserId == attendance.UserId 
								&& p.Month == request.Month 
								&& p.Year == request.Year);

						bool isUpdate = false;

						if (existingPayslip != null)
						{
							existingPayslip.StandardWorkDays = standardDays;
							existingPayslip.GrossSalary = Math.Round(grossIncome, 2);
							existingPayslip.InsuranceDeduction = Math.Round(totalInsuranceDeduction, 2);
							existingPayslip.FamilyDeduction = Math.Round(familyDeduction, 2);
							existingPayslip.AssessableIncome = Math.Round(assessableIncome, 2);
							existingPayslip.TaxAmount = Math.Round(taxAmount, 2);
							existingPayslip.NetSalary = Math.Round(netSalary, 2);
							existingPayslip.UpdatedAt = DateTime.UtcNow;
							isUpdate = true;
						}
						else
						{
							var newPayslip = new Payslip
							{
								UserId = attendance.UserId,
								Month = request.Month,
								Year = request.Year,
								StandardWorkDays = standardDays,
								GrossSalary = Math.Round(grossIncome, 2),
								InsuranceDeduction = Math.Round(totalInsuranceDeduction, 2),
								FamilyDeduction = Math.Round(familyDeduction, 2),
								AssessableIncome = Math.Round(assessableIncome, 2),
								TaxAmount = Math.Round(taxAmount, 2),
								NetSalary = Math.Round(netSalary, 2),
								Status = "DRAFT",
								CreatedAt = DateTime.UtcNow
							};
							_context.Payslips.Add(newPayslip);
						}

						await _context.SaveChangesAsync();

						results.Add(new
						{
							userId = attendance.UserId,
							netSalary = Math.Round(netSalary, 2),
							grossSalary = Math.Round(grossIncome, 2),
							insuranceDeduction = Math.Round(totalInsuranceDeduction, 2),
							taxAmount = Math.Round(taxAmount, 2),
							action = isUpdate ? "updated" : "created"
						});
					}
					catch (Exception ex)
					{
						errors.Add(new
						{
							userId = attendance.UserId,
							error = ex.Message
						});
					}
				}

				_logger.LogInformation(
					"Tính lương hàng loạt tháng {Month}/{Year}: {SuccessCount} thành công, {ErrorCount} lỗi",
					request.Month,
					request.Year,
					results.Count,
					errors.Count
				);

				return Ok(new
				{
					message = $"Tính lương hàng loạt hoàn tất",
					month = request.Month,
					year = request.Year,
					summary = new
					{
						total = attendances.Count,
						success = results.Count,
						failed = errors.Count
					},
					results,
					errors
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tính lương hàng loạt tháng {Month}/{Year}", request.Month, request.Year);
				return StatusCode(500, new { message = "Lỗi server khi tính lương hàng loạt", error = ex.Message });
			}
		}

		// GET: api/Payslips
		[HttpGet]
		public async Task<ActionResult<IEnumerable<object>>> GetPayslips()
		{
			try
			{
				var payslips = await _context.Payslips
					.Include(p => p.User)
					.Select(p => new
					{
						p.Id,
						p.UserId,
						UserName = p.User != null ? p.User.Name : null,
						UserEmail = p.User != null ? p.User.Email : null,
						p.Month,
						p.Year,
						p.StandardWorkDays,
						p.GrossSalary,
						p.InsuranceDeduction,
						p.FamilyDeduction,
						p.AssessableIncome,
						p.TaxAmount,
						p.NetSalary,
						p.Status,
						p.CreatedAt,
						p.UpdatedAt,
						p.PaidAt
					})
					.OrderByDescending(p => p.Year)
					.ThenByDescending(p => p.Month)
					.ThenBy(p => p.UserName)
					.ToListAsync();

				return Ok(payslips);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách phiếu lương");
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách phiếu lương", error = ex.Message });
			}
		}

		// GET: api/Payslips/5
		[HttpGet("{id}")]
		public async Task<ActionResult<object>> GetPayslip(int id)
		{
			try
			{
				var payslip = await _context.Payslips
					.Include(p => p.User)
					.Where(p => p.Id == id)
					.Select(p => new
					{
						p.Id,
						p.UserId,
						UserName = p.User != null ? p.User.Name : null,
						UserEmail = p.User != null ? p.User.Email : null,
						p.Month,
						p.Year,
						p.StandardWorkDays,
						p.GrossSalary,
						p.InsuranceDeduction,
						p.FamilyDeduction,
						p.AssessableIncome,
						p.TaxAmount,
						p.NetSalary,
						p.Status,
						p.CreatedAt,
						p.UpdatedAt,
						p.PaidAt
					})
					.FirstOrDefaultAsync();

				if (payslip == null)
				{
					return NotFound(new { message = "Không tìm thấy phiếu lương" });
				}

				return Ok(payslip);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy thông tin phiếu lương với ID: {PayslipId}", id);
				return StatusCode(500, new { message = "Lỗi server khi lấy thông tin phiếu lương", error = ex.Message });
			}
		}

		// GET: api/Payslips/user/5
		[HttpGet("user/{userId}")]
		public async Task<ActionResult<IEnumerable<object>>> GetPayslipsByUserId(int userId)
		{
			try
			{
				var payslips = await _context.Payslips
					.Include(p => p.User)
					.Where(p => p.UserId == userId)
					.Select(p => new
					{
						p.Id,
						p.UserId,
						UserName = p.User != null ? p.User.Name : null,
						p.Month,
						p.Year,
						p.StandardWorkDays,
						p.GrossSalary,
						p.InsuranceDeduction,
						p.FamilyDeduction,
						p.AssessableIncome,
						p.TaxAmount,
						p.NetSalary,
						p.Status,
						p.CreatedAt,
						p.PaidAt
					})
					.OrderByDescending(p => p.Year)
					.ThenByDescending(p => p.Month)
					.ToListAsync();

				return Ok(payslips);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách phiếu lương của user ID: {UserId}", userId);
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách phiếu lương", error = ex.Message });
			}
		}

		// GET: api/Payslips/user/5/month/12/year/2024
		[HttpGet("user/{userId}/month/{month}/year/{year}")]
		public async Task<ActionResult<object>> GetPayslipByUserAndPeriod(int userId, int month, int year)
		{
			try
			{
				if (month < 1 || month > 12)
				{
					return BadRequest(new { message = "Tháng phải từ 1-12" });
				}

				if (year < 2020 || year > 2100)
				{
					return BadRequest(new { message = "Năm không hợp lệ" });
				}

				var payslip = await _context.Payslips
					.Include(p => p.User)
					.Where(p => p.UserId == userId && p.Month == month && p.Year == year)
					.Select(p => new
					{
						p.Id,
						p.UserId,
						UserName = p.User != null ? p.User.Name : null,
						UserEmail = p.User != null ? p.User.Email : null,
						p.Month,
						p.Year,
						p.StandardWorkDays,
						p.GrossSalary,
						p.InsuranceDeduction,
						p.FamilyDeduction,
						p.AssessableIncome,
						p.TaxAmount,
						p.NetSalary,
						p.Status,
						p.CreatedAt,
						p.UpdatedAt,
						p.PaidAt
					})
					.FirstOrDefaultAsync();

				if (payslip == null)
				{
					return NotFound(new { message = $"Không tìm thấy phiếu lương tháng {month}/{year} của user này" });
				}

				return Ok(payslip);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy phiếu lương của user {UserId} tháng {Month}/{Year}", userId, month, year);
				return StatusCode(500, new { message = "Lỗi server khi lấy phiếu lương", error = ex.Message });
			}
		}
		// GET: api/Payslips/month/12/year/2024
		[HttpGet("month/{month}/year/{year}")]
		public async Task<ActionResult<object>> GetPayslipsByPeriod(int month, int year)
		{
			try
			{
				if (month < 1 || month > 12)
				{
					return BadRequest(new { message = "Tháng phải từ 1-12" });
				}

				if (year < 2020 || year > 2100)
				{
					return BadRequest(new { message = "Năm không hợp lệ" });
				}

				var payslips = await _context.Payslips
					.Include(p => p.User)
						.ThenInclude(u => u.Department)
					.Include(p => p.User)
						.ThenInclude(u => u.Position)
					.Where(p => p.Month == month && p.Year == year)
					.Select(p => new
					{
						p.Id,
						p.UserId,
						UserName = p.User != null ? p.User.Name : null,
						UserEmail = p.User != null ? p.User.Email : null,
						Department = p.User != null && p.User.Department != null ? p.User.Department.Name : null,
						Position = p.User != null && p.User.Position != null ? p.User.Position.PositionName : null,
						p.Month,
						p.Year,
						p.StandardWorkDays,
						p.GrossSalary,
						p.InsuranceDeduction,
						p.FamilyDeduction,
						p.AssessableIncome,
						p.TaxAmount,
						p.NetSalary,
						p.Status,
						p.CreatedAt,
						p.PaidAt
					})
					.OrderBy(p => p.Department)
					.ThenBy(p => p.UserName)
					.ToListAsync();

				// Tính thống kê
				var totalEmployees = payslips.Count;
				var totalGrossSalary = payslips.Sum(p => p.GrossSalary);
				var totalInsuranceDeduction = payslips.Sum(p => p.InsuranceDeduction);
				var totalTaxAmount = payslips.Sum(p => p.TaxAmount);
				var totalNetSalary = payslips.Sum(p => p.NetSalary);
				var paidCount = payslips.Count(p => p.Status == "PAID");
				var draftCount = payslips.Count(p => p.Status == "DRAFT");

				return Ok(new
				{
					month,
					year,
					payslips,
					statistics = new
					{
						totalEmployees,
						totalGrossSalary,
						totalInsuranceDeduction,
						totalTaxAmount,
						totalNetSalary,
						paidCount,
						draftCount
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách phiếu lương tháng {Month}/{Year}", month, year);
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách phiếu lương", error = ex.Message });
			}
		}

		// GET: api/Payslips/status/PAID
		[HttpGet("status/{status}")]
		public async Task<ActionResult<IEnumerable<object>>> GetPayslipsByStatus(string status)
		{
			try
			{
				if (status != "DRAFT" && status != "PAID")
				{
					return BadRequest(new { message = "Status phải là 'DRAFT' hoặc 'PAID'" });
				}

				var payslips = await _context.Payslips
					.Include(p => p.User)
					.Where(p => p.Status == status)
					.Select(p => new
					{
						p.Id,
						p.UserId,
						UserName = p.User != null ? p.User.Name : null,
						p.Month,
						p.Year,
						p.NetSalary,
						p.Status,
						p.CreatedAt,
						p.PaidAt
					})
					.OrderByDescending(p => p.CreatedAt)
					.ToListAsync();

				return Ok(payslips);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách phiếu lương theo status: {Status}", status);
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách phiếu lương", error = ex.Message });
			}
		}

		// POST: api/Payslips
		[HttpPost]
		public async Task<ActionResult<object>> CreatePayslip(Payslip payslip)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Kiểm tra user có tồn tại không
				var userExists = await _context.Users.AnyAsync(u => u.Id == payslip.UserId);
				if (!userExists)
				{
					return BadRequest(new { message = "User không tồn tại" });
				}

				// Validate month & year
				if (payslip.Month < 1 || payslip.Month > 12)
				{
					return BadRequest(new { message = "Tháng phải từ 1-12" });
				}

				if (payslip.Year < 2020 || payslip.Year > 2100)
				{
					return BadRequest(new { message = "Năm không hợp lệ" });
				}

				// Validate standard work days
				if (payslip.StandardWorkDays < 1 || payslip.StandardWorkDays > 31)
				{
					return BadRequest(new { message = "Số công chuẩn phải từ 1-31" });
				}

				// Validate status
				if (payslip.Status != "DRAFT" && payslip.Status != "PAID")
				{
					return BadRequest(new { message = "Status phải là 'DRAFT' hoặc 'PAID'" });
				}

				// Kiểm tra đã tồn tại phiếu lương cho user trong tháng này chưa
				var existingPayslip = await _context.Payslips
					.FirstOrDefaultAsync(p => p.UserId == payslip.UserId 
						&& p.Month == payslip.Month 
						&& p.Year == payslip.Year);

				if (existingPayslip != null)
				{
					return BadRequest(new { message = $"User này đã có phiếu lương tháng {payslip.Month}/{payslip.Year}. Vui lòng sử dụng phương thức cập nhật." });
				}

				payslip.CreatedAt = DateTime.UtcNow;
				
				// Nếu status là PAID và chưa có PaidAt thì tự động set
				if (payslip.Status == "PAID" && payslip.PaidAt == null)
				{
					payslip.PaidAt = DateTime.UtcNow;
				}

				_context.Payslips.Add(payslip);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã tạo phiếu lương mới với ID: {PayslipId} cho User ID: {UserId} tháng {Month}/{Year}", 
					payslip.Id, payslip.UserId, payslip.Month, payslip.Year);

				// Load thông tin để trả về
				var createdPayslip = await _context.Payslips
					.Include(p => p.User)
					.Where(p => p.Id == payslip.Id)
					.Select(p => new
					{
						p.Id,
						p.UserId,
						UserName = p.User != null ? p.User.Name : null,
						UserEmail = p.User != null ? p.User.Email : null,
						p.Month,
						p.Year,
						p.StandardWorkDays,
						p.GrossSalary,
						p.InsuranceDeduction,
						p.FamilyDeduction,
						p.AssessableIncome,
						p.TaxAmount,
						p.NetSalary,
						p.Status,
						p.CreatedAt,
						p.UpdatedAt,
						p.PaidAt
					})
					.FirstOrDefaultAsync();

				return CreatedAtAction(nameof(GetPayslip), new { id = payslip.Id }, new
				{
					message = "Tạo phiếu lương thành công",
					payslip = createdPayslip
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo phiếu lương mới");
				return StatusCode(500, new { message = "Lỗi server khi tạo phiếu lương", error = ex.Message });
			}
		}

		// PUT: api/Payslips/5
		[HttpPut("{id}")]
		public async Task<ActionResult<object>> UpdatePayslip(int id, [FromBody] Dictionary<string, object?> updateData)
		{
			try
			{
				var existingPayslip = await _context.Payslips.FindAsync(id);
				if (existingPayslip == null)
				{
					return NotFound(new { message = "Không tìm thấy phiếu lương" });
				}

				var oldStatus = existingPayslip.Status;

				foreach (var kvp in updateData)
				{
					var propertyName = kvp.Key;
					var value = kvp.Value?.ToString();

					switch (propertyName.ToLower())
					{
						case "userid":
							if (kvp.Value != null)
							{
								if (int.TryParse(kvp.Value.ToString(), out int userId))
								{
									var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
									if (!userExists)
									{
										return BadRequest(new { message = "User không tồn tại" });
									}

									// Kiểm tra trùng lặp
									var duplicatePayslip = await _context.Payslips
										.FirstOrDefaultAsync(p => p.UserId == userId 
											&& p.Month == existingPayslip.Month 
											&& p.Year == existingPayslip.Year 
											&& p.Id != id);

									if (duplicatePayslip != null)
									{
										return BadRequest(new { message = "User này đã có phiếu lương trong tháng này" });
									}

									existingPayslip.UserId = userId;
								}
								else
								{
									return BadRequest(new { message = "User ID không hợp lệ" });
								}
							}
							break;

						case "month":
							if (kvp.Value != null)
							{
								if (int.TryParse(kvp.Value.ToString(), out int month))
								{
									if (month < 1 || month > 12)
									{
										return BadRequest(new { message = "Tháng phải từ 1-12" });
									}

									// Kiểm tra trùng lặp
									var duplicatePayslip = await _context.Payslips
										.FirstOrDefaultAsync(p => p.UserId == existingPayslip.UserId 
											&& p.Month == month 
											&& p.Year == existingPayslip.Year 
											&& p.Id != id);

									if (duplicatePayslip != null)
									{
										return BadRequest(new { message = "User này đã có phiếu lương trong tháng này" });
									}

									existingPayslip.Month = month;
								}
								else
								{
									return BadRequest(new { message = "Tháng không hợp lệ" });
								}
							}
							break;

						case "year":
							if (kvp.Value != null)
							{
								if (int.TryParse(kvp.Value.ToString(), out int year))
								{
									if (year < 2020 || year > 2100)
									{
										return BadRequest(new { message = "Năm không hợp lệ" });
									}

									// Kiểm tra trùng lặp
									var duplicatePayslip = await _context.Payslips
										.FirstOrDefaultAsync(p => p.UserId == existingPayslip.UserId 
											&& p.Month == existingPayslip.Month 
											&& p.Year == year 
											&& p.Id != id);

									if (duplicatePayslip != null)
									{
										return BadRequest(new { message = "User này đã có phiếu lương trong tháng này" });
									}

									existingPayslip.Year = year;
								}
								else
								{
									return BadRequest(new { message = "Năm không hợp lệ" });
								}
							}
							break;

						case "standardworkdays":
							if (kvp.Value != null)
							{
								if (int.TryParse(kvp.Value.ToString(), out int standardWorkDays))
								{
									if (standardWorkDays < 1 || standardWorkDays > 31)
									{
										return BadRequest(new { message = "Số công chuẩn phải từ 1-31" });
									}
									existingPayslip.StandardWorkDays = standardWorkDays;
								}
								else
								{
									return BadRequest(new { message = "Số công chuẩn không hợp lệ" });
								}
							}
							break;

						case "grosssalary":
							if (kvp.Value != null)
							{
								if (decimal.TryParse(kvp.Value.ToString(), out decimal grossSalary))
								{
									existingPayslip.GrossSalary = grossSalary;
								}
								else
								{
									return BadRequest(new { message = "Lương gộp không hợp lệ" });
								}
							}
							break;

						case "insurancededuction":
							if (kvp.Value != null)
							{
								if (decimal.TryParse(kvp.Value.ToString(), out decimal insuranceDeduction))
								{
									existingPayslip.InsuranceDeduction = insuranceDeduction;
								}
								else
								{
									return BadRequest(new { message = "Tiền bảo hiểm không hợp lệ" });
								}
							}
							break;

						case "familydeduction":
							if (kvp.Value != null)
							{
								if (decimal.TryParse(kvp.Value.ToString(), out decimal familyDeduction))
								{
									existingPayslip.FamilyDeduction = familyDeduction;
								}
								else
								{
									return BadRequest(new { message = "Giảm trừ gia cảnh không hợp lệ" });
								}
							}
							break;

						case "assessableincome":
							if (kvp.Value != null)
							{
								if (decimal.TryParse(kvp.Value.ToString(), out decimal assessableIncome))
								{
									existingPayslip.AssessableIncome = assessableIncome;
								}
								else
								{
									return BadRequest(new { message = "Thu nhập tính thuế không hợp lệ" });
								}
							}
							break;

						case "taxamount":
							if (kvp.Value != null)
							{
								if (decimal.TryParse(kvp.Value.ToString(), out decimal taxAmount))
								{
									existingPayslip.TaxAmount = taxAmount;
								}
								else
								{
									return BadRequest(new { message = "Tiền thuế không hợp lệ" });
								}
							}
							break;

						case "netsalary":
							if (kvp.Value != null)
							{
								if (decimal.TryParse(kvp.Value.ToString(), out decimal netSalary))
								{
									existingPayslip.NetSalary = netSalary;
								}
								else
								{
									return BadRequest(new { message = "Lương thực lĩnh không hợp lệ" });
								}
							}
							break;

						case "status":
							if (!string.IsNullOrEmpty(value))
							{
								if (value != "DRAFT" && value != "PAID")
								{
									return BadRequest(new { message = "Status phải là 'DRAFT' hoặc 'PAID'" });
								}
								existingPayslip.Status = value;
								
								// Nếu chuyển từ DRAFT sang PAID và chưa có PaidAt thì tự động set
								if (oldStatus == "DRAFT" && value == "PAID" && existingPayslip.PaidAt == null)
								{
									existingPayslip.PaidAt = DateTime.UtcNow;
								}
							}
							break;

						case "id":
						case "createdat":
						case "updatedat":
						case "paidat":
							// Bỏ qua các trường này
							break;

						default:
							// Bỏ qua các trường không được hỗ trợ
							break;
					}
				}

				existingPayslip.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã cập nhật phiếu lương với ID: {PayslipId}", id);

				// Load thông tin để trả về
				var updatedPayslip = await _context.Payslips
					.Include(p => p.User)
					.Where(p => p.Id == id)
					.Select(p => new
					{
						p.Id,
						p.UserId,
						UserName = p.User != null ? p.User.Name : null,
						UserEmail = p.User != null ? p.User.Email : null,
						p.Month,
						p.Year,
						p.StandardWorkDays,
						p.GrossSalary,
						p.InsuranceDeduction,
						p.FamilyDeduction,
						p.AssessableIncome,
						p.TaxAmount,
						p.NetSalary,
						p.Status,
						p.CreatedAt,
						p.UpdatedAt,
						p.PaidAt
					})
					.FirstOrDefaultAsync();

				return Ok(new { message = "Cập nhật phiếu lương thành công", payslip = updatedPayslip });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật phiếu lương với ID: {PayslipId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật phiếu lương", error = ex.Message });
			}
		}

		// PUT: api/Payslips/5/mark-paid
		[HttpPut("{id}/mark-paid")]
		public async Task<ActionResult<object>> MarkAsPaid(int id)
		{
			try
			{
				var payslip = await _context.Payslips.FindAsync(id);
				if (payslip == null)
				{
					return NotFound(new { message = "Không tìm thấy phiếu lương" });
				}

				if (payslip.Status == "PAID")
				{
					return BadRequest(new { message = "Phiếu lương này đã được thanh toán rồi" });
				}

				payslip.Status = "PAID";
				payslip.PaidAt = DateTime.UtcNow;
				payslip.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã đánh dấu phiếu lương ID: {PayslipId} là đã thanh toán", id);

				return Ok(new 
				{ 
					message = "Đã đánh dấu phiếu lương là đã thanh toán",
					payslipId = id,
					paidAt = payslip.PaidAt
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi đánh dấu phiếu lương đã thanh toán với ID: {PayslipId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật phiếu lương", error = ex.Message });
			}
		}

		// DELETE: api/Payslips/5
		[HttpDelete("{id}")]
		public async Task<ActionResult> DeletePayslip(int id)
		{
			try
			{
				var payslip = await _context.Payslips
					.Include(p => p.User)
					.FirstOrDefaultAsync(p => p.Id == id);

				if (payslip == null)
				{
					return NotFound(new { message = "Không tìm thấy phiếu lương" });
				}

				// Có thể thêm logic không cho xóa nếu đã PAID
				if (payslip.Status == "PAID")
				{
					return BadRequest(new { message = "Không thể xóa phiếu lương đã thanh toán" });
				}

				var deletedInfo = new
				{
					payslip.Id,
					payslip.UserId,
					UserName = payslip.User?.Name,
					payslip.Month,
					payslip.Year,
					payslip.NetSalary,
					payslip.Status,
					payslip.CreatedAt
				};

				_context.Payslips.Remove(payslip);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã xóa phiếu lương với ID: {PayslipId}", id);

				return Ok(new { message = "Xóa phiếu lương thành công", deletedPayslip = deletedInfo });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa phiếu lương với ID: {PayslipId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa phiếu lương", error = ex.Message });
			}
		}
	}
}
