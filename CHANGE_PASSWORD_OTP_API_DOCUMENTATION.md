# API ??I M?T KH?U V?I XÁC TH?C OTP

## T?ng quan

H? th?ng ?ã ???c b? sung tính n?ng ??i m?t kh?u v?i xác th?c OTP qua email. Quy trình g?m 2 b??c:

1. **Yêu c?u OTP**: User nh?p email ? H? th?ng g?i mã OTP 6 s? v? email
2. **Xác th?c và ??i m?t kh?u**: User nh?p OTP + m?t kh?u m?i ? H? th?ng xác th?c và c?p nh?t m?t kh?u

## Các thành ph?n ?ã thêm

### 1. Models

#### `PasswordResetOtp` Model
```csharp
public class PasswordResetOtp
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string OtpCode { get; set; }        // Mã OTP 6 s?
    public DateTime ExpiresAt { get; set; }    // Th?i gian h?t h?n (5 phút)
    public bool IsUsed { get; set; }           // ?ã s? d?ng ch?a
    public DateTime? UsedAt { get; set; }      // Th?i ?i?m s? d?ng
    public DateTime CreatedAt { get; set; }
    public string? IpAddress { get; set; }     // IP address c?a ng??i yêu c?u
    public string? UserAgent { get; set; }     // User agent c?a ng??i yêu c?u
}
```

### 2. DTOs (Data Transfer Objects)

Trong file `erp_backend/Models/DTOs/AuthDtos.cs`:

#### `RequestChangePasswordOtpRequest`
```csharp
public class RequestChangePasswordOtpRequest
{
    [Required(ErrorMessage = "Email là b?t bu?c")]
    [EmailAddress(ErrorMessage = "Email không h?p l?")]
    public string Email { get; set; }
}
```

#### `RequestChangePasswordOtpResponse`
```csharp
public class RequestChangePasswordOtpResponse
{
    public string Message { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; }
}
```

#### `VerifyOtpAndChangePasswordRequest`
```csharp
public class VerifyOtpAndChangePasswordRequest
{
    [Required(ErrorMessage = "Email là b?t bu?c")]
    [EmailAddress(ErrorMessage = "Email không h?p l?")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Mã OTP là b?t bu?c")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP ph?i có 6 ch? s?")]
    public string Otp { get; set; }

    [Required(ErrorMessage = "M?t kh?u m?i là b?t bu?c")]
    [MinLength(8, ErrorMessage = "M?t kh?u ph?i có ít nh?t 8 ký t?")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Xác nh?n m?t kh?u là b?t bu?c")]
    [Compare("NewPassword", ErrorMessage = "M?t kh?u xác nh?n không kh?p")]
    public string ConfirmPassword { get; set; }
}
```

#### `ChangePasswordResponse`
```csharp
public class ChangePasswordResponse
{
    public string Message { get; set; }
    public DateTime ChangedAt { get; set; }
}
```

### 3. Services

#### `IPasswordResetOtpService` Interface
```csharp
public interface IPasswordResetOtpService
{
    Task<(bool success, string otp, DateTime expiresAt, string message)> GenerateOtpAsync(string email, string? ipAddress, string? userAgent);
    Task<(bool isValid, string message)> ValidateOtpAsync(string email, string otp);
    Task<bool> MarkOtpAsUsedAsync(string email, string otp);
    Task CleanupExpiredOtpsAsync();
}
```

**Ch?c n?ng:**
- `GenerateOtpAsync`: T?o mã OTP 6 s? ng?u nhiên, l?u vào DB
- `ValidateOtpAsync`: Xác th?c mã OTP (ki?m tra t?n t?i, ch?a s? d?ng, ch?a h?t h?n)
- `MarkOtpAsUsedAsync`: ?ánh d?u OTP ?ã ???c s? d?ng
- `CleanupExpiredOtpsAsync`: D?n d?p các OTP ?ã h?t h?n (>7 ngày)

#### Email Template
Email OTP ???c g?i v?i template HTML chuyên nghi?p, bao g?m:
- ?? Icon và tiêu ?? rõ ràng
- Mã OTP hi?n th? l?n, d? ??c
- ? Th?i gian h?t h?n (5 phút)
- ?? H??ng d?n s? d?ng
- ?? L?u ý b?o m?t

