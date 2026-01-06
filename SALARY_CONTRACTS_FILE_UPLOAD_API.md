# API Documentation: Salary Contracts with File Upload

## Tổng quan
API này cho phép quản lý hợp đồng lương của nhân viên với khả năng upload file đính kèm (hợp đồng lao động, thông tư, cam kết, v.v.)

## Base URL
```
http://localhost:5000/api/SalaryContracts
```

---

## 1. Tạo mới Salary Contract (với file đính kèm)

### Endpoint
```http
POST /api/SalaryContracts
```

### Headers
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

### Request Body (Form-Data)
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| UserId | int | Yes | ID của nhân viên |
| BaseSalary | decimal | Yes | Lương cơ bản (VD: 20000000) |
| InsuranceSalary | decimal | Yes | Lương đóng bảo hiểm (0 = tự động tính) |
| ContractType | string | Yes | Loại hợp đồng: "OFFICIAL" hoặc "FREELANCE" |
| DependentsCount | int | No | Số người phụ thuộc (0-20) |
| HasCommitment08 | bool | No | Có cam kết 08 không? |
| Attachment | file | No | File đính kèm (PDF, DOC, DOCX, JPG, PNG - Max 5MB) |

### Example Request (Postman)
1. Chọn method: **POST**
2. URL: `http://localhost:5000/api/SalaryContracts`
3. Headers:
   - Key: `Authorization` - Value: `Bearer your_jwt_token`
4. Body:
   - Chọn **form-data**
   - Thêm các fields:
     ```
     UserId: 1
     BaseSalary: 20000000
     InsuranceSalary: 0
     ContractType: OFFICIAL
     DependentsCount: 2
     HasCommitment08: false
     Attachment: [Chọn file từ máy tính]
     ```

### Example Response (Success)
```json
{
  "message": "Tạo hợp đồng lương thành công",
  "data": {
    "id": 1,
    "userId": 1,
    "baseSalary": 20000000,
    "insuranceSalary": 5682000,
    "contractType": "OFFICIAL",
    "dependentsCount": 2,
    "hasCommitment08": false,
    "attachmentPath": "/uploads/salary-contracts/1/a1b2c3d4-e5f6-7890-1234-567890abcdef.pdf",
    "attachmentFileName": "hop_dong_lao_dong.pdf",
    "createdAt": "2026-01-05T09:15:00Z",
    "updatedAt": null,
    "userName": "Nguyễn Văn A",
    "userEmail": "nguyenvana@example.com"
  }
}
```

### Error Responses
```json
// User không tồn tại
{
  "message": "User không tồn tại"
}

// User đã có hợp đồng
{
  "message": "User này đã có Salary Contract",
  "existingContractId": 1,
  "hint": "Sử dụng PUT /api/SalaryContracts/{id} để cập nhật"
}

// File không hợp lệ
{
  "message": "File không hợp lệ. Chỉ chấp nhận: .pdf, .doc, .docx, .jpg, .jpeg, .png"
}

// File quá lớn
{
  "message": "File quá lớn. Kích thước tối đa: 5MB"
}
```

---

## 2. Lấy thông tin Salary Contract theo ID

### Endpoint
```http
GET /api/SalaryContracts/{id}
```

### Example Request
```http
GET http://localhost:5000/api/SalaryContracts/1
Authorization: Bearer {token}
```

### Example Response
```json
{
  "message": "Lấy thông tin hợp đồng thành công",
  "data": {
    "id": 1,
    "userId": 1,
    "baseSalary": 20000000,
    "insuranceSalary": 5682000,
    "contractType": "OFFICIAL",
    "dependentsCount": 2,
    "hasCommitment08": false,
    "attachmentPath": "/uploads/salary-contracts/1/a1b2c3d4-e5f6-7890-1234-567890abcdef.pdf",
    "attachmentFileName": "hop_dong_lao_dong.pdf",
    "createdAt": "2026-01-05T09:15:00Z",
    "updatedAt": null,
    "userName": "Nguyễn Văn A",
    "userEmail": "nguyenvana@example.com"
  }
}
```

