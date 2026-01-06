using erp_backend.Data;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TaxBracketsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<TaxBracketsController> _logger;

		public TaxBracketsController(ApplicationDbContext context, ILogger<TaxBracketsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: api/TaxBrackets
		[HttpGet]
		public async Task<ActionResult<IEnumerable<TaxBracket>>> GetTaxBrackets()
		{
			try
			{
				var taxBrackets = await _context.TaxBrackets
					.OrderBy(t => t.MinIncome)
					.ToListAsync();

				return Ok(taxBrackets);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách b?c thu?");
				return StatusCode(500, new { message = "L?i server khi l?y danh sách b?c thu?", error = ex.Message });
			}
		}

		// GET: api/TaxBrackets/5
		[HttpGet("{id}")]
		public async Task<ActionResult<TaxBracket>> GetTaxBracket(int id)
		{
			try
			{
				var taxBracket = await _context.TaxBrackets.FindAsync(id);

				if (taxBracket == null)
				{
					return NotFound(new { message = "Không tìm th?y b?c thu?" });
				}

				return Ok(taxBracket);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y thông tin b?c thu? v?i ID: {TaxBracketId}", id);
				return StatusCode(500, new { message = "L?i server khi l?y thông tin b?c thu?", error = ex.Message });
			}
		}

		// GET: api/TaxBrackets/calculate?income=50000000
		[HttpGet("calculate")]
		public async Task<ActionResult<object>> CalculateTax([FromQuery] decimal income)
		{
			try
			{
				if (income < 0)
				{
					return BadRequest(new { message = "Thu nh?p không ???c âm" });
				}

				var taxBrackets = await _context.TaxBrackets
					.OrderBy(t => t.MinIncome)
					.ToListAsync();

				if (!taxBrackets.Any())
				{
					return BadRequest(new { message = "Ch?a có c?u hình b?c thu? trong h? th?ng" });
				}

				decimal totalTax = 0;
				decimal remainingIncome = income;
				var taxDetails = new List<object>();

				foreach (var bracket in taxBrackets)
				{
					if (remainingIncome <= 0)
						break;

					// Tính thu nh?p ch?u thu? trong b?c này
					decimal taxableIncome = 0;

					if (bracket.MaxIncome.HasValue)
					{
						// B?c có gi?i h?n trên
						if (income >= bracket.MinIncome && income <= bracket.MaxIncome.Value)
						{
							taxableIncome = income - bracket.MinIncome + 1;
						}
						else if (income > bracket.MaxIncome.Value && remainingIncome > 0)
						{
							taxableIncome = bracket.MaxIncome.Value - bracket.MinIncome + 1;
						}
					}
					else
					{
						// B?c không có gi?i h?n trên (b?c cao nh?t)
						if (income >= bracket.MinIncome)
						{
							taxableIncome = income - bracket.MinIncome + 1;
						}
					}

					if (taxableIncome > 0)
					{
						decimal taxForBracket = taxableIncome * (decimal)bracket.TaxRate;
						totalTax += taxForBracket;
						remainingIncome -= taxableIncome;

						taxDetails.Add(new
						{
							bracket = bracket.Notes ?? $"B?c t? {bracket.MinIncome:N0} ??n {(bracket.MaxIncome.HasValue ? bracket.MaxIncome.Value.ToString("N0") : "không gi?i h?n")}",
							minIncome = bracket.MinIncome,
							maxIncome = bracket.MaxIncome,
							taxRate = $"{bracket.TaxRate * 100}%",
							taxableIncome = taxableIncome,
							taxAmount = Math.Round(taxForBracket, 2)
						});
					}
				}

				return Ok(new
				{
					income = income,
					totalTax = Math.Round(totalTax, 2),
					netIncome = Math.Round(income - totalTax, 2),
					effectiveTaxRate = income > 0 ? $"{Math.Round((totalTax / income) * 100, 2)}%" : "0%",
					taxDetails
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi tính thu? cho thu nh?p: {Income}", income);
				return StatusCode(500, new { message = "L?i server khi tính thu?", error = ex.Message });
			}
		}

		// POST: api/TaxBrackets
		[HttpPost]
		public async Task<ActionResult<TaxBracket>> CreateTaxBracket(TaxBracket taxBracket)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Validate logic
				if (taxBracket.MaxIncome.HasValue && taxBracket.MinIncome > taxBracket.MaxIncome.Value)
				{
					return BadRequest(new { message = "Thu nh?p t?i thi?u không ???c l?n h?n thu nh?p t?i ?a" });
				}

				// Check overlap with existing brackets
				var overlapping = await _context.TaxBrackets
					.Where(t => t.Id != taxBracket.Id)
					.Where(t => 
						(t.MinIncome <= taxBracket.MinIncome && (!t.MaxIncome.HasValue || t.MaxIncome.Value >= taxBracket.MinIncome)) ||
						(taxBracket.MaxIncome.HasValue && t.MinIncome <= taxBracket.MaxIncome.Value && (!t.MaxIncome.HasValue || t.MaxIncome.Value >= taxBracket.MaxIncome.Value))
					)
					.FirstOrDefaultAsync();

				if (overlapping != null)
				{
					return BadRequest(new { message = "B?c thu? m?i b? trùng l?p v?i b?c thu? ?ã t?n t?i" });
				}

				taxBracket.CreatedAt = DateTime.UtcNow;
				_context.TaxBrackets.Add(taxBracket);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã t?o b?c thu? m?i v?i ID: {TaxBracketId}", taxBracket.Id);

				return CreatedAtAction(nameof(GetTaxBracket), new { id = taxBracket.Id }, taxBracket);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi t?o b?c thu? m?i");
				return StatusCode(500, new { message = "L?i server khi t?o b?c thu?", error = ex.Message });
			}
		}

		// PUT: api/TaxBrackets/5
		[HttpPut("{id}")]
		public async Task<ActionResult<TaxBracket>> UpdateTaxBracket(int id, TaxBracket taxBracket)
		{
			try
			{
				if (id != taxBracket.Id)
				{
					return BadRequest(new { message = "ID không kh?p" });
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var existingTaxBracket = await _context.TaxBrackets.FindAsync(id);
				if (existingTaxBracket == null)
				{
					return NotFound(new { message = "Không tìm th?y b?c thu?" });
				}

				// Validate logic
				if (taxBracket.MaxIncome.HasValue && taxBracket.MinIncome > taxBracket.MaxIncome.Value)
				{
					return BadRequest(new { message = "Thu nh?p t?i thi?u không ???c l?n h?n thu nh?p t?i ?a" });
				}

				// Check overlap with other brackets
				var overlapping = await _context.TaxBrackets
					.Where(t => t.Id != id)
					.Where(t => 
						(t.MinIncome <= taxBracket.MinIncome && (!t.MaxIncome.HasValue || t.MaxIncome.Value >= taxBracket.MinIncome)) ||
						(taxBracket.MaxIncome.HasValue && t.MinIncome <= taxBracket.MaxIncome.Value && (!t.MaxIncome.HasValue || t.MaxIncome.Value >= taxBracket.MaxIncome.Value))
					)
					.FirstOrDefaultAsync();

				if (overlapping != null)
				{
					return BadRequest(new { message = "B?c thu? m?i b? trùng l?p v?i b?c thu? ?ã t?n t?i" });
				}

				existingTaxBracket.MinIncome = taxBracket.MinIncome;
				existingTaxBracket.MaxIncome = taxBracket.MaxIncome;
				existingTaxBracket.TaxRate = taxBracket.TaxRate;
				existingTaxBracket.Notes = taxBracket.Notes;
				existingTaxBracket.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã c?p nh?t b?c thu? v?i ID: {TaxBracketId}", id);

				return Ok(existingTaxBracket);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi c?p nh?t b?c thu? v?i ID: {TaxBracketId}", id);
				return StatusCode(500, new { message = "L?i server khi c?p nh?t b?c thu?", error = ex.Message });
			}
		}

		// DELETE: api/TaxBrackets/5
		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteTaxBracket(int id)
		{
			try
			{
				var taxBracket = await _context.TaxBrackets.FindAsync(id);
				if (taxBracket == null)
				{
					return NotFound(new { message = "Không tìm th?y b?c thu?" });
				}

				_context.TaxBrackets.Remove(taxBracket);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã xóa b?c thu? v?i ID: {TaxBracketId}", id);

				return Ok(new { message = "Xóa b?c thu? thành công", deletedTaxBracket = taxBracket });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi xóa b?c thu? v?i ID: {TaxBracketId}", id);
				return StatusCode(500, new { message = "L?i server khi xóa b?c thu?", error = ex.Message });
			}
		}

		// DELETE: api/TaxBrackets/clear-all
		[HttpDelete("clear-all")]
		public async Task<ActionResult> ClearAllTaxBrackets()
		{
			try
			{
				var allBrackets = await _context.TaxBrackets.ToListAsync();
				var count = allBrackets.Count;

				if (count == 0)
				{
					return Ok(new { message = "Không có b?c thu? nào ?? xóa" });
				}

				_context.TaxBrackets.RemoveRange(allBrackets);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã xóa t?t c? {Count} b?c thu?", count);

				return Ok(new { message = $"?ã xóa t?t c? {count} b?c thu? thành công", count });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi xóa t?t c? b?c thu?");
				return StatusCode(500, new { message = "L?i server khi xóa b?c thu?", error = ex.Message });
			}
		}
	}
}
