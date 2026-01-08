# ?? API Documentation: Upload Commitment 08 File

## ?? T?ng quan

API này cho phép **nhân viên t? upload file cam k?t Thông t? 08** sau khi ?ã ???c c?u hình l??ng v?i `HasCommitment08 = true`.

- **Endpoint**: `POST /api/SalaryContracts/upload-commitment08/{id}`
- **Authentication**: Required (JWT Bearer Token)
- **Content-Type**: `multipart/form-data`
- **Authorization**: Ch? ch? s? h?u h?p ??ng m?i ???c upload file

---

## ?? Request Details

### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | ? Yes | ID c?a Salary Contract c?n upload file |

### Request Body (Form Data)

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| `File` | IFormFile | ? Yes | File cam k?t Thông t? 08 ?ã ?i?n và ký | Extensions: `.pdf`, `.doc`, `.docx`, `.jpg`, `.jpeg`, `.png`<br>Max size: 5MB |

### Headers

```http
Authorization: Bearer {jwt_token}
Content-Type: multipart/form-data
```

---

## ?? Security & Authorization

### Authentication
- API yêu c?u JWT token h?p l?
- Token ph?i ch?a claim `id`, `userId`, `sub`, ho?c `NameIdentifier`

### Authorization Rules
1. ? Nhân viên CH? ???c upload file cho h?p ??ng c?a **chính mình**
2. ? Không ???c upload cho h?p ??ng c?a ng??i khác
3. ? H?p ??ng ph?i có `HasCommitment08 = true`

### Security Checks
```csharp
// 1. Ki?m tra ownership
if (contract.UserId != currentUserId.Value)
{
    return StatusCode(403, "Không có quy?n upload file cho h?p ??ng này");
}

// 2. Ki?m tra yêu c?u cam k?t
if (!contract.HasCommitment08)
{
    return BadRequest("H?p ??ng này không yêu c?u cam k?t Thông t? 08");
}
```

---

## ? Success Response (200 OK)

### Response Body

```json
{
  "message": "? Upload cam k?t Thông t? 08 thành công!",
  "data": {
    "contractId": 1,
    "userId": 123,
    "userName": "Nguy?n V?n A",
    "filePath": "uploads/salary-contracts/Nguyen_Van_A_123/commitment08/cam-ket-tt08_20250109123456.pdf",
    "fileName": "cam-ket-tt08_20250109123456.pdf",
    "fileSize": "2.5 MB",
    "uploadedAt": "2025-01-09T12:34:56.789Z",
    "uploadedBy": 123
  },
  "hint": "B?n có th? c?p nh?t file m?i b?t c? lúc nào n?u c?n"
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `message` | string | Thông báo k?t qu? |
| `data.contractId` | int | ID c?a Salary Contract |
| `data.userId` | int | ID c?a user |
| `data.userName` | string | Tên user |
| `data.filePath` | string | ???ng d?n file ?ã l?u (relative path) |
| `data.fileName` | string | Tên file ?ã l?u |
| `data.fileSize` | string | Kích th??c file (formatted) |
| `data.uploadedAt` | datetime | Th?i ?i?m upload |
| `data.uploadedBy` | int | User ID ng??i upload |

---

## ? Error Responses

### 1. Unauthorized (401)

**Khi**: Token không h?p l? ho?c không tìm th?y userId trong claims

```json
{
  "message": "Không th? xác ??nh thông tin ng??i dùng. Vui lòng ??ng nh?p l?i."
}
```

---

### 2. Forbidden (403)

**Khi**: User c? g?ng upload file cho h?p ??ng c?a ng??i khác

```json
{
  "message": "? B?n không có quy?n upload file cho c?u hình l??ng này",
  "detail": "Ch? ???c upload file cho h?p ??ng c?a chính mình"
}
```

**Log Warning:**
```
User {CurrentUserId} attempted to upload commitment for contract {ContractId} owned by User {OwnerId}
```

---

### 3. Not Found (404)

**Khi**: Không tìm th?y Salary Contract v?i ID ???c cung c?p

```json
{
  "message": "Không tìm th?y c?u hình l??ng v?i ID này"
}
```

---

### 4. Bad Request (400)

#### Case 1: H?p ??ng không yêu c?u cam k?t TT08

```json
{
  "message": "C?u hình l??ng này không yêu c?u cam k?t Thông t? 08",
  "detail": "B?n không c?n upload file cam k?t cho lo?i h?p ??ng này"
}
```

#### Case 2: File không ???c ch?n

```json
{
  "message": "?? Vui lòng ch?n file ?? upload",
  "acceptedFormats": ".pdf, .doc, .docx, .jpg, .jpeg, .png",
  "maxSize": "5MB"
}
```

#### Case 3: File không h?p l?

```json
{
  "message": "? File không h?p l?. Ch? ch?p nh?n: .pdf, .doc, .docx, .jpg, .jpeg, .png",
  "uploadedFile": "document.txt",
  "acceptedFormats": [".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png"]
}
```

#### Case 4: File quá l?n

```json
{
  "message": "? File quá l?n (7.5MB). Kích th??c t?i ?a: 5MB",
  "uploadedSize": "7.5MB",
  "maxSize": "5MB"
}
```

---

### 5. Internal Server Error (500)

**Khi**: Có l?i không mong mu?n x?y ra

```json
{
  "message": "? Có l?i x?y ra khi upload file",
  "error": "Error message details",
  "detail": "Vui lòng th? l?i ho?c liên h? IT support"
}
```

---

## ?? File Upload Logic

### 1. File Naming Convention

```
{original-name-without-extension}_{timestamp}.{extension}

