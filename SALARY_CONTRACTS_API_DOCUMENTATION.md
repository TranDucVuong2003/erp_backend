# ?? API Documentation: Salary Contracts Controller

## Base Information
- **Base URL**: `/api/SalaryContracts`
- **Authentication**: Required (JWT Bearer Token)
- **Content-Type**: 
  - POST/PUT: `multipart/form-data`
  - GET/DELETE: `application/json`

---

## ?? Table of Contents
1. [Create Salary Contract](#1-create-salary-contract)
2. [Get Salary Contract by ID](#2-get-salary-contract-by-id)
3. [Get Salary Contract by User ID](#3-get-salary-contract-by-user-id)
4. [Get All Salary Contracts](#4-get-all-salary-contracts)
5. [Update Salary Contract](#5-update-salary-contract)
6. [Delete Salary Contract](#6-delete-salary-contract)
7. [Download Thông T? 08 Template (HTML)](#7-download-thông-t?-08-template-html) ? NEW
8. [Download Thông T? 08 Template (PDF)](#8-download-thông-t?-08-template-pdf) ? NEW
9. [Check Thông T? 08 Requirement](#9-check-thông-t?-08-requirement) ? NEW
10. [Common Models](#common-models)
11. [Error Responses](#error-responses)

---

## 1. Create Salary Contract

### Endpoint
```http
POST /api/SalaryContracts
```

### Authentication
```
Authorization: Bearer {jwt_token}
```

### Request Headers
```http
Content-Type: multipart/form-data
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Request Body (Form-Data)

| Field | Type | Required | Default | Validation | Description |
|-------|------|----------|---------|------------|-------------|
| `UserId` | integer | ? Yes | - | Must exist in Users table | ID c?a nhân viên |
| `BaseSalary` | decimal | ? Yes | - | >= 0 | L??ng c? b?n (VD: 20000000) |
| `InsuranceSalary` | decimal | ? Yes | - | >= 0 | L??ng ?óng b?o hi?m (0 = t? ??ng tính) |
| `ContractType` | string | ? Yes | "OFFICIAL" | "OFFICIAL" or "FREELANCE" | Lo?i h?p ??ng |
| `DependentsCount` | integer | ? No | 0 | 0-20 | S? ng??i ph? thu?c |
| `HasCommitment08` | boolean | ? No | false | - | Có cam k?t 08 không? |
| `Attachment` | file | ? No | null | Max 5MB, .pdf/.doc/.docx/.jpg/.jpeg/.png | File ?ính kèm |

### Request Example (Form-Data)

**Using Postman:**
```
UserId: 38
BaseSalary: 20000000
InsuranceSalary: 0
ContractType: OFFICIAL
DependentsCount: 2
HasCommitment08: false
Attachment: [Select file from computer]
```

**Using cURL:**
```bash
curl -X POST "http://localhost:5000/api/SalaryContracts" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "UserId=38" \
  -F "BaseSalary=20000000" \
  -F "InsuranceSalary=0" \
  -F "ContractType=OFFICIAL" \
  -F "DependentsCount=2" \
  -F "HasCommitment08=false" \
  -F "Attachment=@/path/to/contract.pdf"
```

**Using JavaScript (Fetch):**
```javascript
const formData = new FormData();
formData.append('UserId', 38);
formData.append('BaseSalary', 20000000);
formData.append('InsuranceSalary', 0);
formData.append('ContractType', 'OFFICIAL');
formData.append('DependentsCount', 2);
formData.append('HasCommitment08', false);
formData.append('Attachment', fileInput.files[0]);

const response = await fetch('http://localhost:5000/api/SalaryContracts', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});

const result = await response.json();
```

### Success Response (201 Created)

```json
{
  "message": "T?o h?p ??ng l??ng thành công",
  "data": {
    "id": 1,
    "userId": 38,
    "baseSalary": 20000000,
    "insuranceSalary": 5682000,
    "contractType": "OFFICIAL",
    "dependentsCount": 2,
    "hasCommitment08": false,
    "attachmentPath": "/uploads/salary-contracts/Nguyen_Van_A_38/7502c47f-1fca-4ae1-817f-1698cccfd608.pdf",
    "attachmentFileName": "hop_dong_lao_dong.pdf",
    "createdAt": "2026-01-05T09:15:30.123Z",
    "updatedAt": null,
    "userName": "Nguy?n Vân A",
    "userEmail": "nguyenvana@company.com"
  }
}
```

### Error Responses

**400 Bad Request - User không t?n t?i:**
```json
{
  "message": "User không t?n t?i"
}
```

**400 Bad Request - User ?ã có contract:**
```json
{
  "message": "Nhân viên ?ã ???c c?u hình l??ng",
  "existingContractId": 5,
  "hint": "S? d?ng PUT /api/SalaryContracts/{id} ?? c?p nh?t"
}
```

**400 Bad Request - File không h?p l?:**
```json
{
  "message": "File không h?p l?. Ch? ch?p nh?n: .pdf, .doc, .docx, .jpg, .jpeg, .png"
}
```

**400 Bad Request - File quá l?n:**
```json
{
  "message": "File quá l?n. Kích th??c t?i ?a: 5MB"
}
```

**500 Internal Server Error:**
```json
{
  "message": "Có l?i x?y ra khi t?o h?p ??ng",
  "error": "Detailed error message"
}
```

### Business Logic Notes

1. **Auto Calculate Insurance Salary**: 
   - N?u `InsuranceSalary = 0`, h? th?ng t? ??ng tính theo công th?c:
   ```
   InsuranceSalary = MIN_WAGE_REGION_1_2026 * TRAINED_WORKER_RATE
   InsuranceSalary = 5,310,000 * 1.07 = 5,682,000 VND
   ```

2. **Folder Structure**:
   - File ???c l?u t?i: `wwwroot/uploads/salary-contracts/{Username}_{UserId}/`
   - Tên file ???c generate v?i GUID ?? tránh trùng l?p
   - VD: `Nguyen_Van_A_38/7502c47f-1fca-4ae1-817f-1698cccfd608.pdf`

3. **File Naming Sanitization**:
   - Lo?i b? d?u ti?ng Vi?t: "Nguy?n" ? "Nguyen"
   - Thay kho?ng tr?ng và ký t? ??c bi?t b?ng underscore
   - K?t h?p v?i UserID ?? ??m b?o unique

---

## 2. Get Salary Contract by ID

### Endpoint
```http
GET /api/SalaryContracts/{id}
```

### Authentication
```
Authorization: Bearer {jwt_token}
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | ? Yes | ID c?a Salary Contract |

### Request Example

**cURL:**
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
const response = await fetch('http://localhost:5000/api/SalaryContracts/1', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const result = await response.json();
```

### Success Response (200 OK)

```json
{
  "message": "L?y thông tin h?p ??ng thành công",
  "data": {
    "id": 1,
    "userId": 38,
    "baseSalary": 20000000,
    "insuranceSalary": 5682000,
    "contractType": "OFFICIAL",
    "dependentsCount": 2,
    "hasCommitment08": false,
    "attachmentPath": "/uploads/salary-contracts/Nguyen_Van_A_38/7502c47f-1fca-4ae1-817f-1698cccfd608.pdf",
    "attachmentFileName": "hop_dong_lao_dong.pdf",
    "createdAt": "2026-01-05T09:15:30.123Z",
    "updatedAt": "2026-01-05T10:30:45.678Z",
    "userName": "Nguy?n Vân A",
    "userEmail": "nguyenvana@company.com"
  }
}
```

### Error Response (404 Not Found)

```json
{
  "message": "Salary Contract không t?n t?i"
}
```

---

## 3. Get Salary Contract by User ID

### Endpoint
```http
GET /api/SalaryContracts/user/{userId}
```

### Authentication
```
Authorization: Bearer {jwt_token}
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | integer | ? Yes | ID c?a User/Nhân viên |

### Request Example

**cURL:**
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/user/38" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
const response = await fetch('http://localhost:5000/api/SalaryContracts/user/38', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const result = await response.json();
```

### Success Response (200 OK)

```json
{
  "message": "L?y thông tin h?p ??ng thành công",
  "data": {
    "id": 1,
    "userId": 38,
    "baseSalary": 20000000,
    "insuranceSalary": 5682000,
    "contractType": "OFFICIAL",
    "dependentsCount": 2,
    "hasCommitment08": false,
    "attachmentPath": "/uploads/salary-contracts/Nguyen_Van_A_38/7502c47f-1fca-4ae1-817f-1698cccfd608.pdf",
    "attachmentFileName": "hop_dong_lao_dong.pdf",
    "createdAt": "2026-01-05T09:15:30.123Z",
    "updatedAt": null,
    "userName": "Nguy?n Vân A",
    "userEmail": "nguyenvana@company.com"
  }
}
```

### Error Response (404 Not Found)

```json
{
  "message": "User ch?a có Salary Contract"
}
```

---

## 4. Get All Salary Contracts

### Endpoint
```http
GET /api/SalaryContracts
```

### Authentication
```
Authorization: Bearer {jwt_token}
```

### Request Example

**cURL:**
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
const response = await fetch('http://localhost:5000/api/SalaryContracts', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const result = await response.json();
```

### Success Response (200 OK)

```json
{
  "message": "L?y danh sách h?p ??ng thành công",
  "data": [
    {
      "id": 1,
      "userId": 38,
      "baseSalary": 20000000,
      "insuranceSalary": 5682000,
      "contractType": "OFFICIAL",
      "dependentsCount": 2,
      "hasCommitment08": false,
      "attachmentPath": "/uploads/salary-contracts/Nguyen_Van_A_38/7502c47f-1fca-4ae1-817f-1698cccfd608.pdf",
      "attachmentFileName": "hop_dong_lao_dong.pdf",
      "createdAt": "2026-01-05T09:15:30.123Z",
      "updatedAt": null,
      "userName": "Nguy?n Vân A",
      "userEmail": "nguyenvana@company.com"
    },
    {
      "id": 2,
      "userId": 42,
      "baseSalary": 15000000,
      "insuranceSalary": 5682000,
      "contractType": "FREELANCE",
      "dependentsCount": 0,
      "hasCommitment08": true,
      "attachmentPath": "/uploads/salary-contracts/Tran_Thi_B_42/abc123-guid.pdf",
      "attachmentFileName": "contract_freelance.pdf",
      "createdAt": "2026-01-04T08:20:15.456Z",
      "updatedAt": "2026-01-05T11:45:22.789Z",
      "userName": "Tr?n Th? B",
      "userEmail": "tranthib@company.com"
    }
  ],
  "total": 2
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `message` | string | Thông báo k?t qu? |
| `data` | array | M?ng các Salary Contract objects |
| `total` | integer | T?ng s? contracts |

### Sorting
- Data ???c s?p x?p theo `CreatedAt` gi?m d?n (m?i nh?t tr??c)

---

## 5. Update Salary Contract

### Endpoint
```http
PUT /api/SalaryContracts/{id}
```

### Authentication
```
Authorization: Bearer {jwt_token}
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | ? Yes | ID c?a Salary Contract c?n c?p nh?t |

### Request Headers
```http
Content-Type: multipart/form-data
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Request Body (Form-Data)

**?? T?t c? các field ??u OPTIONAL - ch? g?i nh?ng field c?n c?p nh?t**

| Field | Type | Required | Validation | Description |
|-------|------|----------|------------|-------------|
| `BaseSalary` | decimal | ? No | >= 0 | L??ng c? b?n m?i |
| `InsuranceSalary` | decimal | ? No | >= 0 | L??ng b?o hi?m m?i (0 = auto calculate) |
| `ContractType` | string | ? No | "OFFICIAL" or "FREELANCE" | Lo?i h?p ??ng m?i |
| `DependentsCount` | integer | ? No | 0-20 | S? ng??i ph? thu?c m?i |
| `HasCommitment08` | boolean | ? No | - | Tr?ng thái cam k?t 08 m?i |
| `Attachment` | file | ? No | Max 5MB | File ?ính kèm m?i (xóa file c?) |

### Request Example (Form-Data)

**Scenario 1: Ch? c?p nh?t l??ng**
```
BaseSalary: 25000000
```

**Scenario 2: C?p nh?t l??ng + file m?i**
```
BaseSalary: 25000000
InsuranceSalary: 6000000
Attachment: [Select new file]
```

**Scenario 3: Ch? thay file ?ính kèm**
```
Attachment: [Select new file]
```

**cURL Example:**
```bash
curl -X PUT "http://localhost:5000/api/SalaryContracts/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "BaseSalary=25000000" \
  -F "DependentsCount=3" \
  -F "Attachment=@/path/to/new-contract.pdf"
```

**JavaScript Example:**
```javascript
const formData = new FormData();
formData.append('BaseSalary', 25000000);
formData.append('DependentsCount', 3);
if (newFile) {
  formData.append('Attachment', newFile);
}

const response = await fetch('http://localhost:5000/api/SalaryContracts/1', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});

const result = await response.json();
```

### Success Response (200 OK)

```json
{
  "message": "C?p nh?t h?p ??ng l??ng thành công",
  "data": {
    "id": 1,
    "userId": 38,
    "baseSalary": 25000000,
    "insuranceSalary": 5682000,
    "contractType": "OFFICIAL",
    "dependentsCount": 3,
    "hasCommitment08": false,
    "attachmentPath": "/uploads/salary-contracts/Nguyen_Van_A_38/new-guid-file.pdf",
    "attachmentFileName": "hop_dong_moi.pdf",
    "createdAt": "2026-01-05T09:15:30.123Z",
    "updatedAt": "2026-01-05T14:22:10.456Z",
    "userName": "Nguy?n Vân A",
    "userEmail": "nguyenvana@company.com"
  }
}
```

### Error Responses

**404 Not Found - Contract không t?n t?i:**
```json
{
  "message": "Salary Contract không t?n t?i"
}
```

**400 Bad Request - File không h?p l?:**
```json
{
  "message": "File không h?p l?. Ch? ch?p nh?n: .pdf, .doc, .docx, .jpg, .jpeg, .png"
}
```

**400 Bad Request - File quá l?n:**
```json
{
  "message": "File quá l?n. Kích th??c t?i ?a: 5MB"
}
```

**500 Internal Server Error:**
```json
{
  "message": "Có l?i x?y ra khi c?p nh?t h?p ??ng",
  "error": "Detailed error message"
}
```

### Business Logic Notes

1. **Partial Update**: 
   - Ch? nh?ng field ???c g?i lên m?i ???c c?p nh?t
   - Field không g?i s? gi? nguyên giá tr? c?

2. **File Replacement**:
   - Khi upload file m?i, file c? s? t? ??ng b? xóa
   - File m?i ???c l?u vào cùng folder v?i tên GUID m?i

3. **UpdatedAt Timestamp**:
   - T? ??ng c?p nh?t thành th?i gian hi?n t?i

---

## 6. Delete Salary Contract

### Endpoint
```http
DELETE /api/SalaryContracts/{id}
```

### Authentication
```
Authorization: Bearer {jwt_token}
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | ? Yes | ID c?a Salary Contract c?n xóa |

### Request Example

**cURL:**
```bash
curl -X DELETE "http://localhost:5000/api/SalaryContracts/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
const response = await fetch('http://localhost:5000/api/SalaryContracts/1', {
  method: 'DELETE',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const result = await response.json();
```

### Success Response (200 OK)

```json
{
  "message": "Xóa h?p ??ng l??ng thành công"
}
```

### Error Responses

**404 Not Found:**
```json
{
  "message": "Salary Contract không t?n t?i"
}
```

**500 Internal Server Error:**
```json
{
  "message": "Có l?i x?y ra khi xóa h?p ??ng",
  "error": "Detailed error message"
}
```

### Business Logic Notes

1. **Cascade Delete**:
   - File ?ính kèm (n?u có) s? t? ??ng b? xóa kh?i server
   - Record trong database b? xóa v?nh vi?n

2. **No Soft Delete**:
   - ?ây là hard delete, không th? khôi ph?c
   - Cân nh?c thêm soft delete trong t??ng lai

---

## 7. Download Thông T? 08 Template (HTML) ? NEW

### Endpoint
```http
GET /api/SalaryContracts/Template08/html
```

### Authentication
```
Authorization: Bearer {jwt_token}
```

### Request Example

**cURL:**
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/Template08/html" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
const response = await fetch('http://localhost:5000/api/SalaryContracts/Template08/html', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const result = await response.blob();
const url = window.URL.createObjectURL(result);
const a = document.createElement('a');
a.href = url;
a.download = 'template_thong_tu_08.html';
document.body.appendChild(a);
a.click();
a.remove();
```

### Success Response (200 OK)

```html
<!-- HTML file content -->
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>M?u Thông T? 08</title>
    <style>
        /* Basic styles for the template */
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
        }
        h1, h2, h3 {
            color: #333;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }
        th, td {
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }
        th {
            background-color: #f2f2f2;
        }
    </style>
</head>
<body>

<h1>M?u Thông T? 08</h1>
<p>H?i ng?y ... th?ng t? ...</p>

<h2>Thông tin h?p ??ng</h2>
<table>
    <tr>
        <th>STT</th>
        <th>N?m</th>
        <th>C? th? lo?i h?p ??ng</th>
        <th>S? ng??i ph? thu?c</th>
        <th>C?o cam k?t 08</th>
    </tr>
    <tr>
        <td>1</td>
        <td>2026</td>
        <td>Chính th?c</td>
        <td>2</td>
        <td>Có</td>
    </tr>
    <!-- More rows as needed -->
</table>

<h2>Chi t?c h?p ??ng</h2>
<ul>
    <li><strong>L??ng c? b?n:</strong> 20,000,000 VN?</li>
    <li><strong>L??ng ?óng b?o hi?m:</strong> 5,682,000 VN? (t? ??ng tính)</li>
    <!-- More fields as needed -->
</ul>

<h3>Ch? ý:</h3>
<p>- Vui l?ng ?i??n ?? ??n c?c thông tin c?n thi?t.</p>
<p>- G?i l?i b?ng c?ch t? t?o file PDF t? HTML template n??c này.</p>

</body>
</html>
```

### Error Responses

**401 Unauthorized - Token kh?ng h?p l?:
```json
{
  "message": "Unauthorized"
}
```

**500 Internal Server Error:**
```json
{
  "message": "Có l?i kh?i t? Template",
  "error": "Detailed error message"
}
```

---

## 8. Download Thông T? 08 Template (PDF) ? NEW

### Endpoint
```http
GET /api/SalaryContracts/Template08/pdf
```

### Authentication
```
Authorization: Bearer {jwt_token}
```

### Request Example

**cURL:**
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/Template08/pdf" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
const response = await fetch('http://localhost:5000/api/SalaryContracts/Template08/pdf', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const result = await response.blob();
const url = window.URL.createObjectURL(result);
const a = document.createElement('a');
a.href = url;
a.download = 'template_thong_tu_08.pdf';
document.body.appendChild(a);
a.click();
a.remove();
```

### Success Response (200 OK)

**Content-Disposition:** `attachment; filename="template_thong_tu_08.pdf"`

**Response Body:** Binary data of the PDF file.

### Error Responses

**401 Unauthorized - Token kh?ng h?p l?:**
```json
{
  "message": "Unauthorized"
}
```

**500 Internal Server Error:**
```json
{
  "message": "Có l?i kh?i t? Template",
  "error": "Detailed error message"
}
```

---

## 9. Check Thông T? 08 Requirement ? NEW

### Endpoint
```http
POST /api/SalaryContracts/Template08/check
```

### Authentication
```
Authorization: Bearer {jwt_token}
```

### Request Body (JSON)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `BaseSalary` | decimal | ? Yes | L??ng c? b?n |
| `DependentsCount` | integer | ? Yes | S? ng??i ph? thu?c |
| `HasCommitment08` | boolean | ? Yes | Có cam k?t 08 không? |

### Request Example (JSON)
```json
{
  "BaseSalary": 20000000,
  "DependentsCount": 2,
  "HasCommitment08": false
}
```

### Request Example (Raw)
```bash
curl -X POST "http://localhost:5000/api/SalaryContracts/Template08/check" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"BaseSalary":20000000,"DependentsCount":2,"HasCommitment08":false}'
```

### Success Response (200 OK)

```json
{
  "message": "Ki?m tra th?nh công",
  "data": {
    "baseSalary": true,
    "insuranceSalary": true,
    "contractType": true,
    "dependentsCount": true,
    "hasCommitment08": true
  }
}
```

### Error Responses

**400 Bad Request - Thi?u tr??ng:**
```json
{
  "message": "Thi?u tr??ng yêu c?u: BaseSalary, DependentsCount, HasCommitment08"
}
```

**500 Internal Server Error:**
```json
{
  "message": "Có l?i kh?i t? ki?m tra",
  "error": "Detailed error message"
}
```

---

## Common Models

### SalaryContractResponseDto

```typescript
{
  id: number,                    // Primary Key
  userId: number,                // Foreign Key to Users
  baseSalary: number,            // L??ng c? b?n (decimal)
  insuranceSalary: number,       // L??ng ?óng BH (decimal)
  contractType: string,          // "OFFICIAL" | "FREELANCE"
  dependentsCount: number,       // 0-20
  hasCommitment08: boolean,      // true | false
  attachmentPath: string | null, // "/uploads/salary-contracts/..."
  attachmentFileName: string | null, // "hop_dong.pdf"
  createdAt: string,             // ISO 8601 datetime
  updatedAt: string | null,      // ISO 8601 datetime
  userName: string | null,       // Tên nhân viên
  userEmail: string | null       // Email nhân viên
}
```

### CreateSalaryContractDto

```typescript
{
  UserId: number,              // Required
  BaseSalary: number,          // Required, >= 0
  InsuranceSalary: number,     // Required, >= 0
  ContractType: string,        // Required, "OFFICIAL" | "FREELANCE"
  DependentsCount: number,     // Optional, 0-20, default: 0
  HasCommitment08: boolean,    // Optional, default: false
  Attachment: File | null      // Optional, max 5MB
}
```

### UpdateSalaryContractDto

```typescript
{
  BaseSalary?: number,         // Optional, >= 0
  InsuranceSalary?: number,    // Optional, >= 0
  ContractType?: string,       // Optional, "OFFICIAL" | "FREELANCE"
  DependentsCount?: number,    // Optional, 0-20
  HasCommitment08?: boolean,   // Optional
  Attachment?: File            // Optional, max 5MB
}
```

---

## Error Responses

### Standard Error Format

```json
{
  "message": "Error description in Vietnamese",
  "error": "Detailed technical error (only in 500 errors)"
}
```

### HTTP Status Codes

| Code | Description | When |
|------|-------------|------|
| 200 | OK | Successful GET/PUT/DELETE |
| 201 | Created | Successful POST |
| 400 | Bad Request | Validation failed, file invalid |
| 401 | Unauthorized | Missing or invalid JWT token |
| 404 | Not Found | Contract/User not found |
| 500 | Internal Server Error | Server-side error |

---

## File Upload Specifications

### Allowed Extensions
```
.pdf, .doc, .docx, .jpg, .jpeg, .png
```

### Max File Size
```
5 MB (5,242,880 bytes)
```

### Storage Location
```
wwwroot/uploads/salary-contracts/{Username}_{UserId}/{GUID}.{ext}
```

### Example Paths
```
/uploads/salary-contracts/Nguyen_Van_A_38/7502c47f-1fca-4ae1-817f-1698cccfd608.pdf
/uploads/salary-contracts/Tran_Thi_B_42/abc123-def456-ghi789.docx
/uploads/salary-contracts/Le_Van_C_55/photo-scan.jpg
```

### Access File
```
GET http://localhost:5000/uploads/salary-contracts/Nguyen_Van_A_38/7502c47f-1fca-4ae1-817f-1698cccfd608.pdf
```

---

## Security & Authentication

### JWT Token Required
All endpoints require valid JWT token in Authorization header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

### Token Claims Required
- Valid user ID
- Not expired
- Valid signature

### File Security
- Extension validation
- Size validation
- GUID filename to prevent path traversal
- Stored outside of public web directory structure

---

## Rate Limiting

?? **Note**: Currently no rate limiting implemented. Consider adding in production:
- 100 requests per minute per user
- 10 file uploads per hour per user

---

## Testing Examples

### Postman Collection Structure

```
?? Salary Contracts
??? ?? Create Contract (with file)
??? ?? Create Contract (without file)
??? ?? Get Contract by ID
??? ?? Get Contract by User ID
??? ?? Get All Contracts
??? ?? Update Contract (partial)
??? ?? Update Contract (with new file)
??? ?? Delete Contract
```

### React/TypeScript Integration

```typescript
// services/salaryContractService.ts

interface CreateSalaryContractRequest {
  userId: number;
  baseSalary: number;
  insuranceSalary: number;
  contractType: 'OFFICIAL' | 'FREELANCE';
  dependentsCount?: number;
  hasCommitment08?: boolean;
  attachment?: File;
}

export const salaryContractService = {
  async create(data: CreateSalaryContractRequest) {
    const formData = new FormData();
    formData.append('UserId', data.userId.toString());
    formData.append('BaseSalary', data.baseSalary.toString());
    formData.append('InsuranceSalary', data.insuranceSalary.toString());
    formData.append('ContractType', data.contractType);
    
    if (data.dependentsCount !== undefined) {
      formData.append('DependentsCount', data.dependentsCount.toString());
    }
    
    if (data.hasCommitment08 !== undefined) {
      formData.append('HasCommitment08', data.hasCommitment08.toString());
    }
    
    if (data.attachment) {
      formData.append('Attachment', data.attachment);
    }

    const response = await fetch('/api/SalaryContracts', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${getToken()}`
      },
      body: formData
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  },

  async getById(id: number) {
    const response = await fetch(`/api/SalaryContracts/${id}`, {
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error('Failed to fetch contract');
    }

    return await response.json();
  },

  async getByUserId(userId: number) {
    const response = await fetch(`/api/SalaryContracts/user/${userId}`, {
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  },

  async getAll() {
    const response = await fetch('/api/SalaryContracts', {
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error('Failed to fetch contracts');
    }

    return await response.json();
  },

  async update(id: number, data: Partial<CreateSalaryContractRequest>) {
    const formData = new FormData();
    
    if (data.baseSalary !== undefined) {
      formData.append('BaseSalary', data.baseSalary.toString());
    }
    
    if (data.insuranceSalary !== undefined) {
      formData.append('InsuranceSalary', data.insuranceSalary.toString());
    }
    
    if (data.contractType) {
      formData.append('ContractType', data.contractType);
    }
    
    if (data.dependentsCount !== undefined) {
      formData.append('DependentsCount', data.dependentsCount.toString());
    }
    
    if (data.hasCommitment08 !== undefined) {
      formData.append('HasCommitment08', data.hasCommitment08.toString());
    }
    
    if (data.attachment) {
      formData.append('Attachment', data.attachment);
    }

    const response = await fetch(`/api/SalaryContracts/${id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${getToken()}`
      },
      body: formData
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  },

  async delete(id: number) {
    const response = await fetch(`/api/SalaryContracts/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  }
};
```

---

## Troubleshooting

### Common Issues

**1. "File quá l?n" Error**

Solution: Check file size before upload
```javascript
if (file.size > 5 * 1024 * 1024) {
  alert('File không ???c v??t quá 5MB');
  return;
}
```

**2. "File không h?p l?" Error**

Solution: Validate extension
```javascript
const allowedExtensions = ['.pdf', '.doc', '.docx', '.jpg', '.jpeg', '.png'];
const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();

if (!allowedExtensions.includes(fileExtension)) {
  alert('Ch? ch?p nh?n file PDF, DOC, DOCX, JPG, PNG');
  return;
}
```

**3. File không hi?n th? sau khi upload**

Check:
- `app.UseStaticFiles()` enabled in `Program.cs`
- File path starts with `/uploads/`
- wwwroot folder has correct permissions

**4. "User ?ã có Salary Contract"**

Solution: Use PUT endpoint instead of POST
```javascript
// Check if user already has contract
const existingContract = await salaryContractService.getByUserId(userId);

if (existingContract) {
  // Update existing
  await salaryContractService.update(existingContract.data.id, newData);
} else {
  // Create new
  await salaryContractService.create(newData);
}
```

---

## Performance Considerations

1. **File Upload Size**: Max 5MB to prevent server overload
2. **Database Queries**: Using `.Include(c => c.User)` for eager loading
3. **File Storage**: Local storage - consider cloud storage for production
4. **Caching**: No caching implemented - consider Redis for frequently accessed contracts

---

## Future Enhancements

### Planned Features
- [ ] Multiple file attachments support
- [ ] Contract version history
- [ ] Contract approval workflow
- [ ] Email notification on contract creation/update
- [ ] Export contracts to Excel/PDF report
- [ ] Soft delete with restore capability
- [ ] Audit log for contract changes
- [ ] File preview functionality
- [ ] Digital signature integration

---

## Change Log

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-01-05 | Initial API implementation with file upload |
| 1.1.0 | TBD | Add multiple files support |

---

## Support & Contact

For issues or questions:
- **Email**: support@company.com
- **Documentation**: [Internal Wiki]
- **Bug Reports**: [Issue Tracker]

---

**Last Updated**: 2026-01-05  
**API Version**: 1.0.0  
**Author**: Development Team
