# ?? Thay ??i File M?u Thông T? 08: HTML ? DOCX

## ?? Thay ??i

### **Tr??c:**
- File: `Cam_Ket_Thong_Tu_08.html`
- Format: HTML
- Content-Type: `text/html`

### **Sau:**
- File: `mau-so-8-mst-tt86.docx` ?
- Format: Microsoft Word (DOCX)
- Content-Type: `application/vnd.openxmlformats-officedocument.wordprocessingml.document`

---

## ? ?ã c?p nh?t trong code

### 1. **Controller Method: `DownloadCommitment08Template()`**

**Thay ??i:**
```csharp
// TR??C
var filePath = Path.Combine(..., "Cam_Ket_Thong_Tu_08.html");
var fileName = "Mau_Cam_Ket_Thong_Tu_08.html";
return File(fileBytes, "text/html", fileName);

// SAU
var filePath = Path.Combine(..., "mau-so-8-mst-tt86.docx"); // ?
var fileName = "Mau_Cam_Ket_Thong_Tu_08.docx"; // ?
var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; // ?
return File(fileBytes, contentType, fileName);
```

### 2. **Response trong `CheckCommitment08Required()`**

**Thêm field m?i:**
```csharp
data = new
{
    // ...existing fields...
    fileFormat = "DOCX", // ? NEW
    note = "Nhân viên c?n t?i v? file DOCX, ?i?n và n?p l?i..." // ? UPDATED
}
```

---

## ?? C?u trúc file hi?n t?i

```
erp_backend/
??? wwwroot/
    ??? templates/
        ??? salary-forms/
            ??? mau-so-8-mst-tt86.docx  ? File m?i (DOCX)
            ??? Cam_Ket_Thong_Tu_08.pdf ?? Optional (PDF)
```

---

## ?? ?i?m khác bi?t HTML vs DOCX

| ??c ?i?m | HTML | DOCX |
|----------|------|------|
| **M? b?ng** | Browser | Microsoft Word, Google Docs |
| **Ch?nh s?a** | Text editor | Word processor |
| **In ?n** | C?n print t? browser | Tr?c ti?p t? Word |
| **Ký tên** | Khó (c?n print r?i scan) | D? (có th? ký ?i?n t? trong Word) |
| **Format** | Có th? b? l?ch | Chu?n, không l?ch |
| **File size** | Nh? (~50KB) | L?n h?n (~20KB+) |

---

## ?? ?u ?i?m c?a DOCX

1. ? **D? ch?nh s?a**: User m? Word và ?i?n tr?c ti?p
2. ? **Format chu?n**: Không b? l?ch khi in
3. ? **Ký ?i?n t?**: Có th? ký b?ng Adobe Sign, DocuSign
4. ? **Professional**: Trông chuyên nghi?p h?n
5. ? **T??ng thích**: M?i ng??i ??u có Word ho?c Google Docs

---

## ?? API Endpoints (Sau khi c?p nh?t)

### 1. Download DOCX Template
```http
GET /api/SalaryContracts/download-commitment08-template
```

**Response:**
- Content-Type: `application/vnd.openxmlformats-officedocument.wordprocessingml.document`
- File: `Mau_Cam_Ket_Thong_Tu_08.docx`

### 2. Check Requirement
```http
GET /api/SalaryContracts/check-commitment08-required/{userId}
```

**Response:**
```json
{
  "message": "Ki?m tra thành công",
  "data": {
    "userId": 38,
    "hasCommitment08": true,
    "isRequired": true,
    "baseSalary": 1800000,
    "downloadTemplateUrl": "/api/SalaryContracts/download-commitment08-template",
    "fileFormat": "DOCX",  // ?? NEW
    "note": "Nhân viên c?n t?i v? file DOCX, ?i?n và n?p l?i..."
  }
}
```

---

## ?? Testing

### Test 1: Download file DOCX
```bash
curl -X GET "http://localhost:5000/api/SalaryContracts/download-commitment08-template" \
  --output "test.docx"

# Verify
file test.docx
# Expected: Microsoft Word 2007+
```

### Test 2: M? file trong Word
1. Double-click `test.docx`
2. File ph?i m? ???c trong Microsoft Word
3. Có th? ch?nh s?a và ?i?n thông tin

