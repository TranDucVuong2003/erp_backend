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
                                name = "Dashboard",
                                path = "/",
                                icon = "HomeIcon"
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
                            //new
                            //{
                            //    name = "Task",
                            //    path = "/tasks",
                            //    icon = "CalendarIcon"
                            //},
                            new
                            {
                                name = "Service",
                                icon = "CogIcon",
                                children = new[]
                                {
                                    new { name = "Service", path = "/service" },
                                    new { name = "Addons", path = "/addons" }
                                }
                            },
                            new
                            {
                                name = "Support",
                                path = "/sessions",
                                icon = "LifebuoyIcon"
                            },
                            //new
                            //{
                            //    name = "Report",
                            //    path = "/reports",
                            //    icon = "DocumentChartBarIcon"
                            //},
                            new
                            {
                                name = "User management",
                                path = "/usermanagement",
                                icon = "CogIcon"
                            },
                            new
                            {
                                name = "HelpDesk",
                                icon = "LifebuoyIcon",
                                children = new[]
                                {
                                    new { name = "All Tickets", path = "/helpdesk" },
                                    new { name = "Categories", path = "/ticket-categories" }
                                }
                            }
                        }
                    });
                }
                else
                {
                    // Menu giới hạn cho User thường
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
                                name = "Customer",
                                path = "/customers",
                                icon = "UsersIcon"
                            },
                            new
                            {
                                name = "My Tickets",
                                path = "/my-tickets",
                                icon = "LifebuoyIcon"
                            },
                            new
                            {
                                name = "Tasks",
                                path = "/tasks",
                                icon = "CalendarIcon"
                            },
                            new
                            {
                                name = "Support",
                                path = "/sessions",
                                icon = "LifebuoyIcon"
                            },
                            new
                            {
                                name = "Profile",
                                path = "/profile",
                                icon = "CogIcon"
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

        ///// <summary>
        ///// Lấy thống kê dashboard theo role
        ///// </summary>
        //[HttpGet("dashboard-stats")]
        //public ActionResult<object> GetDashboardStats()
        //{
        //    try
        //    {
        //        var role = GetCurrentUserRole();
        //        var userId = GetCurrentUserId();

        //        if (role?.ToLower() == "admin")
        //        {
        //            // Stats đầy đủ cho Admin
        //            return Ok(new
        //            {
        //                role = "Admin",
        //                stats = new
        //                {
        //                    totalUsers = "Loading...",
        //                    totalCustomers = "Loading...",
        //                    totalTickets = "Loading...",
        //                    totalSales = "Loading...",
        //                    openTickets = "Loading...",
        //                    pendingContracts = "Loading..."
        //                },
        //                hasFullAccess = true
        //            });
        //        }
        //        else
        //        {
        //            // Stats giới hạn cho User
        //            return Ok(new
        //            {
        //                role = "User",
        //                stats = new
        //                {
        //                    myTickets = "Loading...",
        //                    myTasks = "Loading...",
        //                    completedTickets = "Loading..."
        //                },
        //                hasFullAccess = false
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting dashboard stats");
        //        return StatusCode(500, new { message = "Lỗi server khi lấy thống kê", error = ex.Message });
        //    }
        //}

        ///// <summary>
        ///// Kiểm tra quyền truy cập một route cụ thể
        ///// </summary>
        //[HttpGet("check-access")]
        //public ActionResult<object> CheckAccess([FromQuery] string path)
        //{
        //    try
        //    {
        //        var role = GetCurrentUserRole();
        //        var isAdmin = role?.ToLower() == "admin";

        //        // Định nghĩa các route chỉ Admin mới truy cập được
        //        var adminOnlyRoutes = new[]
        //        {
        //            "/usermanagement",
        //            "/reports",
        //            "/helpdesk", // All tickets
        //            "/ticket-categories",
        //            "/sales",
        //            "/contract",
        //            "/service",
        //            "/addons"
        //        };

        //        var hasAccess = isAdmin || !adminOnlyRoutes.Any(route => path.StartsWith(route));

        //        return Ok(new
        //        {
        //            path = path,
        //            hasAccess = hasAccess,
        //            role = role ?? "User",
        //            isAdmin = isAdmin,
        //            message = hasAccess ? "Có quyền truy cập" : "Không có quyền truy cập"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error checking access for path {Path}", path);
        //        return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        //    }
        //}

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