### 4. Database Migration

Migration `AddPasswordResetOtpTable` ?ã t?o b?ng `password_reset_otps` v?i các indexes:
- `IX_password_reset_otps_Email`
- `IX_password_reset_otps_Email_OtpCode`
- `IX_password_reset_otps_ExpiresAt`
- `IX_password_reset_otps_IsUsed`

---

## API Endpoints

### 1. Yêu c?u g?i OTP

**Endpoint:** `POST /api/auth/request-change-password-otp`

**Authentication:** Không yêu c?u (AllowAnonymous)

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response thành công (200 OK):**
```json
{
  "message": "Mã OTP ?ã ???c g?i ??n email c?a b?n",
  "expiresAt": "2024-01-01T10:05:00Z",
  "email": "user@example.com"
}
```

**Response l?i:**
- **400 Bad Request**: Validation l?i (email không h?p l?)
- **500 Internal Server Error**: L?i server

**L?u ý:**
- N?u email không t?n t?i, API v?n tr? v? success ?? tránh l? thông tin user
- Mã OTP có hi?u l?c 5 phút
- N?u ?ã có OTP còn hi?u l?c (>2 phút), h? th?ng s? tái s? d?ng OTP c?

**Ví d? s? d?ng v?i cURL:**
```bash
curl -X POST https://api.yourdomain.com/api/auth/request-change-password-otp \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com"
  }'
```

**Ví d? s? d?ng v?i JavaScript/Fetch:**
```javascript
const response = await fetch('/api/auth/request-change-password-otp', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'user@example.com'
  })
});

const data = await response.json();
console.log(data);
```

---

### 2. Xác th?c OTP và ??i m?t kh?u

**Endpoint:** `POST /api/auth/verify-otp-and-change-password`

**Authentication:** Không yêu c?u (AllowAnonymous)

**Request Body:**
```json
{
  "email": "user@example.com",
  "otp": "123456",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

**Validation Rules:**
- `email`: Required, ph?i là email h?p l?
- `otp`: Required, ph?i có ?úng 6 ký t?
- `newPassword`: Required, t?i thi?u 8 ký t?
- `confirmPassword`: Required, ph?i kh?p v?i `newPassword`

**Response thành công (200 OK):**
```json
{
  "message": "??i m?t kh?u thành công. Vui lòng ??ng nh?p l?i v?i m?t kh?u m?i",
  "changedAt": "2024-01-01T10:03:45Z"
}
```

**Response l?i:**
- **400 Bad Request**: 
  - Validation l?i
  - Mã OTP không chính xác
  - Mã OTP ?ã h?t h?n
  ```json
  {
    "message": "Mã OTP không chính xác"
  }
  ```
  ho?c
  ```json
  {
    "message": "Mã OTP ?ã h?t h?n. Vui lòng yêu c?u mã m?i"
  }
  ```
- **404 Not Found**: Không tìm th?y ng??i dùng
- **500 Internal Server Error**: L?i server

**Hành vi ??c bi?t:**
- Sau khi ??i m?t kh?u thành công, t?t c? session ??ng nh?p hi?n t?i c?a user s? b? h?y (revoke)
- User ph?i ??ng nh?p l?i v?i m?t kh?u m?i
- Mã OTP s? ???c ?ánh d?u ?ã s? d?ng và không th? dùng l?i

**Ví d? s? d?ng v?i cURL:**
```bash
curl -X POST https://api.yourdomain.com/api/auth/verify-otp-and-change-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "otp": "123456",
    "newPassword": "NewSecurePassword123!",
    "confirmPassword": "NewSecurePassword123!"
  }'
```

**Ví d? s? d?ng v?i JavaScript/Fetch:**
```javascript
const response = await fetch('/api/auth/verify-otp-and-change-password', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'user@example.com',
    otp: '123456',
    newPassword: 'NewSecurePassword123!',
    confirmPassword: 'NewSecurePassword123!'
  })
});

const data = await response.json();
if (response.ok) {
  console.log('M?t kh?u ?ã ???c ??i thành công!');
  // Redirect ??n trang login
  window.location.href = '/login';
} else {
  console.error('L?i:', data.message);
}
```

---

## Quy trình s? d?ng t? Frontend

### B??c 1: T?o form yêu c?u OTP

```jsx
import { useState } from 'react';

