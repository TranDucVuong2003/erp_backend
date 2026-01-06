using System.Security.Cryptography;
using erp_backend.Data;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Services
{
    public interface IAccountActivationService
    {
        Task<string> GenerateActivationTokenAsync(int userId, int expiryHours = 24);
        Task<(bool isValid, User? user, string message)> ValidateAndActivateTokenAsync(string token);
    }

    public class AccountActivationService : IAccountActivationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountActivationService> _logger;

        public AccountActivationService(
            ApplicationDbContext context,
            ILogger<AccountActivationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateActivationTokenAsync(int userId, int expiryHours = 24)
        {
            // Generate secure random token
            var token = GenerateSecureToken();
            
            var activationToken = new AccountActivationToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryHours),
                IsUsed = false
            };

            _context.AccountActivationTokens.Add(activationToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated activation token for user {UserId}, expires at {ExpiresAt}", 
                userId, activationToken.ExpiresAt);

            return token;
        }

        public async Task<(bool isValid, User? user, string message)> ValidateAndActivateTokenAsync(string token)
        {
            try
            {
                // Tìm token trong database
                var activationToken = await _context.AccountActivationTokens
                    .Include(t => t.User)
                        .ThenInclude(u => u!.Role)
                    .Include(t => t.User)
                        .ThenInclude(u => u!.Position)
                    .Include(t => t.User)
                        .ThenInclude(u => u!.Department)
                    .FirstOrDefaultAsync(t => t.Token == token);

                
				if (activationToken == null)
                {
					
					_logger.LogWarning("Invalid activation token attempted: {Token}", token);
                    return (false, null, "Link kích hoạt không hợp lệ");
                }
				

				// Kiểm tra token đã được sử dụng chưa
				if (activationToken.IsUsed)
                {
					_logger.LogWarning("Already used activation token attempted for user {UserId}", activationToken.UserId);
                    return (false, null, "Link kích hoạt đã được sử dụng");
                }

                // Kiểm tra token đã hết hạn chưa
                if (activationToken.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Expired activation token attempted for user {UserId}", activationToken.UserId);
                    return (false, null, "Link kích hoạt đã hết hạn. Vui lòng liên hệ quản trị viên để được hỗ trợ");
                }

                var user = activationToken.User;
                if (user == null)
                {
                    _logger.LogError("User not found for activation token {TokenId}", activationToken.Id);
                    return (false, null, "Không tìm thấy thông tin người dùng");
                }

                //var userId = activationToken.UserId;
                // Kiểm tra trạng thái user
                //if (user.Status != "active")
                //{
                //    _logger.LogWarning("Inactive user {UserId} attempted activation", user.Id);
                //    return (false, null, "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên");
                //}

					var id = activationToken.UserId;

				var existingUser = await _context.Users.FindAsync(id);

				if (existingUser == null)
				{
					return (false, null, "Người dùng không tồn tại");
				}
				existingUser.Status = "active";
                _context.Users.Update(existingUser);
				await _context.SaveChangesAsync();

				// Đánh dấu token đã được sử dụng
				activationToken.IsUsed = true;
                activationToken.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();


                _logger.LogInformation("Successfully validated activation token for user {UserId}", user.Id);
                return (true, user, "Xác thực thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating activation token");
                return (false, null, "Lỗi hệ thống khi xác thực token");
            }
        }

        private string GenerateSecureToken()
        {
            // Generate 32 bytes random token
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            // Convert to base64 and make URL-safe
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
