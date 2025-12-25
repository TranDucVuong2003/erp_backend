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
	public class MonthlyAttendancesController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<MonthlyAttendancesController> _logger;

		public MonthlyAttendancesController(ApplicationDbContext context, ILogger<MonthlyAttendancesController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: api/MonthlyAttendances
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetMonthlyAttendances()
		{
			try
			{
				var attendances = await _context.MonthlyAttendances
					.Include(a => a.User)
					.Select(a => new
					{
						a.Id,
						a.UserId,
						UserName = a.User != null ? a.User.Name : null,
						UserEmail = a.User != null ? a.User.Email : null,
						a.Month,
						a.Year,
						a.ActualWorkDays,
						a.CreatedAt,
						a.UpdatedAt
					})
					.OrderByDescending(a => a.Year)
					.ThenByDescending(a => a.Month)
					.ThenBy(a => a.UserName)
					.ToListAsync();

				return Ok(attendances);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách chấm công");
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách chấm công", error = ex.Message });
			}
		}

		// GET: api/MonthlyAttendances/5
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<object>> GetMonthlyAttendance(int id)
		{
			try
			{
				var attendance = await _context.MonthlyAttendances
					.Include(a => a.User)
					.Where(a => a.Id == id)
					.Select(a => new
					{
						a.Id,
						a.UserId,
						UserName = a.User != null ? a.User.Name : null,
						UserEmail = a.User != null ? a.User.Email : null,
						a.Month,
						a.Year,
						a.ActualWorkDays,
						a.CreatedAt,
						a.UpdatedAt
					})
					.FirstOrDefaultAsync();

				if (attendance == null)
				{
					return NotFound(new { message = "Không tìm thấy bản chấm công" });
				}

				return Ok(attendance);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy thông tin chấm công với ID: {AttendanceId}", id);
				return StatusCode(500, new { message = "Lỗi server khi lấy thông tin chấm công", error = ex.Message });
			}
		}

		// GET: api/MonthlyAttendances/user/5
		[HttpGet("user/{userId}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<object>>> GetMonthlyAttendancesByUserId(int userId)
		{
			try
			{
				var attendances = await _context.MonthlyAttendances
					.Include(a => a.User)
					.Where(a => a.UserId == userId)
					.Select(a => new
					{
						a.Id,
						a.UserId,
						UserName = a.User != null ? a.User.Name : null,
						UserEmail = a.User != null ? a.User.Email : null,
						a.Month,
						a.Year,
						a.ActualWorkDays,
						a.CreatedAt,
						a.UpdatedAt
					})
					.OrderByDescending(a => a.Year)
					.ThenByDescending(a => a.Month)
					.ToListAsync();

				return Ok(attendances);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách chấm công của user ID: {UserId}", userId);
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách chấm công", error = ex.Message });
			}
		}

		// GET: api/MonthlyAttendances/user/5/month/12/year/2024
		[HttpGet("user/{userId}/month/{month}/year/{year}")]
		[Authorize]
		public async Task<ActionResult<object>> GetMonthlyAttendanceByUserAndPeriod(int userId, int month, int year)
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

				var attendance = await _context.MonthlyAttendances
					.Include(a => a.User)
					.Where(a => a.UserId == userId && a.Month == month && a.Year == year)
					.Select(a => new
					{
						a.Id,
						a.UserId,
						UserName = a.User != null ? a.User.Name : null,
						UserEmail = a.User != null ? a.User.Email : null,
						a.Month,
						a.Year,
						a.ActualWorkDays,
						a.CreatedAt,
						a.UpdatedAt
					})
					.FirstOrDefaultAsync();

				if (attendance == null)
				{
					return NotFound(new { message = $"Không tìm thấy bản chấm công tháng {month}/{year} của user này" });
				}

				return Ok(attendance);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy chấm công của user {UserId} tháng {Month}/{Year}", userId, month, year);
				return StatusCode(500, new { message = "Lỗi server khi lấy chấm công", error = ex.Message });
			}
		}

		// GET: api/MonthlyAttendances/month/12/year/2024
		[HttpGet("month/{month}/year/{year}")]
		[Authorize]
		public async Task<ActionResult<object>> GetMonthlyAttendancesByPeriod(int month, int year)
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

				var attendances = await _context.MonthlyAttendances
					.Include(a => a.User)
					.Where(a => a.Month == month && a.Year == year)
					.Select(a => new
					{
						a.Id,
						a.UserId,
						UserName = a.User != null ? a.User.Name : null,
						UserEmail = a.User != null ? a.User.Email : null,
						Department = a.User != null && a.User.Department != null ? a.User.Department.Name : null,
						Position = a.User != null && a.User.Position != null ? a.User.Position.PositionName : null,
						a.Month,
						a.Year,
						a.ActualWorkDays,
						a.CreatedAt,
						a.UpdatedAt
					})
					.OrderBy(a => a.Department)
					.ThenBy(a => a.UserName)
					.ToListAsync();

				// Tính thống kê
				var totalEmployees = attendances.Count;
				var totalWorkDays = attendances.Sum(a => a.ActualWorkDays);
				var averageWorkDays = totalEmployees > 0 ? totalWorkDays / totalEmployees : 0;

				return Ok(new
				{
					month,
					year,
					attendances,
					statistics = new
					{
						totalEmployees,
						totalWorkDays,
						averageWorkDays = Math.Round(averageWorkDays, 2)
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách chấm công tháng {Month}/{Year}", month, year);
				return StatusCode(500, new { message = "Lỗi server khi lấy danh sách chấm công", error = ex.Message });
			}
		}

		// POST: api/MonthlyAttendances
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<object>> CreateMonthlyAttendance(MonthlyAttendance attendance)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Kiểm tra user có tồn tại không
				var userExists = await _context.Users.AnyAsync(u => u.Id == attendance.UserId);
				if (!userExists)
				{
					return BadRequest(new { message = "User không tồn tại" });
				}

				// Validate month
				if (attendance.Month < 1 || attendance.Month > 12)
				{
					return BadRequest(new { message = "Tháng phải từ 1-12" });
				}

				// Validate year
				if (attendance.Year < 2020 || attendance.Year > 2100)
				{
					return BadRequest(new { message = "Năm không hợp lệ" });
				}

				// Validate actual work days
				if (attendance.ActualWorkDays < 0 || attendance.ActualWorkDays > 31)
				{
					return BadRequest(new { message = "Số ngày công phải từ 0-31" });
				}

				// Kiểm tra đã tồn tại bản chấm công cho user trong tháng này chưa
				var existingAttendance = await _context.MonthlyAttendances
					.FirstOrDefaultAsync(a => a.UserId == attendance.UserId 
						&& a.Month == attendance.Month 
						&& a.Year == attendance.Year);

				if (existingAttendance != null)
				{
					return BadRequest(new { message = $"User này đã có bản chấm công tháng {attendance.Month}/{attendance.Year}. Vui lòng sử dụng phương thức cập nhật." });
				}

				attendance.CreatedAt = DateTime.UtcNow;
				_context.MonthlyAttendances.Add(attendance);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã tạo bản chấm công mới với ID: {AttendanceId} cho User ID: {UserId} tháng {Month}/{Year}", 
					attendance.Id, attendance.UserId, attendance.Month, attendance.Year);

				// Load user information để trả về
				var createdAttendance = await _context.MonthlyAttendances
					.Include(a => a.User)
					.Where(a => a.Id == attendance.Id)
					.Select(a => new
					{
						a.Id,
						a.UserId,
						UserName = a.User != null ? a.User.Name : null,
						UserEmail = a.User != null ? a.User.Email : null,
						a.Month,
						a.Year,
						a.ActualWorkDays,
						a.CreatedAt,
						a.UpdatedAt
					})
					.FirstOrDefaultAsync();

				return CreatedAtAction(nameof(GetMonthlyAttendance), new { id = attendance.Id }, new
				{
					message = "Tạo bản chấm công thành công",
					attendance = createdAttendance
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo bản chấm công mới");
				return StatusCode(500, new { message = "Lỗi server khi tạo bản chấm công", error = ex.Message });
			}
		}

		// POST: api/MonthlyAttendances/batch
		// API tạo chấm công hàng loạt cho nhiều user trong cùng tháng
		[HttpPost("batch")]
		[Authorize]
		public async Task<ActionResult<object>> CreateMonthlyAttendanceBatch([FromBody] MonthlyAttendanceBatchRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Validate input
				if (request.Month < 1 || request.Month > 12)
				{
					return BadRequest(new { message = "Tháng phải từ 1-12" });
				}

				if (request.Year < 2020 || request.Year > 2100)
				{
					return BadRequest(new { message = "Năm không hợp lệ" });
				}

				if (request.Attendances == null || request.Attendances.Count == 0)
				{
					return BadRequest(new { message = "Danh sách chấm công không được để trống" });
				}

				// Kiểm tra trùng lặp userId trong request
				var duplicateUserIds = request.Attendances
					.GroupBy(a => a.UserId)
					.Where(g => g.Count() > 1)
					.Select(g => g.Key)
					.ToList();

				if (duplicateUserIds.Any())
				{
					return BadRequest(new { 
						message = "Danh sách có userId trùng lặp", 
						duplicateUserIds 
					});
				}

				// Lấy danh sách userId từ request
				var userIds = request.Attendances.Select(a => a.UserId).ToList();

				// Kiểm tra tất cả user có tồn tại không
				var existingUsers = await _context.Users
					.Where(u => userIds.Contains(u.Id))
					.Select(u => u.Id)
					.ToListAsync();

				var invalidUserIds = userIds.Except(existingUsers).ToList();
				if (invalidUserIds.Any())
				{
					return BadRequest(new { 
						message = "Một số user không tồn tại", 
						invalidUserIds 
					});
				}

				// Kiểm tra user nào đã có chấm công trong tháng này
				var existingAttendances = await _context.MonthlyAttendances
					.Where(a => userIds.Contains(a.UserId) 
						&& a.Month == request.Month 
						&& a.Year == request.Year)
					.Select(a => new { a.UserId, a.Id })
					.ToListAsync();

				var existingUserIds = existingAttendances.Select(a => a.UserId).ToList();

				var results = new List<object>();
				var errors = new List<object>();
				var skipped = new List<object>();

				// Lọc ra những user chưa có chấm công
				var newAttendances = request.Attendances
					.Where(a => !existingUserIds.Contains(a.UserId))
					.ToList();

				// Thêm các bản chấm công mới
				foreach (var item in newAttendances)
				{
					try
					{
						// Validate work days
						if (item.ActualWorkDays < 0 || item.ActualWorkDays > 31)
						{
							errors.Add(new
							{
								userId = item.UserId,
								error = "Số ngày công phải từ 0-31"
							});
							continue;
						}

						var attendance = new MonthlyAttendance
						{
							UserId = item.UserId,
							Month = request.Month,
							Year = request.Year,
							ActualWorkDays = item.ActualWorkDays,
							CreatedAt = DateTime.UtcNow
						};

						_context.MonthlyAttendances.Add(attendance);
						await _context.SaveChangesAsync();

						// Load thông tin user
						var createdAttendance = await _context.MonthlyAttendances
							.Include(a => a.User)
							.Where(a => a.Id == attendance.Id)
							.Select(a => new
							{
								a.Id,
								a.UserId,
								UserName = a.User != null ? a.User.Name : null,
								UserEmail = a.User != null ? a.User.Email : null,
								a.Month,
								a.Year,
								a.ActualWorkDays,
								a.CreatedAt
							})
							.FirstOrDefaultAsync();

						results.Add(createdAttendance!);
					}
					catch (Exception ex)
					{
						errors.Add(new
						{
							userId = item.UserId,
							error = ex.Message
						});
					}
				}

				// Lấy thông tin các user đã có chấm công (bị skip)
				foreach (var existingUserId in existingUserIds)
				{
					var existingRecord = existingAttendances.First(a => a.UserId == existingUserId);
					var user = await _context.Users
						.Where(u => u.Id == existingUserId)
						.Select(u => new { u.Id, u.Name, u.Email })
						.FirstOrDefaultAsync();

					skipped.Add(new
					{
						userId = existingUserId,
						userName = user?.Name,
						userEmail = user?.Email,
						reason = $"User đã có bản chấm công tháng {request.Month}/{request.Year}",
						existingAttendanceId = existingRecord.Id
					});
				}

				_logger.LogInformation(
					"Tạo chấm công hàng loạt tháng {Month}/{Year}: {SuccessCount} thành công, {SkipCount} bỏ qua, {ErrorCount} lỗi",
					request.Month,
					request.Year,
					results.Count,
					skipped.Count,
					errors.Count
				);

				return Ok(new
				{
					message = "Tạo chấm công hàng loạt hoàn tất",
					month = request.Month,
					year = request.Year,
					summary = new
					{
						total = request.Attendances.Count,
						success = results.Count,
						skipped = skipped.Count,
						failed = errors.Count
					},
					results,
					skipped,
					errors
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo chấm công hàng loạt tháng {Month}/{Year}", request.Month, request.Year);
				return StatusCode(500, new { message = "Lỗi server khi tạo chấm công hàng loạt", error = ex.Message });
			}
		}

		// PUT: api/MonthlyAttendances/5
		[HttpPut("{id}")]
		[Authorize]
		public async Task<ActionResult<object>> UpdateMonthlyAttendance(int id, [FromBody] Dictionary<string, object?> updateData)
		{
			try
			{
				var existingAttendance = await _context.MonthlyAttendances.FindAsync(id);
				if (existingAttendance == null)
				{
					return NotFound(new { message = "Không tìm thấy bản chấm công" });
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
										return BadRequest(new { message = "User không tồn tại" });
									}

									// Kiểm tra user mới có bản chấm công trong tháng này chưa
									var duplicateAttendance = await _context.MonthlyAttendances
										.FirstOrDefaultAsync(a => a.UserId == userId 
											&& a.Month == existingAttendance.Month 
											&& a.Year == existingAttendance.Year 
											&& a.Id != id);

									if (duplicateAttendance != null)
									{
										return BadRequest(new { message = "User này đã có bản chấm công trong tháng này" });
									}

									existingAttendance.UserId = userId;
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
									var duplicateAttendance = await _context.MonthlyAttendances
										.FirstOrDefaultAsync(a => a.UserId == existingAttendance.UserId 
											&& a.Month == month 
											&& a.Year == existingAttendance.Year 
											&& a.Id != id);

									if (duplicateAttendance != null)
									{
										return BadRequest(new { message = "User này đã có bản chấm công trong tháng này" });
									}

									existingAttendance.Month = month;
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
									var duplicateAttendance = await _context.MonthlyAttendances
										.FirstOrDefaultAsync(a => a.UserId == existingAttendance.UserId 
											&& a.Month == existingAttendance.Month 
											&& a.Year == year 
											&& a.Id != id);

									if (duplicateAttendance != null)
									{
										return BadRequest(new { message = "User này đã có bản chấm công trong tháng này" });
									}

									existingAttendance.Year = year;
								}
								else
								{
									return BadRequest(new { message = "Năm không hợp lệ" });
								}
							}
							break;

						case "actualworkdays":
							if (kvp.Value != null)
							{
								if (float.TryParse(kvp.Value.ToString(), out float workDays))
								{
									if (workDays < 0 || workDays > 31)
									{
										return BadRequest(new { message = "Số ngày công phải từ 0-31" });
									}
									existingAttendance.ActualWorkDays = workDays;
								}
								else
								{
									return BadRequest(new { message = "Số ngày công không hợp lệ" });
								}
							}
							break;

						case "id":
						case "createdat":
						case "updatedat":
							// Bỏ qua các trường này
							break;

						default:
							// Bỏ qua các trường không được hỗ trợ
							break;
					}
				}

				existingAttendance.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã cập nhật bản chấm công với ID: {AttendanceId}", id);

				// Load user information để trả về
				var updatedAttendance = await _context.MonthlyAttendances
					.Include(a => a.User)
					.Where(a => a.Id == id)
					.Select(a => new
					{
						a.Id,
						a.UserId,
						UserName = a.User != null ? a.User.Name : null,
						UserEmail = a.User != null ? a.User.Email : null,
						a.Month,
						a.Year,
						a.ActualWorkDays,
						a.CreatedAt,
						a.UpdatedAt
					})
					.FirstOrDefaultAsync();

				return Ok(new { message = "Cập nhật bản chấm công thành công", attendance = updatedAttendance });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật bản chấm công với ID: {AttendanceId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật bản chấm công", error = ex.Message });
			}
		}

		// DELETE: api/MonthlyAttendances/5
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<ActionResult> DeleteMonthlyAttendance(int id)
		{
			try
			{
				var attendance = await _context.MonthlyAttendances
					.Include(a => a.User)
					.FirstOrDefaultAsync(a => a.Id == id);

				if (attendance == null)
				{
					return NotFound(new { message = "Không tìm thấy bản chấm công" });
				}

				var deletedInfo = new
				{
					attendance.Id,
					attendance.UserId,
					UserName = attendance.User?.Name,
					attendance.Month,
					attendance.Year,
					attendance.ActualWorkDays,
					attendance.CreatedAt
				};

				_context.MonthlyAttendances.Remove(attendance);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã xóa bản chấm công với ID: {AttendanceId}", id);

				return Ok(new { message = "Xóa bản chấm công thành công", deletedAttendance = deletedInfo });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa bản chấm công với ID: {AttendanceId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa bản chấm công", error = ex.Message });
			}
		}
	}
}
