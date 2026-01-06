using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace erp_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class MenuController : ControllerBase
	{
		private readonly ILogger<MenuController> _logger;

		public MenuController(ILogger<MenuController> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Lấy sidebar menu theo role của user
		/// Admin: Full menu với tất cả chức năng
		/// Manager: Menu có quyền xem báo cáo và KPI Dashboard
		/// User: Menu giới hạn chỉ những gì họ được phép truy cập
		/// </summary>
		[HttpGet("sidebar")]
		public ActionResult<object> GetSidebarMenu()
		{
			try
			{
				var role = GetCurrentUserRole();
				var userId = GetCurrentUserId();

				_logger.LogInformation("Getting sidebar menu for user {UserId} with role {Role}", userId, role);

				if (role?.ToLower() == "admin")
				{
					// Menu đầy đủ cho Admin
					return Ok(new
					{
						role = "Admin",
						userId = userId,
						menu = new object[]
						{
							new
							{
								name = "Tổng quan",
								path = "/",
								icon = "HomeIcon"
							},
							new
							{
								name = "Thông báo",
								path = "/notifications",
								icon = "BellIcon"
							},
							new
							{
								name = "Khách hàng",
								path = "/customers",
								icon = "UsersIcon"
							},
							new
							{
								name = "Bán hàng",
								icon = "ChartBarIcon",
								children = new[]
								{
									new { name = "Đơn hàng", path = "/sales" },
									new { name = "Hợp đồng", path = "/contract" }
								}
							},
							new
							{
								name = "Báo giá",
								path = "/quotes",
								icon = "DocumentTextIcon"
							},
							new
							{
								name = "Dữ liệu công ty",
								path = "/companies",
								icon = "CalendarIcon"
							},
							new
							{
								name = "Dịch vụ",
								icon = "CogIcon",
								children = new[]
								{
									new { name = "Dịch vụ", path = "/service" },
									new { name = "Dịch vụ đi kèm", path = "/addons" }
								}
							},
							new
							{
								name = "Phiên đăng nhập",
								path = "/sessions",
								icon = "LifebuoyIcon"
							},
							new
							{
								name = "Chỉ tiêu (KPI)",
								icon = "ChartPieIcon",
								children = new[]
								{
									new { name = "Tổng Quan chỉ tiêu", path = "/kpi/dashboard" },
									new { name = "Quản lý chỉ tiêu", path = "/kpi/management" },
									//new { name = "KPI của tôi", path = "/kpi/my-kpi" },
									new { name = "Bảng xếp hạng", path = "/kpi/leaderboard" },
									new { name = "Bậc hoa hồng", path = "/kpi/commission-rates" }
								}
							},
							new
							{
								name = "Quản lý người dùng",
								path = "/usermanagement",
								icon = "CogIcon"
							},
							new
							{
								name = "Kế toán",
								icon = "CurrencyDollarIcon",
								children = new[]
								{
									new { name = "Lương", path = "/accounting/salary" }
								}
							},
							new
							{
								name = "Hỗ trợ",
								icon = "LifebuoyIcon",
								path = "/helpdesk"
							}
						}
					});
				}
				else if (role?.ToLower() == "manager")
				{
					// Menu cho Manager (có quyền xem báo cáo và KPI Dashboard)
					return Ok(new
					{
						role = "Manager",
						userId = userId,
						menu = new object[]
						{
							new
							{
								name = "Dashboard",
								path = "/",
								icon = "HomeIcon"
							},
							new
							{
								name = "Thông báo",
								path = "/notifications",
								icon = "BellIcon"
							},
							new
							{
								name = "Customer",
								path = "/customers",
								icon = "UsersIcon"
							},
							new
							{
								name = "Sale",
								icon = "ChartBarIcon",
								children = new[]
								{
									new { name = "Sales order", path = "/sales" },
									new { name = "Contract", path = "/contract" }
								}
							},
							new
							{
								name = "Quote",
								path = "/quotes",
								icon = "DocumentTextIcon"
							},
							new
							{
								name = "Lead",
								path = "/companies",
								icon = "CalendarIcon"
							},
							new
							{
								name = "KPI",
								icon = "ChartPieIcon",
								children = new[]
								{
									new { name = "Dashboard KPI", path = "/kpi/dashboard" },
									new { name = "KPI của tôi", path = "/kpi/my-kpi" },
									new { name = "Bảng xếp hạng", path = "/kpi/leaderboard" }
								}
							},
							new
							{
								name = "My Tickets",
								path = "/helpdesk",
								icon = "LifebuoyIcon"
							}
						}
					});
				}
				else
				{
					// Menu cho User
					return Ok(new
					{
						role = "User",
						userId = userId,
						menu = new object[]
						{
							new
							{
								name = "Dashboard",
								path = "/",
								icon = "HomeIcon"
							},
							new
							{
								name = "Thông báo",
								path = "/notifications",
								icon = "BellIcon"
							},
							new
							{
								name = "Customer",
								path = "/customers",
								icon = "UsersIcon"
							},
							new
							{
								name = "Sale",
								icon = "ChartBarIcon",
								children = new[]
								{
									new { name = "Sales order", path = "/sales" },
									new { name = "Contract", path = "/contract" }
								}
							},
							new
							{
								name = "Quote",
								path = "/quotes",
								icon = "DocumentTextIcon"
							},
							new
							{
								name = "Lead",
								path = "/companies",
								icon = "CalendarIcon"
							},
							new
							{
								name = "KPI",
								icon = "ChartPieIcon",
								children = new[]
								{
									new { name = "KPI của tôi", path = "/kpi/my-kpi" },
									new { name = "Bảng xếp hạng", path = "/kpi/leaderboard" }
								}
							},
							new
							{
								name = "My Tickets",
								path = "/helpdesk",
								icon = "LifebuoyIcon"
							}
						}
					});
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting sidebar menu for user {UserId}", GetCurrentUserId());
				return StatusCode(500, new { message = "Lỗi server khi lấy menu", error = ex.Message });
			}
		}

		private int GetCurrentUserId()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
							 ?? User.FindFirst("sub")?.Value
							 ?? User.FindFirst("UserId")?.Value
							 ?? User.FindFirst("id")?.Value;

			return int.TryParse(userIdClaim, out var userId) ? userId : 0;
		}

		private string? GetCurrentUserRole()
		{
			return User.FindFirst(ClaimTypes.Role)?.Value
				   ?? User.FindFirst("role")?.Value
				   ?? User.FindFirst("Role")?.Value;
		}
	}
}
