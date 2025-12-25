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
        Task SendPaymentSuccessNotificationAsync(Contract contract, decimal amount, string paymentType, string transactionId, DateTime transactionDate, Customer customer, User? saleUser);
        Task SendPasswordResetOtpAsync(string email, string userName, string otpCode, DateTime expiresAt);
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

        public async Task SendPaymentSuccessNotificationAsync(
            Contract contract, 
            decimal amount, 
            string paymentType, 
            string transactionId, 
            DateTime transactionDate,
            Customer customer,
            User? saleUser)
        {
            try
            {
                // Validate email configuration first
                if (!ValidateEmailConfiguration())
                {
                    _logger.LogWarning("Email configuration is incomplete. Skipping payment notification for contract {ContractId}", contract.Id);
                    return;
                }

                _logger.LogInformation("Preparing to send payment success notification for contract {ContractId}", contract.Id);

                // Xác định người nhận - sử dụng Dictionary để phân biệt loại người nhận
                var recipients = new Dictionary<string, string>(); // email -> recipient type

                // 1. Email khách hàng
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    recipients[customer.Email.Trim().ToLower()] = "customer";
                }
                if (!string.IsNullOrEmpty(customer.RepresentativeEmail))
                {
                    recipients[customer.RepresentativeEmail.Trim().ToLower()] = "customer";
                }

                // 2. Email sale (người tạo khách hàng)
                if (saleUser != null && !string.IsNullOrEmpty(saleUser.Email))
                {
                    recipients[saleUser.Email.Trim().ToLower()] = "sale";
                }
                if (saleUser != null && !string.IsNullOrEmpty(saleUser.SecondaryEmail))
                {
                    recipients[saleUser.SecondaryEmail.Trim().ToLower()] = "sale";
                }

                // 3. Email admin - lấy từ config hoặc tìm user có role admin
                var adminEmail = _configuration["AdminEmail"];
                if (!string.IsNullOrEmpty(adminEmail))
                {
                    recipients[adminEmail.Trim().ToLower()] = "admin";
                }

                if (!recipients.Any())
                {
                    _logger.LogWarning("No email recipients for payment notification on contract {ContractId}", contract.Id);
                    return;
                }

                _logger.LogInformation("Preparing to send payment notification emails to {RecipientCount} recipients for contract {ContractId}", 
                    recipients.Count, contract.Id);

                // Lấy thông tin SMTP từ config
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];

                // Gửi email riêng cho từng loại người nhận
                foreach (var recipient in recipients)
                {
                    try
                    {
                        var mail = new MailMessage
                        {
                            From = new MailAddress(senderEmail, senderName),
                            Subject = GeneratePaymentEmailSubject(contract, paymentType),
                            Body = FormatPaymentEmailBody(contract, amount, paymentType, transactionId, transactionDate, customer, saleUser, recipient.Value),
                            IsBodyHtml = true
                        };

                        mail.To.Add(recipient.Key);

                        using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                        {
                            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                            smtpClient.EnableSsl = true;
                            smtpClient.Timeout = 30000;

                            await smtpClient.SendMailAsync(mail);
                        }

                        _logger.LogInformation("Payment notification email sent successfully to {Email} ({RecipientType}) for contract {ContractId}", 
                            recipient.Key, recipient.Value, contract.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send payment notification to {Email} for contract {ContractId}", recipient.Key, contract.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment notification for contract {ContractId}", contract.Id);
            }
        }

        /// <summary>
        /// Gửi email mã OTP để đổi mật khẩu
        /// </summary>
        public async Task SendPasswordResetOtpAsync(string email, string userName, string otpCode, DateTime expiresAt)
        {
            try
            {
                // Validate email configuration first
                if (!ValidateEmailConfiguration())
                {
                    _logger.LogWarning("Email configuration is incomplete. Skipping OTP email for {Email}", email);
                    return;
                }

                _logger.LogInformation("Preparing to send OTP email to {Email}", email);

                // Lấy thông tin SMTP từ config
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];

                // Tính thời gian hết hạn
                var expiryMinutes = (int)(expiresAt - DateTime.UtcNow).TotalMinutes;

                // Tạo email message
                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Mã OTP đổi mật khẩu - ERP System",
                    Body = FormatOtpEmailBody(userName, otpCode, expiresAt, expiryMinutes),
                    IsBodyHtml = true
                };

                mail.To.Add(email);

                // Tạo SMTP client và gửi email
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 30000; // 30 seconds timeout

                    await smtpClient.SendMailAsync(mail);
                }

                _logger.LogInformation("OTP email sent successfully to {Email}", email);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending OTP email to {Email}: {StatusCode} - {Message}", 
                    email, smtpEx.StatusCode, smtpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP email to {Email}", email);
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
            
            // Lấy frontend URL từ configuration
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var ticketUrl = $"{frontendUrl}/helpdesk";

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
                <a href='{ticketUrl}' class='btn'>Xem Ticket</a>
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

        private string GeneratePaymentEmailSubject(Contract contract, string paymentType)
        {
            var paymentTypeText = paymentType switch
            {
                "deposit50" => "Đặt cọc 50%",
                "final50" => "Thanh toán nốt 50%",
                "full100" => "Thanh toán 100%",
                _ => "Thanh toán"
            };

            return $"[Hợp đồng #{contract.NumberContract}] Xác nhận {paymentTypeText} thành công";
        }

        /// <summary>
        /// Format email body cho OTP đổi mật khẩu
        /// </summary>
        private string FormatOtpEmailBody(string userName, string otpCode, DateTime expiresAt, int expiryMinutes)
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
        .otp-box {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; margin: 30px 0; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .otp-code {{ font-size: 48px; font-weight: bold; letter-spacing: 10px; margin: 20px 0; font-family: 'Courier New', monospace; text-shadow: 2px 2px 4px rgba(0,0,0,0.2); }}
        .warning-box {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .icon {{ font-size: 48px; }}
        .info-box {{ background-color: #e7f5ff; padding: 15px; border-radius: 4px; margin: 15px 0; border-left: 4px solid #0066cc; }}
        .timer {{ background-color: #ffc107; color: #333; padding: 10px 20px; border-radius: 20px; display: inline-block; font-weight: bold; margin-top: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='icon'>🔐</div>
            <h1 style='margin: 10px 0;'>Đổi mật khẩu</h1>
            <p style='font-size: 16px; margin: 0;'>Mã OTP xác thực</p>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            
            <p>Bạn đã yêu cầu đổi mật khẩu cho tài khoản ERP của mình. Dưới đây là mã OTP để xác thực:</p>
            
            <div class='otp-box'>
                <div style='font-size: 18px; margin-bottom: 10px;'>Mã OTP của bạn là:</div>
                <div class='otp-code'>{otpCode}</div>
                <div class='timer'>⏰ Có hiệu lực trong {expiryMinutes} phút</div>
            </div>

            <div class='info-box'>
                <h4 style='margin-top: 0; color: #0066cc;'>📋 Hướng dẫn sử dụng</h4>
                <ol style='margin: 10px 0; padding-left: 20px;'>
                    <li>Nhập mã OTP <strong>{otpCode}</strong> vào form đổi mật khẩu</li>
                    <li>Nhập mật khẩu mới (tối thiểu 8 ký tự)</li>
                    <li>Xác nhận mật khẩu mới</li>
                    <li>Nhấn &quot;Đổi mật khẩu&quot; để hoàn tất</li>
                </ol>
            </div>
            
            <div class='warning-box'>
                <strong>⚠️ Lưu ý quan trọng:</strong>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>Mã OTP này chỉ có hiệu lực trong <strong>{expiryMinutes} phút</strong></li>
                    <li>Mã OTP chỉ được sử dụng <strong>một lần duy nhất</strong></li>
                    <li>Không chia sẻ mã OTP này với bất kỳ ai</li>
                    <li>Nếu bạn không yêu cầu đổi mật khẩu, vui lòng bỏ qua email này và liên hệ với quản trị viên ngay</li>
                </ul>
            </div>

            <div class='info-box'>
                <p style='margin: 0;'><strong>🕐 Thời gian hết hạn:</strong> {expiresAt:dd/MM/yyyy HH:mm:ss} UTC</p>
                <p style='margin: 10px 0 0 0; font-size: 12px; color: #666;'>
                    (Giờ Việt Nam: {expiresAt.AddHours(7):dd/MM/yyyy HH:mm:ss})
                </p>
            </div>

            <p style='margin-top: 30px; color: #666;'>
                Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ với quản trị viên để được hỗ trợ.
            </p>
        </div>
        <div class='footer'>
            <p>📧 Email này được gửi tự động từ ERP System, vui lòng không trả lời.</p>
            <p>Nếu bạn gặp vấn đề khi đổi mật khẩu, vui lòng liên hệ với quản trị viên hệ thống.</p>
            <p>&copy; {DateTime.Now.Year} ERP System - Hệ thống quản lý doanh nghiệp</p>
        </div>
    </div>
</body>
</html>";
        }

        private string FormatPaymentEmailBody(
            Contract contract, 
            decimal amount, 
            string paymentType, 
            string transactionId, 
            DateTime transactionDate,
            Customer customer,
            User? saleUser,
            string recipientType)
        {
            var paymentTypeText = paymentType switch
            {
                "deposit50" => "Đặt cọc 50%",
                "final50" => "Thanh toán nốt 50%",
                "full100" => "Thanh toán 100%",
                _ => "Thanh toán"
            };

            var greeting = recipientType switch
            {
                "customer" => $"Kính gửi quý khách hàng <strong>{customer.Name ?? customer.CompanyName}</strong>,",
                "sale" => $"Xin chào <strong>{saleUser?.Name}</strong>,",
                "admin" => "Kính gửi Quản trị viên,",
                _ => "Xin chào,"
            };

            var mainMessage = recipientType switch
            {
                "customer" => $"Chúng tôi xác nhận đã nhận được khoản thanh toán <strong>{paymentTypeText}</strong> của quý khách cho hợp đồng số <strong>#{contract.NumberContract}</strong>.",
                "sale" => $"Khách hàng <strong>{customer.Name ?? customer.CompanyName}</strong> đã thanh toán thành công <strong>{paymentTypeText}</strong> cho hợp đồng số <strong>#{contract.NumberContract}</strong>.",
                "admin" => $"Hệ thống đã ghi nhận khoản thanh toán <strong>{paymentTypeText}</strong> cho hợp đồng số <strong>#{contract.NumberContract}</strong> của khách hàng <strong>{customer.Name ?? customer.CompanyName}</strong>.",
                _ => $"Khoản thanh toán <strong>{paymentTypeText}</strong> cho hợp đồng số <strong>#{contract.NumberContract}</strong> đã được xác nhận."
            };

            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var contractUrl = recipientType == "customer" 
                ? $"{frontendUrl}" 
                : $"{frontendUrl}/contracts/{contract.Id}";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); padding: 30px; text-align: center; color: white; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 30px; background-color: #f8f9fa; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding: 20px; text-align: center; background-color: #fff; border-radius: 0 0 10px 10px; }}
        .payment-info {{ background-color: white; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0; border-radius: 4px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .info-row {{ padding: 10px 0; border-bottom: 1px solid #eee; }}
        .info-row:last-child {{ border-bottom: none; }}
        .info-label {{ font-weight: bold; color: #28a745; display: inline-block; width: 180px; }}
        .info-value {{ color: #333; }}
        .success-badge {{ background-color: #28a745; color: white; padding: 8px 16px; border-radius: 20px; display: inline-block; font-weight: bold; }}
        .btn {{ display: inline-block; padding: 12px 30px; background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white !important; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .btn:hover {{ box-shadow: 0 6px 8px rgba(0,0,0,0.15); }}
        .icon {{ font-size: 48px; }}
        .highlight {{ color: #28a745; font-weight: bold; }}
        .customer-info {{ background-color: #e7f5ff; padding: 15px; border-radius: 4px; margin: 15px 0; }}
        .thank-you {{ background-color: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='icon'>✅</div>
            <h1 style='margin: 10px 0;'>Thanh toán thành công!</h1>
            <p style='font-size: 16px; margin: 0;'>{paymentTypeText}</p>
        </div>
        <div class='content'>
            <p>{greeting}</p>
            
            <p>{mainMessage}</p>
            
            <div class='payment-info'>
                <h3 style='color: #28a745; margin-top: 0;'>💰 Thông tin thanh toán</h3>
                <div class='info-row'>
                    <span class='info-label'>📋 Số hợp đồng:</span>
                    <span class='info-value'><strong>#{contract.NumberContract}</strong></span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>💵 Số tiền đã thanh toán:</span>
                    <span class='info-value'><strong>{amount:N0} VNĐ</strong></span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>📊 Loại thanh toán:</span>
                    <span class='info-value'><span class='success-badge'>{paymentTypeText}</span></span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>🔖 Mã giao dịch:</span>
                    <span class='info-value'>{transactionId}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>🕐 Thời gian:</span>
                    <span class='info-value'>{transactionDate:dd/MM/yyyy HH:mm:ss}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>✅ Trạng thái hợp đồng:</span>
                    <span class='info-value'><strong>{contract.Status}</strong></span>
                </div>
            </div>

            {(recipientType == "customer" ? $@"
            <div class='customer-info'>
                <h4 style='margin-top: 0; color: #0066cc;'>👤 Thông tin khách hàng</h4>
                <p><strong>Tên:</strong> {customer.Name ?? customer.CompanyName}</p>
                {(!string.IsNullOrEmpty(customer.PhoneNumber) ? $"<p><strong>Số điện thoại:</strong> {customer.PhoneNumber}</p>" : "")}
                {(!string.IsNullOrEmpty(customer.Email) ? $"<p><strong>Email:</strong> {customer.Email}</p>" : "")}
            </div>" : "")}

            {(recipientType == "sale" || recipientType == "admin" ? $@"
            <div class='customer-info'>
                <h4 style='margin-top: 0; color: #0066cc;'>👤 Thông tin khách hàng</h4>
                <p><strong>Tên khách hàng:</strong> {customer.Name ?? customer.CompanyName}</p>
                <p><strong>Loại khách hàng:</strong> {customer.CustomerType}</p>
                {(!string.IsNullOrEmpty(customer.PhoneNumber) ? $"<p><strong>Số điện thoại:</strong> {customer.PhoneNumber}</p>" : "")}
                {(!string.IsNullOrEmpty(customer.Email) ? $"<p><strong>Email:</strong> {customer.Email}</p>" : "")}
            </div>" : "")}

            {(recipientType == "sale" && saleUser != null ? $@"
            <div class='customer-info'>
                <h4 style='margin-top: 0; color: #0066cc;'>👨‍💼 Sale phụ trách</h4>
                <p><strong>Tên:</strong> {saleUser.Name}</p>
                <p><strong>Email:</strong> {saleUser.Email}</p>
                {(!string.IsNullOrEmpty(saleUser.PhoneNumber) ? $"<p><strong>Số điện thoại:</strong> {saleUser.PhoneNumber}</p>" : "")}
            </div>" : "")}

            <div class='thank-you'>
                {(recipientType == "customer" 
                    ? "<strong>🙏 Cảm ơn quý khách!</strong><br>Chúng tôi cam kết cung cấp dịch vụ tốt nhất cho quý khách. Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi." 
                    : recipientType == "sale"
                    ? "<strong>🎉 Chúc mừng!</strong><br>Khách hàng của bạn đã thanh toán thành công. Vui lòng theo dõi và hỗ trợ khách hàng trong quá trình sử dụng dịch vụ."
                    : "<strong>📊 Thông báo hệ thống</strong><br>Giao dịch đã được ghi nhận và cập nhật vào hệ thống tự động.")}
            </div>
            
            {(recipientType != "customer" ? $@"
            <div style='text-align: center;'>
                <a href='{contractUrl}' class='btn'>📄 Xem chi tiết hợp đồng</a>
            </div>" : "")}
        </div>
        <div class='footer'>
            <p>📧 Email này được gửi tự động từ ERP System, vui lòng không trả lời.</p>
            {(recipientType == "customer" 
                ? "<p>Nếu có thắc mắc, vui lòng liên hệ với nhân viên sale phụ trách hoặc hotline: [SỐ HOTLINE]</p>" 
                : "")}
            <p>&copy; {DateTime.Now.Year} ERP System - Hệ thống quản lý doanh nghiệp</p>
        </div>
    </div>
</body>
</html>";
        }

        private string FormatPasswordResetEmailBody(string userName, string otpCode, DateTime expiresAt)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); padding: 30px; text-align: center; color: white; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 30px; background-color: #f8f9fa; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding: 20px; text-align: center; background-color: #fff; border-radius: 0 0 10px 10px; }}
        .otp-box {{ background-color: white; padding: 20px; border-left: 4px solid #dc3545; margin: 20px 0; border-radius: 4px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .otp-label {{ font-weight: bold; color: #dc3545; display: inline-block; margin-bottom: 10px; }}
        .otp-code {{ font-size: 24px; letter-spacing: 2px; color: #333; font-weight: bold; }}
        .btn {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); color: white !important; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .btn:hover {{ box-shadow: 0 6px 8px rgba(0,0,0,0.15); }}
        .warning-box {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .icon {{ font-size: 24px; margin-right: 10px; }}
        .highlight {{ color: #dc3545; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1><span class='icon'>🔐</span>Xác nhận thay đổi mật khẩu</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            
            <p>Chúng tôi đã nhận được yêu cầu thay đổi mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã xác nhận dưới đây để hoàn tất quá trình:</p>
            
            <div class='otp-box'>
                <span class='otp-label'>Mã xác nhận:</span>
                <span class='otp-code'>{otpCode}</span>
            </div>
            
            <p>Liên kết này sẽ hết hạn vào <strong>{expiresAt:dd/MM/yyyy HH:mm:ss}</strong>. Vui lòng sử dụng mã xác nhận trong thời gian quy định.</p>
            
            <div style='text-align: center;'>
                <a href='{GetPasswordResetLink(otpCode)}' class='btn'>🔗 Đặt lại mật khẩu của bạn</a>
            </div>
            
            <div class='warning-box'>
                <strong>⚠️ Lưu ý:</strong>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>Mã xác nhận chỉ có giá trị trong 5 phút.</li>
                    <li>Nếu bạn không yêu cầu thay đổi mật khẩu, vui lòng bỏ qua email này.</li>
                </ul>
            </div>
        </div>
        <div class='footer'>
            <p>📧 Email này được gửi tự động từ ERP System, vui lòng không trả lời.</p>
            <p>&copy; {DateTime.Now.Year} ERP System - Hệ thống quản lý doanh nghiệp</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetLink(string otpCode)
        {
            // Giả sử link reset password có dạng: /reset-password?token=OTP_CODE
            return $"/reset-password?token={otpCode}";
        }
    }
}