using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using erp_backend.Models.DTOs;
using erp_backend.Services;
using System.Security.Claims;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class NotificationsController : ControllerBase
	{
		private readonly INotificationService _notificationService;
		private readonly ILogger<NotificationsController> _logger;

		public NotificationsController(
			INotificationService notificationService,
			ILogger<NotificationsController> logger)
		{
			_notificationService = notificationService;
			_logger = logger;
		}

		/// <summary>
		/// POST /api/notifications
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
		{
			try
			{
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (!int.TryParse(userIdClaim, out int userId))
				{
					return Unauthorized(new { success = false, message = "Invalid user token" });
				}

				var notification = await _notificationService.CreateAndSendNotificationAsync(dto, userId);

				return Ok(new
				{
					success = true,
					message = "Thông báo đã được tạo và gửi thành công",
					data = notification
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating notification");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi tạo thông báo",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// [USER] Lấy danh sách thông báo của user hiện tại (có phân trang)
		/// GET /api/notifications/my-notifications?unreadOnly=false&page=1&pageSize=20
		/// </summary>
		[HttpGet("my-notifications")]
		public async Task<IActionResult> GetMyNotifications(
			[FromQuery] bool unreadOnly = false,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 20)
		{
			try
			{
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (!int.TryParse(userIdClaim, out int userId))
				{
					return Unauthorized(new { success = false, message = "Invalid user token" });
				}

				var (notifications, totalCount) = await _notificationService.GetUserNotificationsAsync(
					userId, unreadOnly, page, pageSize);

				return Ok(new
				{
					success = true,
					data = notifications,
					pagination = new
					{
						page,
						pageSize,
						totalCount,
						totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user notifications");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi lấy thông báo",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// [USER] Lấy số lượng thông báo chưa đọc
		/// GET /api/notifications/unread-count
		/// </summary>
		[HttpGet("unread-count")]
		public async Task<IActionResult> GetUnreadCount()
		{
			try
			{
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (!int.TryParse(userIdClaim, out int userId))
				{
					return Unauthorized(new { success = false, message = "Invalid user token" });
				}

				var count = await _notificationService.GetUnreadCountAsync(userId);

				return Ok(new
				{
					success = true,
					data = new { unreadCount = count }
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting unread count");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi lấy số thông báo chưa đọc",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// [USER] Đánh dấu một thông báo là đã đọc
		/// POST /api/notifications/mark-as-read
		/// </summary>
		[HttpPost("mark-as-read")]
		public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadDto dto)
		{
			try
			{
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (!int.TryParse(userIdClaim, out int userId))
				{
					return Unauthorized(new { success = false, message = "Invalid user token" });
				}

				await _notificationService.MarkAsReadAsync(dto.NotificationId, userId);

				return Ok(new
				{
					success = true,
					message = "Đã đánh dấu thông báo là đã đọc"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error marking notification as read");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi đánh dấu đã đọc",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// [USER] Đánh dấu tất cả thông báo là đã đọc
		/// POST /api/notifications/mark-all-as-read
		/// </summary>
		[HttpPost("mark-all-as-read")]
		public async Task<IActionResult> MarkAllAsRead()
		{
			try
			{
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (!int.TryParse(userIdClaim, out int userId))
				{
					return Unauthorized(new { success = false, message = "Invalid user token" });
				}

				await _notificationService.MarkAllAsReadAsync(userId);

				return Ok(new
				{
					success = true,
					message = "Đã đánh dấu tất cả thông báo là đã đọc"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error marking all notifications as read");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi đánh dấu tất cả đã đọc",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// [USER] Xóa thông báo của mình
		/// DELETE /api/notifications/my-notifications/{id}
		/// </summary>
		[HttpDelete("my-notifications/{id}")]
		public async Task<IActionResult> DeleteMyNotification(int id)
		{
			try
			{
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (!int.TryParse(userIdClaim, out int userId))
				{
					return Unauthorized(new { success = false, message = "Invalid user token" });
				}

				await _notificationService.DeleteUserNotificationAsync(id, userId);

				return Ok(new
				{
					success = true,
					message = "Đã xóa thông báo thành công"
				});
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new
				{
					success = false,
					message = ex.Message
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting user notification");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi xóa thông báo",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// [ADMIN] Xem tất cả thông báo đã tạo (có phân trang)
		/// GET /api/notifications/admin/all?page=1&pageSize=20
		/// </summary>
		[HttpGet("admin/all")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAllNotifications(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 20)
		{
			try
			{
				var (notifications, totalCount) = await _notificationService.GetAllNotificationsAsync(page, pageSize);

				return Ok(new
				{
					success = true,
					data = notifications,
					pagination = new
					{
						page,
						pageSize,
						totalCount,
						totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting all notifications");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi lấy danh sách thông báo",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// [ADMIN] Xem chi tiết trạng thái đọc của một thông báo (ai đã đọc, ai chưa đọc)
		/// GET /api/notifications/{id}/read-status
		/// </summary>
		[HttpGet("{id}/read-status")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetNotificationReadStatus(int id)
		{
			try
			{
				var readStatus = await _notificationService.GetNotificationReadStatusAsync(id);

				return Ok(new
				{
					success = true,
					message = "Lấy chi tiết trạng thái đọc thành công",
					data = readStatus
				});
			}
			catch (InvalidOperationException ex)
			{
				return NotFound(new
				{
					success = false,
					message = ex.Message
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting notification read status");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi lấy trạng thái đọc thông báo",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// [ADMIN] Xóa thông báo (xóa cả Notification và UserNotifications)
		/// DELETE /api/notifications/{id}
		/// </summary>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteNotification(int id)
		{
			try
			{
				await _notificationService.DeleteNotificationAsync(id);

				return Ok(new
				{
					success = true,
					message = "Đã xóa thông báo thành công"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting notification");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi xóa thông báo",
					error = ex.Message
				});
			}
		}
	}
}
