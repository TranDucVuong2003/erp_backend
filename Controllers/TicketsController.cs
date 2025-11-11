using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using erp_backend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization; // ✅ THÊM

namespace erp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(ApplicationDbContext context, IEmailService emailService, ILogger<TicketsController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: api/Tickets/my-tickets
        /// <summary>
        /// Lấy danh sách tickets theo role của user
        /// - Admin: Thấy tất cả tickets
        /// - User: Chỉ thấy tickets được phân công cho mình (AssignedToId = userId)
        /// </summary>
        [HttpGet("my-tickets")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetMyTickets(
            [FromQuery] string? priority,
            [FromQuery] string? status,
            [FromQuery] int? urgencyLevel,
            [FromQuery] int? categoryId)
        {
            try
            {
                // Lấy thông tin user hiện tại từ JWT token
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin user" });
                }

                // Build query
                var query = _context.Tickets
                    .Include(t => t.Customer)
                    .Include(t => t.Category)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.CreatedBy)
                    .AsQueryable();

                // ✅ PHÂN QUYỀN: Nếu không phải Admin, chỉ lấy tickets được phân công
                var isAdmin = IsCurrentUserAdmin();
                if (!isAdmin)
                {
                    query = query.Where(t => t.AssignedToId == currentUserId);
                }

                // Apply filters
                if (!string.IsNullOrEmpty(priority))
                    query = query.Where(t => t.Priority.ToLower() == priority.ToLower());

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(t => t.Status.ToLower() == status.ToLower());

                if (urgencyLevel.HasValue)
                    query = query.Where(t => t.UrgencyLevel == urgencyLevel.Value);

                if (categoryId.HasValue)
                    query = query.Where(t => t.CategoryId == categoryId.Value);

                var tickets = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Lấy danh sách tickets thành công",
                    role = GetCurrentUserRole() ?? "User",
                    userId = currentUserId,
                    isAdmin = isAdmin,
                    totalTickets = tickets.Count,
                    data = tickets
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "Lỗi server khi lấy danh sách tickets", error = ex.Message });
            }
        }

        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets(
            [FromQuery] int? customerId,
            [FromQuery] int? categoryId,
            [FromQuery] int? assignedToId,
            [FromQuery] string? priority,
            [FromQuery] string? status,
            [FromQuery] int? urgencyLevel)
        {
            var query = _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.Category)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .AsQueryable();

            // Apply filters
            if (customerId.HasValue)
                query = query.Where(t => t.CustomerId == customerId.Value);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            if (assignedToId.HasValue)
                query = query.Where(t => t.AssignedToId == assignedToId.Value);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority.ToLower() == priority.ToLower());

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status.ToLower() == status.ToLower());

            if (urgencyLevel.HasValue)
                query = query.Where(t => t.UrgencyLevel == urgencyLevel.Value);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // GET: api/Tickets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.Category)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return ticket;
        }

        // GET: api/Tickets/5/logs
        [HttpGet("{id}/logs")]
        public async Task<ActionResult<IEnumerable<TicketLog>>> GetTicketLogs(int id)
        {
            var ticketExists = await _context.Tickets.AnyAsync(t => t.Id == id);
            if (!ticketExists)
            {
                return NotFound("Ticket không tồn tại.");
            }

            var logs = await _context.TicketLogs
                .Include(tl => tl.User)
                .Where(tl => tl.TicketId == id)
                .OrderByDescending(tl => tl.CreatedAt)
                .ToListAsync();

            return Ok(logs);
        }

        // PUT: api/Tickets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return BadRequest();
            }

            // Normalize status
            ticket.Status = NormalizeStatus(ticket.Status);

            // Verify required foreign keys exist
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == ticket.CustomerId);
            if (!customerExists)
            {
                return BadRequest("Customer không tồn tại.");
            }

            var categoryExists = await _context.TicketCategories.AnyAsync(tc => tc.Id == ticket.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("TicketCategory không tồn tại.");
            }

            // Verify optional foreign keys if they are provided
            if (ticket.AssignedToId.HasValue)
            {
                var assignedUserExists = await _context.Users.AnyAsync(u => u.Id == ticket.AssignedToId.Value);
                if (!assignedUserExists)
                {
                    return BadRequest("Assigned User không tồn tại.");
                }
            }

            if (ticket.CreatedById.HasValue)
            {
                var createdByExists = await _context.Users.AnyAsync(u => u.Id == ticket.CreatedById.Value);
                if (!createdByExists)
                {
                    return BadRequest("Created By User không tồn tại.");
                }
            }

            // Handle ClosedAt timestamp based on status
            if (IsClosedStatus(ticket.Status))
            {
                if (ticket.ClosedAt == null)
                {
                    ticket.ClosedAt = DateTime.UtcNow;
                }
            }
            else
            {
                ticket.ClosedAt = null;
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Entry(ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/Tickets/5/assign
        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] int? assignedToId)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)
                    .FirstOrDefaultAsync(t => t.Id == id);
                    
                if (ticket == null)
                {
                    return NotFound();
                }

                User? newAssignedUser = null;
                if (assignedToId.HasValue)
                {
                    newAssignedUser = await _context.Users.FindAsync(assignedToId.Value);
                    if (newAssignedUser == null)
                    {
                        return BadRequest("User không tồn tại.");
                    }
                }

                // Get current user for logging
                var currentUserId = GetCurrentUserId();
                var currentUser = await _context.Users.FindAsync(currentUserId);

                var oldAssigned = ticket.AssignedTo?.Name ?? "Chưa phân công";
                var newAssigned = newAssignedUser?.Name ?? "Chưa phân công";

                ticket.AssignedToId = assignedToId;
                ticket.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send email notification about assignment change
                if (currentUser != null)
                {
                    var logContent = $"Ticket đã được phân công từ '{oldAssigned}' sang '{newAssigned}'";
                    
                    // Load full ticket data for email
                    await _context.Entry(ticket)
                        .Reference(t => t.AssignedTo)
                        .LoadAsync();
                        
                    await _emailService.SendTicketLogNotificationAsync(ticket, currentUser, logContent);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ticket {TicketId}", id);
                return StatusCode(500, new { message = "Error assigning ticket", error = ex.Message });
            }
        }

        // PUT: api/Tickets/5/status
        [HttpPut("{id}/status")]
        public async Task<ActionResult<Ticket>> UpdateTicketStatus(int id, [FromBody] UpdateTicketStatusRequest request)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Customer)
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.Id == id);
                    
                if (ticket == null)
                {
                    return NotFound();
                }

                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(request.Status))
                {
                    return BadRequest("Status không được để trống.");
                }

                // Normalize and validate status
                var normalizedStatus = NormalizeStatus(request.Status);
                if (!IsValidStatus(normalizedStatus))
                {
                    return BadRequest($"Status '{request.Status}' không hợp lệ. Các giá trị hợp lệ: Open, In Progress, Closed, On Hold, Cancelled");
                }

                // Get current user for logging
                var currentUserId = GetCurrentUserId();
                var currentUser = await _context.Users.FindAsync(currentUserId);

                var oldStatus = ticket.Status;
                ticket.Status = normalizedStatus;
                ticket.UpdatedAt = DateTime.UtcNow;

                // If status indicates closed, set ClosedAt
                if (IsClosedStatus(normalizedStatus))
                {
                    ticket.ClosedAt = DateTime.UtcNow;
                }
                else
                {
                    ticket.ClosedAt = null;
                }

                await _context.SaveChangesAsync();

                // Send email notification about status change
                if (currentUser != null)
                {
                    var logContent = $"Trạng thái ticket đã được thay đổi từ '{oldStatus}' thành '{normalizedStatus}'";
                    await _emailService.SendTicketLogNotificationAsync(ticket, currentUser, logContent);
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket status for ticket {TicketId}", id);
                return StatusCode(500, new { message = "Error updating ticket status", error = ex.Message });
            }
        }

        // POST: api/Tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            try
            {
                // Normalize status
                ticket.Status = NormalizeStatus(ticket.Status);

                // Validate status
                if (!IsValidStatus(ticket.Status))
                {
                    return BadRequest($"Status '{ticket.Status}' không hợp lệ. Các giá trị hợp lệ: Open, In Progress, Closed, On Hold, Cancelled");
                }

                // Verify required foreign keys exist
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == ticket.CustomerId);
                if (!customerExists)
                {
                    return BadRequest("Customer không tồn tại.");
                }

                var categoryExists = await _context.TicketCategories.AnyAsync(tc => tc.Id == ticket.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest("TicketCategory không tồn tại.");
                }

                // Verify optional foreign keys if they are provided
                if (ticket.AssignedToId.HasValue)
                {
                    var assignedUserExists = await _context.Users.AnyAsync(u => u.Id == ticket.AssignedToId.Value);
                    if (!assignedUserExists)
                    {
                        return BadRequest("Assigned User không tồn tại.");
                    }
                }

                if (ticket.CreatedById.HasValue)
                {
                    var createdByExists = await _context.Users.AnyAsync(u => u.Id == ticket.CreatedById.Value);
                    if (!createdByExists)
                    {
                        return BadRequest("Created By User không tồn tại.");
                    }
                }

                // Handle ClosedAt for new tickets
                if (IsClosedStatus(ticket.Status))
                {
                    ticket.ClosedAt = DateTime.UtcNow;
                }

                ticket.CreatedAt = DateTime.UtcNow;
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Load related data for response and email
                await LoadTicketRelatedData(ticket);

                // Send email notification for new ticket
                if (ticket.CreatedById.HasValue)
                {
                    var createdByUser = await _context.Users.FindAsync(ticket.CreatedById.Value);
                    if (createdByUser != null)
                    {
                        var logContent = $"Ticket mới đã được tạo với tiêu đề: {ticket.Title}";
                        await _emailService.SendTicketLogNotificationAsync(ticket, createdByUser, logContent);
                    }
                }

                return CreatedAtAction("GetTicket", new { id = ticket.Id }, ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new ticket");
                return StatusCode(500, new { message = "Error creating ticket", error = ex.Message });
            }
        }

        // DELETE: api/Tickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Check if there are ticket logs
            var hasLogs = await _context.TicketLogs.AnyAsync(tl => tl.TicketId == id);
            if (hasLogs)
            {
                return BadRequest("Không thể xóa ticket có logs. Hãy xóa logs trước.");
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }

        private async Task LoadTicketRelatedData(Ticket ticket)
        {
            await _context.Entry(ticket)
                .Reference(t => t.Customer)
                .LoadAsync();
            
            await _context.Entry(ticket)
                .Reference(t => t.Category)
                .LoadAsync();

            if (ticket.AssignedToId.HasValue)
            {
                await _context.Entry(ticket)
                    .Reference(t => t.AssignedTo)
                    .LoadAsync();
            }

            if (ticket.CreatedById.HasValue)
            {
                await _context.Entry(ticket)
                    .Reference(t => t.CreatedBy)
                    .LoadAsync();
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

        /// <summary>
        /// Lấy role của user hiện tại từ JWT token
        /// </summary>
        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value
                   ?? User.FindFirst("role")?.Value
                   ?? User.FindFirst("Role")?.Value;
        }

        /// <summary>
        /// Kiểm tra xem user hiện tại có phải Admin không
        /// </summary>
        private bool IsCurrentUserAdmin()
        {
            var role = GetCurrentUserRole();
            return role?.ToLower() == "admin";
        }

        /// <summary>
        /// Normalize status string to standard format
        /// </summary>
        private string NormalizeStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "Open";

            return status.Trim().ToLower() switch
            {
                "open" or "new" => "Open",
                "in progress" or "working" or "inprogress" or "in_progress" => "In Progress",
                "closed" or "completed" or "resolved" or "done" => "Closed",
                "on hold" or "pending" or "onhold" => "On Hold",
                "cancelled" or "canceled" => "Cancelled",
                _ => status.Trim() // Return original if no match
            };
        }

        /// <summary>
        /// Check if status is valid
        /// </summary>
        private bool IsValidStatus(string status)
        {
            var validStatuses = new[] { "Open", "In Progress", "Closed", "On Hold", "Cancelled" };
            return validStatuses.Contains(status);
        }

        /// <summary>
        /// Check if status indicates ticket is closed
        /// </summary>
        private bool IsClosedStatus(string status)
        {
            return status?.ToLower() switch
            {
                "closed" or "completed" or "resolved" or "done" => true,
                _ => false
            };
        }
    }
}