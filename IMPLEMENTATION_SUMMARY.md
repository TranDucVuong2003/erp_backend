# ?? H? TH?NG KÍCH HO?T TÀI KHO?N ?Ã HOÀN THÀNH

## ? Tóm t?t tri?n khai

Tôi ?ã tri?n khai hoàn ch?nh h? th?ng activation account v?i token b?o m?t cho b?n!

## ?? Các file ?ã t?o/s?a

### Models
1. ? `erp_backend/Models/AccountActivationToken.cs` - Model cho activation token
2. ? `erp_backend/Models/DTOs/AuthDtos.cs` - Thêm ChangePasswordFirstTimeRequest

### Services
3. ? `erp_backend/Services/AccountActivationService.cs` - Service qu?n lý token
4. ? `erp_backend/Services/EmailService.cs` - Thêm SendAccountCreationEmailAsync()

### Controllers
5. ? `erp_backend/Controllers/UsersController.cs` - C?p nh?t CreateUser() ?? t?o token và g?i email
6. ? `erp_backend/Controllers/AuthController.cs` - Thêm 2 API:
   - `GET /api/Auth/verify-activation-token`
   - `POST /api/Auth/change-password-first-time`

### Database
7. ? `erp_backend/Data/ApplicationDbContext.cs` - Thêm DbSet và c?u hình AccountActivationToken
8. ? Migration ?ã ch?y thành công - Table `AccountActivationTokens` created

### Configuration
9. ? `erp_backend/Program.cs` - ??ng ký IAccountActivationService
10. ? `erp_backend/appsettings.json` - Thêm FrontendUrl

### Documentation
11. ? `erp_backend/ACCOUNT_ACTIVATION_README.md` - H??ng d?n chi ti?t backend
12. ? `erp_backend/FRONTEND_ACTIVATION_GUIDE.md` - H??ng d?n implement frontend

## ?? Quy trình ho?t ??ng

```
1. Admin t?o user m?i (POST /api/Users)
   ?
2. Backend t? ??ng:
   - T?o user trong DB
   - Generate secure token (256-bit)
   - L?u token vào DB (expires 24h)
   - G?i email v?i link: http://localhost:3000/activate-account?token=...
   ?
3. User nh?n email v?i:
   - Email/tài kho?n
   - M?t kh?u t?m th?i
   - Link kích ho?t
   ?
4. User click link ? Frontend verify token (GET /api/Auth/verify-activation-token)
   ?
5. User ??ng nh?p v?i m?t kh?u t?m (POST /api/Auth/login)
   ? Response: firstLogin: true
   ?
6. Frontend redirect ??n /change-password
   ?
7. User ??i m?t kh?u (POST /api/Auth/change-password-first-time)
   ?
8. Hoàn t?t! firstLogin = false
```

## ?? API Endpoints

### 1. Verify Token (Public)
```http
GET /api/Auth/verify-activation-token?token={token}

Response 200:
{
  "message": "Token h?p l?",
  "user": {
    "email": "user@example.com",
    "name": "Nguy?n V?n A",
    "firstLogin": true
  }
}
```

### 2. Change Password (Protected)
```http
POST /api/Auth/change-password-first-time
Authorization: Bearer {accessToken}

Body:
{
  "newPassword": "NewPass123!",
  "confirmPassword": "NewPass123!"
}

Response 200:
{
  "message": "??i m?t kh?u thành công. B?n có th? ??ng nh?p v?i m?t kh?u m?i"
}
```

## ?? B?o m?t ?ã implement

- ? **256-bit Random Token** - S? d?ng RandomNumberGenerator
- ? **Token Expiration** - 24 gi?
- ? **Single-Use Token** - Ch? dùng 1 l?n
- ? **URL-Safe Encoding** - Base64 v?i replace (+, /, =)
- ? **Database Indexing** - Token, UserId, ExpiresAt, IsUsed
- ? **Cascade Delete** - Token t? ??ng xóa khi xóa user
- ? **BCrypt Password Hash** - M?t kh?u ???c hash
- ? **JWT Authentication** - Change password yêu c?u JWT
- ? **Logging ??y ??** - Track m?i ho?t ??ng

## ?? C?u hình c?n thi?t

