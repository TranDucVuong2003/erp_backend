# ?? API Documentation: Thông T? 08 Template Download

## T?ng quan

Các API này cho phép:
1. ? T?i file m?u Cam k?t Thông t? 08/2019/TT-BTC
2. ? Ki?m tra nhân viên có c?n ?i?n Thông t? 08 không
3. ? H? tr? c? ??nh d?ng HTML và PDF

---

## ?? Danh sách API

### 1. Download File M?u (HTML)

**Endpoint:**
```http
GET /api/SalaryContracts/download-commitment08-template
```

**Authentication:** ? Không b?t bu?c (AllowAnonymous)

**Description:** T?i file m?u Cam k?t Thông t? 08 ??nh d?ng HTML

#### Request Example

**cURL:**
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/download-commitment08-template" \
  --output "Mau_Cam_Ket_Thong_Tu_08.html"
```

**JavaScript:**
```javascript
const downloadTemplate = async () => {
  const response = await fetch('/api/SalaryContracts/download-commitment08-template');
  
  if (response.ok) {
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'Mau_Cam_Ket_Thong_Tu_08.html';
    document.body.appendChild(a);
    a.click();
    a.remove();
  }
};
```

**React Example:**
```jsx
const DownloadCommitment08Button = () => {
  const handleDownload = async () => {
    try {
      const response = await fetch('/api/SalaryContracts/download-commitment08-template');
      
      if (!response.ok) {
        throw new Error('Download failed');
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'Mau_Cam_Ket_Thong_Tu_08.html';
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error downloading file:', error);
      alert('Không th? t?i file m?u');
    }
  };

  return (
    <button onClick={handleDownload}>
      ?? T?i m?u Cam k?t Thông t? 08
    </button>
  );
};
```

#### Success Response (200 OK)

**Response Type:** `text/html`

**Headers:**
```
Content-Type: text/html
Content-Disposition: attachment; filename="Mau_Cam_Ket_Thong_Tu_08.html"
```

**Body:** File HTML ???c t?i v?

#### Error Response (404 Not Found)

```json
{
  "message": "File m?u không t?n t?i"
}
```

---

### 2. Download File M?u (PDF)

**Endpoint:**
```http
GET /api/SalaryContracts/download-commitment08-template-pdf
```

**Authentication:** ? Không b?t bu?c (AllowAnonymous)

**Description:** T?i file m?u Cam k?t Thông t? 08 ??nh d?ng PDF (n?u có)

#### Request Example

**cURL:**
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/download-commitment08-template-pdf" \
  --output "Mau_Cam_Ket_Thong_Tu_08.pdf"
```

**JavaScript:**
```javascript
const downloadPdfTemplate = async () => {
  const response = await fetch('/api/SalaryContracts/download-commitment08-template-pdf');
  
  if (response.ok) {
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'Mau_Cam_Ket_Thong_Tu_08.pdf';
    document.body.appendChild(a);
    a.click();
    a.remove();
  }
};
```

#### Success Response (200 OK)

**Response Type:** `application/pdf`

**Headers:**
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="Mau_Cam_Ket_Thong_Tu_08.pdf"
```

**Body:** File PDF ???c t?i v?

#### Error Response (404 Not Found)

```json
{
  "message": "File m?u PDF không t?n t?i. Vui lòng s? d?ng endpoint HTML"
}
```

---

### 3. Ki?m tra yêu c?u Thông t? 08

**Endpoint:**
```http
GET /api/SalaryContracts/check-commitment08-required/{userId}
```

**Authentication:** ? B?t bu?c (JWT Bearer Token)

**Description:** Ki?m tra nhân viên có c?n ?i?n Cam k?t Thông t? 08 không

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | integer | ? Yes | ID c?a nhân viên |

#### Request Example

**cURL:**
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/check-commitment08-required/38" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
const checkCommitment08Required = async (userId) => {
  const response = await fetch(`/api/SalaryContracts/check-commitment08-required/${userId}`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });

  const result = await response.json();
  return result;
};
```

**React Hook Example:**
```jsx
const useCommitment08Check = (userId) => {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkRequirement = async () => {
      try {
        const response = await fetch(
          `/api/SalaryContracts/check-commitment08-required/${userId}`,
          {
            headers: {
              'Authorization': `Bearer ${getToken()}`,
              'Content-Type': 'application/json'
            }
          }
        );

        const result = await response.json();
        setData(result);
      } catch (error) {
        console.error('Error checking commitment08:', error);
      } finally {
        setLoading(false);
      }
    };

    if (userId) {
      checkRequirement();
    }
  }, [userId]);

  return { data, loading };
};
```

#### Success Response (200 OK)

**Case 1: Nhân viên C?N ?i?n Thông t? 08**
```json
{
  "message": "Ki?m tra thành công",
  "data": {
    "userId": 38,
    "hasCommitment08": true,
    "isRequired": true,
    "baseSalary": 1800000,
    "downloadTemplateUrl": "/api/SalaryContracts/download-commitment08-template",
    "note": "Nhân viên c?n t?i v?, ?i?n và n?p l?i Cam k?t Thông t? 08"
  }
}
```

**Case 2: Nhân viên KHÔNG c?n ?i?n Thông t? 08**
```json
{
  "message": "Ki?m tra thành công",
  "data": {
    "userId": 42,
    "hasCommitment08": false,
    "isRequired": false,
    "baseSalary": 20000000,
    "downloadTemplateUrl": null,
    "note": "Nhân viên không c?n ?i?n Cam k?t Thông t? 08"
  }
}
```

#### Error Response (404 Not Found)

```json
{
  "message": "User ch?a có c?u hình l??ng"
}
```

#### Error Response (500 Internal Server Error)

```json
{
  "message": "Có l?i x?y ra",
  "error": "Detailed error message"
}
```

---

## ?? Business Logic

### Khi nào nhân viên c?n ?i?n Thông t? 08?

Theo **Thông t? 08/2019/TT-BTC**, nhân viên c?n ?i?n Cam k?t khi:

1. ? `HasCommitment08 = true` trong b?ng `SalaryContracts`
2. ? L??ng c? b?n `BaseSalary ? 2,000,000 VN?/tháng` (24 tri?u/n?m)
3. ? Không có thu nh?p t? n?i khác trong n?m d??ng l?ch

### Quy trình th?c t?

```
???????????????????????????????????????????????????????????
? Admin t?o Salary Contract v?i HasCommitment08 = true   ?
???????????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????????
? Frontend g?i API check-commitment08-required            ?
???????????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????????
? N?u isRequired = true, hi?n th? nút Download            ?
???????????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????????
? User click nút ? Download file m?u HTML/PDF             ?
???????????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????????
? User ?i?n thông tin vào file                            ?
???????????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????????
? User upload file ?ã ?i?n qua API (Attachment field)     ?
???????????????????????????????????????????????????????????
```

---

## ?? Frontend Integration Examples

### Example 1: Hi?n th? thông báo và nút download

```jsx
import React, { useState, useEffect } from 'react';

const SalaryContractCommitment08 = ({ userId }) => {
  const [requirement, setRequirement] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkRequirement = async () => {
      try {
        const response = await fetch(
          `/api/SalaryContracts/check-commitment08-required/${userId}`,
          {
            headers: {
              'Authorization': `Bearer ${localStorage.getItem('token')}`,
              'Content-Type': 'application/json'
            }
          }
        );

        const result = await response.json();
        setRequirement(result.data);
      } catch (error) {
        console.error('Error:', error);
      } finally {
        setLoading(false);
      }
    };

    checkRequirement();
  }, [userId]);

  const handleDownload = async () => {
    const response = await fetch('/api/SalaryContracts/download-commitment08-template');
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'Mau_Cam_Ket_Thong_Tu_08.html';
    a.click();
    window.URL.revokeObjectURL(url);
  };

  if (loading) return <div>?ang ki?m tra...</div>;

  if (!requirement?.isRequired) {
    return (
      <div className="alert alert-info">
        <p>?? {requirement?.note}</p>
      </div>
    );
  }

  return (
    <div className="alert alert-warning">
      <h4>?? Yêu c?u ?i?n Cam k?t Thông t? 08</h4>
      <p>{requirement.note}</p>
      <p>L??ng c? b?n: <strong>{requirement.baseSalary.toLocaleString()} VN?</strong></p>
      
      <div className="mt-3">
        <button 
          className="btn btn-primary" 
          onClick={handleDownload}
        >
          ?? T?i m?u Cam k?t Thông t? 08
        </button>
      </div>

      <div className="mt-3 small text-muted">
        <p><strong>H??ng d?n:</strong></p>
        <ol>
          <li>Click nút trên ?? t?i file m?u</li>
          <li>M? file và ?i?n ??y ?? thông tin cá nhân</li>
          <li>In ra 02 b?n, ký tên</li>
          <li>Quay l?i trang này ?? upload file ?ã ký</li>
        </ol>
      </div>
    </div>
  );
};

export default SalaryContractCommitment08;
```

### Example 2: Download button component

```jsx
const DownloadCommitment08Button = ({ format = 'html' }) => {
  const [downloading, setDownloading] = useState(false);

  const handleDownload = async () => {
    setDownloading(true);
    
    try {
      const endpoint = format === 'pdf' 
        ? '/api/SalaryContracts/download-commitment08-template-pdf'
        : '/api/SalaryContracts/download-commitment08-template';

      const response = await fetch(endpoint);
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message);
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Mau_Cam_Ket_Thong_Tu_08.${format}`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);

      // Success notification
      alert('T?i file m?u thành công!');
    } catch (error) {
      console.error('Download error:', error);
      alert(`L?i: ${error.message}`);
    } finally {
      setDownloading(false);
    }
  };

  return (
    <button 
      onClick={handleDownload}
      disabled={downloading}
      className="btn btn-outline-primary"
    >
      {downloading ? (
        <>
          <span className="spinner-border spinner-border-sm me-2" />
          ?ang t?i...
        </>
      ) : (
        <>
          ?? T?i m?u {format.toUpperCase()}
        </>
      )}
    </button>
  );
};

