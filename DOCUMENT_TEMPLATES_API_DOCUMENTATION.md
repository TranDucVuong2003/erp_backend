# Document Templates API Documentation

## ?? T?ng Quan

API qu?n lý Document Templates cho phép CRUD và qu?n lý các HTML templates dùng ?? generate documents nh? h?p ??ng, báo giá, email, báo cáo l??ng.

**Base URL:** `/api/DocumentTemplates`

**Authentication:** Bearer Token (JWT)

**Authorization:** M?t s? endpoints yêu c?u role Admin

---

## ?? Model: DocumentTemplate

### Properties

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | `int` | Auto | Primary key |
| `Name` | `string(100)` | ? | Tên template (VD: "Contract Template") |
| `TemplateType` | `string(50)` | ? | Lo?i template: "contract", "quote", "email", "salary_report" |
| `Code` | `string(50)` | ? | Mã ??nh danh duy nh?t (VD: "CONTRACT_DEFAULT") |
| `HtmlContent` | `string` | ? | N?i dung HTML template |
| `Description` | `string(500)` | ? | Mô t? template |
| `AvailablePlaceholders` | `string` | ? | JSON array các placeholder có s?n |
| `Version` | `int` | Auto | Version c?a template (t? ??ng t?ng) |
| `IsActive` | `bool` | Default: true | Template có active không |
| `IsDefault` | `bool` | Default: false | Template m?c ??nh cho lo?i này |
| `CreatedByUserId` | `int?` | Auto | ID user t?o template |
| `CreatedAt` | `DateTime` | Auto | Th?i gian t?o (UTC) |
| `UpdatedAt` | `DateTime?` | Auto | Th?i gian c?p nh?t cu?i (UTC) |

### Navigation Properties

```json
{
  "CreatedByUser": {
    "Id": 1,
    "Username": "admin",
    "Email": "admin@example.com"
  }
}
```

---

## ?? Authorization

### Roles

- **All authenticated users:** Có th? ??c templates (GET)
- **Admin only:** T?o, s?a, xóa templates (POST, PUT, DELETE)

### Headers

```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

---

## ?? API Endpoints

### 1. L?y Danh Sách Templates

**GET** `/api/DocumentTemplates`

L?y t?t c? templates ?ang active, có th? filter theo type.

#### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `type` | `string` | ? | Filter theo TemplateType (VD: "contract", "quote") |

#### Request Example

```http
GET /api/DocumentTemplates?type=contract
Authorization: Bearer {token}
```

#### Response Success (200 OK)

```json
{
  "success": true,
  "totalCount": 2,
  "data": [
    {
      "id": 1,
      "name": "Contract Individual Template",
      "templateType": "contract",
      "code": "CONTRACT_INDIVIDUAL",
      "htmlContent": "<html>...</html>",
      "description": "Template cho h?p ??ng khách hàng cá nhân",
      "availablePlaceholders": "[\"{{CustomerName}}\",\"{{ContractNumber}}\"]",
      "version": 1,
      "isActive": true,
      "isDefault": true,
      "createdByUserId": 1,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null,
      "createdByUser": {
        "id": 1,
        "username": "admin",
        "email": "admin@example.com"
      }
    }
  ]
}
```

#### Response Error (500)

```json
{
  "success": false,
  "message": "L?i server khi l?y danh sách templates",
  "error": "Error message"
}
```

---

### 2. L?y Template Theo ID

**GET** `/api/DocumentTemplates/{id}`

L?y chi ti?t 1 template theo ID.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | `int` | ? | ID c?a template |

#### Request Example

```http
GET /api/DocumentTemplates/5
Authorization: Bearer {token}
```

#### Response Success (200 OK)

```json
{
  "success": true,
  "data": {
    "id": 5,
    "name": "Quote Default Template",
    "templateType": "quote",
    "code": "QUOTE_DEFAULT",
    "htmlContent": "<html>...</html>",
    "description": "Template m?c ??nh cho báo giá",
    "availablePlaceholders": "[\"{{CustomerName}}\",\"{{TotalAmount}}\"]",
    "version": 3,
    "isActive": true,
    "isDefault": true,
    "createdByUserId": 1,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-15T00:00:00Z",
    "createdByUser": {
      "id": 1,
      "username": "admin"
    }
  }
}
```

#### Response Error (404)

```json
{
  "message": "Không tìm th?y template"
}
```

---

### 3. L?y Template Theo Code

**GET** `/api/DocumentTemplates/by-code/{code}`

L?y template theo mã code duy nh?t.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `code` | `string` | ? | Code c?a template (VD: "CONTRACT_DEFAULT") |

#### Request Example

```http
GET /api/DocumentTemplates/by-code/CONTRACT_DEFAULT
Authorization: Bearer {token}
```

#### Response Success (200 OK)

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Contract Default",
    "templateType": "contract",
    "code": "CONTRACT_DEFAULT",
    "htmlContent": "<html>...</html>",
    // ...other fields
  }
}
```

