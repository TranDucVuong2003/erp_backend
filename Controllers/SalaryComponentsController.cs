using erp_backend.Data;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	//[Authorize]
	public class SalaryComponentsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<SalaryComponentsController> _logger;

		public SalaryComponentsController(ApplicationDbContext context, ILogger<SalaryComponentsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: api/SalaryComponents
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetSalaryComponents()
		{
			try
			{
				var salaryComponents = await _context.SalaryComponents
					.Include(s => s.User)
					.Select(s => new
					{
						s.Id,
						s.UserId,
						UserName = s.User != null ? s.User.Name : null,
						UserEmail = s.User != null ? s.User.Email : null,
						s.Month,
						s.Year,
						s.Amount,
						s.Type,
						s.Reason,
						s.CreatedAt,
						s.UpdatedAt
					})
					.OrderByDescending(s => s.Year)
					.ThenByDescending(s => s.Month)
					.ThenByDescending(s => s.CreatedAt)
					.ToListAsync();

				return Ok(salaryComponents);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách các kho?n th??ng/ph?t");
				return StatusCode(500, new { message = "L?i server khi l?y danh sách các kho?n th??ng/ph?t", error = ex.Message });
			}
		}

		// GET: api/SalaryComponents/5
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<object>> GetSalaryComponent(int id)
		{
			try
			{
				var salaryComponent = await _context.SalaryComponents
					.Include(s => s.User)
					.Where(s => s.Id == id)
					.Select(s => new
					{
						s.Id,
						s.UserId,
						UserName = s.User != null ? s.User.Name : null,
						UserEmail = s.User != null ? s.User.Email : null,
						s.Month,
						s.Year,
						s.Amount,
						s.Type,
						s.Reason,
						s.CreatedAt,
						s.UpdatedAt
					})
					.FirstOrDefaultAsync();

				if (salaryComponent == null)
				{
					return NotFound(new { message = "Không tìm th?y kho?n th??ng/ph?t" });
				}

				return Ok(salaryComponent);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y thông tin kho?n th??ng/ph?t v?i ID: {SalaryComponentId}", id);
				return StatusCode(500, new { message = "L?i server khi l?y thông tin kho?n th??ng/ph?t", error = ex.Message });
			}
		}

		// GET: api/SalaryComponents/user/5
		[HttpGet("user/{userId}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetSalaryComponentsByUserId(int userId)
		{
			try
			{
				var salaryComponents = await _context.SalaryComponents
					.Include(s => s.User)
					.Where(s => s.UserId == userId)
					.Select(s => new
					{
						s.Id,
						s.UserId,
						UserName = s.User != null ? s.User.Name : null,
						UserEmail = s.User != null ? s.User.Email : null,
						s.Month,
						s.Year,
						s.Amount,
						s.Type,
						s.Reason,
						s.CreatedAt,
						s.UpdatedAt
					})
					.OrderByDescending(s => s.Year)
					.ThenByDescending(s => s.Month)
					.ThenByDescending(s => s.CreatedAt)
					.ToListAsync();

				return Ok(salaryComponents);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách kho?n th??ng/ph?t c?a user ID: {UserId}", userId);
				return StatusCode(500, new { message = "L?i server khi l?y danh sách kho?n th??ng/ph?t", error = ex.Message });
			}
		}

		// GET: api/SalaryComponents/user/5/month/12/year/2024
		[HttpGet("user/{userId}/month/{month}/year/{year}")]
		[Authorize]
		public async Task<ActionResult<object>> GetSalaryComponentsByUserAndPeriod(int userId, int month, int year)
		{
			try
			{
				if (month < 1 || month > 12)
				{
					return BadRequest(new { message = "Tháng ph?i t? 1-12" });
				}

				if (year < 2020 || year > 2100)
				{
					return BadRequest(new { message = "N?m không h?p l?" });
				}

				var salaryComponents = await _context.SalaryComponents
					.Include(s => s.User)
					.Where(s => s.UserId == userId && s.Month == month && s.Year == year)
					.Select(s => new
					{
						s.Id,
						s.UserId,
						UserName = s.User != null ? s.User.Name : null,
						UserEmail = s.User != null ? s.User.Email : null,
						s.Month,
						s.Year,
						s.Amount,
						s.Type,
						s.Reason,
						s.CreatedAt,
						s.UpdatedAt
					})
					.OrderByDescending(s => s.CreatedAt)
					.ToListAsync();

				// Tính t?ng theo lo?i
				var totalIn = salaryComponents.Where(s => s.Type == "in").Sum(s => s.Amount);
				var totalOut = salaryComponents.Where(s => s.Type == "out").Sum(s => s.Amount);
				var netAmount = totalIn - totalOut;

				return Ok(new
				{
					userId,
					month,
					year,
					components = salaryComponents,
					summary = new
					{
						totalBonus = totalIn,
						totalDeduction = totalOut,
						netAmount
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y kho?n th??ng/ph?t c?a user {UserId} tháng {Month}/{Year}", userId, month, year);
				return StatusCode(500, new { message = "L?i server khi l?y kho?n th??ng/ph?t", error = ex.Message });
			}
		}

		// GET: api/SalaryComponents/month/12/year/2024
		[HttpGet("month/{month}/year/{year}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetSalaryComponentsByPeriod(int month, int year)
		{
			try
			{
				if (month < 1 || month > 12)
				{
					return BadRequest(new { message = "Tháng ph?i t? 1-12" });
				}

				if (year < 2020 || year > 2100)
				{
					return BadRequest(new { message = "N?m không h?p l?" });
				}

				var salaryComponents = await _context.SalaryComponents
					.Include(s => s.User)
					.Where(s => s.Month == month && s.Year == year)
					.Select(s => new
					{
						s.Id,
						s.UserId,
						UserName = s.User != null ? s.User.Name : null,
						UserEmail = s.User != null ? s.User.Email : null,
						s.Month,
						s.Year,
						s.Amount,
						s.Type,
						s.Reason,
						s.CreatedAt,
						s.UpdatedAt
					})
					.OrderBy(s => s.UserId)
					.ThenByDescending(s => s.CreatedAt)
					.ToListAsync();

				return Ok(salaryComponents);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách kho?n th??ng/ph?t tháng {Month}/{Year}", month, year);
				return StatusCode(500, new { message = "L?i server khi l?y danh sách kho?n th??ng/ph?t", error = ex.Message });
			}
		}

		// POST: api/SalaryComponents
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<object>> CreateSalaryComponent(SalaryComponent salaryComponent)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Ki?m tra user có t?n t?i không
				var userExists = await _context.Users.AnyAsync(u => u.Id == salaryComponent.UserId);
				if (!userExists)
				{
					return BadRequest(new { message = "User không t?n t?i" });
				}

				// Validate month
				if (salaryComponent.Month < 1 || salaryComponent.Month > 12)
				{
					return BadRequest(new { message = "Tháng ph?i t? 1-12" });
				}

				// Validate year
				if (salaryComponent.Year < 2020 || salaryComponent.Year > 2100)
				{
					return BadRequest(new { message = "N?m không h?p l?" });
				}

				// Validate type
				if (salaryComponent.Type != "in" && salaryComponent.Type != "out")
				{
					return BadRequest(new { message = "Lo?i ph?i là 'in' (c?ng) ho?c 'out' (tr?)" });
				}

				// Validate amount
				if (salaryComponent.Amount <= 0)
				{
					return BadRequest(new { message = "S? ti?n ph?i l?n h?n 0" });
				}

				salaryComponent.CreatedAt = DateTime.UtcNow;
				_context.SalaryComponents.Add(salaryComponent);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã t?o kho?n {Type} m?i v?i ID: {SalaryComponentId} cho User ID: {UserId}", 
					salaryComponent.Type == "in" ? "th??ng" : "ph?t", salaryComponent.Id, salaryComponent.UserId);

				// Load user information ?? tr? v?
				var createdSalaryComponent = await _context.SalaryComponents
					.Include(s => s.User)
					.Where(s => s.Id == salaryComponent.Id)
					.Select(s => new
					{
						s.Id,
						s.UserId,
						UserName = s.User != null ? s.User.Name : null,
						UserEmail = s.User != null ? s.User.Email : null,
						s.Month,
						s.Year,
						s.Amount,
						s.Type,
						s.Reason,
						s.CreatedAt,
						s.UpdatedAt
					})
					.FirstOrDefaultAsync();

				return CreatedAtAction(nameof(GetSalaryComponent), new { id = salaryComponent.Id }, new
				{
					message = $"T?o kho?n {(salaryComponent.Type == "in" ? "th??ng" : "ph?t")} thành công",
					salaryComponent = createdSalaryComponent
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi t?o kho?n th??ng/ph?t m?i");
				return StatusCode(500, new { message = "L?i server khi t?o kho?n th??ng/ph?t", error = ex.Message });
			}
		}

		// PUT: api/SalaryComponents/5
		[HttpPut("{id}")]
		[Authorize]
		public async Task<ActionResult<object>> UpdateSalaryComponent(int id, [FromBody] Dictionary<string, object?> updateData)
		{
			try
			{
				var existingSalaryComponent = await _context.SalaryComponents.FindAsync(id);
				if (existingSalaryComponent == null)
				{
					return NotFound(new { message = "Không tìm th?y kho?n th??ng/ph?t" });
				}

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
										return BadRequest(new { message = "User không t?n t?i" });
									}
									existingSalaryComponent.UserId = userId;
								}
								else
								{
									return BadRequest(new { message = "User ID không h?p l?" });
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
										return BadRequest(new { message = "Tháng ph?i t? 1-12" });
									}
									existingSalaryComponent.Month = month;
								}
								else
								{
									return BadRequest(new { message = "Tháng không h?p l?" });
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
										return BadRequest(new { message = "N?m không h?p l?" });
									}
									existingSalaryComponent.Year = year;
								}
								else
								{
									return BadRequest(new { message = "N?m không h?p l?" });
								}
							}
							break;

						case "amount":
							if (kvp.Value != null)
							{
								if (decimal.TryParse(kvp.Value.ToString(), out decimal amount))
								{
									if (amount <= 0)
									{
										return BadRequest(new { message = "S? ti?n ph?i l?n h?n 0" });
									}
									existingSalaryComponent.Amount = amount;
								}
								else
								{
									return BadRequest(new { message = "S? ti?n không h?p l?" });
								}
							}
							break;

						case "type":
							if (!string.IsNullOrEmpty(value))
							{
								if (value != "in" && value != "out")
								{
									return BadRequest(new { message = "Lo?i ph?i là 'in' (c?ng) ho?c 'out' (tr?)" });
								}
								existingSalaryComponent.Type = value;
							}
							break;

						case "reason":
							if (!string.IsNullOrEmpty(value))
							{
								if (value.Length > 500)
								{
									return BadRequest(new { message = "Lý do không ???c v??t quá 500 ký t?" });
								}
								existingSalaryComponent.Reason = value;
							}
							break;

						case "id":
						case "createdat":
						case "updatedat":
							// B? qua các tr??ng này
							break;

						default:
							// B? qua các tr??ng không ???c h? tr?
							break;
					}
				}

				existingSalaryComponent.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã c?p nh?t kho?n th??ng/ph?t v?i ID: {SalaryComponentId}", id);

				// Load user information ?? tr? v?
				var updatedSalaryComponent = await _context.SalaryComponents
					.Include(s => s.User)
					.Where(s => s.Id == id)
					.Select(s => new
					{
						s.Id,
						s.UserId,
						UserName = s.User != null ? s.User.Name : null,
						UserEmail = s.User != null ? s.User.Email : null,
						s.Month,
						s.Year,
						s.Amount,
						s.Type,
						s.Reason,
						s.CreatedAt,
						s.UpdatedAt
					})
					.FirstOrDefaultAsync();

				return Ok(new { message = "C?p nh?t kho?n th??ng/ph?t thành công", salaryComponent = updatedSalaryComponent });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi c?p nh?t kho?n th??ng/ph?t v?i ID: {SalaryComponentId}", id);
				return StatusCode(500, new { message = "L?i server khi c?p nh?t kho?n th??ng/ph?t", error = ex.Message });
			}
		}

		// DELETE: api/SalaryComponents/5
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<ActionResult> DeleteSalaryComponent(int id)
		{
			try
			{
				var salaryComponent = await _context.SalaryComponents
					.Include(s => s.User)
					.FirstOrDefaultAsync(s => s.Id == id);

				if (salaryComponent == null)
				{
					return NotFound(new { message = "Không tìm th?y kho?n th??ng/ph?t" });
				}

				var deletedInfo = new
				{
					salaryComponent.Id,
					salaryComponent.UserId,
					UserName = salaryComponent.User?.Name,
					salaryComponent.Month,
					salaryComponent.Year,
					salaryComponent.Amount,
					salaryComponent.Type,
					salaryComponent.Reason,
					salaryComponent.CreatedAt
				};

				_context.SalaryComponents.Remove(salaryComponent);
				await _context.SaveChangesAsync();

				_logger.LogInformation("?ã xóa kho?n th??ng/ph?t v?i ID: {SalaryComponentId}", id);

				return Ok(new { message = "Xóa kho?n th??ng/ph?t thành công", deletedSalaryComponent = deletedInfo });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi xóa kho?n th??ng/ph?t v?i ID: {SalaryComponentId}", id);
				return StatusCode(500, new { message = "L?i server khi xóa kho?n th??ng/ph?t", error = ex.Message });
			}
		}
	}
}
