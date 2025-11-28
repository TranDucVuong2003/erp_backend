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
        private readonly IAccountActivationService _activationService;

        public AuthController(
            ApplicationDbContext context,
            JwtService jwtService,
            ILogger<AuthController> logger,
            IAccountActivationService activationService)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _activationService = activationService;
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
                    .Include(u => u.Role)
                    .Include(u => u.Position)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return BadRequest(new { message = "Email hoặc mật khẩu không chính xác" });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    return BadRequest(new { message = "Email hoặc mật khẩu không chính xác" });
                }

                // Check if user account is active
                if (user.Status != "active")
                {
                    return BadRequest(new { message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin để được hỗ trợ." });
                }

                // Check or create ActiveAccount record
                var activeAccount = await _context.ActiveAccounts
                    .FirstOrDefaultAsync(a => a.UserId == user.Id);

                string message = "Đăng nhập thành công";
                
                if (activeAccount.FirstLogin == true)
                {

                    message = "Bạn phải đổi mật khẩu trước khi đăng nhập vào hệ thống";
                }
                else if(activeAccount.FirstLogin.ToString() == "")
                {
                    message = "Bạn phải đổi mật khẩu trước khi đăng nhập vào hệ thống";
                }
                else
                {
					message = "Đăng nhập thành công.";
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
                    FirstLogin = activeAccount.FirstLogin,
                    Message = message,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Position = user.Position?.PositionName ?? string.Empty,
                        Role = user.Role?.Name ?? string.Empty,
                        Status = user.Status
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
                        .ThenInclude(u => u.Role)
                    .Include(t => t.User)
                        .ThenInclude(u => u.Position)
                    .Include(t => t.User)
                        .ThenInclude(u => u.Department)
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

                // Check if user account is active
                if (user.Status != "active")
                {
                    // Revoke the current token
                    storedToken.IsRevoked = true;
                    storedToken.RevokedAt = DateTime.UtcNow;
                    storedToken.ReasonRevoked = "User account is inactive";
                    await _context.SaveChangesAsync();
                    
                    return Unauthorized(new { message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin để được hỗ trợ." });
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

        [HttpGet("admin/all-sessions")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<SessionInfo>>> GetAllSessions(
            [FromQuery] int? userId = null,
            [FromQuery] bool includeRevoked = false,
            [FromQuery] bool includeExpired = false)
        {
            try
            {
                var query = _context.JwtTokens
                    .Include(t => t.User)
                        .ThenInclude(u => u.Role)
                    .AsQueryable();

                // Filter by specific user if provided
                if (userId.HasValue)
                {
                    query = query.Where(t => t.UserId == userId.Value);
                }

                // Filter revoked sessions
                if (!includeRevoked)
                {
                    query = query.Where(t => !t.IsRevoked);
                }

                // Filter expired sessions
                if (!includeExpired)
                {
                    query = query.Where(t => t.Expiration > DateTime.UtcNow);
                }

                var currentRefreshToken = Request.Cookies["refreshToken"];

                var sessions = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new SessionInfo
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        UserName = t.User != null ? t.User.Name : string.Empty,
                        UserEmail = t.User != null ? t.User.Email : string.Empty,
                        UserRole = t.User != null && t.User.Role != null ? t.User.Role.Name : string.Empty,
                        DeviceInfo = t.DeviceInfo ?? "Unknown Device",
                        IpAddress = t.IpAddress ?? "Unknown IP",
                        UserAgent = t.UserAgent,
                        CreatedAt = t.CreatedAt,
                        ExpiresAt = t.Expiration,
                        IsUsed = t.IsUsed,
                        IsRevoked = t.IsRevoked,
                        RevokedAt = t.RevokedAt,
                        ReasonRevoked = t.ReasonRevoked,
                        IsActive = !t.IsRevoked && t.Expiration > DateTime.UtcNow,
                        IsCurrentSession = t.Token == currentRefreshToken
                    })
                    .ToListAsync();

                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all sessions");
                return StatusCode(500, new { message = "Lỗi server khi lấy thông tin tất cả phiên đăng nhập", error = ex.Message });
            }
        }

        [HttpPost("admin/revoke-session/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AdminRevokeSession(int id, [FromBody] RevokeSessionRequest? request = null)
        {
            try
            {
                var session = await _context.JwtTokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (session == null)
                {
                    return NotFound(new { message = "Không tìm thấy phiên đăng nhập" });
                }

                session.IsRevoked = true;
                session.RevokedAt = DateTime.UtcNow;
                session.ReasonRevoked = request?.Reason ?? "Revoked by admin";
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Thu hồi phiên đăng nhập thành công",
                    session = new
                    {
                        id = session.Id,
                        userId = session.UserId,
                        userName = session.User?.Name,
                        deviceInfo = session.DeviceInfo,
                        revokedAt = session.RevokedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking session by admin");
                return StatusCode(500, new { message = "Lỗi server khi thu hồi phiên đăng nhập", error = ex.Message });
            }
        }

        [HttpGet("verify-activation-token")]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyActivationToken([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { message = "Token không được để trống" });
                }

                var (isValid, user, message) = await _activationService.ValidateAndActivateTokenAsync(token);

                if (!isValid)
                {
                    return BadRequest(new { message });
                }

                // Trả về thông tin user để frontend hiển thị
                return Ok(new
                {
                    message = "Token hợp lệ",
                    user = new
                    {
                        email = user!.Email,
                        name = user.Name,
                        firstLogin = true // Để frontend biết cần đổi mật khẩu
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying activation token");
                return StatusCode(500, new { message = "Lỗi server khi xác thực token" });
            }
        }

        [HttpPost("change-password-first-time")]
        [Authorize]
        public async Task<ActionResult> ChangePasswordFirstTime([FromBody] ChangePasswordFirstTimeRequest request)
        {
            try
            {
                // Lấy user ID từ JWT token
                var userIdClaim = User.FindFirst("userid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { message = "Mật khẩu mới không được để trống" });
                }

                if (request.NewPassword.Length < 8)
                {
                    return BadRequest(new { message = "Mật khẩu phải có ít nhất 8 ký tự" });
                }

                // Tìm user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                // Kiểm tra ActiveAccount
                var activeAccount = await _context.ActiveAccounts
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (activeAccount == null)
                {
                    return BadRequest(new { message = "Không tìm thấy thông tin kích hoạt tài khoản" });
                }

                if (!activeAccount.FirstLogin)
                {
                    return BadRequest(new { message = "Tài khoản đã được kích hoạt trước đó" });
                }

                // Cập nhật mật khẩu
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // Đánh dấu không còn là lần đăng nhập đầu
                activeAccount.FirstLogin = false;

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} changed password on first login", userId);

                return Ok(new
                {
                    message = "Đổi mật khẩu thành công. Bạn có thể đăng nhập với mật khẩu mới"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password on first login");
                return StatusCode(500, new { message = "Lỗi server khi đổi mật khẩu" });
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
