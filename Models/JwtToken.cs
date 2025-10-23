using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
	public class JwtToken
	{
		public int Id { get; set; }

		[Required]
		public string Token { get; set; } = string.Empty;

		public DateTime Expiration { get; set; }

		[ForeignKey("User")]
		public int UserId { get; set; }
		public virtual User? User { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Thông tin bổ sung cho tracking
		public string? DeviceInfo { get; set; }
		public string? IpAddress { get; set; }
		public string? UserAgent { get; set; }
		public bool IsUsed { get; set; } = false;
		public bool IsRevoked { get; set; } = false;
		public DateTime? RevokedAt { get; set; }
		public string? ReasonRevoked { get; set; }
		public string? ReplacedByToken { get; set; }
	}
}