#### Response Error (404)

```json
{
  "message": "Template v?i code 'INVALID_CODE' không t?n t?i"
}
```

---

### 4. L?y Template M?c ??nh Theo Lo?i

**GET** `/api/DocumentTemplates/default/{templateType}`

L?y template m?c ??nh (IsDefault=true) cho m?t lo?i c? th?.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `templateType` | `string` | ? | Lo?i template: "contract", "quote", "email", "salary_report" |

#### Request Example

```http
GET /api/DocumentTemplates/default/contract
Authorization: Bearer {token}
```

#### Response Success (200 OK)

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Contract Individual Template",
    "templateType": "contract",
    "code": "CONTRACT_INDIVIDUAL",
    "isDefault": true,
    // ...other fields
  }
}
```

#### Response Error (404)

```json
{
  "message": "Không tìm th?y template m?c ??nh cho lo?i 'invalid_type'"
}
```

---

### 5. L?y Danh Sách Lo?i Templates

**GET** `/api/DocumentTemplates/types`

L?y danh sách các lo?i template có trong h? th?ng và th?ng kê.

#### Request Example

```http
GET /api/DocumentTemplates/types
Authorization: Bearer {token}
```

#### Response Success (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "type": "contract",
      "count": 2,
      "hasDefault": true
    },
    {
      "type": "quote",
      "count": 1,
      "hasDefault": true
    },
    {
      "type": "email",
      "count": 4,
      "hasDefault": false
    },
    {
      "type": "salary_report",
      "count": 1,
      "hasDefault": true
    }
  ]
}
```

---

### 6. T?o Template M?i

**POST** `/api/DocumentTemplates`

**?? Requires: Admin Role**

T?o template m?i trong h? th?ng.

#### Request Body

```json
{
  "name": "New Contract Template",
  "templateType": "contract",
  "code": "CONTRACT_NEW",
  "htmlContent": "<html><body>{{CustomerName}}</body></html>",
  "description": "Mô t? template",
  "availablePlaceholders": "[\"{{CustomerName}}\",\"{{ContractNumber}}\"]",
  "isActive": true,
  "isDefault": false
}
```

#### Required Fields

- `name` (string, max 100)
- `templateType` (string, max 50)
- `code` (string, max 50, unique)
- `htmlContent` (string)

#### Request Example

```http
POST /api/DocumentTemplates
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "name": "New Quote Template",
  "templateType": "quote",
  "code": "QUOTE_CUSTOM",
  "htmlContent": "<html>...</html>",
  "description": "Custom quote template",
  "isActive": true,
  "isDefault": false
}
```

#### Response Success (201 Created)

```json
{
  "success": true,
  "message": "T?o template thành công",
  "data": {
    "id": 10,
    "name": "New Quote Template",
    "templateType": "quote",
    "code": "QUOTE_CUSTOM",
    "version": 1,
    "createdAt": "2024-01-20T10:00:00Z",
    // ...other fields
  }
}
```

#### Response Error (400 Bad Request)

```json
{
  "message": "Code 'QUOTE_CUSTOM' ?ã t?n t?i"
}
```

#### Notes

- `CreatedByUserId` t? ??ng l?y t? JWT token
- `CreatedAt` t? ??ng set = UTC now
- `Version` t? ??ng = 1
- N?u `IsDefault = true`, các templates cùng type khác s? b? b? IsDefault

