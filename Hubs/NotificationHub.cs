using Microsoft.AspNetCore.SignalR;

namespace erp_backend.Hubs
{
	/// <summary>
	/// SignalR Hub ?? g?i thông báo real-time ??n users
	/// </summary>
	public class NotificationHub : Hub
	{
		private readonly ILogger<NotificationHub> _logger;

		public NotificationHub(ILogger<NotificationHub> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// User tham gia nhóm ?? nh?n thông báo (theo UserId)
		/// </summary>
		public async Task JoinUserGroup(int userId)
		{
			var groupName = $"User_{userId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
			_logger.LogInformation("? User {UserId} (ConnectionId: {ConnectionId}) joined notification group", userId, Context.ConnectionId);
		}

		/// <summary>
		/// User r?i kh?i nhóm
		/// </summary>
		public async Task LeaveUserGroup(int userId)
		{
			var groupName = $"User_{userId}";
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
			_logger.LogInformation("?? User {UserId} (ConnectionId: {ConnectionId}) left notification group", userId, Context.ConnectionId);
		}

		public override async Task OnConnectedAsync()
		{
			_logger.LogInformation("?? Client connected: {ConnectionId}", Context.ConnectionId);
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			_logger.LogInformation("?? Client disconnected: {ConnectionId}", Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}
	}
}
