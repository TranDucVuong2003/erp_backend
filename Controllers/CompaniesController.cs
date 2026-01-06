using erp_backend.Data;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CompaniesController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<CompaniesController> _logger;

		public CompaniesController(ApplicationDbContext context, ILogger<CompaniesController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: api/Companies
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
		{
			try
			{
				// Lấy role từ JWT token
				var roleClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
				var role = roleClaim?.Value;

				// Lấy UserId từ JWT token
				var userIdClaim = User.FindFirst("userid")?.Value;

				var query = _context.Companies.Include(c => c.User).AsQueryable();

				// Nếu role là "user" thì chỉ lấy công ty do user đó quản lý
				if (role != null && role.ToLower() == "user")
				{
					if (userIdClaim != null)
					{
						var userId = int.Parse(userIdClaim);
						query = query.Where(c => c.UserId == userId);
						_logger.LogInformation($"User role detected. Filtering companies for UserId: {userId}");
					}
					else
					{
						_logger.LogWarning("User role detected but UserId claim not found");
						return Forbid();
					}
				}
				else if (role != null && role.ToLower() == "admin")
				{
					// Admin có thể xem tất cả công ty
					_logger.LogInformation("Admin role detected. Returning all companies");
				}
				else
				{
					// Nếu không có role hoặc role không hợp lệ
					_logger.LogWarning($"Invalid or missing role: {role}");
					return Forbid();
				}

				var companies = await query.ToListAsync();
				return Ok(companies);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách công ty");
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách công ty", error = ex.Message });
			}
		}

		// GET: api/Companies/5
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<Company>> GetCompany(int id)
		{
			try
			{
				var company = await _context.Companies
					.Include(c => c.User)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (company == null)
				{
					return NotFound(new { message = "Không tìm thấy công ty" });
				}

				return Ok(company);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy thông tin công ty với ID: {CompanyId}", id);
				return StatusCode(500, new { message = "Lỗi server khi lấy thông tin công ty", error = ex.Message });
			}
		}

		// POST: api/Companies
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<Company>> CreateCompany(Company company)
		{
			try
			{
				// Kiểm tra model validation
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Kiểm tra trùng mã số thuế (nếu có)
				if (!string.IsNullOrWhiteSpace(company.Mst))
				{
					var existingCompany = await _context.Companies
						.FirstOrDefaultAsync(c => c.Mst == company.Mst);
					if (existingCompany != null)
					{
						return BadRequest(new { message = "Mã số thuế đã tồn tại" });
					}
				}

				// Kiểm tra user có tồn tại không
				var userExists = await _context.Users.AnyAsync(u => u.Id == company.UserId);
				if (!userExists)
				{
					return BadRequest(new { message = "User không tồn tại" });
				}

				// Gán thời gian tạo UTC
				company.CreatedAt = DateTime.UtcNow;

				// Fix DateTime UTC cho NgayCapGiayPhep và NgayHoatDong
				if (company.NgayCapGiayPhep.HasValue)
					company.NgayCapGiayPhep = ToUtc(company.NgayCapGiayPhep.Value);

				if (company.NgayHoatDong.HasValue)
					company.NgayHoatDong = ToUtc(company.NgayHoatDong.Value);

				_context.Companies.Add(company);
				await _context.SaveChangesAsync();

				// Reload company với User navigation property
				var savedCompany = await _context.Companies
					.Include(c => c.User)
					.FirstOrDefaultAsync(c => c.Id == company.Id);

				return CreatedAtAction(nameof(GetCompany), new { id = savedCompany.Id }, savedCompany);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo công ty mới");
				return StatusCode(500, new { message = "Lỗi server khi tạo công ty", error = ex.Message });
			}
		}

		// PATCH: api/Companies/5
		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> UpdateCompany(int id, [FromBody] Dictionary<string, object> updateData)
		{
			try
			{
				// Kiểm tra công ty có tồn tại không
				var existingCompany = await _context.Companies.FindAsync(id);
				if (existingCompany == null)
				{
					return NotFound(new { message = "Không tìm thấy công ty" });
				}

				// Kiểm tra có trường userId trong request không
				if (!updateData.ContainsKey("userId") && !updateData.ContainsKey("UserId"))
				{
					return BadRequest(new { message = "Thiếu trường userId" });
				}

				// Lấy userId từ request - Fix cho JsonElement
				int newUserId;
				object userIdValue = updateData.ContainsKey("userId") ? updateData["userId"] : updateData["UserId"];
				
				if (userIdValue is System.Text.Json.JsonElement jsonElement)
				{
					if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
					{
						newUserId = jsonElement.GetInt32();
					}
					else
					{
						return BadRequest(new { message = "UserId phải là số nguyên" });
					}
				}
				else
				{
					newUserId = Convert.ToInt32(userIdValue);
				}

				// Kiểm tra user có tồn tại không
				var userExists = await _context.Users.AnyAsync(u => u.Id == newUserId);
				if (!userExists)
				{
					return BadRequest(new { message = "User không tồn tại" });
				}

				// Cập nhật UserId
				existingCompany.UserId = newUserId;
				existingCompany.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				// Reload company với User navigation property
				var updatedCompany = await _context.Companies
					.Include(c => c.User)
					.FirstOrDefaultAsync(c => c.Id == id);

				return Ok(new { message = "Cập nhật UserId thành công", company = updatedCompany });
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CompanyExists(id))
				{
					return NotFound(new { message = "Không tìm thấy công ty" });
				}
				else
				{
					throw;
				}
			}
			catch (FormatException)
			{
				return BadRequest(new { message = "UserId phải là số nguyên" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật công ty với ID: {CompanyId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật công ty", error = ex.Message });
			}
		}

		// PUT: api/Companies/batch-update
		[HttpPut("batch-update")]
		[Authorize]
		public async Task<IActionResult> BatchUpdateCompanies([FromBody] Dictionary<string, object> updateData)
		{
			try
			{
				// Lấy danh sách companyIds
				if (!updateData.ContainsKey("companyIds") && !updateData.ContainsKey("CompanyIds"))
				{
					return BadRequest(new { message = "Thiếu trường companyIds" });
				}

				// Lấy userId mới
				if (!updateData.ContainsKey("userId") && !updateData.ContainsKey("UserId"))
				{
					return BadRequest(new { message = "Thiếu trường userId" });
				}

				// Parse companyIds
				List<int> companyIds = new List<int>();
				object companyIdsValue = updateData.ContainsKey("companyIds") ? updateData["companyIds"] : updateData["CompanyIds"];
				
				if (companyIdsValue is System.Text.Json.JsonElement companyIdsElement)
				{
					if (companyIdsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
					{
						foreach (var item in companyIdsElement.EnumerateArray())
						{
							if (item.ValueKind == System.Text.Json.JsonValueKind.Number)
							{
								companyIds.Add(item.GetInt32());
							}
						}
					}
					else
					{
						return BadRequest(new { message = "companyIds phải là mảng số nguyên" });
					}
				}

				if (companyIds.Count == 0)
				{
					return BadRequest(new { message = "Danh sách companyIds không được rỗng" });
				}

				// Parse userId
				int newUserId;
				object userIdValue = updateData.ContainsKey("userId") ? updateData["userId"] : updateData["UserId"];
				
				if (userIdValue is System.Text.Json.JsonElement userIdElement)
				{
					if (userIdElement.ValueKind == System.Text.Json.JsonValueKind.Number)
					{
						newUserId = userIdElement.GetInt32();
					}
					else
					{
						return BadRequest(new { message = "UserId phải là số nguyên" });
					}
				}
				else
				{
					newUserId = Convert.ToInt32(userIdValue);
				}

				// Kiểm tra user có tồn tại không
				var userExists = await _context.Users.AnyAsync(u => u.Id == newUserId);
				if (!userExists)
				{
					return BadRequest(new { message = "User không tồn tại" });
				}

				// Lấy danh sách công ty cần cập nhật
				var companies = await _context.Companies
					.Where(c => companyIds.Contains(c.Id))
					.ToListAsync();

				if (companies.Count == 0)
				{
					return NotFound(new { message = "Không tìm thấy công ty nào" });
				}

				// Kiểm tra quyền truy cập (nếu là User thì chỉ update được công ty của mình)
				var roleClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
				var role = roleClaim?.Value;
				var userIdClaim = User.FindFirst("userid")?.Value;

				if (role != null && role.ToLower() == "user" && userIdClaim != null)
				{
					var currentUserId = int.Parse(userIdClaim);
					var unauthorizedCompanies = companies.Where(c => c.UserId != currentUserId).ToList();
					
					if (unauthorizedCompanies.Any())
					{
						return Forbid();
					}
				}

				// Cập nhật UserId cho tất cả các công ty
				var updatedCount = 0;
				var notFoundIds = companyIds.Except(companies.Select(c => c.Id)).ToList();

				foreach (var company in companies)
				{
					company.UserId = newUserId;
					company.UpdatedAt = DateTime.UtcNow;
					updatedCount++;
				}

				await _context.SaveChangesAsync();

				// Reload companies với User navigation property
				var updatedCompanies = await _context.Companies
					.Include(c => c.User)
					.Where(c => companyIds.Contains(c.Id))
					.ToListAsync();

				return Ok(new
				{
					message = $"Cập nhật thành công {updatedCount} công ty",
					updatedCount = updatedCount,
					totalRequested = companyIds.Count,
					notFoundIds = notFoundIds,
					companies = updatedCompanies
				});
			}
			catch (FormatException)
			{
				return BadRequest(new { message = "Dữ liệu không đúng định dạng" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật hàng loạt công ty");
				return StatusCode(500, new { message = "Lỗi server khi cập nhật hàng loạt công ty", error = ex.Message });
			}
		}

		// DELETE: api/Companies/5
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeleteCompany(int id)
		{
			try
			{
				var company = await _context.Companies.FindAsync(id);
				if (company == null)
				{
					return NotFound(new { message = "Không tìm thấy công ty" });
				}

				_context.Companies.Remove(company);
				await _context.SaveChangesAsync();

				return Ok(new { message = "Xóa công ty thành công", deletedId = id });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa công ty với ID: {CompanyId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa công ty", error = ex.Message });
			}
		}

		// DELETE: api/Companies/batch-delete
		[HttpDelete("batch-delete")]
		[Authorize]
		public async Task<IActionResult> BatchDeleteCompanies([FromBody] Dictionary<string, object> deleteData)
		{
			try
			{
				// Lấy danh sách companyIds
				if (!deleteData.ContainsKey("companyIds") && !deleteData.ContainsKey("CompanyIds"))
				{
					return BadRequest(new { message = "Thiếu trường companyIds" });
				}

				// Parse companyIds
				List<int> companyIds = new List<int>();
				object companyIdsValue = deleteData.ContainsKey("companyIds") ? deleteData["companyIds"] : deleteData["CompanyIds"];
				
				if (companyIdsValue is System.Text.Json.JsonElement companyIdsElement)
				{
					if (companyIdsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
					{
						foreach (var item in companyIdsElement.EnumerateArray())
						{
							if (item.ValueKind == System.Text.Json.JsonValueKind.Number)
							{
								companyIds.Add(item.GetInt32());
							}
						}
					}
					else
					{
						return BadRequest(new { message = "companyIds phải là mảng số nguyên" });
					}
				}

				if (companyIds.Count == 0)
				{
					return BadRequest(new { message = "Danh sách companyIds không được rỗng" });
				}

				// Lấy danh sách công ty cần xóa
				var companies = await _context.Companies
					.Where(c => companyIds.Contains(c.Id))
					.ToListAsync();

				if (companies.Count == 0)
				{
					return NotFound(new { message = "Không tìm thấy công ty nào" });
				}

				// Kiểm tra quyền truy cập (nếu là User thì chỉ xóa được công ty của mình)
				var roleClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
				var role = roleClaim?.Value;
				var userIdClaim = User.FindFirst("userid")?.Value;

				if (role != null && role.ToLower() == "user" && userIdClaim != null)
				{
					var currentUserId = int.Parse(userIdClaim);
					var unauthorizedCompanies = companies.Where(c => c.UserId != currentUserId).ToList();
					
					if (unauthorizedCompanies.Any())
					{
						return Forbid();
					}
				}

				// Xóa tất cả các công ty
				var deletedCount = 0;
				var notFoundIds = companyIds.Except(companies.Select(c => c.Id)).ToList();
				var deletedIds = companies.Select(c => c.Id).ToList();
				var deletedCompanyNames = companies.Select(c => new { c.Id, c.TenDoanhNghiep }).ToList();

				_context.Companies.RemoveRange(companies);
				await _context.SaveChangesAsync();
				deletedCount = companies.Count;

				return Ok(new
				{
					message = $"Xóa thành công {deletedCount} công ty",
					deletedCount = deletedCount,
					totalRequested = companyIds.Count,
					deletedIds = deletedIds,
					notFoundIds = notFoundIds,
					deletedCompanies = deletedCompanyNames
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa hàng loạt công ty");
				return StatusCode(500, new { message = "Lỗi server khi xóa hàng loạt công ty", error = ex.Message });
			}
		}

		private bool CompanyExists(int id)
		{
			return _context.Companies.Any(e => e.Id == id);
		}

		// Hàm hỗ trợ chuyển DateTime về UTC an toàn
		private DateTime ToUtc(DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Unspecified)
				return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			if (dateTime.Kind == DateTimeKind.Local)
				return dateTime.ToUniversalTime();
			return dateTime;
		}
	}
}
