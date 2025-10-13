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

		public User? User { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	}
}