Example: cam-ket-tt08_20250109123456.pdf
```

### 2. Storage Path Structure

```
wwwroot/uploads/salary-contracts/{SanitizedUserName}_{UserId}/commitment08/

Example: 
wwwroot/uploads/salary-contracts/Nguyen_Van_A_123/commitment08/cam-ket-tt08_20250109123456.pdf
```

### 3. Folder Name Sanitization

**Input**: "Nguy?n V?n A" (userId: 123)  
**Output**: "Nguyen_Van_A_123"

**Rules:**
- Lo?i b? d?u ti?ng Vi?t
- Thay th? kho?ng tr?ng và ký t? ??c bi?t b?ng `_`
- Lo?i b? underscore liên ti?p
- K?t h?p v?i userId ?? ??m b?o unique

### 4. Old File Handling

- Khi upload file m?i, file c? s? ???c **t? ??ng xóa**
- N?u xóa file c? th?t b?i ? log warning nh?ng v?n ti?p t?c upload file m?i

---

## ?? Database Updates

### Fields Updated in `SalaryContracts` Table

| Field | Value |
|-------|-------|
| `AttachmentPath` | Relative path c?a file ?ã upload |
| `AttachmentFileName` | Tên file ?ã l?u |
| `UpdatedAt` | Timestamp hi?n t?i (UTC) |

---

## ?? Example Usage

### cURL Example

```bash
curl -X POST "https://api.example.com/api/SalaryContracts/upload-commitment08/1" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -F "File=@/path/to/cam-ket-tt08.pdf"
```

### JavaScript (Fetch API)

```javascript
const formData = new FormData();
formData.append('File', fileInput.files[0]);

const response = await fetch('/api/SalaryContracts/upload-commitment08/1', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});

const result = await response.json();
console.log(result);
```

### Axios Example

```javascript
import axios from 'axios';

const uploadCommitment08 = async (contractId, file, token) => {
  const formData = new FormData();
  formData.append('File', file);

  try {
    const response = await axios.post(
      `/api/SalaryContracts/upload-commitment08/${contractId}`,
      formData,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'multipart/form-data'
        }
      }
    );
    
    console.log('? Upload thành công:', response.data);
    return response.data;
  } catch (error) {
    console.error('? Upload th?t b?i:', error.response?.data);
    throw error;
  }
};

