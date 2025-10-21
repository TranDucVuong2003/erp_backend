using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;

namespace erp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets(
            [FromQuery] int? customerId,
            [FromQuery] int? categoryId,
            [FromQuery] int? userId,
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

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority == priority);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

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
                return NotFound("Ticket không t?n t?i.");
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

            // Verify required foreign keys exist
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == ticket.CustomerId);
            if (!customerExists)
            {
                return BadRequest("Customer không t?n t?i.");
            }

            var categoryExists = await _context.TicketCategories.AnyAsync(tc => tc.Id == ticket.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("TicketCategory không t?n t?i.");
            }

            // Verify optional foreign keys if they are provided
            if (ticket.UserId.HasValue)
            {
                var assignedUserExists = await _context.Users.AnyAsync(u => u.Id == ticket.UserId.Value);
                if (!assignedUserExists)
                {
                    return BadRequest("Assigned User không t?n t?i.");
                }
            }

            if (ticket.CreatedById.HasValue)
            {
                var createdByExists = await _context.Users.AnyAsync(u => u.Id == ticket.CreatedById.Value);
                if (!createdByExists)
                {
                    return BadRequest("Created By User không t?n t?i.");
                }
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
        public async Task<IActionResult> AssignTicket(int id, [FromBody] int? userId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            if (userId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId.Value);
                if (!userExists)
                {
                    return BadRequest("User không t?n t?i.");
                }
            }

            ticket.UserId = userId;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Tickets/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] string status)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;

            // If status indicates closed, set ClosedAt
            if (status?.ToLower() == "closed" || status?.ToLower() == "completed") 
            {
                ticket.ClosedAt = DateTime.UtcNow;
            }
            else
            {
                ticket.ClosedAt = null;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
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
            if (ticket.UserId.HasValue)
            {
                var assignedUserExists = await _context.Users.AnyAsync(u => u.Id == ticket.UserId.Value);
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
                    return BadRequest("Created By User không t?n t?i.");
                }
            }

            ticket.CreatedAt = DateTime.UtcNow;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Load related data for response
            await _context.Entry(ticket)
                .Reference(t => t.Customer)
                .LoadAsync();
            
            await _context.Entry(ticket)
                .Reference(t => t.Category)
                .LoadAsync();

            if (ticket.UserId.HasValue)
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

            return CreatedAtAction("GetTicket", new { id = ticket.Id }, ticket);
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
                return BadRequest("Không th? xóa ticket có logs. Hãy xóa logs tr??c.");
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}