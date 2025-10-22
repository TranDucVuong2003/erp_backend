using erp_backend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace erp_backend.Services
{
	public class JwtService
	{
		private readonly IConfiguration _configuration;
		private readonly byte[] _keyBytes;
		private readonly string _issuer;
		private readonly string _audience;
		private readonly int _expiresInMinutes;
		private readonly SymmetricSecurityKey _signingKey;

		public int AccessTokenExpiryMinutes => _expiresInMinutes;

		public JwtService(IConfiguration configuration)
		{
			_configuration = configuration;
			_keyBytes = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSecureKeyWithAtLeast32Characters");
			_issuer = _configuration["Jwt:Issuer"] ?? string.Empty;
			_audience = _configuration["Jwt:Audience"] ?? string.Empty;
			_expiresInMinutes = int.TryParse(_configuration["Jwt:ExpiresInMinutes"], out var m) ? m : 30;
			_signingKey = new SymmetricSecurityKey(_keyBytes);
		}

		public string GenerateAccessToken(User user)
		{
			var tokenHandler = new JwtSecurityTokenHandler();

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Name),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, user.Role)
			};

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddMinutes(_expiresInMinutes),
				Issuer = _issuer,
				Audience = _audience,
				SigningCredentials = new SigningCredentials(
					_signingKey,
					SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		public string GenerateRefreshToken()
		{
			var randomBytes = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}

		public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = _issuer,
				ValidateAudience = true,
				ValidAudience = _audience,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = _signingKey,
				ValidateLifetime = false // Allow expired tokens
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			try
			{
				var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

				if (securityToken is not JwtSecurityToken jwtSecurityToken ||
					!jwtSecurityToken.Header.Alg.Equals(
						SecurityAlgorithms.HmacSha256Signature,
						StringComparison.InvariantCultureIgnoreCase))
				{
					return null;
				}

				return principal;
			}
			catch
			{
				return null;
			}
		}
	}
}