// Usage
const file = document.getElementById('fileInput').files[0];
await uploadCommitment08(1, file, 'your-jwt-token');
```

---

## ?? Testing Scenarios

### ? Happy Path

```gherkin
Given: User ?ã ??ng nh?p v?i JWT token h?p l?
And: User có Salary Contract v?i ID = 1
And: Contract có HasCommitment08 = true
When: User upload file PDF h?p l? (< 5MB)
Then: File ???c l?u thành công
And: Database ???c c?p nh?t
And: Response 200 OK v?i thông tin file
```

### ? Security Test: Unauthorized Access

```gherkin
Given: User A (userId: 100) ?ã ??ng nh?p
And: Salary Contract ID = 1 thu?c v? User B (userId: 200)
When: User A c? g?ng upload file cho Contract ID = 1
Then: Response 403 Forbidden
And: Log c?nh báo ???c ghi l?i
And: File không ???c upload
```

### ? Validation Test: Invalid File Type

```gherkin
Given: User ?ã ??ng nh?p
When: User upload file .exe
Then: Response 400 Bad Request
And: Error message "File không h?p l?"
```

### ? Validation Test: File Too Large

```gherkin
Given: User ?ã ??ng nh?p
When: User upload file PDF 10MB
Then: Response 400 Bad Request
And: Error message "File quá l?n"
```

---

## ?? Related APIs

| API | Method | Endpoint | Description |
|-----|--------|----------|-------------|
| Download Template | GET | `/api/SalaryContracts/download-commitment08-template` | Download file m?u Thông t? 08 |
| Get Contract | GET | `/api/SalaryContracts/{id}` | L?y thông tin h?p ??ng (bao g?m attachment) |
| Update Contract | PUT | `/api/SalaryContracts/{id}` | C?p nh?t thông tin h?p ??ng (Admin only) |
| Create Contract | POST | `/api/SalaryContracts` | T?o h?p ??ng l??ng m?i (Admin only) |

---

## ?? Integration Flow

```mermaid
sequenceDiagram
    participant Employee
    participant Frontend
    participant API
    participant FileService
    participant Database
    participant Storage

    Employee->>Frontend: Click "Upload Cam K?t"
    Frontend->>Frontend: Validate file (size, type)
    Frontend->>API: POST /upload-commitment08/{id}
    API->>API: Verify JWT & Extract userId
    API->>Database: Get SalaryContract by ID
    Database-->>API: Return contract
    API->>API: Check ownership (userId match)
    API->>API: Check HasCommitment08 = true
    API->>FileService: Delete old file (if exists)
    FileService->>Storage: Remove old file
    API->>FileService: Save new file
    FileService->>Storage: Write file to disk
    Storage-->>FileService: File path
    FileService-->>API: File info
    API->>Database: Update AttachmentPath & FileName
    Database-->>API: Success
    API-->>Frontend: 200 OK + file info
    Frontend-->>Employee: Show success message
```

---

## ?? Important Notes

### ?? Security Considerations

1. **Authentication**: Luôn ki?m tra JWT token h?p l?
2. **Authorization**: Validate ownership tr??c khi cho phép upload
3. **File Validation**: Ki?m tra extension và size ?? tránh abuse
4. **Audit Logging**: Log t?t c? các attempt upload (success/fail)

### ?? File Storage

1. **Old Files**: T? ??ng xóa file c? khi upload file m?i
2. **Folder Structure**: M?i user có folder riêng v?i tên sanitized
3. **File Naming**: Timestamp ?? tránh conflict và d? track

### ?? Business Logic

1. **Single Upload**: M?i contract ch? gi? 1 file cam k?t (m?i nh?t)
2. **Re-upload**: Employee có th? upload l?i file m?i b?t c? lúc nào
3. **Email Notification**: Email ???c g?i khi t?o contract ch? không ph?i khi upload

### ?? Monitoring

**Metrics to Track:**
- Upload success rate
- Average file size
- Failed upload reasons
- Unauthorized access attempts

**Log Examples:**
```
? SUCCESS: User 123 (Nguyen Van A) uploaded commitment08 for contract 1
? WARNING: User 100 attempted to upload for contract 1 (owned by User 200)
? ERROR: Failed to upload commitment08 for contract 1: File too large
```

---

## ?? Troubleshooting

### Issue: "Không th? xác ??nh thông tin ng??i dùng"

**Cause**: JWT token không ch?a userId claim  
**Solution**: Ki?m tra JWT generation logic, ensure claim "id" ???c thêm vào token

### Issue: "File không ???c l?u"

**Cause**: Folder permission ho?c disk space  
**Solution**: 
1. Check write permission trên `wwwroot/uploads`
2. Check disk space available
3. Check IIS/Kestrel user permissions

### Issue: "File c? không b? xóa"

**Cause**: File ?ang ???c s? d?ng ho?c permission issue  
**Solution**: 
- Warning ???c log nh?ng upload v?n ti?p t?c
- Manual cleanup n?u c?n

---

## ?? References

- [Main Salary Contracts API Documentation](./SALARY_CONTRACTS_API_DOCUMENTATION.md)
- [Email Service Upload 08 Summary](./EMAIL_UPLOAD_08_SUMMARY.md)
- [Frontend Quick Start Guide](./FRONTEND_CIRCULAR08_QUICKSTART.md)
- [Template Download API](./COMMITMENT08_TEMPLATE_API_DOCUMENTATION.md)

---

**Last Updated**: 2025-01-09  
**API Version**: 1.0  
**Maintained By**: Backend Team
