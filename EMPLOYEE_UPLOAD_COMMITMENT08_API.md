# ?? API Upload Cam K?t Thông T? 08 (Nhân Viên)

## ?? T?ng Quan

API này cho phép **nhân viên t? upload file cam k?t Thông t? 08** sau khi nh?n email thông báo t? Admin.

### ? ??c ?i?m chính:
- ? **Ch? upload file** - Không cho phép s?a thông tin l??ng
- ? **B?o m?t cao** - Ch? upload ???c file c?a chính mình
- ? **Validation ??y ??** - Ki?m tra ??nh d?ng, kích th??c, quy?n s? h?u
- ? **Auto cleanup** - T? ??ng xóa file c? khi upload file m?i
- ? **Th? m?c riêng** - L?u vào `/commitment08` ?? phân bi?t v?i các file khác

---

## ?? Endpoint

```
POST /api/SalaryContracts/{id}/upload-commitment08
```

### Parameters

| Tên | V? trí | Ki?u | B?t bu?c | Mô t? |
|-----|--------|------|----------|-------|
| `id` | URL Path | int | ? Có | ID c?a Salary Contract (l?y t? email) |
| `file` | Form Data | IFormFile | ? Có | File cam k?t ?ã ?i?n và ký |

---

## ?? Authentication

**Yêu c?u:** Bearer Token (JWT) c?a nhân viên

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Claims c?n có trong JWT:
- `id` ho?c `userId` ho?c `sub` ho?c `NameIdentifier`: ID c?a user

---

## ?? Request

### Headers
```http
POST /api/SalaryContracts/123/upload-commitment08
Authorization: Bearer {employee_jwt_token}
Content-Type: multipart/form-data
```

### Body (Form-Data)
```
file: [Select File] - Cam_Ket_TT08_Nguyen_Van_A.pdf
```

### File Requirements
| Tiêu chí | Giá tr? |
|----------|---------|
| **??nh d?ng** | `.pdf`, `.doc`, `.docx`, `.jpg`, `.jpeg`, `.png` |
| **Kích th??c t?i ?a** | 5 MB |
| **N?i dung** | File cam k?t ?ã ?i?n ??y ?? thông tin và có ch? ký |

---

## ? Response Success (200 OK)

```json
{
  "message": "? Upload cam k?t Thông t? 08 thành công!",
  "data": {
    "contractId": 123,
    "userId": 38,
    "userName": "Nguy?n V?n A",
    "filePath": "/uploads/salary-contracts/Nguyen_Van_A_38/commitment08/abc123-def456.pdf",
    "fileName": "Cam_Ket_TT08_Nguyen_Van_A.pdf",
    "fileSize": "1.23 MB",
    "uploadedAt": "2024-01-20T14:30:00Z",
    "uploadedBy": 38
  },
  "hint": "B?n có th? c?p nh?t file m?i b?t c? lúc nào n?u c?n"
}
```

---

## ? Error Responses

### 1. Unauthorized (401) - Ch?a ??ng nh?p

```json
{
  "message": "Không th? xác ??nh thông tin ng??i dùng. Vui lòng ??ng nh?p l?i."
}
```

**Nguyên nhân:**
- JWT token không h?p l? ho?c ?ã h?t h?n
- Không có claim `id`/`userId` trong token

**Gi?i pháp:**
- ??ng nh?p l?i ?? l?y token m?i

---

### 2. Forbidden (403) - Không có quy?n

```json
{
  "message": "? B?n không có quy?n upload file cho c?u hình l??ng này",
  "detail": "Ch? ???c upload file cho h?p ??ng c?a chính mình"
}
```

**Nguyên nhân:**
- User ?ang c? upload file cho contract c?a ng??i khác
- VD: User A (id=38) c? upload cho contract c?a User B (id=50)

**Gi?i pháp:**
- Ki?m tra l?i `contractId` trong URL
- ??m b?o upload ?úng contract c?a mình

---

### 3. Not Found (404) - Không tìm th?y contract

