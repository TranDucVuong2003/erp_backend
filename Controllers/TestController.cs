using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using erp_backend.Services;
using erp_backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace erp_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for testing
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(IEmailService emailService, ApplicationDbContext context, ILogger<TestController> logger)
        {
            _emailService = emailService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("send-test-email/{ticketId}")]
        public async Task<IActionResult> SendTestEmail(int ticketId)
        {
            try
            {
                // L?y ticket t? database v?i ??y ?? thông tin
                var ticket = await _context.Tickets
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Category)
                    .Include(t => t.Customer)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket == null)
                {
                    return NotFound($"Ticket with ID {ticketId} not found");
                }

                // L?y user hi?n t?i (ng??i test)
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return BadRequest("Unable to get current user ID from token");
                }

                var currentUser = await _context.Users.FindAsync(currentUserId);

                if (currentUser == null)
                {
                    return BadRequest($"Current user with ID {currentUserId} not found in database");
                }

                // G?i email test
                var testLogContent = $"?ây là email test ???c g?i lúc {DateTime.Now:dd/MM/yyyy HH:mm:ss} b?i {currentUser.Name}";
                
                await _emailService.SendTicketLogNotificationAsync(ticket, currentUser, testLogContent);

                return Ok(new { 
                    message = "Test email sent successfully", 
                    ticketId = ticketId,
                    currentUser = new { currentUser.Id, currentUser.Name, currentUser.Email },
                    ticketInfo = new
                    {
                        ticket.Id,
                        ticket.Title,
                        CreatedBy = ticket.CreatedBy != null ? new { ticket.CreatedBy.Id, ticket.CreatedBy.Name, ticket.CreatedBy.Email, ticket.CreatedBy.SecondaryEmail } : null,
                        AssignedTo = ticket.AssignedTo != null ? new { ticket.AssignedTo.Id, ticket.AssignedTo.Name, ticket.AssignedTo.Email, ticket.AssignedTo.SecondaryEmail } : null
                    },
                    recipients = new[] { 
                        ticket.CreatedBy?.Email,
                        ticket.CreatedBy?.SecondaryEmail,
                        ticket.AssignedTo?.Email,
                        ticket.AssignedTo?.SecondaryEmail
                    }.Where(e => !string.IsNullOrEmpty(e)).Distinct()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email for ticket {TicketId}", ticketId);
                return StatusCode(500, new { message = "Error sending test email", error = ex.Message });
            }
        }

        [HttpGet("ticket-email-info/{ticketId}")]
        public async Task<IActionResult> GetTicketEmailInfo(int ticketId)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket == null)
                {
                    return NotFound($"Ticket with ID {ticketId} not found");
                }

                var recipients = new List<string>();
                
                if (ticket.CreatedBy != null && !string.IsNullOrEmpty(ticket.CreatedBy.Email))
                    recipients.Add(ticket.CreatedBy.Email);
                    
                if (ticket.CreatedBy != null && !string.IsNullOrEmpty(ticket.CreatedBy.SecondaryEmail))
                    recipients.Add(ticket.CreatedBy.SecondaryEmail);
                    
                if (ticket.AssignedTo != null && !string.IsNullOrEmpty(ticket.AssignedTo.Email))
                    recipients.Add(ticket.AssignedTo.Email);
                    
                if (ticket.AssignedTo != null && !string.IsNullOrEmpty(ticket.AssignedTo.SecondaryEmail))
                    recipients.Add(ticket.AssignedTo.SecondaryEmail);

                return Ok(new
                {
                    ticketId = ticket.Id,
                    title = ticket.Title,
                    createdBy = ticket.CreatedBy != null ? new
                    {
                        id = ticket.CreatedBy.Id,
                        name = ticket.CreatedBy.Name,
                        email = ticket.CreatedBy.Email,
                        secondaryEmail = ticket.CreatedBy.SecondaryEmail,
                        hasEmail = !string.IsNullOrEmpty(ticket.CreatedBy.Email),
                        hasSecondaryEmail = !string.IsNullOrEmpty(ticket.CreatedBy.SecondaryEmail)
                    } : null,
                    assignedTo = ticket.AssignedTo != null ? new
                    {
                        id = ticket.AssignedTo.Id,
                        name = ticket.AssignedTo.Name,
                        email = ticket.AssignedTo.Email,
                        secondaryEmail = ticket.AssignedTo.SecondaryEmail,
                        hasEmail = !string.IsNullOrEmpty(ticket.AssignedTo.Email),
                        hasSecondaryEmail = !string.IsNullOrEmpty(ticket.AssignedTo.SecondaryEmail)
                    } : null,
                    potentialRecipients = recipients.Distinct().ToList(),
                    recipientCount = recipients.Distinct().Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket email info for ticket {TicketId}", ticketId);
                return StatusCode(500, new { message = "Error getting ticket email info", error = ex.Message });
            }
        }

        [HttpGet("email-config-status")]
        public IActionResult CheckEmailConfiguration()
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            
            var emailConfig = new
            {
                SmtpServer = config["Email:SmtpServer"],
                SmtpPort = config["Email:SmtpPort"],
                Username = config["Email:Username"],
                SenderEmail = config["Email:SenderEmail"],
                SenderName = config["Email:SenderName"],
                HasPassword = !string.IsNullOrEmpty(config["Email:Password"])
            };

            var isValid = !string.IsNullOrEmpty(emailConfig.SmtpServer) &&
                         !string.IsNullOrEmpty(emailConfig.Username) &&
                         !string.IsNullOrEmpty(emailConfig.SenderEmail) &&
                         emailConfig.HasPassword;

            return Ok(new { 
                isConfigurationValid = isValid,
                configuration = emailConfig
            });
        }

        [HttpGet("debug-user-claims")]
        public IActionResult DebugUserClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var userId = GetCurrentUserId();
            
            return Ok(new { 
                userId = userId,
                claims = claims,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                identity = User.Identity?.Name
            });
        }

        private int GetCurrentUserId()
        {
            // Try multiple claim types to find user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value
                             ?? User.FindFirst("UserId")?.Value
                             ?? User.FindFirst("id")?.Value;

            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}