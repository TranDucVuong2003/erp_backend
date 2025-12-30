using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using erp_backend.Hubs;

namespace erp_backend.Services
{
	public class NotificationService : INotificationService
	{
		private readonly ApplicationDbContext _context;
		private readonly IHubContext<NotificationHub> _hubContext;
		private readonly ILogger<NotificationService> _logger;
		private readonly IEmailService _emailService;

		public NotificationService(
			ApplicationDbContext context,
			IHubContext<NotificationHub> hubContext,
			ILogger<NotificationService> logger,
			IEmailService emailService)
		{
			_context = context;
			_hubContext = hubContext;
			_logger = logger;
			_emailService = emailService;
		}

		public async Task<Notification> CreateAndSendNotificationAsync(CreateNotificationDto dto, int createdByUserId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 1. T?o Notification (1 dòng duy nh?t)
				var notification = new Notification
				{
					Title = dto.Title,
					Content = dto.Content,
					CreatedByUserId = createdByUserId,
					IsActive = true,
					CreatedAt = DateTime.UtcNow
				};

				_context.Notifications.Add(notification);
				await _context.SaveChangesAsync();

				_logger.LogInformation("? Created notification {NotificationId} by User {UserId}", notification.Id, createdByUserId);

				// 2. Xác ??nh danh sách UserIds c?n g?i
				var targetUserIds = await GetTargetUserIdsAsync(dto.TargetType, dto.TargetIds);

				if (targetUserIds.Count == 0)
				{
					_logger.LogWarning("?? No target users found for notification {NotificationId}", notification.Id);
					await transaction.CommitAsync();
					return notification;
				}

				_logger.LogInformation("?? Sending notification {NotificationId} to {Count} users", notification.Id, targetUserIds.Count);

				// 3. Bulk Insert UserNotifications (t?i ?u hi?u su?t)
				var userNotifications = targetUserIds.Select(userId => new UserNotification
				{
					NotificationId = notification.Id,
					UserId = userId,
					IsRead = false,
					ReadAt = null,
					CreatedAt = DateTime.UtcNow
				}).ToList();

				await _context.UserNotifications.AddRangeAsync(userNotifications);
				await _context.SaveChangesAsync();

				_logger.LogInformation("? Inserted {Count} user notification records", userNotifications.Count);

				await transaction.CommitAsync();

				// L?y thông tin users CÓ EMAIL tr??c khi ch?y background task
				var usersWithEmail = await _context.Users
					.Where(u => targetUserIds.Contains(u.Id))
					.Where(u => !string.IsNullOrEmpty(u.Email))
					.Select(u => new { u.Id, u.Name, u.Email, u.SecondaryEmail })
					.ToListAsync();

				// 4. G?i SignalR notification real-time (không ch?n transaction)
				_ = Task.Run(async () =>
				{
					try
					{
						await SendRealtimeNotificationsAsync(notification, targetUserIds);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "? Error sending realtime notifications for {NotificationId}", notification.Id);
					}
				});

				// 5. G?i email cho t?t c? ng??i nh?n (không ch?n transaction)
				// Pass usersWithEmail thay vì query trong background task
				_ = Task.Run(async () =>
				{
					try
					{
						await SendEmailNotificationsAsync(notification, usersWithEmail);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "?? Error sending email notifications for {NotificationId}", notification.Id);
					}
				});

