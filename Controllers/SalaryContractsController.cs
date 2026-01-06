using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using erp_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class SalaryContractsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IFileUploadService _fileUploadService;
		private readonly ILogger<SalaryContractsController> _logger;

		// ✅ Cấu hình file upload
		private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
		private readonly long _maxFileSizeInMB = 5; // 5MB

		public SalaryContractsController(
			ApplicationDbContext context, 
			IFileUploadService fileUploadService,
			ILogger<SalaryContractsController> logger)
		{
			_context = context;
			_fileUploadService = fileUploadService;
			_logger = logger;
		}

		/// <summary>
		/// Sanitize tên người dùng để dùng làm tên folder
		/// VD: "Nguyễn Văn A" -> "Nguyen_Van_A"
		/// </summary>
		private string SanitizeUserNameForFolder(string userName, int userId)
		{
			if (string.IsNullOrWhiteSpace(userName))
				return $"User_{userId}";

			// Loại bỏ dấu tiếng Việt
			string normalized = userName.Normalize(System.Text.NormalizationForm.FormD);
			var stringBuilder = new System.Text.StringBuilder();

			foreach (char c in normalized)
			{
				var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			string result = stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);

			// Thay thế khoảng trắng và ký tự đặc biệt bằng underscore
			result = Regex.Replace(result, @"[^a-zA-Z0-9]", "_");

			// Loại bỏ underscore liên tiếp
			result = Regex.Replace(result, @"_+", "_");

			// Loại bỏ underscore đầu cuối
			result = result.Trim('_');

			// Kết hợp với ID để đảm bảo unique
			return $"{result}_{userId}";
		}

		// POST: api/SalaryContracts
		[HttpPost]
		[Authorize]
		[Consumes("multipart/form-data")]
		public async Task<ActionResult<SalaryContractResponseDto>> CreateContract([FromForm] CreateSalaryContractDto dto)
		{
			try
			{
				// 1. Kiểm tra User
				var user = await _context.Users.FindAsync(dto.UserId);
				if (user == null)
					return BadRequest(new { message = "User không tồn tại" });

				// 2. Kiểm tra User đã có SalaryContract chưa
				var existingContract = await _context.SalaryContracts
					.FirstOrDefaultAsync(c => c.UserId == dto.UserId);
				
				if (existingContract != null)
				{
					return BadRequest(new 
					{ 
						message = "Nhân viên đã được cấu hình lương",
						existingContractId = existingContract.Id,
						hint = "Sử dụng PUT /api/SalaryContracts/{id} để cập nhật"
					});
				}

				// 3. Validate file nếu có
				if (dto.Attachment != null)
				{
					if (!_fileUploadService.IsValidFileExtension(dto.Attachment.FileName, _allowedExtensions))
					{
						return BadRequest(new { message = $"File không hợp lệ. Chỉ chấp nhận: {string.Join(", ", _allowedExtensions)}" });
					}

					if (!_fileUploadService.IsValidFileSize(dto.Attachment.Length, _maxFileSizeInMB))
					{
						return BadRequest(new { message = $"File quá lớn. Kích thước tối đa: {_maxFileSizeInMB}MB" });
					}
				}

				// 4. Tạo contract
				var contract = new SalaryContracts
				{
					UserId = dto.UserId,
					BaseSalary = dto.BaseSalary,
					InsuranceSalary = dto.InsuranceSalary,
					ContractType = dto.ContractType,
					DependentsCount = dto.DependentsCount,
					HasCommitment08 = dto.HasCommitment08,
					CreatedAt = DateTime.UtcNow
				};

				// 5. Logic tính lương đóng Bảo hiểm
				if (contract.InsuranceSalary == 0)
				{
					var minWageConfig = await _context.PayrollConfigs.FindAsync("MIN_WAGE_REGION_1_2026");
					var trainedRateConfig = await _context.PayrollConfigs.FindAsync("TRAINED_WORKER_RATE");

					if (minWageConfig != null && trainedRateConfig != null)
					{
						decimal minWage = decimal.Parse(minWageConfig.Value);
						decimal rate = decimal.Parse(trainedRateConfig.Value);
						contract.InsuranceSalary = Math.Round(minWage * rate, 0);
					}
					else
					{
						contract.InsuranceSalary = 5682000;
					}
				}

				// 6. Xử lý file upload nếu có
				if (dto.Attachment != null)
				{
					// ✅ Tạo tên folder theo tên người dùng
					string folderName = SanitizeUserNameForFolder(user.Name, user.Id);
					
					var (filePath, fileName) = await _fileUploadService.SaveFileAsync(
						dto.Attachment, 
						$"salary-contracts/{folderName}"
					);
					contract.AttachmentPath = filePath;
					contract.AttachmentFileName = fileName;
				}

				_context.SalaryContracts.Add(contract);
				await _context.SaveChangesAsync();

				var response = new SalaryContractResponseDto
				{
					Id = contract.Id,
					UserId = contract.UserId,
					BaseSalary = contract.BaseSalary,
					InsuranceSalary = contract.InsuranceSalary,
					ContractType = contract.ContractType,
					DependentsCount = contract.DependentsCount,
					HasCommitment08 = contract.HasCommitment08,
					AttachmentPath = contract.AttachmentPath,
					AttachmentFileName = contract.AttachmentFileName,
					CreatedAt = contract.CreatedAt,
					UpdatedAt = contract.UpdatedAt,
					UserName = user.Name,
					UserEmail = user.Email
				};

				return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, new { message = "Tạo hợp đồng lương thành công", data = response });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo Salary Contract cho UserId: {UserId}", dto.UserId);
				return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo hợp đồng", error = ex.Message });
			}
		}

		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<SalaryContractResponseDto>> GetContract(int id)
		{
			var contract = await _context.SalaryContracts
				.Include(c => c.User)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (contract == null)
				return NotFound(new { message = "Salary Contract không tồn tại" });

			var response = new SalaryContractResponseDto
			{
				Id = contract.Id,
				UserId = contract.UserId,
				BaseSalary = contract.BaseSalary,
				InsuranceSalary = contract.InsuranceSalary,
				ContractType = contract.ContractType,
				DependentsCount = contract.DependentsCount,
				HasCommitment08 = contract.HasCommitment08,
				AttachmentPath = contract.AttachmentPath,
				AttachmentFileName = contract.AttachmentFileName,
				CreatedAt = contract.CreatedAt,
				UpdatedAt = contract.UpdatedAt,
				UserName = contract.User?.Name,
				UserEmail = contract.User?.Email
			};

			return Ok(new { message = "Lấy thông tin hợp đồng thành công", data = response });
		}

		// PUT: api/SalaryContracts/{id}
		[HttpPut("{id}")]
		[Authorize]
		[Consumes("multipart/form-data")]
		public async Task<ActionResult<SalaryContractResponseDto>> UpdateContract(int id, [FromForm] UpdateSalaryContractDto dto)
		{
			try
			{
				// Kiểm tra contract có tồn tại không
				var contract = await _context.SalaryContracts
					.Include(c => c.User)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (contract == null)
					return NotFound(new { message = "Salary Contract không tồn tại" });

				// Validate file nếu có
				if (dto.Attachment != null)
				{
					if (!_fileUploadService.IsValidFileExtension(dto.Attachment.FileName, _allowedExtensions))
					{
						return BadRequest(new { message = $"File không hợp lệ. Chỉ chấp nhận: {string.Join(", ", _allowedExtensions)}" });
					}

					if (!_fileUploadService.IsValidFileSize(dto.Attachment.Length, _maxFileSizeInMB))
					{
						return BadRequest(new { message = $"File quá lớn. Kích thước tối đa: {_maxFileSizeInMB}MB" });
					}
				}

				// Cập nhật các trường
				if (dto.BaseSalary.HasValue) 
					contract.BaseSalary = dto.BaseSalary.Value;
				
				if (dto.InsuranceSalary.HasValue) 
				{
					contract.InsuranceSalary = dto.InsuranceSalary.Value;
					
					// Logic tính lương đóng Bảo hiểm
					if (contract.InsuranceSalary == 0)
					{
						var minWageConfig = await _context.PayrollConfigs.FindAsync("MIN_WAGE_REGION_1_2026");
						var trainedRateConfig = await _context.PayrollConfigs.FindAsync("TRAINED_WORKER_RATE");

						if (minWageConfig != null && trainedRateConfig != null)
						{
							decimal minWage = decimal.Parse(minWageConfig.Value);
							decimal rate = decimal.Parse(trainedRateConfig.Value);
							contract.InsuranceSalary = Math.Round(minWage * rate, 0);
						}
						else
						{
							contract.InsuranceSalary = 5682000;
						}
					}
				}
				
				if (dto.ContractType != null) 
					contract.ContractType = dto.ContractType;
				
				if (dto.DependentsCount.HasValue) 
					contract.DependentsCount = dto.DependentsCount.Value;
				
				if (dto.HasCommitment08.HasValue) 
					contract.HasCommitment08 = dto.HasCommitment08.Value;

				// Xử lý file mới nếu có
				if (dto.Attachment != null)
				{
					// Xóa file cũ
					if (!string.IsNullOrEmpty(contract.AttachmentPath))
					{
						await _fileUploadService.DeleteFileAsync(contract.AttachmentPath);
					}

					// ✅ Upload file mới với tên folder theo tên người dùng
					string folderName = SanitizeUserNameForFolder(contract.User!.Name, contract.UserId);
					
					var (filePath, fileName) = await _fileUploadService.SaveFileAsync(
						dto.Attachment,
						$"salary-contracts/{folderName}"
					);
					contract.AttachmentPath = filePath;
					contract.AttachmentFileName = fileName;
				}

				contract.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				var response = new SalaryContractResponseDto
				{
					Id = contract.Id,
					UserId = contract.UserId,
					BaseSalary = contract.BaseSalary,
					InsuranceSalary = contract.InsuranceSalary,
					ContractType = contract.ContractType,
					DependentsCount = contract.DependentsCount,
					HasCommitment08 = contract.HasCommitment08,
					AttachmentPath = contract.AttachmentPath,
					AttachmentFileName = contract.AttachmentFileName,
					CreatedAt = contract.CreatedAt,
					UpdatedAt = contract.UpdatedAt,
					UserName = contract.User?.Name,
					UserEmail = contract.User?.Email
				};

				return Ok(new { message = "Cập nhật hợp đồng lương thành công", data = response });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật Salary Contract Id: {Id}", id);
				return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật hợp đồng", error = ex.Message });
			}
		}

		// DELETE: api/SalaryContracts/{id}
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeleteContract(int id)
		{
			try
			{
				var contract = await _context.SalaryContracts.FindAsync(id);
				
				if (contract == null)
					return NotFound(new { message = "Salary Contract không tồn tại" });

				// Xóa file đính kèm nếu có
				if (!string.IsNullOrEmpty(contract.AttachmentPath))
				{
					await _fileUploadService.DeleteFileAsync(contract.AttachmentPath);
				}

				_context.SalaryContracts.Remove(contract);
				await _context.SaveChangesAsync();

				return Ok(new { message = "Xóa hợp đồng lương thành công" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa Salary Contract Id: {Id}", id);
				return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa hợp đồng", error = ex.Message });
			}
		}

		// GET: api/SalaryContracts/user/{userId}
		[HttpGet("user/{userId}")]
		[Authorize]
		public async Task<ActionResult<SalaryContractResponseDto>> GetContractByUserId(int userId)
		{
			var contract = await _context.SalaryContracts
				.Include(c => c.User)
				.FirstOrDefaultAsync(c => c.UserId == userId);

			if (contract == null)
				return NotFound(new { message = "User chưa có Salary Contract" });

			var response = new SalaryContractResponseDto
			{
				Id = contract.Id,
				UserId = contract.UserId,
				BaseSalary = contract.BaseSalary,
				InsuranceSalary = contract.InsuranceSalary,
				ContractType = contract.ContractType,
				DependentsCount = contract.DependentsCount,
				HasCommitment08 = contract.HasCommitment08,
				AttachmentPath = contract.AttachmentPath,
				AttachmentFileName = contract.AttachmentFileName,
				CreatedAt = contract.CreatedAt,
				UpdatedAt = contract.UpdatedAt,
				UserName = contract.User?.Name,
				UserEmail = contract.User?.Email
			};

			return Ok(new { message = "Lấy thông tin hợp đồng thành công", data = response });
		}

		// GET: api/SalaryContracts
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<SalaryContractResponseDto>>> GetAllContracts()
		{
			var contracts = await _context.SalaryContracts
				.Include(c => c.User)
				.OrderByDescending(c => c.CreatedAt)
				.ToListAsync();

			var response = contracts.Select(c => new SalaryContractResponseDto
			{
				Id = c.Id,
				UserId = c.UserId,
				BaseSalary = c.BaseSalary,
				InsuranceSalary = c.InsuranceSalary,
				ContractType = c.ContractType,
				DependentsCount = c.DependentsCount,
				HasCommitment08 = c.HasCommitment08,
				AttachmentPath = c.AttachmentPath,
				AttachmentFileName = c.AttachmentFileName,
				CreatedAt = c.CreatedAt,
				UpdatedAt = c.UpdatedAt,
				UserName = c.User?.Name,
				UserEmail = c.User?.Email
			}).ToList();

			return Ok(new { message = "Lấy danh sách hợp đồng thành công", data = response, total = response.Count });
		}

		/// <summary>
		/// Download file mẫu Cam kết Thông tư 08 (DOCX)
		/// GET: api/SalaryContracts/download-commitment08-template
		/// </summary>
		[HttpGet("download-commitment08-template")]
		public IActionResult DownloadCommitment08Template()
		{
			try
			{
				// ✅ Cập nhật tên file mới: mau-so-8-mst-tt86.docx
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "salary-forms", "mau-so-8-mst-tt86.docx");

				if (!System.IO.File.Exists(filePath))
				{
					_logger.LogWarning("File mẫu Thông tư 08 không tồn tại tại: {FilePath}", filePath);
					return NotFound(new { message = "File mẫu không tồn tại" });
				}

				var fileBytes = System.IO.File.ReadAllBytes(filePath);
				
				// ✅ Cập nhật tên file download và Content-Type cho DOCX
				var fileName = "Mau_Cam_Ket_Thong_Tu_08.docx";
				var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

				return File(fileBytes, contentType, fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi download file mẫu Thông tư 08");
				return StatusCode(500, new { message = "Có lỗi xảy ra khi tải file mẫu", error = ex.Message });
			}
		}

	}
}