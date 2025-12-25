using erp_backend.Data;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Services
{
    public interface IPasswordResetOtpService
    {
        Task<(bool success, string otp, DateTime expiresAt, string message)> GenerateOtpAsync(string email, string? ipAddress, string? userAgent);
        Task<(bool isValid, string message)> ValidateOtpAsync(string email, string otp);
        Task<bool> MarkOtpAsUsedAsync(string email, string otp);
        Task CleanupExpiredOtpsAsync();
    }

    public class PasswordResetOtpService : IPasswordResetOtpService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PasswordResetOtpService> _logger;
        private const int OTP_EXPIRY_MINUTES = 5; // OTP h?t h?n sau 5 phút
        private const int MAX_OTP_ATTEMPTS = 3; // S? l?n nh?p OTP t?i ?a

        public PasswordResetOtpService(ApplicationDbContext context, ILogger<PasswordResetOtpService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// T?o mã OTP m?i cho email
        /// </summary>
        public async Task<(bool success, string otp, DateTime expiresAt, string message)> GenerateOtpAsync(
            string email, 
            string? ipAddress, 
            string? userAgent)
        {
            try
            {
                // Ki?m tra user có t?n t?i không
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    _logger.LogWarning("Attempt to generate OTP for non-existent email: {Email}", email);
                    // Không tr? v? l?i c? th? ?? tránh l? thông tin user
                    return (false, string.Empty, DateTime.UtcNow, "Email không t?n t?i trong h? th?ng");
                }

                // Ki?m tra xem có OTP ch?a s? d?ng và ch?a h?t h?n không
                var existingOtp = await _context.PasswordResetOtps
                    .Where(o => o.Email == email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (existingOtp != null)
                {
                    // Ki?m tra n?u OTP còn hi?u l?c ít nh?t 2 phút thì tr? v? OTP c?
                    if (existingOtp.ExpiresAt > DateTime.UtcNow.AddMinutes(2))
                    {
                        _logger.LogInformation("Reusing existing OTP for email: {Email}", email);
                        return (true, existingOtp.OtpCode, existingOtp.ExpiresAt, 
                            "Mã OTP ?ã ???c g?i tr??c ?ó. Vui lòng ki?m tra email");
                    }
                }

                // T?o mã OTP 6 ch? s? ng?u nhiên
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                var expiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES);

                var passwordResetOtp = new PasswordResetOtp
                {
                    Email = email,
                    OtpCode = otpCode,
                    ExpiresAt = expiresAt,
                    IsUsed = false,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PasswordResetOtps.Add(passwordResetOtp);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Generated OTP for email: {Email}, expires at: {ExpiresAt}", email, expiresAt);

                return (true, otpCode, expiresAt, "Mã OTP ?ã ???c t?o thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for email: {Email}", email);
                return (false, string.Empty, DateTime.UtcNow, "L?i khi t?o mã OTP");
            }
        }

        /// <summary>
        /// Xác th?c mã OTP
        /// </summary>
        public async Task<(bool isValid, string message)> ValidateOtpAsync(string email, string otp)
        {
            try
            {
                var otpRecord = await _context.PasswordResetOtps
                    .Where(o => o.Email == email && o.OtpCode == otp && !o.IsUsed)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otpRecord == null)
                {
                    _logger.LogWarning("Invalid OTP attempt for email: {Email}", email);
                    return (false, "Mã OTP không chính xác");
                }

                if (otpRecord.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Expired OTP attempt for email: {Email}", email);
                    return (false, "Mã OTP ?ã h?t h?n. Vui lòng yêu c?u mã m?i");
                }

                _logger.LogInformation("Valid OTP for email: {Email}", email);
                return (true, "Mã OTP h?p l?");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OTP for email: {Email}", email);
                return (false, "L?i khi xác th?c mã OTP");
            }
        }

        /// <summary>
        /// ?ánh d?u OTP ?ã s? d?ng
        /// </summary>
        public async Task<bool> MarkOtpAsUsedAsync(string email, string otp)
        {
            try
            {
                var otpRecord = await _context.PasswordResetOtps
                    .Where(o => o.Email == email && o.OtpCode == otp && !o.IsUsed)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otpRecord == null)
                    return false;

                otpRecord.IsUsed = true;
                otpRecord.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Marked OTP as used for email: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking OTP as used for email: {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// D?n d?p các OTP ?ã h?t h?n
        /// </summary>
        public async Task CleanupExpiredOtpsAsync()
        {
            try
            {
                var expiredOtps = await _context.PasswordResetOtps
                    .Where(o => o.ExpiresAt < DateTime.UtcNow.AddDays(-7)) // Xóa OTP c? h?n 7 ngày
                    .ToListAsync();

                if (expiredOtps.Any())
                {
                    _context.PasswordResetOtps.RemoveRange(expiredOtps);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Cleaned up {Count} expired OTPs", expiredOtps.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired OTPs");
            }
        }
    }
}