```json
{
  "message": "Không tìm th?y c?u hình l??ng v?i ID này"
}
```

**Nguyên nhân:**
- `contractId` không t?n t?i trong database
- Contract ?ã b? xóa

**Gi?i pháp:**
- Ki?m tra l?i URL t? email
- Liên h? HR ?? xác nh?n

---

### 4. Bad Request (400) - Không yêu c?u cam k?t TT08

```json
{
  "message": "C?u hình l??ng này không yêu c?u cam k?t Thông t? 08",
  "detail": "B?n không c?n upload file cam k?t cho lo?i h?p ??ng này"
}
```

**Nguyên nhân:**
- Contract có `HasCommitment08 = false`
- L??ng nhân viên ?? cao, không c?n cam k?t TT08

**Gi?i pháp:**
- B? qua, không c?n upload gì c?

---

### 5. Bad Request (400) - Ch?a ch?n file

```json
{
  "message": "?? Vui lòng ch?n file ?? upload",
  "acceptedFormats": ".pdf, .doc, .docx, .jpg, .jpeg, .png",
  "maxSize": "5MB"
}
```

**Nguyên nhân:**
- Không g?i file trong request
- File r?ng (0 bytes)

**Gi?i pháp:**
- Ch?n file tr??c khi submit

---

### 6. Bad Request (400) - File không h?p l?

```json
{
  "message": "? File không h?p l?. Ch? ch?p nh?n: .pdf, .doc, .docx, .jpg, .jpeg, .png",
  "uploadedFile": "cam_ket.txt",
  "acceptedFormats": [".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png"]
}
```

**Nguyên nhân:**
- Upload file không ?úng ??nh d?ng (VD: `.txt`, `.zip`, `.rar`)

**Gi?i pháp:**
- Chuy?n file sang ??nh d?ng PDF ho?c DOCX

---

### 7. Bad Request (400) - File quá l?n

```json
{
  "message": "? File quá l?n (8.45MB). Kích th??c t?i ?a: 5MB",
  "uploadedSize": "8.45MB",
  "maxSize": "5MB"
}
```

**Nguyên nhân:**
- File v??t quá 5MB

**Gi?i pháp:**
- Nén file l?i (VD: gi?m ch?t l??ng ?nh trong PDF)
- Ho?c ch?p l?i ?nh v?i resolution th?p h?n

---

### 8. Internal Server Error (500) - L?i server

```json
{
  "message": "? Có l?i x?y ra khi upload file",
  "error": "Disk quota exceeded",
  "detail": "Vui lòng th? l?i ho?c liên h? IT support"
}
```

**Nguyên nhân:**
- Server h?t dung l??ng disk
- L?i k?t n?i database
- L?i file system permissions

**Gi?i pháp:**
- Th? l?i sau vài phút
- Liên h? IT support n?u v?n l?i

---

## ?? Security Features

### 1. **Ownership Validation**
```csharp
// Ch? cho phép nhân viên upload file c?a chính mình
if (contract.UserId != currentUserId.Value)
{
    return Forbidden("B?n không có quy?n...");
}
```

**K?ch b?n b? ch?n:**
- User A c? upload file cho contract c?a User B
- Hacker c? thay ??i `contractId` trong URL

### 2. **File Type Validation**
```csharp
_allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" }
```

**K?ch b?n b? ch?n:**
- Upload file `.exe`, `.bat`, `.sh` (malware)
- Upload file `.zip`, `.rar` ch?a virus

### 3. **File Size Validation**
```csharp
_maxFileSizeInMB = 5; // 5MB
```

**K?ch b?n b? ch?n:**
- Upload file video 500MB (DoS attack)
- Upload file ?nh 50MB không nén

### 4. **JWT Authentication**
```csharp
[Authorize] // B?t bu?c ??ng nh?p
```

**K?ch b?n b? ch?n:**
- Anonymous users c? upload file
- Token ?ã expired ho?c invalid

