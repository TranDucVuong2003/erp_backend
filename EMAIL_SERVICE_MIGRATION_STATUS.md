# ? Email Service Template Migration - Status

## ?? T human t?ng quan

Email Service ?ã ???c chuy?n ??i m?t ph?n ?? s? d?ng templates t? database. Tuy nhiên, **còn l?i compilation** do thi?u các legacy fallback methods.

## ? L?i Hi?n T?i

```
EmailService.cs(84,27,84,62): error CS0103: The name 'SendAccountCreationEmailLegacyAsync' does not exist in the current context
EmailService.cs(161,27,161,58): error CS0103: The name 'SendPasswordResetOtpLegacyAsync' does not exist in the current context  
EmailService.cs(239,27,239,59): error CS0103: The name 'SendNotificationEmailLegacyAsync' does not exist in the current context
EmailService.cs(325,27,325,68): error CS0103: The name 'SendPaymentSuccessNotificationLegacyAsync' does not exist in the current context
```

## ?? Gi?i Pháp

C?n thêm 4 legacy fallback methods vào cu?i class `EmailService` (tr??c d?u `}` ?óng class):

### 1. SendAccountCreationEmailLegacyAsync

```csharp
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
        From = new MailAddress(senderEmail, senderName),
        Subject = "Tài kho?n ERP c?a b?n ?ã ???c t?o - Vui lòng kích ho?t",
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
```

### 2. SendPasswordResetOtpLegacyAsync

```csharp
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
        From = new MailAddress(senderEmail, senderName),
        Subject = "Mã OTP ??i m?t kh?u - ERP System",
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
```

### 3. SendNotificationEmailLegacyAsync

```csharp
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
        From = new MailAddress(senderEmail, senderName),
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
```

### 4. SendPaymentSuccessNotificationLegacyAsync

```csharp
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

            _logger.LogInformation("Payment notification email sent successfully to {Email} ({RecipientType}) for contract {ContractId} using legacy template", 
                recipient.Key, recipient.Value, contract.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send payment notification to {Email} for contract {ContractId}", recipient.Key, contract.Id);
        }
    }
}
```

### 5. SendEmailNotificationAsync (cho Ticket)

File hi?n t?i c?ng thi?u ph??ng th?c `SendEmailNotificationAsync` ???c g?i b?i các ticket notification methods. Method này ph?i ???c thêm vào:

```csharp
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

        // Xác ??nh ng??i nh?n - s? d?ng HashSet ?? tránh trùng l?p
        var recipients = new HashSet<string>();

        // Logic EXCLUDE ng??i th?c hi?n action
        
        // Thêm email ng??i t?o ticket (n?u KHÔNG ph?i ng??i th?c hi?n)
        if (ticket.CreatedBy != null && 
            !string.IsNullOrEmpty(ticket.CreatedBy.Email) &&
            ticket.CreatedBy.Id != actingUser.Id)
        {
            recipients.Add(ticket.CreatedBy.Email.Trim().ToLower());
            _logger.LogDebug("Added CreatedBy email to recipients: {Email} for ticket {TicketId}", 
                ticket.CreatedBy.Email, ticket.Id);
        }

        // Thêm email ng??i ???c phân công (n?u KHÔNG ph?i ng??i th?c hi?n)
        if (ticket.AssignedTo != null && 
            !string.IsNullOrEmpty(ticket.AssignedTo.Email) &&
            ticket.AssignedTo.Id != actingUser.Id)
        {
            recipients.Add(ticket.AssignedTo.Email.Trim().ToLower());
            _logger.LogDebug("Added AssignedTo email to recipients: {Email} for ticket {TicketId}", 
                ticket.AssignedTo.Email, ticket.Id);
        }

        // Thêm secondary emails n?u có
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

        // N?u không có ng??i nh?n, không g?i email
        if (!recipients.Any())
        {
            _logger.LogInformation("No email recipients for ticket notification on ticket {TicketId} after excluding acting user {ActingUserId}", 
                ticket.Id, actingUser.Id);
            return;
        }

        _logger.LogInformation("Preparing to send email for ticket {TicketId} to {RecipientCount} recipients", 
            ticket.Id, recipients.Count);

        // L?y thông tin SMTP t? config
        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUsername = _configuration["Email:Username"];
        var smtpPassword = _configuration["Email:Password"];
        var senderEmail = _configuration["Email:SenderEmail"];
        var senderName = _configuration["Email:SenderName"];

        // T?o email message
        var mail = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = GenerateEmailSubject(ticket, actionType),
            Body = FormatEmailBody(ticket, actingUser, actionType, content, emailType),
            IsBodyHtml = true
        };

        // Thêm ng??i nh?n
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

        // G?i email
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
```

## ?? V? Trí Thêm Code

Thêm các methods trên vào cu?i class `EmailService`, TR??C d?u `}` ?óng class, SAU các formatting methods ?ã có.

## ? Tr?ng Thái Migration

### ?ã Hoàn Thành
1. ? Thêm ApplicationDbContext dependency vào constructor
2. ? Chuy?n ??i `SendAccountCreationEmailAsync` - load t? DB v?i code `EMAIL_ACCOUNT_CREATION`
3. ? Chuy?n ??i `SendPasswordResetOtpAsync` - load t? DB v?i code `EMAIL_PASSWORD_RESET_OTP`
4. ? Chuy?n ??i `SendNotificationEmailAsync` - load t? DB v?i code `EMAIL_NOTIFICATION`
5. ? Chuy?n ??i `SendPaymentSuccessNotificationAsync` - load t? DB v?i code `EMAIL_PAYMENT_SUCCESS`

### Ch?a Hoàn Thành
1. ? Thi?u 4 legacy fallback methods (c?n thêm vào)
2. ? Thi?u method `SendEmailNotificationAsync` cho ticket notifications
3. ? Build ?ang fail do thi?u các methods trên

## ?? K? Ho?ch Ti?p Theo

1. **Thêm các legacy methods vào EmailService.cs**
2. **Run build ?? verify không còn l?i**
3. **Test các ch?c n?ng email:**
   - Account creation
   - Password reset OTP
   - Notifications
   - Payment success
4. **Update documentation**

## ?? Template Mapping

| Email Type | Template Code | Status |
|------------|--------------|--------|
| Account Creation | `EMAIL_ACCOUNT_CREATION` | ? ?ã chuy?n ??i + Có fallback |
| Password Reset OTP | `EMAIL_PASSWORD_RESET_OTP` | ? ?ã chuy?n ??i + Có fallback |
| Notification | `EMAIL_NOTIFICATION` | ? ?ã chuy?n ??i + Có fallback |
| Payment Success | `EMAIL_PAYMENT_SUCCESS` | ? ?ã chuy?n ??i + Có fallback |
| Ticket Notifications | N/A (hardcoded) | ?? V?n dùng hardcoded HTML |
| Customer Notification | N/A (hardcoded) | ?? V?n dùng hardcoded HTML |

## ?? Notes

- **Fallback mechanism**: N?u template không t?n t?i trong database, h? th?ng s? t? ??ng fallback v? hardcoded HTML template
- **Ticket emails**: Ch?a migrate vì ph?c t?p và có nhi?u lo?i khác nhau
- **Customer notification emails**: Ch?a có template code riêng trong database migration

---
**Last Updated:** 2024-12-31  
**Status:** ?? **IN PROGRESS** (C?n fix compilation errors)  
**Next Step:** Thêm các legacy methods vào EmailService.cs
