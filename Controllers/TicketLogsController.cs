using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;

namespace erp_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TicketLogsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public TicketLogsController(ApplicationDbContext context)
		{
			_context = context;
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

			return CreatedAtAction(nameof(GetTicketLog), new { id = ticketLog.Id }, ticketLog);
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
