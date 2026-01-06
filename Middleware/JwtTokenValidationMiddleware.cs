using erp_backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace erp_backend.Middleware
{
    public class JwtTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtTokenValidationMiddleware> _logger;

        public JwtTokenValidationMiddleware(
            RequestDelegate next,
            ILogger<JwtTokenValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Skip validation for non-authenticated endpoints
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Skip validation for login/refresh-token endpoints
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (
                path.Contains("/api/auth/login") ||
                path.Contains("/api/auth/refresh-token") ||
                path.Contains("/api/auth/logout")))
            {
                await _next(context);
                return;
            }

            try
            {
                // Get userId from JWT claims
                var userIdClaim = context.User.FindFirst("userid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    await _next(context);
                    return;
                }

                // Get refresh token from cookie
                var refreshToken = context.Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    // No refresh token, allow access (user might be using only access token)
                    await _next(context);
                    return;
                }

                // Check if the session is still valid in database
                var session = await dbContext.JwtTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => 
                        t.Token == refreshToken && 
                        t.UserId.ToString() == userIdClaim);

                if (session != null)
                {
                    // Check if session is revoked
                    if (session.IsRevoked)
                    {
                        _logger.LogWarning($"Revoked session detected for user {userIdClaim}. Reason: {session.ReasonRevoked}");
                        
                        // Clear the cookie
                        context.Response.Cookies.Delete("refreshToken", new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None
                        });

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Phiên ??ng nh?p c?a b?n ?ã b? thu h?i. Vui lòng ??ng nh?p l?i.",
                            reason = session.ReasonRevoked ?? "Session revoked",
                            revokedAt = session.RevokedAt
                        });
                        return;
                    }

                    // Check if session is expired
                    if (session.Expiration <= DateTime.UtcNow)
                    {
                        _logger.LogWarning($"Expired session detected for user {userIdClaim}");
                        
                        context.Response.Cookies.Delete("refreshToken", new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None
                        });

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Phiên ??ng nh?p ?ã h?t h?n. Vui lòng ??ng nh?p l?i.",
                            expiredAt = session.Expiration
                        });
                        return;
                    }
                }

                // Session is valid, continue
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JWT token validation middleware");
                await _next(context);
            }
        }
    }
}