				return notification;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "? Error creating notification");
				throw;
			}
		}

		private async Task<List<int>> GetTargetUserIdsAsync(string targetType, List<int>? targetIds)
		{
			return targetType switch
			{
				"All" => await _context.Users
					.Where(u => u.Status == "active")
					.Select(u => u.Id)
					.ToListAsync(),

				"Role" => targetIds != null && targetIds.Any()
					? await _context.Users
						.Where(u => u.Status == "active" && targetIds.Contains(u.RoleId))
						.Select(u => u.Id)
						.ToListAsync()
					: new List<int>(),

				"Department" => targetIds != null && targetIds.Any()
					? await _context.Users
						.Where(u => u.Status == "active" && targetIds.Contains(u.DepartmentId))
						.Select(u => u.Id)
						.ToListAsync()
					: new List<int>(),

				"Specific" => targetIds ?? new List<int>(),

				_ => new List<int>()
			};
		}

		private async Task SendRealtimeNotificationsAsync(Notification notification, List<int> targetUserIds)
		{
			var tasks = targetUserIds.Select(async userId =>
			{
				try
				{
					var groupName = $"User_{userId}";
					await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
					{
						id = notification.Id,
						title = notification.Title,
						content = notification.Content,
						createdAt = notification.CreatedAt
					});
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "?? Failed to send notification to User {UserId}", userId);
				}
			});

			await Task.WhenAll(tasks);
			_logger.LogInformation("?? Sent realtime notifications to {Count} users", targetUserIds.Count);
		}

		private async Task SendEmailNotificationsAsync(
			Notification notification, 
			IEnumerable<dynamic> usersWithEmail)
		{
			try
			{
				var usersList = usersWithEmail.ToList();
				
				if (!usersList.Any())
				{
					_logger.LogWarning("?? No users with email found for notification {NotificationId}", notification.Id);
					return;
				}

				_logger.LogInformation("?? Preparing to send emails to {Count} users for notification {NotificationId}", 
					usersList.Count, notification.Id);

				// G?i email song song cho t?t c? users
				var emailTasks = new List<Task>();

				foreach (var user in usersList)
				{
					// G?i email chính
					if (!string.IsNullOrEmpty(user.Email))
					{
						var emailTask = _emailService.SendNotificationEmailAsync(
							user.Email,
							user.Name,
							notification.Title,
							notification.Content,
							notification.CreatedAt
						);
						emailTasks.Add(emailTask);
					}

					// G?i thêm email ph? n?u có
					if (!string.IsNullOrEmpty(user.SecondaryEmail) && 
					    user.SecondaryEmail != user.Email)
					{
						var secondaryEmailTask = _emailService.SendNotificationEmailAsync(
							user.SecondaryEmail,
							user.Name,
							notification.Title,
							notification.Content,
							notification.CreatedAt
						);
						emailTasks.Add(secondaryEmailTask);
					}
				}

				// ??i t?t c? email g?i xong
				await Task.WhenAll(emailTasks);

				_logger.LogInformation("?? Completed sending emails for notification {NotificationId} to {UserCount} users ({EmailCount} emails)", 
					notification.Id, usersList.Count, emailTasks.Count);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "?? Error in SendEmailNotificationsAsync for notification {NotificationId}", notification.Id);
			}
		}

		public async Task<(List<UserNotificationDto> notifications, int totalCount)> GetUserNotificationsAsync(
			int userId,
			bool unreadOnly = false,
			int page = 1,
			int pageSize = 20)
		{
			var query = _context.UserNotifications
				.Include(un => un.Notification)
				.Where(un => un.UserId == userId)
				.Where(un => un.Notification != null && un.Notification.IsActive);

			if (unreadOnly)
			{
				query = query.Where(un => !un.IsRead);
			}

			var totalCount = await query.CountAsync();

			var notifications = await query
				.OrderByDescending(un => un.CreatedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(un => new UserNotificationDto
				{
					Id = un.Id,
					NotificationId = un.Notification!.Id,
					Title = un.Notification!.Title,
					Content = un.Notification.Content,
					CreatedAt = un.Notification.CreatedAt,
					IsRead = un.IsRead,
					ReadAt = un.ReadAt
				})
				.ToListAsync();

			return (notifications, totalCount);
		}

		public async Task<int> GetUnreadCountAsync(int userId)
		{
			return await _context.UserNotifications
				.Include(un => un.Notification)
				.Where(un => un.UserId == userId)
				.Where(un => !un.IsRead)
				.Where(un => un.Notification != null && un.Notification.IsActive)
				.CountAsync();
		}

		public async Task MarkAsReadAsync(int notificationId, int userId)
		{
			var userNotification = await _context.UserNotifications
				.FirstOrDefaultAsync(un => un.NotificationId == notificationId && un.UserId == userId);

			if (userNotification != null && !userNotification.IsRead)
			{
				userNotification.IsRead = true;
				userNotification.ReadAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				_logger.LogInformation("? User {UserId} marked notification {NotificationId} as read", userId, notificationId);
			}
		}

		public async Task MarkAllAsReadAsync(int userId)
		{
			var unreadNotifications = await _context.UserNotifications
				.Where(un => un.UserId == userId && !un.IsRead)
				.ToListAsync();

			if (unreadNotifications.Any())
			{
				foreach (var notification in unreadNotifications)
				{
					notification.IsRead = true;
					notification.ReadAt = DateTime.UtcNow;
				}

				await _context.SaveChangesAsync();
				_logger.LogInformation("? User {UserId} marked {Count} notifications as read", userId, unreadNotifications.Count);
			}
		}

		public async Task<(List<NotificationDto> notifications, int totalCount)> GetAllNotificationsAsync(int page = 1, int pageSize = 20)
		{
			var query = _context.Notifications
				.Include(n => n.CreatedByUser)
				.Where(n => n.IsActive);

			var totalCount = await query.CountAsync();

			var notifications = await query
				.OrderByDescending(n => n.CreatedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(n => new NotificationDto
				{
					Id = n.Id,
					Title = n.Title,
					Content = n.Content,
					IsActive = n.IsActive,
					CreatedAt = n.CreatedAt,
					CreatedByUserName = n.CreatedByUser != null ? n.CreatedByUser.Name : "Unknown",
					TotalRecipients = n.UserNotifications.Count,
					ReadCount = n.UserNotifications.Count(un => un.IsRead)
				})
				.ToListAsync();

			return (notifications, totalCount);
		}

		public async Task DeleteNotificationAsync(int notificationId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 1. Xóa t?t c? UserNotifications liên quan (hard delete)
				var userNotifications = await _context.UserNotifications
					.Where(un => un.NotificationId == notificationId)
					.ToListAsync();

				if (userNotifications.Any())
				{
					_context.UserNotifications.RemoveRange(userNotifications);
					_logger.LogInformation("??? Deleted {Count} user notification records for notification {NotificationId}", 
						userNotifications.Count, notificationId);
				}

				// 2. Xóa Notification (hard delete)
				var notification = await _context.Notifications.FindAsync(notificationId);
				if (notification != null)
				{
					_context.Notifications.Remove(notification);
					await _context.SaveChangesAsync();
					_logger.LogInformation("??? Deleted notification {NotificationId}", notificationId);
				}

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "? Error deleting notification {NotificationId}", notificationId);
				throw;
			}
		}

		public async Task DeleteUserNotificationAsync(int userNotificationId, int userId)
		{
			var userNotification = await _context.UserNotifications
				.FirstOrDefaultAsync(un => un.Id == userNotificationId && un.UserId == userId);

			if (userNotification == null)
			{
				throw new InvalidOperationException("Th?ng b?o kh?ng t?n t?i ho?c kh?ng thu?c v? user n?y");
			}

			_context.UserNotifications.Remove(userNotification);
			await _context.SaveChangesAsync();

			_logger.LogInformation("??? User {UserId} deleted user notification {UserNotificationId}", userId, userNotificationId);
		}

		public async Task<NotificationReadStatusDto> GetNotificationReadStatusAsync(int notificationId)
		{
			// L?y thông tin notification
			var notification = await _context.Notifications
				.Include(n => n.UserNotifications)
					.ThenInclude(un => un.User)
						.ThenInclude(u => u!.Department)
				.Include(n => n.UserNotifications)
					.ThenInclude(un => un.User)
						.ThenInclude(u => u!.Position)
				.FirstOrDefaultAsync(n => n.Id == notificationId);

			if (notification == null)
			{
				throw new InvalidOperationException($"Không tìm th?y thông báo v?i ID {notificationId}");
			}

			// Phân lo?i users ?ã ??c và ch?a ??c
			var readUsers = notification.UserNotifications
				.Where(un => un.IsRead)
				.Select(un => new UserReadInfoDto
				{
					UserId = un.UserId,
					UserName = un.User?.Name ?? "Unknown",
					Email = un.User?.Email ?? "",
					DepartmentName = un.User?.Department?.Name ?? "N/A",
					PositionName = un.User?.Position?.PositionName ?? "N/A",
					IsRead = true,
					ReadAt = un.ReadAt
				})
				.OrderBy(u => u.UserName)
				.ToList();

			var unreadUsers = notification.UserNotifications
				.Where(un => !un.IsRead)
				.Select(un => new UserReadInfoDto
				{
					UserId = un.UserId,
					UserName = un.User?.Name ?? "Unknown",
					Email = un.User?.Email ?? "",
					DepartmentName = un.User?.Department?.Name ?? "N/A",
					PositionName = un.User?.Position?.PositionName ?? "N/A",
					IsRead = false,
					ReadAt = null
				})
				.OrderBy(u => u.UserName)
				.ToList();

			var result = new NotificationReadStatusDto
			{
				NotificationId = notification.Id,
				Title = notification.Title,
				Content = notification.Content,
				CreatedAt = notification.CreatedAt,
				TotalRecipients = notification.UserNotifications.Count,
				ReadCount = readUsers.Count,
				UnreadCount = unreadUsers.Count,
				ReadUsers = readUsers,
				UnreadUsers = unreadUsers
			};

			_logger.LogInformation("?? Retrieved read status for notification {NotificationId}: {ReadCount}/{TotalCount} read", 
				notificationId, result.ReadCount, result.TotalRecipients);

			return result;
		}
	}
}
