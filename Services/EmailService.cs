using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using erp_backend.Models;

namespace erp_backend.Services
{
    public interface IEmailService
    {
        Task SendTicketLogNotificationAsync(Ticket ticket, User actingUser, string logContent);
        Task SendTicketCreatedNotificationAsync(Ticket ticket, User createdByUser);
        Task SendTicketAssignedNotificationAsync(Ticket ticket, User assignedBy, string assignmentDetails);
        Task SendTicketStatusChangedNotificationAsync(Ticket ticket, User changedBy, string oldStatus, string newStatus);
        Task SendAccountCreationEmailAsync(User user, string plainPassword, string activationLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendTicketLogNotificationAsync(Ticket ticket, User actingUser, string logContent)
        {
            await SendEmailNotificationAsync(ticket, actingUser, "Bình luận mới", logContent, "comment");
        }

        public async Task SendTicketCreatedNotificationAsync(Ticket ticket, User createdByUser)
        {
            var content = $"Ticket mới đã được tạo với tiêu đề: {ticket.Title}";
            await SendEmailNotificationAsync(ticket, createdByUser, "Ticket mới được tạo", content, "created");
        }

        public async Task SendTicketAssignedNotificationAsync(Ticket ticket, User assignedBy, string assignmentDetails)
        {
            await SendEmailNotificationAsync(ticket, assignedBy, "Phân công ticket", assignmentDetails, "assigned");
        }

        public async Task SendTicketStatusChangedNotificationAsync(Ticket ticket, User changedBy, string oldStatus, string newStatus)
        {
            var content = $"Trạng thái ticket đã được thay đổi từ '{oldStatus}' thành '{newStatus}'";
            await SendEmailNotificationAsync(ticket, changedBy, "Thay đổi trạng thái", content, "status_changed");
        }

        public async Task SendAccountCreationEmailAsync(User user, string plainPassword, string activationLink)
        {
            try
            {
                // Validate email configuration first
                if (!ValidateEmailConfiguration())
                {
                    _logger.LogWarning("Email configuration is incomplete. Skipping account creation email for user {UserId}", user.Id);
                    return;
                }

                _logger.LogInformation("Preparing to send account creation email to {Email} for user {UserId}", user.Email, user.Id);

                // Lấy thông tin SMTP từ config
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];

                // Tạo email message
                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Tài khoản ERP của bạn đã được tạo - Vui lòng kích hoạt",
                    Body = FormatAccountCreationEmailBody(user, plainPassword, activationLink),
                    IsBodyHtml = true
                };

                // Thêm người nhận
                mail.To.Add(user.Email);

                // Tạo SMTP client và gửi email
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 30000; // 30 seconds timeout

                    await smtpClient.SendMailAsync(mail);
                }

