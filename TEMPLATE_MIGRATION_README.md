# Document Templates Migration Guide

## ?? T?ng Quan

H? th?ng Template Management cho phép qu?n lý t?t c? các lo?i template (h?p ??ng, báo giá, báo cáo, email) trong database thay vì hardcode trong code.

## ?? Các Template ???c Migrate

### 1. Document Templates

| Template Code | Template Name | Type | Description |
|--------------|---------------|------|-------------|
| `QUOTE_DEFAULT` | Báo Giá D?ch V? | quote | Template báo giá d?ch v? m?c ??nh |
| `SALARY_REPORT_DEFAULT` | Báo Cáo Th?ng Kê L??ng | salary_report | Template báo cáo l??ng tháng |
| `CONTRACT_INDIVIDUAL` | H?p ??ng Cá Nhân | contract | Template h?p ??ng cho khách hàng cá nhân |
| `CONTRACT_BUSINESS` | H?p ??ng Doanh Nghi?p | contract | Template h?p ??ng cho doanh nghi?p |

### 2. Email Templates

| Template Code | Template Name | Type | Description |
|--------------|---------------|------|-------------|
| `EMAIL_ACCOUNT_CREATION` | Email T?o Tài Kho?n | email | Email g?i khi t?o tài kho?n m?i |
| `EMAIL_PASSWORD_RESET_OTP` | Email OTP ??i M?t Kh?u | email | Email g?i mã OTP ??i m?t kh?u |
| `EMAIL_NOTIFICATION` | Email Thông Báo Chung | email | Email thông báo chung cho user |
| `EMAIL_PAYMENT_SUCCESS` | Email Xác Nh?n Thanh Toán | email | Email xác nh?n thanh toán thành công |

## ?? Cách Ch?y Migration

### B??c 1: ??m B?o Template Files T?n T?i

Các file template ph?i có trong th? m?c `wwwroot/Templates/`:

```
erp_backend/wwwroot/Templates/
??? QuoteTemplate.html
??? SalaryReportTemplate.html
??? generate_contract_individual.html
??? generate_contract_business.html
??? Email_AccountCreation.html
??? Email_PasswordResetOTP.html
??? Email_Notification.html
??? Email_PaymentSuccess.html
```

### B??c 2: G?i API Migration

**Endpoint:** `POST /api/DocumentTemplates/migrate-from-files`

**Yêu c?u:**
- Ph?i là **Admin** (role = "admin" ho?c "Admin")
- Có JWT token h?p l?

**Ví d? Request (Postman):**

```http
POST https://localhost:7210/api/DocumentTemplates/migrate-from-files
Authorization: Bearer {your_admin_jwt_token}
Content-Type: application/json
```

**Response thành công:**

```json
{
  "success": true,
  "message": "?ã migrate t?t c? templates vào database thành công"
}
```

### B??c 3: Ki?m Tra K?t Qu?

**Xem t?t c? templates:**
```http
GET /api/DocumentTemplates
Authorization: Bearer {your_token}
```

**Xem templates theo lo?i:**
```http
GET /api/DocumentTemplates?type=email
GET /api/DocumentTemplates?type=contract
GET /api/DocumentTemplates?type=quote
```

**Xem template theo code:**
```http
GET /api/DocumentTemplates/by-code/EMAIL_ACCOUNT_CREATION
```

## ?? Placeholders Cho M?i Template

### Email Account Creation
```
{{UserName}}
{{UserEmail}}
{{PlainPassword}}
{{DepartmentName}}
{{PositionName}}
{{ActivationLink}}
{{CurrentYear}}
```

### Email Password Reset OTP
```
{{UserName}}
{{OtpCode}}
{{ExpiryMinutes}}
{{ExpiresAt}}
{{CurrentYear}}
```

### Email Notification
```
{{RecipientName}}
{{NotificationTitle}}
{{NotificationContent}}
{{CreatedAt}}
{{NotificationUrl}}
{{CurrentYear}}
```

### Email Payment Success
```
{{Greeting}}
{{MainMessage}}
{{ContractNumber}}
{{Amount}}
{{PaymentType}}
{{TransactionId}}
{{TransactionDate}}
{{CustomerInfo}}
{{SaleInfo}}
{{ContractUrl}}
{{CurrentYear}}
```

## ?? Qu?n Lý Templates

### T?o Template M?i
```http
POST /api/DocumentTemplates
Authorization: Bearer {admin_token}

{
  "name": "Template M?i",
  "templateType": "email",
  "code": "EMAIL_CUSTOM",
  "htmlContent": "<html>...</html>",
  "description": "Mô t? template",
  "availablePlaceholders": "[\"{{Placeholder1}}\", \"{{Placeholder2}}\"]",
  "isActive": true,
  "isDefault": false
}
```

### C?p Nh?t Template
```http
PUT /api/DocumentTemplates/{id}
Authorization: Bearer {admin_token}

{
  "id": 1,
  "name": "Template ?ã S?a",
  "templateType": "email",
  "code": "EMAIL_CUSTOM",
  "htmlContent": "<html>Updated...</html>",
  "isActive": true,
  "isDefault": false
}
```

### Xóa Template (Soft Delete)
```http
DELETE /api/DocumentTemplates/{id}
Authorization: Bearer {admin_token}
```

### ??t Template Làm M?c ??nh
```http
PATCH /api/DocumentTemplates/{id}/set-default
Authorization: Bearer {admin_token}
```

## ?? L?u Ý Quan Tr?ng

1. **Quy?n Truy C?p:**
   - Ch? Admin m?i có quy?n t?o, s?a, xóa templates
   - User th??ng ch? có quy?n xem

2. **Migration:**
   - Script ki?m tra template ?ã t?n t?i tr??c khi migrate
   - N?u template ?ã có, s? b? qua và log warning
   - Có th? ch?y l?i nhi?u l?n mà không lo duplicate

3. **Template Code:**
   - M?i template có code duy nh?t (unique)
   - Không ???c trùng code khi t?o m?i

4. **Version Control:**
   - M?i l?n c?p nh?t template, version s? t? ??ng t?ng
   - Giúp theo dõi l?ch s? thay ??i

5. **IsDefault:**
   - Ch? có 1 template m?c ??nh cho m?i lo?i (templateType)
   - Khi ??t template m?i làm default, template c? s? t? ??ng b? default

## ?? Troubleshooting

### L?i 403 Forbidden
- Ki?m tra role c?a user trong JWT token
- ??m b?o role là "admin" ho?c "Admin"
- Verify token ch?a h?t h?n

### L?i Không Tìm Th?y File
- Ki?m tra file template t?n t?i trong `wwwroot/Templates/`
- Ki?m tra tên file chính xác (có phân bi?t ch? hoa/th??ng)

### Template Không Hi?n Th?
- Ki?m tra `IsActive = true`
- Ki?m tra filter theo `templateType`

## ?? Tài Li?u API ??y ??

Xem chi ti?t t?t c? API endpoints t?i Swagger:
```
https://localhost:7210/swagger
```

## ?? Next Steps

1. ? ?ã migrate t?t c? templates vào database
2. ? C?p nh?t EmailService ?? l?y template t? database thay vì hardcode
3. ? T?o UI qu?n lý templates cho Admin
4. ? Thêm tính n?ng preview template
5. ? Thêm version history cho templates

---
**Phiên b?n:** 1.0  
**Ngày c?p nh?t:** 2024-01-11  
**Ng??i t?o:** Development Team
