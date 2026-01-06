# H??ng d?n kích ho?t tài kho?n ng??i dùng

## ? T?ng quan
H? th?ng ?ã ???c c?p nh?t ?? t? ??ng g?i email thông báo khi t?o tài kho?n m?i cho ng??i dùng v?i:
- ? **Activation Token b?o m?t** (256-bit, 24h expiry, single-use)
- ? **Email ch?a**: Tài kho?n, m?t kh?u t?m th?i, link kích ho?t
- ? **API endpoints**: Verify token và change password
- ? **Database**: Table AccountActivationTokens ?ã ???c migrate

## ?? Các thành ph?n ?ã tri?n khai

### 1. Backend Components

#### Models
- ? `AccountActivationToken.cs` - Model cho token kích ho?t
- ? `ChangePasswordFirstTimeRequest.cs` - DTO cho ??i m?t kh?u

#### Services
- ? `AccountActivationService.cs` - Generate và validate token
- ? `EmailService.SendAccountCreationEmailAsync()` - G?i email v?i template HTML ??p

#### Controllers & APIs
- ? `UsersController.CreateUser()` - T?o user và g?i email v?i token
- ? `AuthController.VerifyActivationToken()` - API verify token
- ? `AuthController.ChangePasswordFirstTime()` - API ??i m?t kh?u

#### Database
- ? Migration `AddAccountActivationToken` ?ã apply thành công
- ? Table `AccountActivationTokens` v?i indexes t?i ?u

### 2. Configuration

File `appsettings.json` ?ã ???c c?u hình:
```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "rongcon838@gmail.com",
    "Password": "", // ?? C?N C?P NH?T
    "SenderEmail": "rongcon838@gmail.com",
    "SenderName": "ERP Ticket System"
  },
  "FrontendUrl": "http://localhost:3000"
}
```

**?? L?U Ý:** B?n c?n c?p nh?t Email Password v?i App Password t? Gmail.

## ?? Quy trình ho?t ??ng

### B??c 1: Admin t?o user m?i
```http
POST /api/Users
Authorization: Bearer {admin_token}

{
  "name": "Nguy?n V?n A",
  "email": "user@example.com",
  "password": "TempPass123!",
  "positionId": 1,
  "departmentId": 1,
  "roleId": 2,
  "firstLogin": true
}
```

### B??c 2: Backend t? ??ng x? lý
1. ? T?o user trong database
2. ? Hash m?t kh?u
3. ? T?o ActiveAccount v?i FirstLogin = true
4. ? Generate activation token (256-bit, expires 24h)
5. ? L?u token vào database
6. ? G?i email v?i token link

### B??c 3: User nh?n email
Email ch?a:
- ?? Email/Tài kho?n
- ?? M?t kh?u t?m th?i
- ?? Thông tin cá nhân (tên, phòng ban, ch?c v?)
- ?? Link kích ho?t: `http://localhost:3000/activate-account?token={secure_token}`
- ?? H??ng d?n 4 b??c

### B??c 4: User click link và verify token
Frontend g?i API:
```http
GET /api/Auth/verify-activation-token?token={token}
```

Response n?u thành công:
```json
{
  "message": "Token h?p l?",
  "user": {
    "email": "user@example.com",
    "name": "Nguy?n V?n A",
    "firstLogin": true
  }
}
```

### B??c 5: User ??ng nh?p v?i m?t kh?u t?m th?i
```http
POST /api/Auth/login

{
  "email": "user@example.com",
  "password": "TempPass123!",
  "deviceInfo": "Chrome on Windows"
}
```

Response:
```json
{
  "accessToken": "eyJhbGc...",
  "firstLogin": true,
  "message": "B?n ph?i ??i m?t kh?u tr??c khi ??ng nh?p vào h? th?ng",
  "user": { ... }
}
```

