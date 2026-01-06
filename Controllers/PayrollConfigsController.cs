using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;

namespace erp_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PayrollConfigsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public PayrollConfigsController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/PayrollConfigs
		[HttpGet]
		public async Task<ActionResult<IEnumerable<PayrollConfig>>> GetPayrollConfigs()
		{
			return await _context.PayrollConfigs.ToListAsync();
		}

		// GET: api/PayrollConfigs/MIN_WAGE_REGION_1_2026
		[HttpGet("{key}")]
		public async Task<ActionResult<PayrollConfig>> GetPayrollConfig(string key)
		{
			var payrollConfig = await _context.PayrollConfigs.FindAsync(key);

			if (payrollConfig == null)
			{
				return NotFound(new { message = $"PayrollConfig with key '{key}' not found." });
			}

			return payrollConfig;
		}

		// PUT: api/PayrollConfigs/MIN_WAGE_REGION_1_2026
		[HttpPut("{key}")]
		public async Task<IActionResult> PutPayrollConfig(string key, PayrollConfig payrollConfig)
		{
			if (key != payrollConfig.Key)
			{
				return BadRequest(new { message = "Key mismatch." });
			}

			_context.Entry(payrollConfig).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!PayrollConfigExists(key))
				{
					return NotFound(new { message = $"PayrollConfig with key '{key}' not found." });
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}

		// POST: api/PayrollConfigs
		[HttpPost]
		public async Task<ActionResult<PayrollConfig>> PostPayrollConfig(PayrollConfig payrollConfig)
		{
			if (string.IsNullOrWhiteSpace(payrollConfig.Key))
			{
				return BadRequest(new { message = "Key is required." });
			}

			if (PayrollConfigExists(payrollConfig.Key))
			{
				return Conflict(new { message = $"PayrollConfig with key '{payrollConfig.Key}' already exists." });
			}

			_context.PayrollConfigs.Add(payrollConfig);
			
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				if (PayrollConfigExists(payrollConfig.Key))
				{
					return Conflict(new { message = $"PayrollConfig with key '{payrollConfig.Key}' already exists." });
				}
				else
				{
					throw;
				}
			}

			return CreatedAtAction(nameof(GetPayrollConfig), new { key = payrollConfig.Key }, payrollConfig);
		}

		// DELETE: api/PayrollConfigs/MIN_WAGE_REGION_1_2026
		[HttpDelete("{key}")]
		public async Task<IActionResult> DeletePayrollConfig(string key)
		{
			var payrollConfig = await _context.PayrollConfigs.FindAsync(key);
			if (payrollConfig == null)
			{
				return NotFound(new { message = $"PayrollConfig with key '{key}' not found." });
			}

			_context.PayrollConfigs.Remove(payrollConfig);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool PayrollConfigExists(string key)
		{
			return _context.PayrollConfigs.Any(e => e.Key == key);
		}
	}
}
