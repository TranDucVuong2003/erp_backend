# ?? Quick Start: Frontend Integration - Upload Cam K?t TT08

## ?? Overview

Khi admin t?o salary contract v?i `HasCommitment08 = true`, h? th?ng t? ??ng g?i email cho nhân viên.
Email ch?a 2 links:
1. **Download file m?u** ? Backend API
2. **Upload file ?ã ?i?n** ? Frontend page `/circular-08`

---

## ?? Links trong Email

### 1. Download Template Button
```html
<a href='{{DownloadTemplateLink}}' class='btn-secondary'>
  ?? T?i m?u Cam k?t 08
</a>
```

**Value:** `{BackendUrl}/api/SalaryContracts/download-commitment08-template`

**Example:** `http://localhost:5000/api/SalaryContracts/download-commitment08-template`

---

### 2. Upload Button
```html
<a href='{{UploadAttachmentLink}}' class='btn-primary'>
  ?? Upload Cam k?t Thông t? 08 ngay
</a>
```

**Value:** `{FrontendUrl}/circular-08`

**Example:** `http://localhost:3000/circular-08`

---

## ?? Frontend Page Implementation

### Route: `/circular-08`

```tsx
// pages/circular-08/index.tsx

import { useState, useEffect } from 'react';
import { useAuth } from '@/hooks/useAuth';
import { apiClient } from '@/lib/api';

export default function Circular08Page() {
  const { user } = useAuth();
  const [contract, setContract] = useState(null);
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);

  useEffect(() => {
    loadContract();
  }, []);

  const loadContract = async () => {
    try {
      const res = await apiClient.get(`/api/SalaryContracts/user/${user.id}`);
      setContract(res.data.data);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = async () => {
    const link = document.createElement('a');
    link.href = `${process.env.NEXT_PUBLIC_API_URL}/api/SalaryContracts/download-commitment08-template`;
    link.download = 'Mau_Cam_Ket_Thong_Tu_08.docx';
    link.click();
  };

  const handleUpload = async () => {
    if (!file) return;

    setUploading(true);
    const formData = new FormData();
    formData.append('Attachment', file);

    try {
      await apiClient.put(`/api/SalaryContracts/${contract.id}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      alert('? Upload thành công!');
      loadContract(); // Reload
    } catch (error) {
      alert('? Upload th?t b?i!');
    } finally {
      setUploading(false);
    }
  };

  if (loading) return <div>?ang t?i...</div>;

  if (!contract?.hasCommitment08) {
    return (
      <div className="alert alert-info">
        ? B?n không c?n ?i?n Cam k?t Thông t? 08
      </div>
    );
  }

  if (contract.attachmentPath) {
    return (
      <div className="alert alert-success">
        <h3>? ?ã hoàn thành!</h3>
        <p>File: <strong>{contract.attachmentFileName}</strong></p>
        <a 
          href={`${process.env.NEXT_PUBLIC_API_URL}${contract.attachmentPath}`}
          target="_blank"
          className="btn btn-primary"
        >
          ?? Xem file ?ã upload
        </a>
      </div>
    );
  }

  return (
    <div className="container">
      <h1>?? Upload Cam K?t Thông T? 08</h1>
      
      <div className="alert alert-warning">
        <p>L??ng: <strong>{contract.baseSalary.toLocaleString()} VN?</strong></p>
        <p>H?n cu?i: <strong>
          {new Date(contract.createdAt).addDays(7).toLocaleDateString('vi-VN')}
        </strong></p>
      </div>

      <div className="steps">
        <h3>?? H??ng d?n:</h3>
        <ol>
          <li>
            <button onClick={handleDownload} className="btn btn-secondary">
              ?? T?i file m?u
            </button>
          </li>
          <li>?i?n thông tin vào file Word</li>
          <li>Ký tên và ?óng d?u (n?u có)</li>
          <li>
            <input 
              type="file" 
              accept=".pdf,.doc,.docx"
              onChange={(e) => setFile(e.target.files[0])}
            />
          </li>
        </ol>
      </div>

      <button 
        onClick={handleUpload}
        disabled={!file || uploading}
        className="btn btn-primary"
      >
        {uploading ? '? ?ang upload...' : '?? Upload ngay'}
      </button>
    </div>
  );
}
```

---

## ?? API Calls

### 1. Get User's Contract
```typescript
GET /api/SalaryContracts/user/{userId}
Authorization: Bearer {token}

Response:
{
  "message": "L?y thông tin h?p ??ng thành công",
  "data": {
    "id": 123,
    "userId": 38,
    "baseSalary": 15000000,
    "hasCommitment08": true,
    "attachmentPath": null,  // null = ch?a upload
    "attachmentFileName": null,
    "createdAt": "2024-01-20T10:00:00Z"
  }
}
```

### 2. Download Template
```typescript
GET /api/SalaryContracts/download-commitment08-template

Response: File download (DOCX)
Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document
File name: Mau_Cam_Ket_Thong_Tu_08.docx
```

### 3. Upload File
```typescript
PUT /api/SalaryContracts/{contractId}
Authorization: Bearer {token}
Content-Type: multipart/form-data

FormData:
- Attachment: File