function RequestOtpForm() {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      const response = await fetch('/api/auth/request-change-password-otp', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email })
      });
      
      const data = await response.json();
      setMessage(data.message);
      
      if (response.ok) {
        // Chuy?n sang b??c 2: nh?p OTP
        // onNext() ho?c navigate('/verify-otp')
      }
    } catch (error) {
      setMessage('Có l?i x?y ra. Vui lòng th? l?i.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        placeholder="Nh?p email c?a b?n"
        required
      />
      <button type="submit" disabled={loading}>
        {loading ? '?ang g?i...' : 'G?i mã OTP'}
      </button>
      {message && <p>{message}</p>}
    </form>
  );
}
```

### B??c 2: T?o form xác th?c OTP và ??i m?t kh?u

```jsx
import { useState } from 'react';

function VerifyOtpAndChangePasswordForm({ email }) {
  const [formData, setFormData] = useState({
    otp: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (formData.newPassword !== formData.confirmPassword) {
      setMessage('M?t kh?u xác nh?n không kh?p');
      return;
    }
    
    setLoading(true);
    
    try {
      const response = await fetch('/api/auth/verify-otp-and-change-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email,
          otp: formData.otp,
          newPassword: formData.newPassword,
          confirmPassword: formData.confirmPassword
        })
      });
      
      const data = await response.json();
      setMessage(data.message);
      
      if (response.ok) {
        // Chuy?n ??n trang login sau 2 giây
        setTimeout(() => {
          window.location.href = '/login';
        }, 2000);
      }
    } catch (error) {
      setMessage('Có l?i x?y ra. Vui lòng th? l?i.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        name="otp"
        value={formData.otp}
        onChange={handleChange}
        placeholder="Nh?p mã OTP (6 s?)"
        maxLength={6}
        required
      />
      <input
        type="password"
        name="newPassword"
        value={formData.newPassword}
        onChange={handleChange}
        placeholder="M?t kh?u m?i (t?i thi?u 8 ký t?)"
        minLength={8}
        required
      />
      <input
        type="password"
        name="confirmPassword"
        value={formData.confirmPassword}
        onChange={handleChange}
        placeholder="Xác nh?n m?t kh?u m?i"
        required
      />
      <button type="submit" disabled={loading}>
        {loading ? '?ang x? lý...' : '??i m?t kh?u'}
      </button>
      {message && <p>{message}</p>}
    </form>
  );
}
```

### B??c 3: T?o flow hoàn ch?nh

```jsx
import { useState } from 'react';

function ChangePasswordFlow() {
  const [step, setStep] = useState(1); // 1: Request OTP, 2: Verify OTP
  const [email, setEmail] = useState('');

  return (
    <div className="change-password-container">
      <h2>??i m?t kh?u</h2>
      
      {step === 1 && (
        <RequestOtpForm 
          onSuccess={(userEmail) => {
            setEmail(userEmail);
            setStep(2);
          }}
        />
      )}
      
      {step === 2 && (
        <VerifyOtpAndChangePasswordForm 
          email={email}
          onBack={() => setStep(1)}
        />
      )}
    </div>
  );
}
```

---

## B?o m?t

### 1. OTP Security
- ? Mã OTP 6 s? ng?u nhiên
- ? Có th?i gian h?t h?n (5 phút)
- ? Ch? s? d?ng ???c 1 l?n
- ? L?u IP address và User Agent c?a ng??i yêu c?u

### 2. Password Security
- ? M?t kh?u ???c hash b?ng BCrypt
- ? Validation m?t kh?u t?i thi?u 8 ký t?
- ? T? ??ng revoke t?t c? session c? sau khi ??i m?t kh?u

### 3. Email Security
- ? Không ti?t l? thông tin user t?n t?i hay không
- ? Email ch?a warning v? b?o m?t
- ? Email g?i qua SMTP v?i SSL/TLS

### 4. Rate Limiting (Khuy?n ngh?)
Nên thêm rate limiting cho API `/request-change-password-otp` ?? tránh spam:
```csharp
// Ví d?: Gi?i h?n 3 l?n request OTP trong 15 phút cho m?i IP
```

---

## Testing

### Test Case 1: Request OTP thành công
```bash
POST /api/auth/request-change-password-otp
{
  "email": "existing@example.com"
}