### 5. **Auto Cleanup Old Files**
```csharp
// Xóa file c? tr??c khi upload file m?i
if (!string.IsNullOrEmpty(contract.AttachmentPath))
{
    await _fileUploadService.DeleteFileAsync(contract.AttachmentPath);
}
```

**L?i ích:**
- Không t?n disk space
- Luôn gi? file m?i nh?t

---

## ?? File Storage Structure

```
wwwroot/
??? uploads/
    ??? salary-contracts/
        ??? Nguyen_Van_A_38/          # Folder theo tên + userId
            ??? commitment08/          # ? Th? m?c riêng cho cam k?t TT08
                ??? abc123-def456.pdf  # File v?i GUID name
                ??? xyz789-ghi012.pdf  # File c? (s? b? xóa khi upload m?i)
```

**L?i ích:**
- ? Tách bi?t file cam k?t TT08 v?i các file khác (h?p ??ng lao ??ng, ph? l?c,...)
- ? D? backup/restore theo t?ng user
- ? D? audit: "User X ?ã upload file gì khi nào"

---

## ?? Testing Guide

### **Test 1: Upload file thành công**

#### Postman Setup:
1. **Method:** POST
2. **URL:** `http://localhost:5000/api/SalaryContracts/123/upload-commitment08`
3. **Headers:**
   - Key: `Authorization`
   - Value: `Bearer {employee_jwt_token}`
4. **Body:** (ch?n `form-data`)
   - Key: `file` (ch?n type = `File`)
   - Value: [Ch?n file PDF t? máy tính]

#### Expected:
```json
{
  "message": "? Upload cam k?t Thông t? 08 thành công!",
  "data": {
    "contractId": 123,
    "fileName": "Cam_Ket_TT08.pdf"
  }
}
```

---

### **Test 2: Upload file c?a ng??i khác (Expected: 403)**

#### Postman Setup:
- Dùng token c?a **User A (id=38)**
- Upload vào contract c?a **User B (id=50)**

#### Expected:
```json
{
  "message": "? B?n không có quy?n upload file cho c?u hình l??ng này"
}
```

---

### **Test 3: Upload file quá l?n (Expected: 400)**

#### Postman Setup:
- Upload file 10MB

#### Expected:
```json
{
  "message": "? File quá l?n (10.00MB). Kích th??c t?i ?a: 5MB"
}
```

---

### **Test 4: Upload file sai ??nh d?ng (Expected: 400)**

#### Postman Setup:
- Upload file `.txt` ho?c `.zip`

#### Expected:
```json
{
  "message": "? File không h?p l?. Ch? ch?p nh?n: .pdf, .doc, .docx, .jpg, .jpeg, .png"
}
```

---

### **Test 5: Upload l?i file m?i (Update)**

#### Postman Setup:
1. Upload file `v1.pdf` ? Success
2. Upload file `v2.pdf` ? Success

#### Expected:
- File `v1.pdf` b? xóa
- Ch? còn file `v2.pdf` trong database và disk
- `AttachmentPath` và `AttachmentFileName` ???c update

---

## ?? Frontend Integration (React/Next.js)

### **Example Component**