Response:
{
  "message": "C?p nh?t h?p ??ng l??ng thành công",
  "data": {
    "id": 123,
    "attachmentPath": "/uploads/salary-contracts/Nguyen_Van_A_38/abc123.pdf",
    "attachmentFileName": "cam_ket.pdf"
  }
}
```

---

## ?? UI Components (Tailwind CSS)

### Alert Component
```tsx
// components/Alert.tsx
export const Alert = ({ type, children }) => {
  const styles = {
    info: 'bg-blue-100 border-blue-400 text-blue-700',
    warning: 'bg-yellow-100 border-yellow-400 text-yellow-700',
    success: 'bg-green-100 border-green-400 text-green-700',
  };

  return (
    <div className={`border-l-4 p-4 ${styles[type]}`}>
      {children}
    </div>
  );
};
```

### File Upload Component
```tsx
// components/FileUpload.tsx
export const FileUpload = ({ onChange, accept = '.pdf,.doc,.docx' }) => {
  return (
    <label className="block">
      <span className="sr-only">Choose file</span>
      <input
        type="file"
        accept={accept}
        onChange={(e) => onChange(e.target.files[0])}
        className="block w-full text-sm text-gray-500
          file:mr-4 file:py-2 file:px-4
          file:rounded-md file:border-0
          file:text-sm file:font-semibold
          file:bg-blue-50 file:text-blue-700
          hover:file:bg-blue-100"
      />
    </label>
  );
};
```

---

## ?? Environment Variables

```env
# .env.local
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_FRONTEND_URL=http://localhost:3000
```

---

## ? Validation Rules

### Client-side
```typescript
const validateFile = (file: File) => {
  const allowedTypes = [
    'application/pdf',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
  ];
  
  const maxSize = 5 * 1024 * 1024; // 5MB

  if (!allowedTypes.includes(file.type)) {
    throw new Error('File không h?p l?. Ch? ch?p nh?n PDF, DOC, DOCX');
  }

  if (file.size > maxSize) {
    throw new Error('File quá l?n. Kích th??c t?i ?a: 5MB');
  }

  return true;
};
```

---

## ?? Testing

### Test Download
```bash
# Browser
http://localhost:5000/api/SalaryContracts/download-commitment08-template

# cURL
curl -O "http://localhost:5000/api/SalaryContracts/download-commitment08-template"
```

### Test Upload
```bash
curl -X PUT "http://localhost:5000/api/SalaryContracts/123" \
  -H "Authorization: Bearer {token}" \
  -F "Attachment=@test.pdf"
```

---

## ?? Responsive Design

```tsx
// Mobile-first approach
<div className="container mx-auto px-4 py-8">
  <div className="max-w-2xl mx-auto">
    {/* Content */}
  </div>
</div>

// Responsive grid
<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
  {/* Items */}
</div>
```

---

## ?? Error Handling

```typescript
const handleUpload = async () => {
  try {
    // Validate
    validateFile(file);

    setUploading(true);
    const formData = new FormData();
    formData.append('Attachment', file);

    const response = await apiClient.put(
      `/api/SalaryContracts/${contract.id}`,
      formData
    );

    if (response.ok) {
      toast.success('? Upload thành công!');
      router.push('/salary-contracts');
    }
  } catch (error) {
    if (error.response?.status === 400) {
      toast.error(error.response.data.message);
    } else if (error.response?.status === 401) {
      toast.error('B?n không có quy?n upload file này');
    } else {
      toast.error('? Có l?i x?y ra, vui lòng th? l?i');
    }
  } finally {
    setUploading(false);
  }
};
```

---

## ?? User Flow

```
1. Nhân viên nh?n email
   ?
2. Click "Upload Cam k?t Thông t? 08 ngay"
   ?
3. Redirect ??n: {FrontendUrl}/circular-08
   ?
4. H? th?ng load contract c?a user
   ?
5a. N?u hasCommitment08 = false
    ? Hi?n th?: "B?n không c?n ?i?n"
   
5b. N?u ?ã có attachmentPath
    ? Hi?n th?: "?ã hoàn thành" + link xem file
   
5c. N?u ch?a upload
    ? Hi?n th? form upload
   ?
6. User click "T?i file m?u"
   ? Download file DOCX t? backend
   ?
7. User ?i?n thông tin vào Word
   ?
8. User ch?n file và click "Upload ngay"
   ? Call API PUT /api/SalaryContracts/{id}
   ?
9. Backend validate và l?u file
   ?
10. Success ? Hi?n th? thông báo + redirect
```

---

## ?? Security Notes

1. **Authentication:**
   - User ch? xem/upload contract c?a mình
   - Admin có th? xem t?t c?

2. **File Validation:**
   - Client-side: Ki?m tra type và size
   - Server-side: Validate l?i + scan virus (optional)

3. **CORS:**
   ```csharp
   // Backend: Program.cs
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowFrontend", policy =>
       {
           policy.WithOrigins("http://localhost:3000")
                 .AllowAnyMethod()
                 .AllowAnyHeader();
       });
   });
   ```

---

## ?? Support

**Backend APIs:**
- GET `/api/SalaryContracts/user/{userId}` - L?y contract
- GET `/api/SalaryContracts/download-commitment08-template` - Download m?u
- PUT `/api/SalaryContracts/{id}` - Upload file

**Issues?**
- Check browser console for errors
- Verify JWT token is valid
- Check network tab for API responses
- Contact backend team if API returns 500

---

**? Happy coding!**