// Usage
<DownloadCommitment08Button format="html" />
<DownloadCommitment08Button format="pdf" />
```

### Example 3: Admin view - Check list nhân viên c?n ?i?n

```jsx
const EmployeeCommitment08List = () => {
  const [employees, setEmployees] = useState([]);

  useEffect(() => {
    const fetchEmployees = async () => {
      // L?y danh sách t?t c? contracts
      const response = await fetch('/api/SalaryContracts', {
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        }
      });

      const result = await response.json();
      
      // Filter nh?ng ng??i c?n ?i?n Thông t? 08
      const needCommitment = result.data.filter(
        contract => contract.hasCommitment08
      );

      setEmployees(needCommitment);
    };

    fetchEmployees();
  }, []);

  return (
    <div>
      <h3>?? Danh sách nhân viên c?n ?i?n Thông t? 08</h3>
      <p className="text-muted">
        {employees.length} nhân viên c?n ?i?n Cam k?t
      </p>

      <table className="table">
        <thead>
          <tr>
            <th>Tên nhân viên</th>
            <th>Email</th>
            <th>L??ng c? b?n</th>
            <th>Tr?ng thái file</th>
            <th>Hành ??ng</th>
          </tr>
        </thead>
        <tbody>
          {employees.map(emp => (
            <tr key={emp.userId}>
              <td>{emp.userName}</td>
              <td>{emp.userEmail}</td>
              <td>{emp.baseSalary.toLocaleString()} VN?</td>
              <td>
                {emp.attachmentPath ? (
                  <span className="badge bg-success">? ?ã n?p</span>
                ) : (
                  <span className="badge bg-warning">?? Ch?a n?p</span>
                )}
              </td>
              <td>
                {emp.attachmentPath ? (
                  <a 
                    href={`http://localhost:5000${emp.attachmentPath}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="btn btn-sm btn-info"
                  >
                    ?? Xem file
                  </a>
                ) : (
                  <span className="text-muted">-</span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
```

---

## ?? File Structure

```
erp_backend/
??? wwwroot/
    ??? templates/
        ??? salary-forms/
            ??? Cam_Ket_Thong_Tu_08.html  ? File m?u HTML
            ??? Cam_Ket_Thong_Tu_08.pdf   ?? Optional - PDF version
```

---

## ?? Security Notes

1. **AllowAnonymous**: 
   - Download endpoints không yêu c?u auth ?? nhân viên m?i có th? t?i
   - N?u mu?n b?o m?t h?n, thêm `[Authorize]` attribute

2. **File Path Validation**:
   - API ch? cho phép t?i file t? th? m?c `wwwroot/templates/salary-forms/`
   - Không có path traversal risk

3. **Rate Limiting**:
   - Nên thêm rate limiting cho download endpoints
   - VD: 10 downloads per minute per IP

---

## ?? N?i dung File M?u

File HTML m?u bao g?m:

? **Header**: Tiêu ?? chính th?c v?i logo qu?c gia
? **Thông tin cá nhân**: Các tr??ng ?? ?i?n (H? tên, CMND, ??a ch?, v.v.)
? **N?i dung cam k?t**: 6 ?i?u kho?n theo ?úng Thông t? 08/2019/TT-BTC
? **Ph?n ký tên**: V? trí ký c?a nhân viên
? **H??ng d?n**: Ph?n note ?? h??ng d?n ng??i ?i?n (?n khi in)
? **Print-ready**: CSS t?i ?u cho in ?n ra gi?y A4

---

## ? Testing Checklist

### Test Case 1: Download HTML Template
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/download-commitment08-template" \
  --output "test.html"

# Verify file downloaded
ls -lh test.html
```

### Test Case 2: Check Commitment08 Required (User có)
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/check-commitment08-required/38" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Expected: isRequired = true
```

### Test Case 3: Check Commitment08 Required (User không có)
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/check-commitment08-required/42" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Expected: isRequired = false
```

### Test Case 4: User không t?n t?i
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/check-commitment08-required/99999" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Expected: 404 - "User ch?a có c?u hình l??ng"
```

---

## ?? Deployment Notes

### Production Checklist

- [ ] Upload file m?u HTML vào server production
- [ ] (Optional) Upload file m?u PDF n?u có
- [ ] Verify ???ng d?n file ?úng trong production environment
- [ ] Test download t? production URL
- [ ] Thêm rate limiting cho download endpoints
- [ ] Monitor download logs
- [ ] Backup file m?u

### File Permissions (Linux)

```bash
chmod 644 wwwroot/templates/salary-forms/Cam_Ket_Thong_Tu_08.html
chown www-data:www-data wwwroot/templates/salary-forms/Cam_Ket_Thong_Tu_08.html
```

---

## ?? Related APIs

| Endpoint | Description |
|----------|-------------|
| `POST /api/SalaryContracts` | T?o contract m?i (set HasCommitment08) |
| `PUT /api/SalaryContracts/{id}` | C?p nh?t contract (upload file ?ã ?i?n) |
| `GET /api/SalaryContracts/user/{userId}` | Xem contract c?a user (check attachmentPath) |

---

**Last Updated**: 2026-01-06  
**Version**: 1.0.0  
**Author**: Development Team