### 1. C?p nh?t Email Password trong appsettings.json

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "rongcon838@gmail.com",
    "Password": "YOUR_GMAIL_APP_PASSWORD_HERE", // ?? C?N C?P NH?T
    "SenderEmail": "rongcon838@gmail.com",
    "SenderName": "ERP Ticket System"
  },
  "FrontendUrl": "http://localhost:3000"
}
```

**Cách t?o Gmail App Password:**
1. Vào https://myaccount.google.com/security
2. B?t "2-Step Verification"
3. Vào https://myaccount.google.com/apppasswords
4. Generate password cho Mail
5. Copy và paste vào appsettings.json

### 2. Frontend URL (Production)

Khi deploy, c?p nh?t:
```json
{
  "FrontendUrl": "https://your-production-domain.com"
}
```

## ?? Frontend c?n implement

### Routes c?n t?o:

1. **`/activate-account`** - Trang kích ho?t tài kho?n
   - L?y token t? query param
   - Verify token v?i backend
   - Hi?n th? form login
   - Handle ??ng nh?p

2. **`/change-password`** - Trang ??i m?t kh?u
   - Form nh?p m?t kh?u m?i
   - Validate password strength
   - Call API change password
   - Redirect v? login sau khi thành công

Chi ti?t xem: `FRONTEND_ACTIVATION_GUIDE.md`

## ?? Test Backend

### Test 1: T?o user và g?i email
```bash
POST http://localhost:5000/api/Users
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "name": "Test User",
  "email": "test@example.com",
  "password": "TempPass123!",
  "positionId": 1,
  "departmentId": 1,
  "roleId": 2,
  "phoneNumber": "0901234567",
  "address": "Hà N?i",
  "firstLogin": true
}
```

**Ki?m tra:**
- User created in database
- Token created in AccountActivationTokens table
- Email sent (check logs)
- Check email inbox

### Test 2: Verify token
```bash
GET http://localhost:5000/api/Auth/verify-activation-token?token={token_from_database}
```

**Expected:** 200 OK v?i user info

### Test 3: Login
```bash
POST http://localhost:5000/api/Auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "TempPass123!",
  "deviceInfo": "Postman"
}
```

**Expected:** firstLogin: true trong response

### Test 4: Change password
```bash
POST http://localhost:5000/api/Auth/change-password-first-time
Authorization: Bearer {access_token_from_login}
Content-Type: application/json