---

### 7. C?p Nh?t Template

**PUT** `/api/DocumentTemplates/{id}`

**?? Requires: Admin Role**

C?p nh?t thông tin template ?ã t?n t?i.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | `int` | ? | ID c?a template c?n update |

#### Request Body

```json
{
  "id": 10,
  "name": "Updated Template Name",
  "templateType": "quote",
  "code": "QUOTE_CUSTOM",
  "htmlContent": "<html>Updated content...</html>",
  "description": "Updated description",
  "availablePlaceholders": "[\"{{CustomerName}}\",\"{{Amount}}\"]",
  "isActive": true,
  "isDefault": true
}
```

#### Request Example

```http
PUT /api/DocumentTemplates/10
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "id": 10,
  "name": "Updated Quote Template",
  "templateType": "quote",
  "code": "QUOTE_CUSTOM",
  "htmlContent": "<html>...</html>",
  "isActive": true,
  "isDefault": false
}
```

#### Response Success (200 OK)

```json
{
  "success": true,
  "message": "C?p nh?t template thành công",
  "data": {
    "id": 10,
    "name": "Updated Quote Template",
    "version": 2,
    "updatedAt": "2024-01-20T12:00:00Z",
    // ...other fields
  }
}
```

#### Response Error (400)

```json
{
  "message": "ID không kh?p"
}
```

```json
{
  "message": "Code 'QUOTE_CUSTOM' ?ã ???c s? d?ng b?i template khác"
}
```

#### Response Error (404)

```json
{
  "message": "Không tìm th?y template"
}
```

#### Notes

- `Version` t? ??ng t?ng lên 1
- `UpdatedAt` t? ??ng set = UTC now
- N?u set `IsDefault = true`, templates cùng type khác s? b? b? IsDefault
- `CreatedByUserId` và `CreatedAt` không thay ??i

---

### 8. Xóa Template (Soft Delete)

**DELETE** `/api/DocumentTemplates/{id}`

**?? Requires: Admin Role**

Xóa m?m template (set IsActive = false).

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | `int` | ? | ID c?a template c?n xóa |

#### Request Example

```http
DELETE /api/DocumentTemplates/10
Authorization: Bearer {admin_token}
```

#### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Xóa template thành công"
}
```

#### Response Error (404)

```json
{
  "message": "Không tìm th?y template"
}
```

#### Notes

- Không xóa v?t lý kh?i database
- Ch? set `IsActive = false`
- `UpdatedAt` ???c c?p nh?t
- Template v?n t?n t?i trong database nh?ng không hi?n th?

---

### 9. ??t Template Làm M?c ??nh

**PATCH** `/api/DocumentTemplates/{id}/set-default`

**?? Requires: Admin Role**

??t template thành m?c ??nh cho lo?i c?a nó.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | `int` | ? | ID c?a template |

#### Request Example

```http
PATCH /api/DocumentTemplates/5/set-default
Authorization: Bearer {admin_token}
```

#### Response Success (200 OK)

```json
{
  "success": true,
  "message": "?ã ??t 'Quote Default Template' làm template m?c ??nh cho lo?i quote"
}
```

#### Response Error (404)

```json
{
  "message": "Không tìm th?y template"
}
```

#### Notes

- T? ??ng b? `IsDefault` c?a templates cùng lo?i khác
- Ch? có 1 template m?c ??nh cho m?i `TemplateType`

---

### 10. Migrate Templates T? Files

**POST** `/api/DocumentTemplates/migrate-from-files`

**?? Requires: Admin Role**

Migrate t?t c? HTML templates t? `wwwroot/Templates/` vào database.

#### Request Example

```http
POST /api/DocumentTemplates/migrate-from-files
Authorization: Bearer {admin_token}
```

#### Response Success (200 OK)

```json
{
  "success": true,
  "message": "?ã migrate t?t c? templates vào database thành công"
}
```

#### Response Error (500)

```json
{
  "success": false,
  "message": "L?i khi migrate templates",
  "error": "Error details"
}
```

#### Notes

- Ch? ch?y 1 l?n khi migration t? file-based sang database
- Migrate các templates:
  - QuoteTemplate.html ? QUOTE_DEFAULT
  - SalaryReportTemplate.html ? SALARY_REPORT_DEFAULT
  - generate_contract_individual.html ? CONTRACT_INDIVIDUAL
  - generate_contract_business.html ? CONTRACT_BUSINESS
  - Email_*.html ? EMAIL_* templates

---

## ?? Template Types Reference

### Các Lo?i Template H? Tr?

| TemplateType | Code Examples | Description |
|--------------|---------------|-------------|
| `contract` | `CONTRACT_INDIVIDUAL`<br>`CONTRACT_BUSINESS` | Templates cho h?p ??ng |
| `quote` | `QUOTE_DEFAULT` | Templates cho báo giá |
| `salary_report` | `SALARY_REPORT_DEFAULT` | Templates cho báo cáo l??ng |
| `email` | `EMAIL_ACCOUNT_CREATION`<br>`EMAIL_PASSWORD_RESET_OTP`<br>`EMAIL_NOTIFICATION`<br>`EMAIL_PAYMENT_SUCCESS` | Templates cho email |

### Template Code Conventions

- **Format:** `{TYPE}_{VARIANT}`
- **Examples:**
  - `QUOTE_DEFAULT` - Báo giá m?c ??nh
  - `CONTRACT_INDIVIDUAL` - H?p ??ng cá nhân
  - `CONTRACT_BUSINESS` - H?p ??ng doanh nghi?p
  - `EMAIL_ACCOUNT_CREATION` - Email t?o tài kho?n

---

## ?? Placeholders System

### Available Placeholders

Templates s? d?ng placeholders theo format `{{PlaceholderName}}`.

#### Contract Placeholders
```json
[
  "{{CustomerName}}",
  "{{CustomerAddress}}",
  "{{CustomerPhone}}",
  "{{CustomerEmail}}",
  "{{ContractNumber}}",
  "{{ContractDate}}",
  "{{TotalAmount}}",
  "{{ServiceName}}",
  "{{StartDate}}",
  "{{EndDate}}"
]
```

#### Quote Placeholders
```json
[
  "{{CustomerName}}",
  "{{QuoteNumber}}",
  "{{QuoteDate}}",
  "{{ValidUntil}}",
  "{{TotalAmount}}",
  "{{ItemsTable}}",
  "{{Notes}}"
]
```

#### Salary Report Placeholders
```json
[
  "{{Month}}",
  "{{Year}}",
  "{{DepartmentName}}",
  "{{EmployeesTable}}",
  "{{TotalSalary}}",
  "{{CreatedDate}}",
  "{{CreatedBy}}"
]
```

#### Email Placeholders
```json
[
  "{{Username}}",
  "{{Email}}",
  "{{PasswordResetLink}}",
  "{{OTPCode}}",
  "{{NotificationMessage}}",
  "{{PaymentAmount}}",
  "{{TransactionId}}"
]
```

---

## ?? Testing Guide

### 1. Test L?y Danh Sách Templates

```bash
curl -X GET "https://api.example.com/api/DocumentTemplates" \
  -H "Authorization: Bearer {token}"
```

### 2. Test Filter Theo Type

```bash
curl -X GET "https://api.example.com/api/DocumentTemplates?type=contract" \
  -H "Authorization: Bearer {token}"
```

### 3. Test L?y Template M?c ??nh

```bash
curl -X GET "https://api.example.com/api/DocumentTemplates/default/quote" \
  -H "Authorization: Bearer {token}"
```

### 4. Test T?o Template M?i

```bash
curl -X POST "https://api.example.com/api/DocumentTemplates" \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Template",
    "templateType": "quote",
    "code": "QUOTE_TEST",
    "htmlContent": "<html><body>Test</body></html>",
    "isActive": true
  }'
```

### 5. Test Update Template

```bash
curl -X PUT "https://api.example.com/api/DocumentTemplates/10" \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 10,
    "name": "Updated Test Template",
    "templateType": "quote",
    "code": "QUOTE_TEST",
    "htmlContent": "<html><body>Updated</body></html>",
    "isActive": true
  }'