---

## 3. Lấy Salary Contract theo UserId

### Endpoint
```http
GET /api/SalaryContracts/user/{userId}
```

### Example Request
```http
GET http://localhost:5000/api/SalaryContracts/user/1
Authorization: Bearer {token}
```

---

## 4. Lấy tất cả Salary Contracts

### Endpoint
```http
GET /api/SalaryContracts
```

### Example Request
```http
GET http://localhost:5000/api/SalaryContracts
Authorization: Bearer {token}
```

### Example Response
```json
{
  "message": "Lấy danh sách hợp đồng thành công",
  "data": [
    {
      "id": 1,
      "userId": 1,
      "baseSalary": 20000000,
      "insuranceSalary": 5682000,
      "contractType": "OFFICIAL",
      "dependentsCount": 2,
      "hasCommitment08": false,
      "attachmentPath": "/uploads/salary-contracts/1/a1b2c3d4-e5f6-7890-1234-567890abcdef.pdf",
      "attachmentFileName": "hop_dong_lao_dong.pdf",
      "createdAt": "2026-01-05T09:15:00Z",
      "updatedAt": null,
      "userName": "Nguyễn Văn A",
      "userEmail": "nguyenvana@example.com"
    }
  ],
  "total": 1
}
```

---

## 5. Cập nhật Salary Contract (với file mới)

### Endpoint
```http
PUT /api/SalaryContracts/{id}
```

### Headers
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

### Request Body (Form-Data)
Tất cả các fields đều **optional**, chỉ gửi những field cần cập nhật:

| Field | Type | Description |
|-------|------|-------------|
| BaseSalary | decimal | Lương cơ bản mới |
| InsuranceSalary | decimal | Lương bảo hiểm mới |
| ContractType | string | Loại hợp đồng mới |
| DependentsCount | int | Số người phụ thuộc mới |
| HasCommitment08 | bool | Trạng thái cam kết 08 mới |
| Attachment | file | File đính kèm mới (sẽ xóa file cũ) |

### Example Request (Postman)
1. Method: **PUT**
2. URL: `http://localhost:5000/api/SalaryContracts/1`
3. Headers: `Authorization: Bearer your_token`
4. Body (form-data):
   ```
   BaseSalary: 25000000
   Attachment: [Chọn file mới]
   ```

### Example Response
```json
{
  "message": "Cập nhật hợp đồng lương thành công",
  "data": {
    "id": 1,
    "userId": 1,
    "baseSalary": 25000000,
    "insuranceSalary": 5682000,
    "contractType": "OFFICIAL",
    "dependentsCount": 2,
    "hasCommitment08": false,
    "attachmentPath": "/uploads/salary-contracts/1/new-file-guid.pdf",
    "attachmentFileName": "hop_dong_moi.pdf",
    "createdAt": "2026-01-05T09:15:00Z",
    "updatedAt": "2026-01-05T10:30:00Z",
    "userName": "Nguyễn Văn A",
    "userEmail": "nguyenvana@example.com"
  }
}
```

**Lưu ý:** Khi upload file mới, file cũ sẽ tự động bị xóa khỏi server.

---

## 6. Xóa Salary Contract

### Endpoint
```http
DELETE /api/SalaryContracts/{id}
```

### Example Request
```http
DELETE http://localhost:5000/api/SalaryContracts/1
Authorization: Bearer {token}
```

### Example Response
```json
{
  "message": "Xóa hợp đồng lương thành công"
}
```

**Lưu ý:** File đính kèm cũng sẽ bị xóa khỏi server.

---

## 7. Tải file đính kèm

### URL Format
```
http://localhost:5000{attachmentPath}
```