### B??c 6: User ??i m?t kh?u
```http
POST /api/Auth/change-password-first-time
Authorization: Bearer {accessToken}

{
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

Response:
```json
{
  "message": "??i m?t kh?u thành công. B?n có th? ??ng nh?p v?i m?t kh?u m?i"
}
```

### B??c 7: Hoàn t?t
- ? Token ???c ?ánh d?u ?ã s? d?ng
- ? FirstLogin = false
- ? User có th? ??ng nh?p bình th??ng

## ?? API Endpoints

### 1. Verify Activation Token
- **Method:** GET
- **URL:** `/api/Auth/verify-activation-token?token={token}`
- **Auth:** None (AllowAnonymous)
- **Response:** User info n?u token h?p l?

### 2. Change Password First Time
- **Method:** POST
- **URL:** `/api/Auth/change-password-first-time`
- **Auth:** Bearer Token (Required)
- **Body:**
  ```json
  {
    "newPassword": "string (min 8 chars)",
    "confirmPassword": "string (must match)"
  }
  ```

## ?? Frontend Implementation

Chi ti?t xem file: `FRONTEND_ACTIVATION_GUIDE.md`

Tóm t?t c?n implement:
1. **Route `/activate-account`** - Verify token và hi?n th? form login
2. **Route `/change-password`** - Form ??i m?t kh?u
3. **Handle FirstLogin flag** - Redirect n?u c?n ??i m?t kh?u
4. **API integration** - 3 endpoints: verify, login, change-password

## ?? B?o m?t

### Token Security
- ? **256-bit random token** - S? d?ng RandomNumberGenerator
- ? **URL-safe encoding** - Base64 v?i replace (+, /, =)
- ? **24h expiration** - T? ??ng h?t h?n sau 24 gi?
- ? **Single-use** - Ch? s? d?ng 1 l?n, ?ánh d?u IsUsed
- ? **Database storage** - Token stored securely with indexes
- ? **Cascade delete** - Token t? ??ng xóa khi xóa user

### Password Security
- ? **BCrypt hashing** - Password ???c hash tr??c khi l?u
- ? **Minimum 8 characters** - Validation ? c? backend và frontend
- ? **Password confirmation** - ??m b?o user nh?p ?úng
- ? **First login enforcement** - B?t bu?c ??i m?t kh?u

### API Security
- ? **JWT Authentication** - Change password yêu c?u JWT
- ? **CORS configured** - Ch? cho phép frontend origins
- ? **Rate limiting** - Nên implement ?? tránh brute force
- ? **Logging** - Ghi log ??y ?? các ho?t ??ng

## ?? Testing

### Test Cases Backend

1. **Test t?o user và g?i email:**
```bash
POST /api/Users
# Ki?m tra:
- User ???c t?o thành công
- Token ???c t?o trong database
- Email ???c g?i (check logs)
```

2. **Test verify token h?p l?:**
```bash
GET /api/Auth/verify-activation-token?token={valid_token}
# Expected: 200 OK v?i user info
```

3. **Test verify token h?t h?n:**
```bash
# ??i >24h ho?c update ExpiresAt trong DB
GET /api/Auth/verify-activation-token?token={expired_token}
# Expected: 400 Bad Request "Link kích ho?t ?ã h?t h?n"
```

4. **Test verify token ?ã dùng:**
```bash
# Dùng token 2 l?n
GET /api/Auth/verify-activation-token?token={used_token}
# Expected: 400 Bad Request "Link kích ho?t ?ã ???c s? d?ng"
```

5. **Test login v?i FirstLogin = true:**
```bash
POST /api/Auth/login
# Expected: FirstLogin: true trong response
```

6. **Test change password:**
```bash
POST /api/Auth/change-password-first-time
# Expected: Password updated, FirstLogin = false
```

### Test v?i Postman

Collection export:
```json
{
  "info": { "name": "Account Activation Tests" },
  "item": [
    {
      "name": "Create User",
      "request": {
        "method": "POST",
        "url": "{{baseUrl}}/api/Users",
        "header": [{"key": "Authorization", "value": "Bearer {{adminToken}}"}],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"name\": \"Test User\",\n  \"email\": \"test@example.com\",\n  \"password\": \"TempPass123!\",\n  \"positionId\": 1,\n  \"departmentId\": 1,\n  \"roleId\": 2,\n  \"firstLogin\": true\n}"
        }
      }
    },
    {
      "name": "Verify Token",
      "request": {
        "method": "GET",
        "url": "{{baseUrl}}/api/Auth/verify-activation-token?token={{activationToken}}"
      }
    },
    {
      "name": "Login",
      "request": {
        "method": "POST",
        "url": "{{baseUrl}}/api/Auth/login",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"email\": \"test@example.com\",\n  \"password\": \"TempPass123!\"\n}"
        }
      }
    },
    {
      "name": "Change Password",
      "request": {
        "method": "POST",
        "url": "{{baseUrl}}/api/Auth/change-password-first-time",
        "header": [{"key": "Authorization", "value": "Bearer {{accessToken}}"}],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"newPassword\": \"NewPass123!\",\n  \"confirmPassword\": \"NewPass123!\"\n}"
        }
      }
    }
  ]
}
```

## ?? Database Schema

### Table: AccountActivationTokens
```sql
CREATE TABLE "AccountActivationTokens" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "Token" VARCHAR(500) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "ExpiresAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "IsUsed" BOOLEAN NOT NULL DEFAULT FALSE,
    "UsedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_AccountActivationTokens_Users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_AccountActivationTokens_Token" 
    ON "AccountActivationTokens" ("Token");
    