```tsx
'use client';

import { useState, useEffect } from 'react';
import { useAuth } from '@/hooks/useAuth';

export default function UploadCommitment08Page() {
  const { user, token } = useAuth();
  const [contract, setContract] = useState(null);
  const [selectedFile, setSelectedFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // L?y thông tin contract c?a user
  useEffect(() => {
    const fetchContract = async () => {
      try {
        const res = await fetch(`/api/SalaryContracts/user/${user.id}`, {
          headers: { Authorization: `Bearer ${token}` }
        });
        const data = await res.json();
        setContract(data.data);
      } catch (err) {
        setError('Không th? t?i thông tin h?p ??ng');
      }
    };

    if (user?.id) fetchContract();
  }, [user, token]);

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    
    // Validate client-side
    if (file) {
      const allowedTypes = [
        'application/pdf',
        'application/msword',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'image/jpeg',
        'image/png'
      ];

      if (!allowedTypes.includes(file.type)) {
        setError('File không h?p l?. Ch? ch?p nh?n PDF, DOC, DOCX, JPG, PNG');
        setSelectedFile(null);
        return;
      }

      if (file.size > 5 * 1024 * 1024) { // 5MB
        setError('File quá l?n. Kích th??c t?i ?a: 5MB');
        setSelectedFile(null);
        return;
      }

      setSelectedFile(file);
      setError('');
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      setError('Vui lòng ch?n file');
      return;
    }

    setUploading(true);
    setError('');
    setSuccess('');

    const formData = new FormData();
    formData.append('file', selectedFile);

    try {
      const res = await fetch(`/api/SalaryContracts/${contract.id}/upload-commitment08`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`
        },
        body: formData
      });

      const data = await res.json();

      if (res.ok) {
        setSuccess(data.message);
        setSelectedFile(null);
        // Reload contract data
        window.location.reload();
      } else {
        setError(data.message || 'Upload th?t b?i');
      }
    } catch (err) {
      setError('Có l?i x?y ra khi upload file');
      console.error(err);
    } finally {
      setUploading(false);
    }
  };

  const downloadTemplate = async () => {
    try {
      const res = await fetch('/api/SalaryContracts/download-commitment08-template');
      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'Mau_Cam_Ket_Thong_Tu_08.docx';
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      alert('Không th? t?i file m?u');
    }
  };

  // Loading state
  if (!contract) {
    return <div className="loading">?ang t?i thông tin...</div>;
  }

  // Không c?n upload
  if (!contract.hasCommitment08) {
    return (
      <div className="alert alert-info">
        <h3>? B?n không c?n ?i?n Cam k?t Thông t? 08</h3>
        <p>Lo?i h?p ??ng c?a b?n không yêu c?u cam k?t này.</p>
      </div>
    );
  }

  // ?ã upload r?i
  if (contract.attachmentPath) {
    return (
      <div className="success-container">
        <div className="alert alert-success">
          <h3>? ?ã upload cam k?t thành công!</h3>
          <p><strong>File:</strong> {contract.attachmentFileName}</p>
          <p><strong>Ngày upload:</strong> {new Date(contract.updatedAt).toLocaleString('vi-VN')}</p>
        </div>
        <div className="actions">
          <a 
            href={`${process.env.NEXT_PUBLIC_API_URL}${contract.attachmentPath}`}
            target="_blank"
            rel="noopener noreferrer"
            className="btn btn-secondary"
          >
            ?? Xem file ?ã upload
          </a>
          <button onClick={() => window.location.reload()} className="btn btn-primary">
            ?? Upload file m?i (thay th?)
          </button>
        </div>
      </div>
    );
  }

  // Form upload
  return (
    <div className="upload-container">
      <h2>?? Upload Cam K?t Thông T? 08</h2>

      <div className="alert alert-warning">
        <h4>?? Yêu c?u quan tr?ng</h4>
        <p>
          Vì l??ng c?a b?n là <strong>{contract.baseSalary.toLocaleString()} VN?/tháng</strong>,
          b?n c?n ?i?n và upload Cam k?t Thông t? 08.
        </p>
      </div>

      <div className="steps">
        <h4>?? Các b??c th?c hi?n:</h4>
        <ol>
          <li>
            <button onClick={downloadTemplate} className="btn btn-link">
              ?? T?i file m?u (.docx)
            </button>
          </li>
          <li>M? file b?ng Word và ?i?n ??y ?? thông tin</li>
          <li>Ký tên và ?óng d?u (n?u có)</li>
          <li>Upload file ?ã hoàn thành</li>
        </ol>
      </div>

      <div className="upload-form">
        <input
          type="file"
          accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
          onChange={handleFileChange}
          className="form-control"
        />
        
        {selectedFile && (
          <div className="file-info">
            <p>?? <strong>{selectedFile.name}</strong></p>
            <p>?? {(selectedFile.size / 1024 / 1024).toFixed(2)} MB</p>
          </div>
        )}

        {error && <div className="alert alert-danger">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}

        <button
          onClick={handleUpload}
          disabled={!selectedFile || uploading}
          className="btn btn-primary btn-lg"
        >
          {uploading ? '? ?ang upload...' : '?? Upload file'}
        </button>
      </div>

      <div className="alert alert-info">
        <h5>?? L?u ý:</h5>
        <ul>
          <li>??nh d?ng: <strong>PDF, DOC, DOCX, JPG, PNG</strong></li>
          <li>Kích th??c t?i ?a: <strong>5MB</strong></li>
          <li>File ph?i rõ ràng, có ch? ký h?p l?</li>
          <li>B?n có th? c?p nh?t file m?i b?t c? lúc nào</li>
        </ul>
      </div>
    </div>
  );
}
```

---

## ?? Logging

API s? log các s? ki?n sau:

### Success Logs:
```
? User 38 (Nguy?n V?n A) successfully uploaded commitment08 for contract 123: Cam_Ket_TT08.pdf
Deleted old commitment file for contract 123: /uploads/old.pdf
```

### Warning Logs:
```
User 38 attempted to upload commitment for contract 123 owned by User 50
Failed to delete old commitment file: /uploads/old.pdf - File not found
```

### Error Logs:
```
? Error uploading commitment08 for contract 123 by user 38
Exception: Disk quota exceeded
```

---

## ?? Related Endpoints

### 1. **Download Template**
```
GET /api/SalaryContracts/download-commitment08-template
```
T?i file m?u cam k?t TT08 (DOCX)

### 2. **Get Contract Info**
```
GET /api/SalaryContracts/user/{userId}
Authorization: Bearer {token}
```
L?y thông tin contract c?a user ?? ki?m tra:
- `hasCommitment08`: Có c?n upload không?
- `attachmentPath`: ?ã upload ch?a?

### 3. **Update Contract (Admin Only)**
```
PUT /api/SalaryContracts/{id}
Authorization: Bearer {admin_token}
```
Admin có th? upload file thay cho nhân viên qua endpoint này

---

## ? Checklist Hoàn Thành

- [x] API endpoint: `POST /api/SalaryContracts/{id}/upload-commitment08`
- [x] JWT authentication v?i claim validation
- [x] Ownership check (ch? upload file c?a mình)
- [x] File type validation
- [x] File size validation (max 5MB)
- [x] Auto cleanup old files
- [x] Separate folder: `/commitment08`
- [x] Comprehensive error handling
- [x] Detailed logging
- [x] Helper method: `GetCurrentUserId()`
- [x] Helper method: `FormatFileSize()`
- [x] Documentation ??y ??
- [x] Frontend integration example

---

## ?? Deployment Notes

### Production Checklist:
- [ ] Ki?m tra `appsettings.json` có ?úng config JWT không
- [ ] Test v?i nhi?u users khác nhau
- [ ] Ki?m tra folder permissions trên server
- [ ] Setup backup scheduler cho th? m?c `/uploads`
- [ ] Monitor disk space usage
- [ ] Setup error alerting (email/Slack khi có l?i 500)

---

## ?? Support

### Liên h?:
- **Backend Developer:** [Your Name]
- **Frontend Developer:** [Frontend Dev Name]
- **IT Support:** support@company.com

### Common Issues:
1. **"Không th? xác ??nh thông tin ng??i dùng"**
   - Check JWT configuration trong `Program.cs`
   - Verify claim names: `id`, `userId`, `sub`

2. **"403 Forbidden"**
   - User ?ang upload file c?a ng??i khác
   - Check `contractId` trong URL

3. **"500 Internal Server Error"**
   - Check server logs: `_logger.LogError`
   - Verify disk space
   - Check file permissions

---

**? API hoàn t?t và s?n sàng s? d?ng!**
