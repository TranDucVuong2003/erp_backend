using BCrypt.Net;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using erp_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace erp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            JwtService jwtService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Email và mật khẩu không được để trống" });
                }

                // Find user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác" });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác" });
                }

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddDays(7);

                // Save refresh token to db
                var jwtToken = new JwtToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    Expiration = expiresAt,
                    DeviceInfo = request.DeviceInfo,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                };

                _context.JwtTokens.Add(jwtToken);
                await _context.SaveChangesAsync();

                // Set refresh token in HttpOnly cookie
                SetRefreshTokenCookie(refreshToken, expiresAt);

                // Create response
                var response = new LoginResponse
                {
                    AccessToken = accessToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.AccessTokenExpiryMinutes),
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Position = user.Position,
                        Role = user.Role
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Lỗi server khi đăng nhập", error = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()
        {
            try
            {
                // Get refresh token from cookie
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { message = "Refresh token không tồn tại" });
                }

                // Get the stored token
                var storedToken = await _context.JwtTokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => 
                        t.Token == refreshToken && 
                        t.Expiration > DateTime.UtcNow && 
                        !t.IsUsed && 
                        !t.IsRevoked);

                if (storedToken == null)
                {
                    return Unauthorized(new { message = "Refresh token không hợp lệ hoặc đã hết hạn" });
                }

                // Get associated user
                var user = storedToken.User;
                if (user == null)
                {
                    return Unauthorized(new { message = "User không tồn tại" });
                }

                // Generate new tokens
                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddDays(7);

                // Revoke old token and save new one
                storedToken.IsUsed = true;
                storedToken.ReplacedByToken = newRefreshToken;

                var jwtToken = new JwtToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    Expiration = expiresAt,
                    DeviceInfo = storedToken.DeviceInfo,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                };

                _context.JwtTokens.Add(jwtToken);
                await _context.SaveChangesAsync();

                // Set refresh token in HttpOnly cookie
                SetRefreshTokenCookie(newRefreshToken, expiresAt);

                // Return new access token
                var response = new RefreshTokenResponse
                {
                    AccessToken = newAccessToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.AccessTokenExpiryMinutes)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { message = "Lỗi server khi refresh token", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                // Get refresh token from cookie
                var refreshToken = Request.Cookies["refreshToken"];
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    // Revoke the token in database
                    var storedToken = await _context.JwtTokens
                        .FirstOrDefaultAsync(t => t.Token == refreshToken);

                    if (storedToken != null)
                    {
                        storedToken.IsRevoked = true;
                        storedToken.RevokedAt = DateTime.UtcNow;
                        storedToken.ReasonRevoked = "User logout";
                        await _context.SaveChangesAsync();
                    }

                    // Clear cookie
                    Response.Cookies.Delete("refreshToken", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None
                    });
                }

                return Ok(new { message = "Đăng xuất thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "Lỗi server khi đăng xuất", error = ex.Message });
            }
        }

        [HttpGet("sessions")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SessionInfo>>> GetUserSessions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User không xác định" });
                }

                var currentRefreshToken = Request.Cookies["refreshToken"];

                var sessions = await _context.JwtTokens
                    .Where(t => 
                        t.UserId.ToString() == userId && 
                        !t.IsRevoked && 
                        t.Expiration > DateTime.UtcNow)
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new SessionInfo
                    {
                        Id = t.Id,
                        DeviceInfo = t.DeviceInfo ?? "Unknown Device",
                        IpAddress = t.IpAddress ?? "Unknown IP",
                        CreatedAt = t.CreatedAt,
                        ExpiresAt = t.Expiration,
                        IsCurrentSession = t.Token == currentRefreshToken
                    })
                    .ToListAsync();

                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user sessions");
                return StatusCode(500, new { message = "Lỗi server khi lấy thông tin phiên đăng nhập", error = ex.Message });
            }
        }

        [HttpPost("revoke-session/{id}")]
        [Authorize]
        public async Task<ActionResult> RevokeSession(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User không xác định" });
                }

                var session = await _context.JwtTokens
                    .FirstOrDefaultAsync(t => 
                        t.Id == id && 
                        t.UserId.ToString() == userId);

                if (session == null)
                {
                    return NotFound(new { message = "Không tìm thấy phiên đăng nhập" });
                }

                // Check if it's current session
                var currentRefreshToken = Request.Cookies["refreshToken"];
                if (session.Token == currentRefreshToken)
                {
                    return BadRequest(new { message = "Không thể thu hồi phiên đăng nhập hiện tại. Hãy đăng xuất." });
                }

                session.IsRevoked = true;
                session.RevokedAt = DateTime.UtcNow;
                session.ReasonRevoked = "Manually revoked by user";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thu hồi phiên đăng nhập thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking session");
                return StatusCode(500, new { message = "Lỗi server khi thu hồi phiên đăng nhập", error = ex.Message });
            }
        }

        [HttpPost("revoke-all-sessions")]
        [Authorize]
        public async Task<ActionResult> RevokeAllSessions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User không xác định" });
                }

                var currentRefreshToken = Request.Cookies["refreshToken"];

                // Get all active sessions except current one
                var sessions = await _context.JwtTokens
                    .Where(t => 
                        t.UserId.ToString() == userId && 
                        !t.IsRevoked &&
                        t.Token != currentRefreshToken)
                    .ToListAsync();

                // Revoke all sessions
                foreach (var session in sessions)
                {
                    session.IsRevoked = true;
                    session.RevokedAt = DateTime.UtcNow;
                    session.ReasonRevoked = "Manually revoked by user (bulk action)";
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = $"Đã thu hồi {sessions.Count} phiên đăng nhập" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all sessions");
                return StatusCode(500, new { message = "Lỗi server khi thu hồi tất cả phiên đăng nhập", error = ex.Message });
            }
        }

        private void SetRefreshTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Prevents JavaScript access
                Expires = expires,
                Secure = true, // HTTPS only
                SameSite = SameSiteMode.None, // Required for cross-site requests
                Path = "/" // Available throughout the app
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}