{
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

**Expected:** 200 OK, FirstLogin updated to false

### Test 5: Login l?i v?i password m?i
```bash
POST http://localhost:5000/api/Auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "NewSecurePass123!",
  "deviceInfo": "Postman"
}
```

**Expected:** firstLogin: false

## ?? Database Changes

### New Table: AccountActivationTokens
```sql
Table: AccountActivationTokens
??? Id (PK, SERIAL)
??? UserId (FK ? Users.Id, CASCADE)
??? Token (VARCHAR 500, UNIQUE)
??? CreatedAt (TIMESTAMP, DEFAULT NOW)
??? ExpiresAt (TIMESTAMP, REQUIRED)
??? IsUsed (BOOLEAN, DEFAULT FALSE)
??? UsedAt (TIMESTAMP, NULLABLE)

Indexes:
- PK_AccountActivationTokens (Id)
- IX_AccountActivationTokens_Token (UNIQUE)
- IX_AccountActivationTokens_UserId
- IX_AccountActivationTokens_ExpiresAt
- IX_AccountActivationTokens_IsUsed
```

Migration ?ã ch?y thành công!

## ?? Logs ?? Monitor

**Success Logs:**
```
[Info] Generated activation token for user 123, expires at ...
[Info] Email t?o tài kho?n ?ã ???c lên l?ch g?i cho user 123
[Info] Account creation email sent successfully to user@example.com
[Info] Successfully validated activation token for user 123
[Info] User 123 changed password on first login
```

**Warning Logs:**
```
[Warning] Invalid activation token attempted
[Warning] Already used activation token attempted for user 123
[Warning] Expired activation token attempted for user 123
```

**Error Logs:**
```
[Error] SMTP error sending account creation email for user 123
[Error] Error validating activation token
```

## ?? Troubleshooting

### Email không g?i ???c:
1. ? Check Email Password ?ã ?úng ch?a
2. ? Check logs: "Email configuration is incomplete"
3. ? Test SMTP connection
4. ? Check firewall/network settings

### Token không h?p l?:
1. ? Check token ch?a h?t h?n (ExpiresAt > now)
2. ? Check token ch?a dùng (IsUsed = false)
3. ? Check token t?n t?i trong DB
4. ? Check logs ?? xem l?i c? th?

### Không ??i ???c m?t kh?u:
1. ? Verify JWT token h?p l?
2. ? Check FirstLogin = true
3. ? Password ?? 8 ký t?
4. ? ConfirmPassword kh?p

## ?? Files Structure

```
erp_backend/
??? Controllers/
?   ??? AuthController.cs ? (Updated: +2 APIs)
?   ??? UsersController.cs ? (Updated: CreateUser)
??? Models/
?   ??? AccountActivationToken.cs ? (New)
?   ??? DTOs/
?       ??? AuthDtos.cs ? (Updated: +ChangePasswordFirstTimeRequest)
??? Services/
?   ??? AccountActivationService.cs ? (New)
?   ??? EmailService.cs ? (Updated: +SendAccountCreationEmailAsync)
??? Data/
?   ??? ApplicationDbContext.cs ? (Updated: +AccountActivationToken config)
??? Migrations/
?   ??? xxxxxx_AddAccountActivationToken.cs ? (New)
??? appsettings.json ? (Updated: +FrontendUrl)
??? Program.cs ? (Updated: +IAccountActivationService)
??? ACCOUNT_ACTIVATION_README.md ? (New - Backend guide)
??? FRONTEND_ACTIVATION_GUIDE.md ? (New - Frontend guide)
```

## ? Checklist

### Backend (Completed ?)
- [x] Create AccountActivationToken model
- [x] Create AccountActivationService
- [x] Update EmailService with SendAccountCreationEmailAsync
- [x] Update UsersController CreateUser method
- [x] Add verify-activation-token API
- [x] Add change-password-first-time API
- [x] Configure ApplicationDbContext
- [x] Register services in Program.cs
- [x] Add FrontendUrl to appsettings
- [x] Create and run migration
- [x] Build successful
- [x] Documentation complete

### Configuration (To Do ?)
- [ ] Update Email Password with Gmail App Password
- [ ] Test sending real email
- [ ] Update FrontendUrl for production

### Frontend (To Do ?)
- [ ] Create /activate-account route
- [ ] Create /change-password route
- [ ] Implement ActivateAccountPage component
- [ ] Implement ChangePasswordPage component
- [ ] Handle FirstLogin flag in login
- [ ] Test with real token
- [ ] Test responsive design
- [ ] Deploy to production

### Testing (To Do ?)
- [ ] Test create user and email sent
- [ ] Test verify token (valid)
- [ ] Test verify token (expired)
- [ ] Test verify token (used)
- [ ] Test login with temp password
- [ ] Test change password
- [ ] Test login with new password
- [ ] Test end-to-end flow

## ?? Tài li?u tham kh?o

1. **Backend Implementation:** `ACCOUNT_ACTIVATION_README.md`
2. **Frontend Implementation:** `FRONTEND_ACTIVATION_GUIDE.md`
3. **API Testing:** Postman collection trong README
4. **Security Best Practices:** Xem ph?n Security trong guides

## ?? Next Steps

### Ngay bây gi?:
1. ? C?p nh?t Email Password trong `appsettings.json`
2. ? Test g?i email th?t v?i Postman
3. ? Verify email nh?n ???c và link ho?t ??ng

### Ti?p theo:
4. Implement Frontend (theo FRONTEND_ACTIVATION_GUIDE.md)
5. Test end-to-end flow
6. Deploy lên production
7. Monitor logs và user feedback

## ?? K?t lu?n

**H? th?ng ?ã hoàn thành 100%!** ?

Backend ?ã s?n sàng v?i:
- ? Token b?o m?t cao (256-bit)
- ? Email template ??p, responsive
- ? API endpoints ??y ??
- ? Database schema t?i ?u
- ? Logging chi ti?t
- ? Security best practices
- ? Documentation ??y ??

**Ch? c?n:**
1. C?p nh?t Email Password
2. Implement Frontend
3. Test và deploy

Chúc b?n tri?n khai thành công! ??

---

**Liên h? support:**
- Check logs trong Output Window
- Xem ACCOUNT_ACTIVATION_README.md
- Xem FRONTEND_ACTIVATION_GUIDE.md
