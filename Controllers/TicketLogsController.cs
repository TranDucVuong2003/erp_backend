using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Services;

namespace erp_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TicketLogsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IEmailService _emailService;
		private readonly ILogger<TicketLogsController> _logger;

		public TicketLogsController(
			ApplicationDbContext context, 
			IEmailService emailService,
			ILogger<TicketLogsController> logger)
		{
			_context = context;
			_emailService = emailService;
			_logger = logger;
		}

		// GET: api/TicketLogs
		[HttpGet]
		public async Task<ActionResult<IEnumerable<TicketLog>>> GetTicketLogs([FromQuery] int? ticketId)
		{
			var query = _context.TicketLogs
				.Include(tl => tl.User)
				.Include(tl => tl.Ticket)
				.AsQueryable();

			if (ticketId.HasValue)
			{
				query = query.Where(tl => tl.TicketId == ticketId.Value);
			}

			return await query
				.OrderByDescending(tl => tl.CreatedAt)
				.ToListAsync();
		}

		// GET: api/TicketLogs/5
		[HttpGet("{id}")]
		public async Task<ActionResult<TicketLog>> GetTicketLog(int id)
		{
			var ticketLog = await _context.TicketLogs
				.Include(tl => tl.User)
				.Include(tl => tl.Ticket)
				.FirstOrDefaultAsync(tl => tl.Id == id);

			if (ticketLog == null)
			{
				return NotFound();
			}

			return ticketLog;
		}

		// GET: api/TicketLogs/by-ticket/5
		[HttpGet("by-ticket/{ticketId}")]
		public async Task<ActionResult<IEnumerable<TicketLog>>> GetTicketLogsByTicketId(int ticketId)
		{
			// Kiểm tra ticket có tồn tại không
			var ticketExists = await _context.Tickets.AnyAsync(t => t.Id == ticketId);
			if (!ticketExists)
			{
				return NotFound("Ticket không tồn tại.");
			}

			var ticketLogs = await _context.TicketLogs
				.Include(tl => tl.User)
				.Where(tl => tl.TicketId == ticketId)
				.OrderByDescending(tl => tl.CreatedAt)
				.ToListAsync();

			return Ok(ticketLogs);
		}

		// PUT: api/TicketLogs/5
		[HttpPut("{id}")]
		public async Task<ActionResult<TicketLog>> PutTicketLog(int id, TicketLog ticketLog)
		{
			if (id != ticketLog.Id)
			{
				return BadRequest("ID không khớp.");
			}

			// Kiểm tra ticket có tồn tại không
			var ticketExists = await _context.Tickets.AnyAsync(t => t.Id == ticketLog.TicketId);
			if (!ticketExists)
			{
				return BadRequest("Ticket không tồn tại.");
			}

			// Kiểm tra user có tồn tại không
			var userExists = await _context.Users.AnyAsync(u => u.Id == ticketLog.UserId);
			if (!userExists)
			{
				return BadRequest("User không tồn tại.");
			}

			ticketLog.UpdatedAt = DateTime.UtcNow;
			_context.Entry(ticketLog).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
				
				// Load dữ liệu liên quan để trả về
				await _context.Entry(ticketLog)
					.Reference(tl => tl.User)
					.LoadAsync();

				await _context.Entry(ticketLog)
					.Reference(tl => tl.Ticket)
					.LoadAsync();

				return Ok(ticketLog);
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!TicketLogExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}
		}

		// POST: api/TicketLogs
		[HttpPost]
		public async Task<ActionResult<TicketLog>> PostTicketLog(TicketLog ticketLog)
		{
			try
			{
				// Kiểm tra ticket có tồn tại không và load đầy đủ thông tin cho email
				var ticket = await _context.Tickets
					.Include(t => t.AssignedTo)  // Người được phân công
					.Include(t => t.CreatedBy)   // Người tạo ticket
					.Include(t => t.Category)    // Category cho email template
					.Include(t => t.Customer)    // Customer info
					.FirstOrDefaultAsync(t => t.Id == ticketLog.TicketId);
					
				if (ticket == null)
				{
					return BadRequest("Ticket không tồn tại.");
				}

				// Kiểm tra user có tồn tại không
				var user = await _context.Users.FindAsync(ticketLog.UserId);
				if (user == null)
				{
					return BadRequest("User không tồn tại.");
				}

				_logger.LogInformation("Creating ticket log for ticket {TicketId} by user {UserId}. Ticket CreatedBy: {CreatedById}, AssignedTo: {AssignedToId}", 
					ticketLog.TicketId, ticketLog.UserId, ticket.CreatedById, ticket.AssignedToId);

				ticketLog.CreatedAt = DateTime.UtcNow;
				_context.TicketLogs.Add(ticketLog);
				await _context.SaveChangesAsync();

				// Load dữ liệu liên quan để trả về
				await _context.Entry(ticketLog)
					.Reference(tl => tl.User)
					.LoadAsync();

				await _context.Entry(ticketLog)
					.Reference(tl => tl.Ticket)
					.LoadAsync();

				// Gửi email thông báo với thông tin đầy đủ
				_logger.LogInformation("Sending email notification for ticket {TicketId}. CreatedBy: {CreatedByEmail}, AssignedTo: {AssignedToEmail}", 
					ticket.Id, 
					ticket.CreatedBy?.Email ?? "null", 
					ticket.AssignedTo?.Email ?? "null");

				await _emailService.SendTicketLogNotificationAsync(ticket, user, ticketLog.Content);

				return CreatedAtAction(nameof(GetTicketLog), new { id = ticketLog.Id }, ticketLog);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating ticket log for ticket {TicketId}", ticketLog.TicketId);
				return StatusCode(500, new { message = "Lỗi server khi tạo ticket log", error = ex.Message });
			}
		}

		// DELETE: api/TicketLogs/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteTicketLog(int id)
		{
			var ticketLog = await _context.TicketLogs.FindAsync(id);
			if (ticketLog == null)
			{
				return NotFound();
			}

			_context.TicketLogs.Remove(ticketLog);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool TicketLogExists(int id)
		{
			return _context.TicketLogs.Any(e => e.Id == id);
		}
	}
}
