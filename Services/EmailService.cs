using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using erp_backend.Models;
using erp_backend.Data;
using Microsoft.EntityFrameworkCore;

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
        Task SendNotificationEmailAsync(string recipientEmail, string recipientName, string notificationTitle, string notificationContent, DateTime createdAt);
        Task SendCustomerNotificationEmailAsync(string recipientEmail, string recipientName, string notificationTitle, string notificationContent, DateTime createdAt, string customerType);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly ApplicationDbContext _context;

        public EmailService(
            IConfiguration configuration, 
            ILogger<EmailService> logger,
            ApplicationDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
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

                // ✅ Lấy template từ database
                var template = await _context.DocumentTemplates
                    .Where(t => t.Code == "EMAIL_ACCOUNT_CREATION" && t.IsActive)
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    _logger.LogWarning("Template EMAIL_ACCOUNT_CREATION not found in database. Falling back to hardcoded template.");
                    // Fallback to old method if template not found
                    await SendAccountCreationEmailLegacyAsync(user, plainPassword, activationLink);
                    return;
                }

                // Lấy thông tin SMTP từ config
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];

                // ✅ Bind dữ liệu vào template
                var htmlBody = template.HtmlContent
                    .Replace("{{UserName}}", user.Name)
                    .Replace("{{UserEmail}}", user.Email)
                    .Replace("{{PlainPassword}}", plainPassword)
                    .Replace("{{DepartmentName}}", user.Department?.Name ?? "Chưa xác định")
                    .Replace("{{PositionName}}", user.Position?.PositionName ?? "Chưa xác định")
                    .Replace("{{ActivationLink}}", activationLink)
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

                // Tạo email message
                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Tài khoản ERP của bạn đã được tạo - Vui lòng kích hoạt",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mail.To.Add(user.Email);

                // Gửi email
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 30000;

                    await smtpClient.SendMailAsync(mail);
                }

                _logger.LogInformation("Account creation email sent successfully to {Email} for user {UserId} using database template", user.Email, user.Id);
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

                // ✅ Lấy template từ database
                var template = await _context.DocumentTemplates
                    .Where(t => t.Code == "EMAIL_PASSWORD_RESET_OTP" && t.IsActive)
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    _logger.LogWarning("Template EMAIL_PASSWORD_RESET_OTP not found in database. Falling back to hardcoded template.");
                    await SendPasswordResetOtpLegacyAsync(email, userName, otpCode, expiresAt);
                    return;
                }

                // Lấy thông tin SMTP từ config
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];

                // Tính thời gian hết hạn
                var expiryMinutes = (int)(expiresAt - DateTime.UtcNow).TotalMinutes;

                // ✅ Bind dữ liệu vào template
                var htmlBody = template.HtmlContent
                    .Replace("{{UserName}}", userName)
                    .Replace("{{OtpCode}}", otpCode)
                    .Replace("{{ExpiryMinutes}}", expiryMinutes.ToString())
                    .Replace("{{ExpiresAt}}", expiresAt.ToString("dd/MM/yyyy HH:mm:ss"))
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

                // Tạo email message
                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Mã OTP đổi mật khẩu - ERP System",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mail.To.Add(email);

                // Gửi email
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 30000;

                    await smtpClient.SendMailAsync(mail);
                }

                _logger.LogInformation("OTP email sent successfully to {Email} using database template", email);
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

        public async Task SendNotificationEmailAsync(string recipientEmail, string recipientName, string notificationTitle, string notificationContent, DateTime createdAt)
        {
            try
            {
                // Validate email configuration first
                if (!ValidateEmailConfiguration())
                {
                    _logger.LogWarning("Email configuration is incomplete. Skipping notification email for {Email}", recipientEmail);
                    return;
                }

                _logger.LogInformation("Preparing to send notification email to {Email}", recipientEmail);

                // ✅ Lấy template từ database
                var template = await _context.DocumentTemplates
                    .Where(t => t.Code == "EMAIL_NOTIFICATION" && t.IsActive)
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    _logger.LogWarning("Template EMAIL_NOTIFICATION not found in database. Falling back to hardcoded template.");
                    await SendNotificationEmailLegacyAsync(recipientEmail, recipientName, notificationTitle, notificationContent, createdAt);
                    return;
                }

                // Lấy thông tin SMTP từ config
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];

                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
                var notificationUrl = $"{frontendUrl}/notifications";

                // ✅ Bind dữ liệu vào template
                var htmlBody = template.HtmlContent
                    .Replace("{{RecipientName}}", recipientName)
                    .Replace("{{NotificationTitle}}", notificationTitle)
                    .Replace("{{NotificationContent}}", notificationContent)
                    .Replace("{{CreatedAt}}", createdAt.ToString("dd/MM/yyyy HH:mm:ss"))
                    .Replace("{{NotificationUrl}}", notificationUrl)
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

                // Tạo email message
                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = $"[ERP Notification] {notificationTitle}",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mail.To.Add(recipientEmail);

                // Gửi email
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 30000;

                    await smtpClient.SendMailAsync(mail);
                }

                _logger.LogInformation("Notification email sent successfully to {Email} using database template", recipientEmail);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending notification email to {Email}: {StatusCode} - {Message}", 
                    recipientEmail, smtpEx.StatusCode, smtpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification email to {Email}", recipientEmail);
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

                // ✅ Lấy template từ database
                var template = await _context.DocumentTemplates
                    .Where(t => t.Code == "EMAIL_PAYMENT_SUCCESS" && t.IsActive)
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    _logger.LogWarning("Template EMAIL_PAYMENT_SUCCESS not found in database. Falling back to hardcoded template.");
                    await SendPaymentSuccessNotificationLegacyAsync(contract, amount, paymentType, transactionId, transactionDate, customer, saleUser);
                    return;
                }

                // Xác định người nhận
                var recipients = new Dictionary<string, string>();

                // 1. Email khách hàng
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    recipients[customer.Email.Trim().ToLower()] = "customer";
                }
                if (!string.IsNullOrEmpty(customer.RepresentativeEmail))
                {
                    recipients[customer.RepresentativeEmail.Trim().ToLower()] = "customer";
                }

                // 2. Email sale
                if (saleUser != null && !string.IsNullOrEmpty(saleUser.Email))
                {
                    recipients[saleUser.Email.Trim().ToLower()] = "sale";
                }
                if (saleUser != null && !string.IsNullOrEmpty(saleUser.SecondaryEmail))
                {
                    recipients[saleUser.SecondaryEmail.Trim().ToLower()] = "sale";
                }

                // 3. Email admin
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
                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";

                // Dữ liệu chung cho template
                var paymentTypeText = paymentType switch
                {
                    "deposit50" => "Đặt cọc 50%",
                    "final50" => "Thanh toán nốt 50%",
                    "full100" => "Thanh toán 100%",
                    _ => "Thanh toán"
                };

                // Gửi email riêng cho từng loại người nhận
                foreach (var recipient in recipients)
                {
                    try
                    {
                        var greeting = recipient.Value switch
                        {
                            "customer" => $"Kính gửi quý khách hàng <strong>{customer.Name ?? customer.CompanyName}</strong>,",
                            "sale" => $"Xin chào <strong>{saleUser?.Name}</strong>,",
                            "admin" => "Kính gửi Quản trị viên,",
                            _ => "Xin chào,"
                        };

                        var mainMessage = recipient.Value switch
                        {
                            "customer" => $"Chúng tôi xác nhận đã nhận được thanh toán {paymentTypeText} cho hợp đồng #{contract.NumberContract}.",
                            "sale" => $"Bạn đã thực hiện thanh toán {paymentTypeText} cho hợp đồng #{contract.NumberContract}.",
                            "admin" => $"Đã có giao dịch thanh toán {paymentTypeText} cho hợp đồng #{contract.NumberContract}.",
                            _ => $"Đã có giao dịch thanh toán {paymentTypeText} cho hợp đồng #{contract.NumberContract}."
                        };

                        var customerInfo = recipient.Value == "customer" ? 
                            $"<p><strong>Thông tin khách hàng:</strong> {customer.Name} - {customer.Email}</p>" : "";

                        var saleInfo = recipient.Value == "sale" && saleUser != null ? 
                            $"<p><strong>Người tạo:</strong> {saleUser.Name} ({saleUser.Email})</p>" : "";

                        var contractUrl = $"{frontendUrl}/contracts/{contract.Id}";

                        // ✅ Bind dữ liệu vào template
                        var htmlBody = template.HtmlContent
                            .Replace("{{Greeting}}", greeting)
                            .Replace("{{MainMessage}}", mainMessage)
                            .Replace("{{ContractNumber}}", contract.NumberContract.ToString())
                            .Replace("{{Amount}}", amount.ToString("N0") + " VNĐ")
                            .Replace("{{PaymentType}}", paymentTypeText)
                            .Replace("{{TransactionId}}", transactionId)
                            .Replace("{{TransactionDate}}", transactionDate.ToString("dd/MM/yyyy HH:mm:ss"))
                            .Replace("{{CustomerInfo}}", customerInfo)
                            .Replace("{{SaleInfo}}", saleInfo)
                            .Replace("{{ContractUrl}}", contractUrl)
                            .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

                        var mail = new MailMessage
                        {
                            From = new MailAddress(senderEmail, senderName),
                            Subject = $"[Hợp đồng #{contract.NumberContract}] Xác nhận {paymentTypeText} thành công",
                            Body = htmlBody,
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

                        _logger.LogInformation("Payment notification email sent successfully to {Email} ({RecipientType}) for contract {ContractId} using database template", 
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

        public async Task SendCustomerNotificationEmailAsync(string recipientEmail, string recipientName, string notificationTitle, string notificationContent, DateTime createdAt, string customerType)
        {
            try
            {
                // Validate email configuration first
                if (!ValidateEmailConfiguration())
                {
                    _logger.LogWarning("Email configuration is incomplete. Skipping customer notification email for {Email}", recipientEmail);
                    return;
                }

                _logger.LogInformation("Preparing to send customer notification email to {Email}", recipientEmail);

                // Lấy thông tin SMTP từ config
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];

                // Tạo email message - sử dụng hardcoded template (không có template riêng cho customer notification trong DB)
                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = $"[Thông báo] {notificationTitle}",
                    Body = FormatCustomerNotificationEmailBody(recipientName, notificationTitle, notificationContent, createdAt, customerType),
                    IsBodyHtml = true
                };

                mail.To.Add(recipientEmail);

                // Gửi email
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 30000;

                    await smtpClient.SendMailAsync(mail);
                }

                _logger.LogInformation("Customer notification email sent successfully to {Email}", recipientEmail);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending customer notification email to {Email}: {StatusCode} - {Message}", 
                    recipientEmail, smtpEx.StatusCode, smtpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending customer notification email to {Email}", recipientEmail);
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

        // ==================== LEGACY FALLBACK METHODS ====================

        private async Task SendAccountCreationEmailLegacyAsync(User user, string plainPassword, string activationLink)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderName = _configuration["Email:SenderName"];

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail ?? "", senderName),
                Subject = "Tài khoản ERP của bạn đã được tạo - Vui lòng kích hoạt",
                Body = FormatAccountCreationEmailBody(user, plainPassword, activationLink),
                IsBodyHtml = true
            };

            mail.To.Add(user.Email);

            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;
                smtpClient.Timeout = 30000;

                await smtpClient.SendMailAsync(mail);
            }

            _logger.LogInformation("Account creation email sent successfully to {Email} for user {UserId} using legacy template", user.Email, user.Id);
        }

        private async Task SendPasswordResetOtpLegacyAsync(string email, string userName, string otpCode, DateTime expiresAt)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderName = _configuration["Email:SenderName"];

            var expiryMinutes = (int)(expiresAt - DateTime.UtcNow).TotalMinutes;

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail ?? "", senderName),
                Subject = "Mã OTP đổi mật khẩu - ERP System",
                Body = FormatOtpEmailBody(userName, otpCode, expiresAt, expiryMinutes),
                IsBodyHtml = true
            };

            mail.To.Add(email);

            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;
                smtpClient.Timeout = 30000;

                await smtpClient.SendMailAsync(mail);
            }

            _logger.LogInformation("OTP email sent successfully to {Email} using legacy template", email);
        }

        private async Task SendNotificationEmailLegacyAsync(string recipientEmail, string recipientName, string notificationTitle, string notificationContent, DateTime createdAt)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderName = _configuration["Email:SenderName"];

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail ?? "", senderName),
                Subject = $"[ERP Notification] {notificationTitle}",
                Body = FormatNotificationEmailBody(recipientName, notificationTitle, notificationContent, createdAt),
                IsBodyHtml = true
            };

            mail.To.Add(recipientEmail);

            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;
                smtpClient.Timeout = 30000;

                await smtpClient.SendMailAsync(mail);
            }

            _logger.LogInformation("Notification email sent successfully to {Email} using legacy template", recipientEmail);
        }

        private async Task SendPaymentSuccessNotificationLegacyAsync(
            Contract contract, decimal amount, string paymentType, string transactionId,
            DateTime transactionDate, Customer customer, User? saleUser)
        {
            var recipients = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(customer.Email))
                recipients[customer.Email.Trim().ToLower()] = "customer";
            if (!string.IsNullOrEmpty(customer.RepresentativeEmail))
                recipients[customer.RepresentativeEmail.Trim().ToLower()] = "customer";
            if (saleUser != null && !string.IsNullOrEmpty(saleUser.Email))
                recipients[saleUser.Email.Trim().ToLower()] = "sale";
            if (saleUser != null && !string.IsNullOrEmpty(saleUser.SecondaryEmail))
                recipients[saleUser.SecondaryEmail.Trim().ToLower()] = "sale";

            var adminEmail = _configuration["AdminEmail"];
            if (!string.IsNullOrEmpty(adminEmail))
                recipients[adminEmail.Trim().ToLower()] = "admin";

            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderName = _configuration["Email:SenderName"];

            foreach (var recipient in recipients)
            {
                try
                {
                    var mail = new MailMessage
                    {
                        From = new MailAddress(senderEmail ?? "", senderName),
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

                    _logger.LogInformation("Payment notification email sent successfully to {Email} ({RecipientType}) for contract {ContractId} using legacy template", 
                        recipient.Key, recipient.Value, contract.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send payment notification to {Email} for contract {ContractId}", recipient.Key, contract.Id);
                }
            }
        }

        // ==================== TICKET EMAIL NOTIFICATION ====================

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

                // Logic EXCLUDE người thực hiện action
                
                // Thêm email người tạo ticket (nếu KHÔNG phải người thực hiện)
                if (ticket.CreatedBy != null && 
                    !string.IsNullOrEmpty(ticket.CreatedBy.Email) &&
                    ticket.CreatedBy.Id != actingUser.Id)
                {
                    recipients.Add(ticket.CreatedBy.Email.Trim().ToLower());
                    _logger.LogDebug("Added CreatedBy email to recipients: {Email} for ticket {TicketId}", 
                        ticket.CreatedBy.Email, ticket.Id);
                }

                // Thêm email người được phân công (nếu KHÔNG phải người thực hiện)
                if (ticket.AssignedTo != null && 
                    !string.IsNullOrEmpty(ticket.AssignedTo.Email) &&
                    ticket.AssignedTo.Id != actingUser.Id)
                {
                    recipients.Add(ticket.AssignedTo.Email.Trim().ToLower());
                    _logger.LogDebug("Added AssignedTo email to recipients: {Email} for ticket {TicketId}", 
                        ticket.AssignedTo.Email, ticket.Id);
                }

                // Thêm secondary emails nếu có
                if (ticket.CreatedBy != null && 
                    !string.IsNullOrEmpty(ticket.CreatedBy.SecondaryEmail) &&
                    ticket.CreatedBy.Id != actingUser.Id)
                {
                    recipients.Add(ticket.CreatedBy.SecondaryEmail.Trim().ToLower());
                }

                if (ticket.AssignedTo != null && 
                    !string.IsNullOrEmpty(ticket.AssignedTo.SecondaryEmail) &&
                    ticket.AssignedTo.Id != actingUser.Id)
                {
                    recipients.Add(ticket.AssignedTo.SecondaryEmail.Trim().ToLower());
                }

                // Nếu không có người nhận, không gửi email
                if (!recipients.Any())
                {
                    _logger.LogInformation("No email recipients for ticket notification on ticket {TicketId} after excluding acting user {ActingUserId}", 
                        ticket.Id, actingUser.Id);
                    return;
                }

                _logger.LogInformation("Preparing to send email for ticket {TicketId} to {RecipientCount} recipients", 
                    ticket.Id, recipients.Count);

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
                    From = new MailAddress(senderEmail ?? "", senderName),
                    Subject = GenerateEmailSubject(ticket, actionType),
                    Body = FormatEmailBody(ticket, actingUser, actionType, content, emailType),
                    IsBodyHtml = true
                };

                // Thêm người nhận
                foreach (var recipient in recipients)
                {
                    try
                    {
                        mail.To.Add(recipient);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to add recipient {Recipient} to email for ticket {TicketId}", recipient, ticket.Id);
                    }
                }

                // Gửi email
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 30000;

                    await smtpClient.SendMailAsync(mail);
                }

                _logger.LogInformation("Email notification sent successfully for ticket {TicketId} to {RecipientCount} recipients", 
                    ticket.Id, recipients.Count);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending email notification for ticket {TicketId}: {StatusCode} - {Message}", 
                    ticket.Id, smtpEx.StatusCode, smtpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification for ticket {TicketId}", ticket.Id);
            }
        }

        // ==================== EMAIL FORMATTING HELPER METHODS ====================

        private string FormatCustomerNotificationEmailBody(string recipientName, string notificationTitle, string notificationContent, DateTime createdAt, string customerType)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>{notificationTitle}</h2>
        </div>
        <div class='content'>
            <p>Kính gửi <strong>{recipientName}</strong>,</p>
            <p>{notificationContent}</p>
            <p><strong>Thời gian:</strong> {createdAt:dd/MM/yyyy HH:mm:ss}</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} ERP System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string FormatAccountCreationEmailBody(User user, string plainPassword, string activationLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; }}
        .credentials {{ background-color: #fff; padding: 15px; border-left: 4px solid #007bff; margin: 15px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 4px; margin-top: 15px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Tài khoản ERP của bạn đã được tạo</h2>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{user.Name}</strong>,</p>
            <p>Tài khoản ERP của bạn đã được tạo thành công. Dưới đây là thông tin đăng nhập:</p>
            <div class='credentials'>
                <p><strong>Email đăng nhập:</strong> {user.Email}</p>
                <p><strong>Mật khẩu tạm thời:</strong> {plainPassword}</p>
                <p><strong>Phòng ban:</strong> {user.Department?.Name ?? "Chưa xác định"}</p>
                <p><strong>Chức vụ:</strong> {user.Position?.PositionName ?? "Chưa xác định"}</p>
            </div>
            <p><strong>⚠️ Lưu ý quan trọng:</strong> Vui lòng kích hoạt tài khoản của bạn bằng cách nhấp vào nút bên dưới:</p>
            <div style='text-align: center;'>
                <a href='{activationLink}' class='button'>Kích hoạt tài khoản</a>
            </div>
            <p style='margin-top: 15px; font-size: 12px; color: #666;'>Hoặc copy link sau vào trình duyệt:<br>{activationLink}</p>
            <p><strong>🔒 Khuyến nghị bảo mật:</strong> Sau khi đăng nhập, vui lòng đổi mật khẩu ngay để bảo mật tài khoản.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} ERP System. All rights reserved.</p>
            <p>Nếu bạn không yêu cầu tạo tài khoản này, vui lòng bỏ qua email này.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string FormatOtpEmailBody(string userName, string otpCode, DateTime expiresAt, int expiryMinutes)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; }}
        .otp-box {{ background-color: #fff; padding: 20px; text-align: center; border: 2px dashed #dc3545; margin: 20px 0; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #dc3545; letter-spacing: 5px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🔒 Mã OTP đổi mật khẩu</h2>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Bạn đã yêu cầu đặt lại mật khẩu. Đây là mã OTP của bạn:</p>
            <div class='otp-box'>
                <div class='otp-code'>{otpCode}</div>
            </div>
            <p><strong>⏰ Mã OTP này sẽ hết hạn sau {expiryMinutes} phút</strong> (vào lúc {expiresAt:dd/MM/yyyy HH:mm:ss})</p>
            <p><strong>⚠️ Lưu ý bảo mật:</strong></p>
            <ul>
                <li>Không chia sẻ mã OTP này với bất kỳ ai</li>
                <li>Nhân viên hệ thống sẽ không bao giờ yêu cầu mã OTP của bạn</li>
                <li>Nếu bạn không yêu cầu đổi mật khẩu, vui lòng bỏ qua email này</li>
            </ul>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} ERP System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string FormatNotificationEmailBody(string recipientName, string notificationTitle, string notificationContent, DateTime createdAt)
        {
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var notificationUrl = $"{frontendUrl}/notifications";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #17a2b8; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #17a2b8; color: white; text-decoration: none; border-radius: 4px; margin-top: 15px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>📢 {notificationTitle}</h2>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{recipientName}</strong>,</p>
            <p>{notificationContent}</p>
            <p><strong>Thời gian:</strong> {createdAt:dd/MM/yyyy HH:mm:ss}</p>
            <div style='text-align: center;'>
                <a href='{notificationUrl}' class='button'>Xem chi tiết</a>
            </div>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} ERP System. All rights reserved.</p>
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

        private string FormatPaymentEmailBody(Contract contract, decimal amount, string paymentType, 
            string transactionId, DateTime transactionDate, Customer customer, User? saleUser, string recipientType)
        {
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var contractUrl = $"{frontendUrl}/contracts/{contract.Id}";

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
                "customer" => $"Chúng tôi xác nhận đã nhận được thanh toán {paymentTypeText} cho hợp đồng #{contract.NumberContract}.",
                "sale" => $"Bạn đã thực hiện thanh toán {paymentTypeText} cho hợp đồng #{contract.NumberContract}.",
                "admin" => $"Đã có giao dịch thanh toán {paymentTypeText} cho hợp đồng #{contract.NumberContract}.",
                _ => $"Đã có giao dịch thanh toán {paymentTypeText} cho hợp đồng #{contract.NumberContract}."
            };

            var customerInfo = recipientType == "customer" ? "" : 
                $"<p><strong>Thông tin khách hàng:</strong> {customer.Name ?? customer.CompanyName} - {customer.Email}</p>";

            var saleInfo = recipientType == "sale" && saleUser != null ? "" : 
                saleUser != null ? $"<p><strong>Người tạo:</strong> {saleUser.Name} ({saleUser.Email})</p>" : "";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; }}
        .payment-info {{ background-color: #fff; padding: 15px; border-left: 4px solid #28a745; margin: 15px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px; margin-top: 15px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>✅ Xác nhận thanh toán thành công</h2>
        </div>
        <div class='content'>
            <p>{greeting}</p>
            <p>{mainMessage}</p>
            <div class='payment-info'>
                <p><strong>Số hợp đồng:</strong> #{contract.NumberContract}</p>
                <p><strong>Số tiền:</strong> {amount:N0} VNĐ</p>
                <p><strong>Loại thanh toán:</strong> {paymentTypeText}</p>
                <p><strong>Mã giao dịch:</strong> {transactionId}</p>
                <p><strong>Thời gian:</strong> {transactionDate:dd/MM/yyyy HH:mm:ss}</p>
                {customerInfo}
                {saleInfo}
            </div>
            <div style='text-align: center;'>
                <a href='{contractUrl}' class='button'>Xem chi tiết hợp đồng</a>
            </div>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} ERP System. All rights reserved.</p>
            <p>Cảm ơn quý khách đã sử dụng dịch vụ của chúng tôi!</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateEmailSubject(Ticket ticket, string actionType)
        {
            return $"[Ticket #{ticket.Id}] {actionType} - {ticket.Title}";
        }

        private string FormatEmailBody(Ticket ticket, User actingUser, string actionType, string content, string emailType)
        {
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var ticketUrl = $"{frontendUrl}/tickets/{ticket.Id}";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6c757d; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; }}
        .ticket-info {{ background-color: #fff; padding: 15px; border-left: 4px solid #6c757d; margin: 15px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px; margin-top: 15px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🎫 {actionType}</h2>
        </div>
        <div class='content'>
            <p>Xin chào,</p>
            <p><strong>{actingUser.Name}</strong> đã thực hiện hành động: <strong>{actionType}</strong></p>
            <div class='ticket-info'>
                <p><strong>Ticket ID:</strong> #{ticket.Id}</p>
                <p><strong>Tiêu đề:</strong> {ticket.Title}</p>
                <p><strong>Trạng thái:</strong> {ticket.Status}</p>
                <p><strong>Nội dung:</strong></p>
                <p>{content}</p>
            </div>
            <div style='text-align: center;'>
                <a href='{ticketUrl}' class='button'>Xem chi tiết ticket</a>
            </div>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} ERP System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