Expected: 200 OK
Response: { "message": "Mã OTP ?ã ???c g?i ??n email c?a b?n", ... }
Email: Nh?n ???c email ch?a mã OTP
```

### Test Case 2: Request OTP v?i email không t?n t?i
```bash
POST /api/auth/request-change-password-otp
{
  "email": "notexist@example.com"
}

Expected: 200 OK (không ti?t l? user không t?n t?i)
Response: { "message": "N?u email t?n t?i trong h? th?ng, mã OTP ?ã ???c g?i", ... }
Email: Không nh?n ???c email
```

### Test Case 3: Verify OTP thành công
```bash
POST /api/auth/verify-otp-and-change-password
{
  "email": "user@example.com",
  "otp": "123456",
  "newPassword": "NewPass123!",
  "confirmPassword": "NewPass123!"
}

Expected: 200 OK
Response: { "message": "??i m?t kh?u thành công...", ... }
Database: Password ?ã ???c c?p nh?t, OTP ?ã ???c ?ánh d?u used
```

### Test Case 4: OTP không chính xác
```bash
POST /api/auth/verify-otp-and-change-password
{
  "email": "user@example.com",
  "otp": "999999",
  "newPassword": "NewPass123!",
  "confirmPassword": "NewPass123!"
}

Expected: 400 Bad Request
Response: { "message": "Mã OTP không chính xác" }
```

### Test Case 5: OTP ?ã h?t h?n
```bash
POST /api/auth/verify-otp-and-change-password
{
  "email": "user@example.com",
  "otp": "123456", // OTP c? h?n 5 phút
  "newPassword": "NewPass123!",
  "confirmPassword": "NewPass123!"
}

Expected: 400 Bad Request
Response: { "message": "Mã OTP ?ã h?t h?n. Vui lòng yêu c?u mã m?i" }
```

---

## Troubleshooting

### V?n ??: Không nh?n ???c email OTP

**Nguyên nhân có th?:**
1. Email config ch?a ?úng trong `appsettings.json`
2. Email b? vào spam folder
3. SMTP credentials không ?úng

**Gi?i pháp:**
1. Ki?m tra config trong `appsettings.json`:
```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "ERP System"
  }
}
```

2. Ki?m tra logs:
```bash
# Xem logs trong Visual Studio Output ho?c console
# Tìm dòng: "OTP email sent successfully to {Email}"
```

### V?n ??: OTP không h?p l?

**Nguyên nhân:**
- OTP ?ã h?t h?n (>5 phút)
- OTP ?ã ???c s? d?ng
- Nh?p sai mã OTP

**Gi?i pháp:**
- Request OTP m?i
- Ki?m tra email ?ã nh?p có chính xác không
- Copy/paste mã OTP t? email thay vì gõ tay

### V?n ??: L?i 500 Internal Server Error

**Ki?m tra:**
1. Database connection
2. Email service configuration
3. Server logs ?? xem chi ti?t l?i

---

## Maintenance

### Cleanup OTP c?

T?o background service ?? t? ??ng d?n d?p OTP c?:

```csharp
// Trong Startup.cs ho?c Program.cs
builder.Services.AddHostedService<OtpCleanupService>();
```

```csharp
// OtpCleanupService.cs
public class OtpCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OtpCleanupService> _logger;

    public OtpCleanupService(IServiceProvider serviceProvider, ILogger<OtpCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var otpService = scope.ServiceProvider.GetRequiredService<IPasswordResetOtpService>();
                
                await otpService.CleanupExpiredOtpsAsync();
                _logger.LogInformation("OTP cleanup completed");
                
                // Ch?y m?i 24 gi?
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OTP cleanup service");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
```

---

## Changelog

### Version 1.0.0 - 2024-01-01
- ? Thêm model `PasswordResetOtp`
- ? Thêm service `PasswordResetOtpService`
- ? Thêm 2 API endpoints: request OTP và verify OTP
- ? Thêm email template cho OTP
- ? T? ??ng revoke sessions sau khi ??i m?t kh?u
- ? Migration database cho b?ng `password_reset_otps`

---

## License

Copyright © 2024 ERP System. All rights reserved.
