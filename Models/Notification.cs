using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
	/// <summary>
	/// B?ng l?u n?i dung thông báo (t?o 1 l?n, nhi?u ng??i nh?n)
	/// </summary>
	[Table("notifications")]
	public class Notification
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(200)]
		public string Title { get; set; } = string.Empty;

		[Required]
		[StringLength(2000)]
		public string Content { get; set; } = string.Empty;

		public bool IsActive { get; set; } = true;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		public int CreatedByUserId { get; set; }

		// Navigation properties
		public virtual User? CreatedByUser { get; set; }
		public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
	}
}