### Example
Nếu `attachmentPath = "/uploads/salary-contracts/1/abc123.pdf"`, thì URL để tải file là:
```
http://localhost:5000/uploads/salary-contracts/1/abc123.pdf
```

---

## Testing với cURL

### Tạo contract với file
```bash
curl -X POST http://localhost:5000/api/SalaryContracts \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "UserId=1" \
  -F "BaseSalary=20000000" \
  -F "InsuranceSalary=0" \
  -F "ContractType=OFFICIAL" \
  -F "DependentsCount=2" \
  -F "HasCommitment08=false" \
  -F "Attachment=@/path/to/file.pdf"
```

### Cập nhật với file mới
```bash
curl -X PUT http://localhost:5000/api/SalaryContracts/1 \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "BaseSalary=25000000" \
  -F "Attachment=@/path/to/new-file.pdf"
```

---

## Frontend Integration (React Example)

### Upload file với axios
```javascript
const handleCreateContract = async (formData) => {
  const data = new FormData();
  data.append('UserId', formData.userId);
  data.append('BaseSalary', formData.baseSalary);
  data.append('InsuranceSalary', formData.insuranceSalary);
  data.append('ContractType', formData.contractType);
  data.append('DependentsCount', formData.dependentsCount);
  data.append('HasCommitment08', formData.hasCommitment08);
  
  if (formData.attachment) {
    data.append('Attachment', formData.attachment);
  }

  try {
    const response = await axios.post('/api/SalaryContracts', data, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'multipart/form-data'
      }
    });
    console.log('Success:', response.data);
  } catch (error) {
    console.error('Error:', error.response.data);
  }
};
```

### Display attachment link
```jsx
{contract.attachmentPath && (
  <a 
    href={`${API_BASE_URL}${contract.attachmentPath}`} 
    target="_blank" 
    rel="noopener noreferrer"
    download={contract.attachmentFileName}
  >
    📎 {contract.attachmentFileName}
  </a>
)}
```

---

## File Validation Rules

### Allowed Extensions
- `.pdf` - Portable Document Format
- `.doc` - Microsoft Word (old)
- `.docx` - Microsoft Word
- `.jpg`, `.jpeg` - Image formats
- `.png` - Image format

### Max File Size
**5MB** (5,242,880 bytes)

### Storage Location
```
wwwroot/uploads/salary-contracts/{userId}/{unique-filename}
```

---

## Security Notes

1. ✅ **Authentication Required**: Tất cả endpoints đều yêu cầu JWT token hợp lệ
2. ✅ **File Validation**: Kiểm tra extension và file size trước khi upload
3. ✅ **Unique Filename**: Sử dụng GUID để tránh trùng lặp và bảo mật
4. ✅ **Auto Cleanup**: File cũ tự động xóa khi upload file mới hoặc xóa contract
5. ⚠️ **Permissions**: Đảm bảo folder `wwwroot/uploads/salary-contracts` có quyền ghi

---

## Troubleshooting

### Lỗi "File quá lớn"
- Kiểm tra `FormOptions.MultipartBodyLengthLimit` trong `Program.cs`
- Mặc định: 10MB (có thể tăng nếu cần)

### Lỗi "Cannot write to directory"
```bash
# Windows
icacls "wwwroot\uploads\salary-contracts" /grant "IIS_IUSRS:(OI)(CI)F"

# Linux
chmod 755 wwwroot/uploads/salary-contracts
```

### File không hiển thị
- Kiểm tra `app.UseStaticFiles()` đã được gọi trong `Program.cs`
- Đảm bảo đường dẫn bắt đầu bằng `/uploads/...`

---

## Change Log

### Version 1.0 (2026-01-05)
- ✅ Thêm support upload file attachment
- ✅ Thêm validation file extension và size
- ✅ Tự động xóa file cũ khi update/delete
- ✅ Thêm DTOs riêng biệt cho request/response
- ✅ Thêm endpoints GET by UserId và GET all
