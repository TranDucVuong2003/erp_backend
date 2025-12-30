using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
	/// <summary>
	/// B?ng l?u tr?ng thái thông báo c?a t?ng user (m?i ng??i nh?n = 1 dòng)
	/// </summary>
	[Table("user_notifications")]
	public class UserNotification
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int NotificationId { get; set; }

		[Required]
		public int UserId { get; set; }

		public bool IsRead { get; set; } = false;

		public DateTime? ReadAt { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public virtual Notification? Notification { get; set; }
		public virtual User? User { get; set; }
	}
}
