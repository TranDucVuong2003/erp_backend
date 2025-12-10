using Microsoft.AspNetCore.SignalR;

namespace erp_backend.Hubs
{
	/// <summary>
	/// SignalR Hub ?? g?i thông báo real-time khi thanh toán thành công
	/// </summary>
	public class PaymentHub : Hub
	{
		private readonly ILogger<PaymentHub> _logger;

		public PaymentHub(ILogger<PaymentHub> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Client tham gia nhóm theo ContractId ?? nh?n thông báo
		/// </summary>
		public async Task JoinContractGroup(int contractId)
		{
			var groupName = $"Contract_{contractId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
			_logger.LogInformation("?? Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
		}

		/// <summary>
		/// Client r?i kh?i nhóm
		/// </summary>
		public async Task LeaveContractGroup(int contractId)
		{
			var groupName = $"Contract_{contractId}";
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
			_logger.LogInformation("?? Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
		}

		/// <summary>
		/// X? lý khi client k?t n?i
		/// </summary>
		public override async Task OnConnectedAsync()
		{
			_logger.LogInformation("? Client connected: {ConnectionId}", Context.ConnectionId);
			await base.OnConnectedAsync();
		}

		/// <summary>
		/// X? lý khi client ng?t k?t n?i
		/// </summary>
		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			_logger.LogInformation("? Client disconnected: {ConnectionId}", Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}
	}
}
