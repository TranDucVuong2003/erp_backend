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
    //[Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }


        [HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }


        [HttpPost]
		[Authorize]
		public async Task<ActionResult<User>> CreateUser(User user)
        {
            try
            {
                // Validate required fields
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra email chính trùng
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    return BadRequest(new { message = "Email đã tồn tại" });
                }

                // Validate và xử lý SecondaryEmail nếu có
                if (!string.IsNullOrWhiteSpace(user.SecondaryEmail))
                {
                    // Validate format email phụ
                    if (!System.Text.RegularExpressions.Regex.IsMatch(user.SecondaryEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        return BadRequest(new { message = "Định dạng email phụ không hợp lệ" });
                    }

                    // Kiểm tra email phụ không trùng với email chính của user khác
                    if (await _context.Users.AnyAsync(u => u.Email == user.SecondaryEmail))
                    {
                        return BadRequest(new { message = "Email phụ không được trùng với email chính của người dùng khác" });
                    }

                    // Kiểm tra email phụ không trùng với email phụ của user khác
                    if (await _context.Users.AnyAsync(u => u.SecondaryEmail == user.SecondaryEmail))
                    {
                        return BadRequest(new { message = "Email phụ đã được sử dụng bởi người dùng khác" });
                    }

                    // Kiểm tra email phụ không trùng với email chính của cùng user
                    if (user.Email == user.SecondaryEmail)
                    {
                        return BadRequest(new { message = "Email phụ không được trùng với email chính" });
                    }
                }
                else
                {
                    // Nếu SecondaryEmail là empty string hoặc whitespace, set về null
                    user.SecondaryEmail = null;
                }

                // Hash password trước khi lưu
                if (!string.IsNullOrEmpty(user.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                user.CreatedAt = DateTime.UtcNow;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo user mới");
                return StatusCode(500, new { message = "Lỗi server khi tạo người dùng", error = ex.Message });
            }
        }


		[HttpPut("{id}")]
		[Authorize]
		public async Task<ActionResult<UpdateUserResponse>> UpdateUser(int id, [FromBody] Dictionary<string, object?> updateData)
		{
			try
			{
				// Kiểm tra xem user có tồn tại không
				var existingUser = await _context.Users.FindAsync(id);
				if (existingUser == null)
				{
					return NotFound(new { message = "Không tìm thấy người dùng" });
				}

				// Kiểm tra ID nếu được gửi trong body
				if (updateData.ContainsKey("id") || updateData.ContainsKey("Id"))
				{
					var idKey = updateData.ContainsKey("id") ? "id" : "Id";
					if (updateData[idKey] != null)
					{
						// Kiểm tra nếu giá trị là JsonElement
						if (updateData[idKey] is System.Text.Json.JsonElement jsonElement)
						{
							// Lấy giá trị int từ JsonElement
							if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number && jsonElement.TryGetInt32(out int jsonId))
							{
								if (jsonId != id)
								{
									return BadRequest(new { message = "ID không khớp với dữ liệu người dùng" });
								}
							}
							else
							{
								return BadRequest(new { message = "Giá trị ID không hợp lệ" });
							}
						}
						else
						{
							// Nếu không phải JsonElement, thử Convert.ToInt32
							try
							{
								if (Convert.ToInt32(updateData[idKey]) != id)
								{
									return BadRequest(new { message = "ID không khớp với dữ liệu người dùng" });
								}
							}
							catch
							{
								return BadRequest(new { message = "Giá trị ID không hợp lệ" });
							}
						}
					}
				}

				// Cập nhật từng trường nếu có trong request
				foreach (var kvp in updateData)
				{
					var propertyName = kvp.Key;
					var value = kvp.Value?.ToString();

					switch (propertyName.ToLower())
					{
						case "name":
							if (!string.IsNullOrEmpty(value))
							{
								if (value.Length > 100)
								{
									return BadRequest(new { message = "Tên không được vượt quá 100 ký tự" });
								}
								existingUser.Name = value;
							}
							break;

						case "email":
							if (!string.IsNullOrEmpty(value))
							{
								if (value.Length > 150)
								{
									return BadRequest(new { message = "Email không được vượt quá 150 ký tự" });
								}

								// Validate email format
								if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
								{
									return BadRequest(new { message = "Định dạng email không hợp lệ" });
								}

								// Kiểm tra email trùng với user khác
								var emailExists = await _context.Users.AnyAsync(u => u.Email == value && u.Id != id);
								if (emailExists)
								{
									return BadRequest(new { message = "Email đã được sử dụng bởi người dùng khác" });
								}
								existingUser.Email = value;
							}
							break;

						case "password":
							if (!string.IsNullOrEmpty(value))
							{
								if (value.Length > 255)
								{
									return BadRequest(new { message = "Mật khẩu không được vượt quá 255 ký tự" });
								}

								// Hash password nếu chưa được hash
								if (!value.StartsWith("$2"))
								{
									existingUser.Password = BCrypt.Net.BCrypt.HashPassword(value);
								}
								else
								{
									existingUser.Password = value;
								}
							}
							break;

						case "position":
							if (value != null)
							{
								if (value.Length > 100)
								{
									return BadRequest(new { message = "Chức vụ không được vượt quá 100 ký tự" });
								}
								existingUser.Position = value;
							}
							break;

						case "phonenumber":
							if (value != null)
							{
								if (value.Length > 20)
								{
									return BadRequest(new { message = "Số điện thoại không được vượt quá 20 ký tự" });
								}
								existingUser.PhoneNumber = value;
							}
							break;

						case "address":
							if (value != null)
							{
								if (value.Length > 500)
								{
									return BadRequest(new { message = "Địa chỉ không được vượt quá 500 ký tự" });
								}
								existingUser.Address = value;
							}
							break;

						case "role":
							if (value != null)
							{
								if (value.Length > 50)
								{
									return BadRequest(new { message = "Vai trò không được vượt quá 50 ký tự" });
								}
								existingUser.Role = value;
							}
							break;

						case "secondaryemail":
							if (value != null)
							{
								if (!string.IsNullOrWhiteSpace(value))
								{
									if (value.Length > 150)
									{
										return BadRequest(new { message = "Email phụ không được vượt quá 150 ký tự" });
									}

									// Validate email format
									if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
									{
										return BadRequest(new { message = "Định dạng email phụ không hợp lệ" });
									}

									// Kiểm tra email phụ không trùng với email chính của user khác
									if (await _context.Users.AnyAsync(u => u.Email == value && u.Id != id))
									{
										return BadRequest(new { message = "Email phụ không được trùng với email chính của người dùng khác" });
									}

									// Kiểm tra email phụ không trùng với email phụ của user khác
									if (await _context.Users.AnyAsync(u => u.SecondaryEmail == value && u.Id != id))
									{
										return BadRequest(new { message = "Email phụ đã được sử dụng bởi người dùng khác" });
									}

									// Kiểm tra email phụ không trùng với email chính của cùng user
									if (existingUser.Email == value)
									{
										return BadRequest(new { message = "Email phụ không được trùng với email chính" });
									}

									existingUser.SecondaryEmail = value;
								}
								else
								{
									// Nếu value là empty string hoặc whitespace, set về null
									existingUser.SecondaryEmail = null;
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

				// Cập nhật thời gian
				existingUser.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				// Tạo response
				var response = new UpdateUserResponse
				{
					Message = "Cập nhật thông tin người dùng thành công",
					User = new UserInfo
					{
						Id = existingUser.Id,
						Name = existingUser.Name,
						Email = existingUser.Email,
						Position = existingUser.Position,
						Role = existingUser.Role
					},
					UpdatedAt = existingUser.UpdatedAt.Value
				};

				return Ok(response);
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!UserExists(id))
				{
					return NotFound(new { message = "Không tìm thấy người dùng" });
				}
				else
				{
					throw;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật user với ID: {UserId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật người dùng", error = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		[Authorize]
		public async Task<ActionResult<DeleteUserResponse>> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                // Lưu thông tin user trước khi xóa để trả về trong response
                var deletedUserInfo = new UserInfo
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Position = user.Position,
                    Role = user.Role
                };

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                // Tạo response
                var response = new DeleteUserResponse
                {
                    Message = "Xóa người dùng thành công",
                    DeletedUser = deletedUserInfo,
                    DeletedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa user với ID: {UserId}", id);
                return StatusCode(500, new { message = "Lỗi server khi xóa người dùng", error = ex.Message });
            }
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}