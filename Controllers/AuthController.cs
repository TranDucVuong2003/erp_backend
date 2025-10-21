using BCrypt.Net;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _config;

		public AuthController(ApplicationDbContext context, IConfiguration config)
		{
			_context = context;
			_config = config;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			try
			{
				// Validate model
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Find user
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
				if (user == null)
					return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

				// Verify password
				if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
					return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

				// Create token
				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
				var expirationTime = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiresInMinutes"]));
				
				var tokenDescriptor = new SecurityTokenDescriptor
				{
					Subject = new ClaimsIdentity(new[]
					{
						new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
						new Claim(ClaimTypes.Name, user.Name),
						new Claim(ClaimTypes.Email, user.Email),
						new Claim(ClaimTypes.Role, user.Role),
						new Claim("userId", user.Id.ToString())
					}),
					Expires = expirationTime,
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
					Issuer = _config["Jwt:Issuer"],
					Audience = _config["Jwt:Audience"]
				};

				var token = tokenHandler.CreateToken(tokenDescriptor);
				var tokenString = tokenHandler.WriteToken(token);

				// Save token to database
				var jwtToken = new JwtToken
				{
					Token = tokenString,
					Expiration = expirationTime,
					UserId = user.Id
				};
				_context.JwtTokens.Add(jwtToken);
				await _context.SaveChangesAsync();

				// Return response
				return Ok(new LoginResponse
				{
					Token = tokenString,
					Expiration = expirationTime,
					User = new UserInfo
					{
						Id = user.Id,
						Name = user.Name,
						Email = user.Email,
						Position = user.Position,
						Role = user.Role
					},
					Message = "Đăng nhập thành công"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		//[HttpPost("register")]
		//[Authorize]
		//public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		//{
		//	try
		//	{
		//		if (!ModelState.IsValid)
		//		{
		//			return BadRequest(ModelState);
		//		}

		//		// Check if email already exists
		//		if (await _context.Users.AnyAsync(u => u.Email == request.Email))
		//		{
		//			return BadRequest(new { message = "Email đã tồn tại" });
		//		}

		//		// Hash password
		//		var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

		//		// Create user
		//		var user = new User
		//		{
		//			Name = request.Name,
		//			Email = request.Email,
		//			Password = hashedPassword,
		//			Position = request.Position,
		//			Role = request.Role,
		//			CreatedAt = DateTime.UtcNow
		//		};

		//		_context.Users.Add(user);
		//		await _context.SaveChangesAsync();

		//		return Ok(new { message = "Đăng ký thành công", userId = user.Id });
		//	}
		//	catch (Exception ex)
		//	{
		//		return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
		//	}
		//}
	}
}
