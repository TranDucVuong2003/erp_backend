using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
	/// <summary>
	/// DTO ?? t?o thông báo m?i
	/// </summary>
	public class CreateNotificationDto
	{
		[Required(ErrorMessage = "Tiêu ?? không ???c ?? tr?ng")]
		[StringLength(200)]
		public string Title { get; set; } = string.Empty;

		[Required(ErrorMessage = "N?i dung không ???c ?? tr?ng")]
		[StringLength(2000)]
		public string Content { get; set; } = string.Empty;

		/// <summary>
		/// Target: All, Role, Department, Specific
		/// </summary>
		[Required]
		[StringLength(50)]
		public string TargetType { get; set; } = "All";

		/// <summary>
		/// Danh sách UserIds (n?u TargetType = Specific)
		/// Danh sách RoleIds (n?u TargetType = Role)
		/// Danh sách DepartmentIds (n?u TargetType = Department)
		/// </summary>
		public List<int>? TargetIds { get; set; }
	}

	/// <summary>
	/// DTO cho Admin xem danh sách thông báo ?ã t?o
	/// </summary>
	public class NotificationDto
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedByUserName { get; set; } = string.Empty;
		public int TotalRecipients { get; set; }
		public int ReadCount { get; set; }
	}

	/// <summary>
	/// DTO cho User xem thông báo c?a mình
	/// </summary>
	public class UserNotificationDto
	{
		public int Id { get; set; }
		public int NotificationId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public bool IsRead { get; set; }
		public DateTime? ReadAt { get; set; }
	}

	/// <summary>
	/// DTO ?? ?ánh d?u ?ã ??c
	/// </summary>
	public class MarkAsReadDto
	{
		[Required]
		public int NotificationId { get; set; }
	}

	/// <summary>
	/// DTO th?ng kê thông báo
	/// </summary>
	public class NotificationStatsDto
	{
		public int UnreadCount { get; set; }
		public int TotalCount { get; set; }
	}

	/// <summary>
	/// DTO cho Admin xem chi ti?t ng??i dùng ?ã ??c/ch?a ??c m?t thông báo
	/// </summary>
	public class NotificationReadStatusDto
	{
		public int NotificationId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public int TotalRecipients { get; set; }
		public int ReadCount { get; set; }
		public int UnreadCount { get; set; }

		public List<UserReadInfoDto> ReadUsers { get; set; } = new();
		public List<UserReadInfoDto> UnreadUsers { get; set; } = new();
	}

	/// <summary>
	/// DTO thông tin chi ti?t user và tr?ng thái ??c
	/// </summary>
	public class UserReadInfoDto
	{
		public int UserId { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string DepartmentName { get; set; } = string.Empty;
		public string PositionName { get; set; } = string.Empty;
		public bool IsRead { get; set; }
		public DateTime? ReadAt { get; set; }
	}
}
