using erp_backend.Models;
using erp_backend.Models.DTOs;

namespace erp_backend.Services
{
	public interface INotificationService
	{
		/// <summary>
		/// T?o thông báo và g?i ??n users (s? d?ng Bulk Insert ?? t?i ?u)
		/// </summary>
		Task<Notification> CreateAndSendNotificationAsync(CreateNotificationDto dto, int createdByUserId);

		/// <summary>
		/// L?y danh sách thông báo c?a user (v?i phân trang)
		/// </summary>
		Task<(List<UserNotificationDto> notifications, int totalCount)> GetUserNotificationsAsync(
			int userId, 
			bool unreadOnly = false, 
			int page = 1, 
			int pageSize = 20);

		/// <summary>
		/// L?y s? l??ng thông báo ch?a ??c
		/// </summary>
		Task<int> GetUnreadCountAsync(int userId);

		/// <summary>
		/// ?ánh d?u m?t thông báo là ?ã ??c
		/// </summary>
		Task MarkAsReadAsync(int notificationId, int userId);

		/// <summary>
		/// ?ánh d?u t?t c? thông báo là ?ã ??c
		/// </summary>
		Task MarkAllAsReadAsync(int userId);

		/// <summary>
		/// Admin xem danh sách thông báo ?ã t?o
		/// </summary>
		Task<(List<NotificationDto> notifications, int totalCount)> GetAllNotificationsAsync(int page = 1, int pageSize = 20);

		/// <summary>
		/// Admin xóa thông báo (soft delete)
		/// </summary>
		Task DeleteNotificationAsync(int notificationId);

		/// <summary>
		/// User t? xóa thông báo c?a mình (ch? xóa ? UserNotifications)
		/// </summary>
		Task DeleteUserNotificationAsync(int userNotificationId, int userId);

		/// <summary>
		/// [ADMIN] L?y chi ti?t tr?ng thái ??c c?a m?t thông báo (ai ?ã ??c, ai ch?a ??c)
		/// </summary>
		Task<NotificationReadStatusDto> GetNotificationReadStatusAsync(int notificationId);
	}
}
