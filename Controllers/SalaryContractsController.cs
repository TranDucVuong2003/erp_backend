using erp_backend.Data;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class SalaryContractsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public SalaryContractsController(ApplicationDbContext context)
		{
			_context = context;
		}

		// POST: api/SalaryContracts
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<SalaryContracts>> CreateContract(SalaryContracts contract)
		{
			// 1. Kiểm tra User
			if (!await _context.Users.AnyAsync(u => u.Id == contract.UserId))
				return BadRequest("User không tồn tại");

			// 2. Kiểm tra User đã có SalaryContract chưa
			var existingContract = await _context.SalaryContracts
				.FirstOrDefaultAsync(c => c.UserId == contract.UserId);
			
			if (existingContract != null)
			{
				return BadRequest(new 
				{ 
					message = "User này đã có Salary Contract",
					existingContractId = existingContract.Id,
					hint = "Sử dụng PUT /api/SalaryContracts/{id} để cập nhật"
				});
			}

			// 3. Logic tính lương đóng Bảo hiểm
			// Nếu InsuranceSalary = 0 thì tự động tính theo mức sàn
			if (contract.InsuranceSalary == 0)
			{
				// Lấy cấu hình lương vùng từ DB
				var minWageConfig = await _context.PayrollConfigs.FindAsync("MIN_WAGE_REGION_1_2026");
				var trainedRateConfig = await _context.PayrollConfigs.FindAsync("TRAINED_WORKER_RATE");

				if (minWageConfig != null && trainedRateConfig != null)
				{
					decimal minWage = decimal.Parse(minWageConfig.Value);
					decimal rate = decimal.Parse(trainedRateConfig.Value);

					// Tự động tính: 5.310.000 * 1.07 = 5.681.700
					contract.InsuranceSalary = Math.Round(minWage * rate, 0);
				}
				else
				{
					// Fallback nếu chưa cấu hình
					contract.InsuranceSalary = 5682000;
				}
			}
			// Nếu > 0 thì giữ nguyên số InsuranceSalary mà Client gửi lên

			contract.CreatedAt = DateTime.UtcNow;
			_context.SalaryContracts.Add(contract);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contract);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<SalaryContracts>> GetContract(int id)
		{
			var contract = await _context.SalaryContracts.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
			return contract == null ? NotFound() : Ok(contract);
		}

		// PUT: api/SalaryContracts/{id}
		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> UpdateContract(int id, SalaryContracts contract)
		{
			if (id != contract.Id)
				return BadRequest("ID không khớp");

			// Kiểm tra contract có tồn tại không
			var existingContract = await _context.SalaryContracts.FindAsync(id);
			if (existingContract == null)
				return NotFound("Salary Contract không tồn tại");

			// Kiểm tra User có tồn tại không
			if (!await _context.Users.AnyAsync(u => u.Id == contract.UserId))
				return BadRequest("User không tồn tại");

			// Kiểm tra UserId mới có trùng với contract khác không (nếu đổi UserId)
			if (existingContract.UserId != contract.UserId)
			{
				var duplicateCheck = await _context.SalaryContracts
					.AnyAsync(c => c.UserId == contract.UserId && c.Id != id);
				
				if (duplicateCheck)
					return BadRequest("User mới đã có Salary Contract khác");
			}

			// Logic tính lương đóng Bảo hiểm
			// Nếu InsuranceSalary = 0 thì tự động tính theo mức sàn
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

			// Cập nhật các field
			existingContract.UserId = contract.UserId;
			existingContract.BaseSalary = contract.BaseSalary;
			existingContract.InsuranceSalary = contract.InsuranceSalary;
			existingContract.ContractType = contract.ContractType;
			existingContract.DependentsCount = contract.DependentsCount;
			existingContract.HasCommitment08 = contract.HasCommitment08;
			existingContract.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			return NoContent(); // 204 No Content
		}
	}
}