### Test 3: Check content type
```bash
curl -I "http://localhost:5000/api/SalaryContracts/download-commitment08-template"

# Expected header:
# Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document
```

---

## ?? Frontend Update (React)

### Old Code:
```jsx
const handleDownload = async () => {
  const response = await fetch('/api/SalaryContracts/download-commitment08-template');
  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'Mau_Cam_Ket_Thong_Tu_08.html'; // ? OLD
  a.click();
};
```

### New Code:
```jsx
const handleDownload = async () => {
  const response = await fetch('/api/SalaryContracts/download-commitment08-template');
  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'Mau_Cam_Ket_Thong_Tu_08.docx'; // ? NEW
  a.click();
  window.URL.revokeObjectURL(url);
};
```

### Display file format info:
```jsx
const Commitment08Download = ({ userId }) => {
  const [requirement, setRequirement] = useState(null);

  // ...fetch logic...

  return (
    <div>
      {requirement?.isRequired && (
        <>
          <p>?? File format: <strong>{requirement.fileFormat}</strong></p>
          <button onClick={handleDownload}>
            T?i m?u Cam k?t (.docx)
          </button>
          <small>
            T?i v? ? M? b?ng Word ? ?i?n thông tin ? L?u & Upload
          </small>
        </>
      )}
    </div>
  );
};
```

---

## ?? L?u ý quan tr?ng

### 1. **File extension validation**
??m b?o `.docx` ???c ch?p nh?n khi user upload l?i:
```csharp
private readonly string[] _allowedExtensions = { 
  ".pdf", ".doc", ".docx", // ? docx ?ã có
  ".jpg", ".jpeg", ".png" 
};
```

### 2. **Xóa file HTML c? (optional)**
```bash
# N?u mu?n d?n d?p
rm wwwroot/templates/salary-forms/Cam_Ket_Thong_Tu_08.html
```

### 3. **Backup file DOCX**
```bash
# T?o backup
cp wwwroot/templates/salary-forms/mau-so-8-mst-tt86.docx \
   wwwroot/templates/salary-forms/mau-so-8-mst-tt86.docx.backup
```

---

## ?? Quy trình m?i cho User

```
1. Admin t?o contract v?i HasCommitment08 = true
   ?
2. User check qua API ? th?y fileFormat = "DOCX"
   ?
3. User download file .docx
   ?
4. User m? file b?ng Microsoft Word/Google Docs
   ?
5. User ?i?n thông tin tr?c ti?p trong Word
   ?
6. User save file (ho?c Save as PDF)
   ?
7. User upload file ?ã ?i?n qua API
   ?
8. Admin xem file ?ã upload
```

---

## ?? So sánh v?i HTML

| B??c | HTML (C?) | DOCX (M?i) |
|------|-----------|------------|
| Download | T?i HTML | T?i DOCX |
| M? file | Browser | Word/Google Docs |
| ?i?n form | S?a HTML code ho?c in r?i vi?t tay | ?i?n tr?c ti?p trong Word |
| L?u | Ph?i print ? scan | Save as PDF ho?c gi? DOCX |
| Upload | Upload scan/PDF | Upload DOCX ho?c PDF |

**?? DOCX d? h?n và chuyên nghi?p h?n!**

---

## ? Build Status

```
? Code updated
? No compilation errors
? Build successful
? Ready to test
```

---

## ?? Related Files

- Controller: `erp_backend/Controllers/SalaryContractsController.cs` ? Updated
- Template: `erp_backend/wwwroot/templates/salary-forms/mau-so-8-mst-tt86.docx` ? New
- Documentation: C?n update `COMMITMENT08_TEMPLATE_API_DOCUMENTATION.md`

---

## ?? Next Steps

1. ? ??t file `mau-so-8-mst-tt86.docx` vào `wwwroot/templates/salary-forms/`
2. ? Test download API
3. ? Test m? file trong Word
4. ? Update frontend ?? download .docx thay vì .html
5. ? Update documentation files

---

**Updated**: 2026-01-06  
**Status**: ? Ready for testing  
**Breaking Change**: ? No (backward compatible - ch? ??i file, API gi? nguyên endpoint)