CREATE INDEX "IX_AccountActivationTokens_UserId" 
    ON "AccountActivationTokens" ("UserId");
    
CREATE INDEX "IX_AccountActivationTokens_ExpiresAt" 
    ON "AccountActivationTokens" ("ExpiresAt");
    
CREATE INDEX "IX_AccountActivationTokens_IsUsed" 
    ON "AccountActivationTokens" ("IsUsed");
```

## ?? Logs và Monitoring

### Important Log Messages

**Success:**
```
[Information] Generated activation token for user 123, expires at 2024-01-02T00:00:00Z
[Information] Email t?o tài kho?n ?ã ???c lên l?ch g?i cho user 123
[Information] Account creation email sent successfully to user@example.com for user 123
[Information] Successfully validated activation token for user 123
[Information] User 123 changed password on first login
```

**Warnings:**
```
[Warning] Invalid activation token attempted: abc123xyz
[Warning] Already used activation token attempted for user 123
[Warning] Expired activation token attempted for user 123
[Warning] Email configuration is incomplete. Skipping account creation email for user 123
```

**Errors:**
```
[Error] SMTP error sending account creation email for user 123: ...
[Error] Error validating activation token
[Error] Error changing password on first login
```

## ?? C?u hình Gmail SMTP

### B??c 1: B?t 2-Step Verification
1. Vào https://myaccount.google.com/security
2. B?t "2-Step Verification"

### B??c 2: T?o App Password
1. Vào https://myaccount.google.com/apppasswords
2. Ch?n "Mail" và "Windows Computer"
3. Click "Generate"
4. Copy password (16 ký t?)

### B??c 3: C?p nh?t appsettings.json
```json
{
  "Email": {
    "Username": "rongcon838@gmail.com",
    "Password": "abcd efgh ijkl mnop"  // App Password 16 chars
  }
}
```

## ?? Deployment Checklist

### Backend
- [ ] C?p nh?t Email Password trong appsettings.Production.json
- [ ] C?p nh?t FrontendUrl cho production
- [ ] Run migration trên production database
- [ ] Test g?i email th?t
- [ ] Configure CORS cho production domain
- [ ] Setup rate limiting
- [ ] Monitor logs

### Frontend
- [ ] Implement /activate-account route
- [ ] Implement /change-password route
- [ ] Configure API URL
- [ ] Test v?i token th?t
- [ ] Test responsive mobile
- [ ] Deploy và test end-to-end

## ?? Troubleshooting

### Email không ???c g?i
1. Ki?m tra logs: "Email configuration is incomplete"
2. Verify Email Password ?ã ?úng
3. Test SMTP connection
4. Ki?m tra firewall/network

### Token không h?p l?
1. Ki?m tra token ch?a h?t h?n (ExpiresAt)
2. Ki?m tra token ch?a ???c dùng (IsUsed)
3. Ki?m tra database có token không
4. Check logs ?? xem l?i chi ti?t

### Không th? ??i m?t kh?u
1. Verify JWT token h?p l?
2. Ki?m tra FirstLogin = true
3. Validate password requirements
4. Check user exists và active

## ?? Support & References

- **Backend Code:** `erp_backend/`
- **Frontend Guide:** `FRONTEND_ACTIVATION_GUIDE.md`
- **Email Service:** `erp_backend/Services/EmailService.cs`
- **Activation Service:** `erp_backend/Services/AccountActivationService.cs`
- **Auth Controller:** `erp_backend/Controllers/AuthController.cs`

## ?? K?t lu?n

H? th?ng activation account ?ã hoàn thành v?i:
- ? Token b?o m?t cao
- ? Email template ??p
- ? API endpoints ??y ??
- ? Database schema t?i ?u
- ? Logging chi ti?t
- ? Tài li?u ??y ??

**Next Steps:**
1. C?p nh?t Email Password
2. Test g?i email th?t
3. Implement frontend
4. Test end-to-end
5. Deploy lên production

Chúc b?n tri?n khai thành công! ??