```

### 6. Test ??t Làm M?c ??nh

```bash
curl -X PATCH "https://api.example.com/api/DocumentTemplates/10/set-default" \
  -H "Authorization: Bearer {admin_token}"
```

### 7. Test Soft Delete

```bash
curl -X DELETE "https://api.example.com/api/DocumentTemplates/10" \
  -H "Authorization: Bearer {admin_token}"
```

---

## ?? Error Handling

### Common Error Codes

| Status Code | Meaning | Common Causes |
|-------------|---------|---------------|
| `400` | Bad Request | - Duplicate Code<br>- Invalid data<br>- ID mismatch |
| `401` | Unauthorized | - Missing token<br>- Invalid token<br>- Expired token |
| `403` | Forbidden | - Không có quy?n Admin<br>- Insufficient permissions |
| `404` | Not Found | - Template không t?n t?i<br>- Invalid ID/Code |
| `500` | Internal Server Error | - Database error<br>- Server exception |

### Error Response Format

```json
{
  "success": false,
  "message": "User-friendly error message",
  "error": "Technical error details"
}
```

---

## ?? Best Practices

### 1. Naming Conventions

? **Good:**
```
Code: QUOTE_DEFAULT
Name: Quote Default Template
```

? **Bad:**
```
Code: quote1
Name: quote
```

### 2. HTML Content

? **Good:**
```html
<html>
<head>
  <meta charset="UTF-8">
  <style>/* CSS here */</style>
</head>
<body>
  {{CustomerName}}
</body>
</html>
```

? **Bad:**
```html
<div>{{Name}}</div>
```

### 3. Placeholder Documentation

```json
{
  "availablePlaceholders": "[\"{{CustomerName}}\",\"{{TotalAmount}}\",\"{{Date}}\"]"
}
```

### 4. Version Control

- M?i l?n update, `Version` t? ??ng t?ng
- Dùng `UpdatedAt` ?? track th?i gian thay ??i
- Có th? implement rollback d?a trên version

### 5. Default Templates

- M?i `TemplateType` nên có 1 template m?c ??nh
- ??t `IsDefault = true` cho template chính
- H? th?ng t? ??ng b? default c?a templates khác

---

## ?? Integration Examples

### Frontend Integration (React/Vue/Angular)

```typescript
// Service class
class DocumentTemplateService {
  private apiUrl = '/api/DocumentTemplates';
  
  async getAllTemplates(type?: string) {
    const params = type ? `?type=${type}` : '';
    const response = await fetch(`${this.apiUrl}${params}`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    return response.json();
  }
  
  async getTemplateById(id: number) {
    const response = await fetch(`${this.apiUrl}/${id}`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    return response.json();
  }
  
  async createTemplate(template: DocumentTemplate) {
    const response = await fetch(this.apiUrl, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${adminToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(template)
    });
    return response.json();
  }
  
  async updateTemplate(id: number, template: DocumentTemplate) {
    const response = await fetch(`${this.apiUrl}/${id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${adminToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(template)
    });
    return response.json();
  }
  
  async deleteTemplate(id: number) {
    const response = await fetch(`${this.apiUrl}/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${adminToken}`
      }
    });
    return response.json();
  }
  
  async setAsDefault(id: number) {
    const response = await fetch(`${this.apiUrl}/${id}/set-default`, {
      method: 'PATCH',
      headers: {
        'Authorization': `Bearer ${adminToken}`
      }
    });
    return response.json();
  }
}
```

---

## ?? Related Documentation

- [TEMPLATE_MIGRATION_README.md](./TEMPLATE_MIGRATION_README.md)
- [MIGRATION_CHANGES_SUMMARY.md](./MIGRATION_CHANGES_SUMMARY.md)
- [CONTRACT_TEMPLATE_MIGRATION_STATUS.md](./CONTRACT_TEMPLATE_MIGRATION_STATUS.md)

---

**Last Updated:** 2024-12-31  
**API Version:** 1.0  
**Status:** ? Production Ready
