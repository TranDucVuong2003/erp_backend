using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace erp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService; // ✅ THÊM
        private readonly IWebHostEnvironment _environment; // ✅ THÊM
        private readonly ILogger<TicketLogsController> _logger;

        public TicketLogsController(
            ApplicationDbContext context, 
            IEmailService emailService,
            IFileService fileService, // ✅ THÊM
            IWebHostEnvironment environment, // ✅ THÊM
            ILogger<TicketLogsController> logger)
        {
            _context = context;
            _emailService = emailService;
            _fileService = fileService; // ✅ THÊM
            _environment = environment; // ✅ THÊM
            _logger = logger;
        }

        // GET: api/TicketLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketLog>>> GetTicketLogs([FromQuery] int? ticketId)
        {
            var query = _context.TicketLogs
                .Include(tl => tl.User)
                .Include(tl => tl.Ticket)
                .Include(tl => tl.Attachments) // ✅ THÊM
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
                .Include(tl => tl.Attachments) // ✅ THÊM
                .FirstOrDefaultAsync(tl => tl.Id == id);

            if (ticketLog == null)
            {
                return NotFound();
            }

            return ticketLog;
        }

        // ✅ POST mới với file upload
        [HttpPost]
        public async Task<ActionResult<TicketLog>> PostTicketLog(
            [FromForm] string content,
            [FromForm] int ticketId,
            [FromForm] int userId,
            [FromForm] List<IFormFile>? attachments)
        {
            try
            {
                // Kiểm tra ticket tồn tại
                var ticket = await _context.Tickets
                    .Include(t => t.AssignedTo)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.Category)
                    .Include(t => t.Customer)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);
                    
                if (ticket == null)
                    return BadRequest(new { message = "Ticket không tồn tại" });

                // Kiểm tra user tồn tại
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return BadRequest(new { message = "User không tồn tại" });

                // Tạo ticket log
                var ticketLog = new TicketLog
                {
                    TicketId = ticketId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TicketLogs.Add(ticketLog);
                await _context.SaveChangesAsync();

                // Xử lý file attachments
                if (attachments != null && attachments.Any())
                {
                    foreach (var file in attachments)
                    {
                        var result = await _fileService.SaveFileAsync(file);
                        
                        if (result.Success)
                        {
                            var attachment = new TicketLogAttachment
                            {
                                TicketLogId = ticketLog.Id,
                                FileName = file.FileName,
                                FilePath = result.FilePath!,
                                FileType = file.ContentType,
                                FileSize = file.Length,
                                Category = _fileService.IsImageFile(file.FileName) ? "image" : "document",
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.TicketLogAttachments.Add(attachment);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to upload file {FileName}: {Error}", 
                                file.FileName, result.ErrorMessage);
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                }

                // Load dữ liệu liên quan
                await _context.Entry(ticketLog)
                    .Collection(tl => tl.Attachments)
                    .LoadAsync();

                await _context.Entry(ticketLog)
                    .Reference(tl => tl.User)
                    .LoadAsync();

                // Gửi email
                await _emailService.SendTicketLogNotificationAsync(ticket, user, ticketLog.Content);

                return CreatedAtAction(nameof(GetTicketLog), new { id = ticketLog.Id }, ticketLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket log with attachments");
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        // ✅ Download attachment
        [HttpGet("attachments/{id}/download")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _context.TicketLogAttachments.FindAsync(id);
            if (attachment == null)
                return NotFound(new { message = "Attachment không tồn tại" });

            var filePath = Path.Combine(_environment.WebRootPath, attachment.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File không tồn tại" });

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, attachment.FileType ?? "application/octet-stream", attachment.FileName);
        }

        // ✅ Delete attachment
        [HttpDelete("attachments/{id}")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var attachment = await _context.TicketLogAttachments.FindAsync(id);
            if (attachment == null)
                return NotFound(new { message = "Attachment không tồn tại" });

            // Xóa file vật lý
            await _fileService.DeleteFileAsync(attachment.FilePath);

            // Xóa record trong DB
            _context.TicketLogAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa attachment thành công" });
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

		private bool TicketLogExists(int id)
		{
			return _context.TicketLogs.Any(e => e.Id == id);
		}
	}
}