                _logger.LogInformation("Account creation email sent successfully to {Email} for user {UserId}", user.Email, user.Id);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending account creation email for user {UserId}: {StatusCode} - {Message}", 
                    user.Id, smtpEx.StatusCode, smtpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending account creation email for user {UserId}", user.Id);
            }
        }

        private async Task SendEmailNotificationAsync(Ticket ticket, User actingUser, string actionType, string content, string emailType)
        {
            try
            {
                // Validate email configuration first
                if (!ValidateEmailConfiguration())
                {
                    _logger.LogWarning("Email configuration is incomplete. Skipping email notification for ticket {TicketId}", ticket.Id);
                    return;
                }

                // Xác định người nhận - sử dụng HashSet để tránh trùng lặp
                var recipients = new HashSet<string>();

                // 🔄 LOGIC MỚI: Chỉ gửi cho người KHÁC, không gửi cho chính người thực hiện hành động
                
                // Thêm email của người tạo ticket (nếu KHÔNG phải là người đang thực hiện hành động)
                if (ticket.CreatedBy != null && 
                    !string.IsNullOrEmpty(ticket.CreatedBy.Email) &&
                    ticket.CreatedBy.Id != actingUser.Id) // ✅ Loại trừ người thực hiện
                {
                    recipients.Add(ticket.CreatedBy.Email.Trim().ToLower());
                    _logger.LogDebug("Added CreatedBy email to recipients: {Email} for ticket {TicketId} (acting user: {ActingUserId})", 
                        ticket.CreatedBy.Email, ticket.Id, actingUser.Id);
                }

                // Thêm email của người được phân công (nếu KHÔNG phải là người đang thực hiện hành động)
                if (ticket.AssignedTo != null && 
                    !string.IsNullOrEmpty(ticket.AssignedTo.Email) &&
                    ticket.AssignedTo.Id != actingUser.Id) // ✅ Loại trừ người thực hiện
                {
                    recipients.Add(ticket.AssignedTo.Email.Trim().ToLower());
                    _logger.LogDebug("Added AssignedTo email to recipients: {Email} for ticket {TicketId} (acting user: {ActingUserId})", 
                        ticket.AssignedTo.Email, ticket.Id, actingUser.Id);
                }

                // Thêm secondary email của người tạo ticket (nếu KHÔNG phải là người thực hiện)
                if (ticket.CreatedBy != null && 
                    !string.IsNullOrEmpty(ticket.CreatedBy.SecondaryEmail) &&
                    ticket.CreatedBy.Id != actingUser.Id) // ✅ Loại trừ người thực hiện
                {
                    recipients.Add(ticket.CreatedBy.SecondaryEmail.Trim().ToLower());
                    _logger.LogDebug("Added CreatedBy secondary email to recipients: {Email} for ticket {TicketId} (acting user: {ActingUserId})", 
                        ticket.CreatedBy.SecondaryEmail, ticket.Id, actingUser.Id);
                }

                // Thêm secondary email của người được phân công (nếu KHÔNG phải là người thực hiện)
                if (ticket.AssignedTo != null && 
                    !string.IsNullOrEmpty(ticket.AssignedTo.SecondaryEmail) &&
                    ticket.AssignedTo.Id != actingUser.Id) // ✅ Loại trừ người thực hiện
                {
                    recipients.Add(ticket.AssignedTo.SecondaryEmail.Trim().ToLower());
                    _logger.LogDebug("Added AssignedTo secondary email to recipients: {Email} for ticket {TicketId} (acting user: {ActingUserId})", 
                        ticket.AssignedTo.SecondaryEmail, ticket.Id, actingUser.Id);
                }

                // Nếu không có người nhận, không gửi email
                if (!recipients.Any())
                {
                    _logger.LogInformation("No email recipients for ticket notification on ticket {TicketId} after excluding acting user {ActingUserId}. CreatedBy: {CreatedById}, AssignedTo: {AssignedToId}", 
                        ticket.Id, 
                        actingUser.Id,
                        ticket.CreatedBy?.Id ?? 0, 
                        ticket.AssignedTo?.Id ?? 0);
                    return;
                }

                _logger.LogInformation("Preparing to send email for ticket {TicketId} to {RecipientCount} recipients: {Recipients} (excluding acting user: {ActingUserId})", 
                    ticket.Id, recipients.Count, string.Join(", ", recipients), actingUser.Id);

                // Lấy thông tin SMTP từ config
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];

                // Tạo email message
                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = GenerateEmailSubject(ticket, actionType),
                    Body = FormatEmailBody(ticket, actingUser, actionType, content, emailType),
                    IsBodyHtml = true
                };

                // Thêm người nhận - chuyển về email gốc (không lowercase)
                foreach (var recipient in recipients)
                {
                    try
                    {
                        mail.To.Add(recipient);
                        _logger.LogDebug("Added recipient to email: {Recipient}", recipient);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to add recipient {Recipient} to email for ticket {TicketId}", recipient, ticket.Id);
                    }
                }

                // Tạo SMTP client và gửi email
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 30000; // 30 seconds timeout

                    await smtpClient.SendMailAsync(mail);
                }

                _logger.LogInformation("Email notification sent successfully for ticket {TicketId} - Type: {EmailType} to {RecipientCount} recipients: {Recipients} (acting user {ActingUserId} excluded)", 
                    ticket.Id, emailType, recipients.Count, string.Join(", ", recipients), actingUser.Id);
            }
            catch (SmtpException smtpEx)
            {
                // Log specific SMTP errors
                _logger.LogError(smtpEx, "SMTP error sending email notification for ticket {TicketId}: {StatusCode} - {Message}", 
                    ticket.Id, smtpEx.StatusCode, smtpEx.Message);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw exception để không ảnh hưởng đến việc lưu TicketLog
                _logger.LogError(ex, "Error sending email notification for ticket {TicketId}", ticket.Id);
            }
        }

        private bool ValidateEmailConfiguration()
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var senderEmail = _configuration["Email:SenderEmail"];

            if (string.IsNullOrEmpty(smtpServer) || 
                string.IsNullOrEmpty(smtpUsername) || 
                string.IsNullOrEmpty(smtpPassword) || 
                string.IsNullOrEmpty(senderEmail))
            {
                _logger.LogWarning("Email configuration is incomplete. Missing required settings.");
                return false;
            }

            return true;
        }

        private string GenerateEmailSubject(Ticket ticket, string actionType)
        {
            return $"[Ticket #{ticket.Id}] {ticket.Title} - {actionType}";
        }

        private string FormatEmailBody(Ticket ticket, User actingUser, string actionType, string content, string emailType)
        {
            var statusColor = GetStatusColor(ticket.Status);
            var actionIcon = GetActionIcon(emailType);

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 15px; border-bottom: 3px solid #0066cc; text-align: center; }}
        .content {{ padding: 20px 0; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 10px; text-align: center; }}
        .ticket-info {{ background-color: #f0f7ff; padding: 15px; border-left: 4px solid #0066cc; margin: 15px 0; border-radius: 4px; }}
        .action-content {{ border-left: 3px solid #28a745; padding-left: 15px; margin: 20px 0; background-color: #f8fff9; padding: 15px; border-radius: 4px; }}
        .highlight {{ color: #0066cc; font-weight: bold; }}
        .status {{ padding: 3px 8px; border-radius: 3px; color: white; font-size: 12px; font-weight: bold; }}
        .action-icon {{ font-size: 20px; margin-right: 10px; }}
        .user-info {{ background-color: #e9ecef; padding: 10px; border-radius: 4px; margin: 10px 0; }}
        .btn {{ display: inline-block; padding: 10px 20px; background-color: #0066cc; color: white; text-decoration: none; border-radius: 4px; }}
        .recipients-info {{ background-color: #fff3cd; padding: 10px; border-radius: 4px; margin: 10px 0; border-left: 4px solid #ffc107; }}
        .urgency {{ padding: 3px 8px; border-radius: 3px; background-color: #ffc107; color: white; font-size: 12px; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2><span class='action-icon'>{actionIcon}</span>Cập nhật trên Ticket #{ticket.Id}</h2>
        </div>
        <div class='content'>
            <p>Ticket <span class='highlight'>{ticket.Title}</span> vừa có cập nhật mới.</p>
            
            <div class='ticket-info'>
                <p><strong>Ticket ID:</strong> #{ticket.Id}</p>
                <p><strong>Tiêu đề:</strong> {ticket.Title}</p>
                <p><strong>Trạng thái:</strong> <span class='status' style='background-color: {statusColor};'>{ticket.Status}</span></p>
                <p><strong>Mức độ khẩn cấp:</strong> <span class='urgency'>{ticket.UrgencyLevel} ⭐</span></p>
                <p><strong>Danh mục:</strong> {ticket.Category?.Name ?? "N/A"}</p>
                <p><strong>Người tạo:</strong> {ticket.CreatedBy?.Name ?? "N/A"} ({ticket.CreatedBy?.Email ?? "N/A"})</p>
                <p><strong>Người được phân công:</strong> {ticket.AssignedTo?.Name ?? "Chưa phân công"} {(ticket.AssignedTo != null ? $"({ticket.AssignedTo.Email})" : "")}</p>
            </div>
            
            <div class='user-info'>
                <p><strong>👤 {actingUser.Name}</strong> đã thực hiện: <strong>{actionType}</strong></p>
                <p><small>📧 {actingUser.Email} | 🕐 {DateTime.Now:dd/MM/yyyy HH:mm:ss}</small></p>
            </div>
            
            <div class='action-content'>
                <h4>Chi tiết cập nhật:</h4>
                <p>{content}</p>
            </div>
            
            <div style='text-align: center; margin: 20px 0;'>
                <p>Vui lòng đăng nhập vào hệ thống để xem chi tiết và phản hồi.</p>
                <a href='#' class='btn'>Xem Ticket</a>
            </div>
        </div>
        <div class='footer'>
            <p>📧 Email này được gửi tự động từ ERP Ticket System, vui lòng không trả lời.</p>
            <p>Email được gửi tới người liên quan (không bao gồm người thực hiện hành động)</p>
            <p>&copy; {DateTime.Now.Year} ERP System - Hệ thống quản lý ticket</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetStatusColor(string? status)
        {
            return status?.ToLower() switch
            {
                "open" or "new" => "#007bff",
                "in progress" or "working" => "#fd7e14",
                "closed" or "completed" or "resolved" => "#28a745",
                "on hold" or "pending" => "#ffc107",
                "cancelled" => "#dc3545",
                _ => "#6c757d"
            };
        }

        private string GetActionIcon(string emailType)
        {
            return emailType switch
            {
                "comment" => "💬",
                "created" => "🆕",
                "assigned" => "👤",
                "status_changed" => "🔄",
                _ => "📝"
            };
        }

        private string FormatAccountCreationEmailBody(User user, string plainPassword, string activationLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; color: white; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 30px; background-color: #f8f9fa; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding: 20px; text-align: center; background-color: #fff; border-radius: 0 0 10px 10px; }}
        .credentials-box {{ background-color: white; padding: 20px; border-left: 4px solid #667eea; margin: 20px 0; border-radius: 4px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .credential-item {{ padding: 10px 0; border-bottom: 1px solid #eee; }}
        .credential-item:last-child {{ border-bottom: none; }}
        .credential-label {{ font-weight: bold; color: #667eea; display: inline-block; width: 150px; }}
        .credential-value {{ color: #333; font-family: 'Courier New', monospace; background-color: #f0f0f0; padding: 5px 10px; border-radius: 3px; }}
        .btn {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white !important; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .btn:hover {{ box-shadow: 0 6px 8px rgba(0,0,0,0.15); }}
        .warning-box {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .icon {{ font-size: 24px; margin-right: 10px; }}
        .highlight {{ color: #667eea; font-weight: bold; }}
        .welcome-text {{ font-size: 18px; margin: 20px 0; }}
        .steps {{ background-color: white; padding: 20px; border-radius: 4px; margin: 20px 0; }}
        .step {{ padding: 10px 0; }}
        .step-number {{ display: inline-block; width: 30px; height: 30px; background-color: #667eea; color: white; border-radius: 50%; text-align: center; line-height: 30px; margin-right: 10px; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1><span class='icon'>🎉</span>Chào mừng đến với ERP System!</h1>
            <p style='font-size: 16px; margin-top: 10px;'>Tài khoản của bạn đã được tạo thành công</p>
        </div>
        <div class='content'>
            <p class='welcome-text'>Xin chào <strong>{user.Name}</strong>,</p>
            
            <p>Tài khoản ERP của bạn đã được tạo thành công. Dưới đây là thông tin đăng nhập của bạn:</p>
            
            <div class='credentials-box'>
                <h3 style='color: #667eea; margin-top: 0;'>🔐 Thông tin đăng nhập</h3>
                <div class='credential-item'>
                    <span class='credential-label'>📧 Email/Tài khoản:</span>
                    <span class='credential-value'>{user.Email}</span>
                </div>
                <div class='credential-item'>
                    <span class='credential-label'>🔑 Mật khẩu tạm thời:</span>
                    <span class='credential-value'>{plainPassword}</span>
                </div>
                <div class='credential-item'>
                    <span class='credential-label'>👤 Họ tên:</span>
                    <span class='credential-value'>{user.Name}</span>
                </div>
                <div class='credential-item'>
                    <span class='credential-label'>🏢 Phòng ban:</span>
                    <span class='credential-value'>{user.Department?.Name ?? "Chưa xác định"}</span>
                </div>
                <div class='credential-item'>
                    <span class='credential-label'>💼 Chức vụ:</span>
                    <span class='credential-value'>{user.Position?.PositionName ?? "Chưa xác định"}</span>
                </div>
            </div>
            
            <div class='warning-box'>
                <strong>⚠️ Lưu ý quan trọng:</strong>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>Đây là mật khẩu tạm thời, vui lòng đổi mật khẩu ngay sau lần đăng nhập đầu tiên</li>
                    <li>Không chia sẻ thông tin này với bất kỳ ai</li>
                    <li>Link kích hoạt có hiệu lực trong 24 giờ</li>
                </ul>
            </div>

            <div class='steps'>
                <h3 style='color: #667eea; margin-top: 0;'>📋 Các bước thực hiện</h3>
                <div class='step'>
                    <span class='step-number'>1</span>
                    <span>Nhấn vào nút &quot;Kích hoạt tài khoản&quot; bên dưới</span>
                </div>
                <div class='step'>
                    <span class='step-number'>2</span>
                    <span>Đăng nhập bằng email và mật khẩu tạm thời</span>
                </div>
                <div class='step'>
                    <span class='step-number'>3</span>
                    <span>Thay đổi mật khẩu theo yêu cầu hệ thống</span>
                </div>
                <div class='step'>
                    <span class='step-number'>4</span>
                    <span>Bắt đầu sử dụng hệ thống ERP</span>
                </div>
            </div>
            
            <div style='text-align: center;'>
                <a href='{activationLink}' class='btn'>🚀 Kích hoạt tài khoản ngay</a>
            </div>

            <p style='margin-top: 30px; color: #666;'>
                Nếu nút không hoạt động, vui lòng sao chép link dưới đây vào trình duyệt:<br>
                <a href='{activationLink}' style='color: #667eea; word-break: break-all;'>{activationLink}</a>
            </p>
        </div>
        <div class='footer'>
            <p>📧 Email này được gửi tự động từ ERP System, vui lòng không trả lời.</p>
            <p>Nếu bạn không yêu cầu tạo tài khoản này, vui lòng liên hệ với quản trị viên ngay lập tức.</p>
            <p>&copy; {DateTime.Now.Year} ERP System - Hệ thống quản lý doanh nghiệp</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}