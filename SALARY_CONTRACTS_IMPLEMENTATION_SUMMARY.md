# 📋 Tóm tắt: Thêm chức năng Upload File cho Salary Contracts

## ✅ Đã hoàn thành

### 1. Cập nhật Model `SalaryContracts`
- ✅ Thêm trường `AttachmentPath` (string, max 500 ký tự)
- ✅ Thêm trường `AttachmentFileName` (string, max 255 ký tự)
- 📁 File: `erp_backend\Models\SalaryContracts.cs`

### 2. Tạo DTOs
- ✅ `CreateSalaryContractDto` - Để tạo mới (có IFormFile? Attachment)
- ✅ `UpdateSalaryContractDto` - Để cập nhật (có IFormFile? Attachment)
- ✅ `SalaryContractResponseDto` - Để trả về response
- 📁 File: `erp_backend\Models\DTOs\SalaryContractDtos.cs`

### 3. Tạo FileUploadService
- ✅ Interface `IFileUploadService`
- ✅ Implementation với các methods:
  - `SaveFileAsync()` - Lưu file vào wwwroot/uploads
  - `DeleteFileAsync()` - Xóa file
  - `GetFileUrl()` - Lấy URL của file
  - `IsValidFileExtension()` - Validate extension
  - `IsValidFileSize()` - Validate kích thước
- 📁 File: `erp_backend\Services\FileUploadService.cs`

### 4. Đăng ký Service trong DI Container
- ✅ Thêm `builder.Services.AddScoped<IFileUploadService, FileUploadService>();`
- 📁 File: `erp_backend\Program.cs`

### 5. Cập nhật Controller
- ✅ Inject `IFileUploadService` vào constructor
- ✅ Thêm `[Consumes("multipart/form-data")]` cho POST và PUT
- ✅ Validation file (extension + size) trước khi upload
- ✅ Auto xóa file cũ khi upload file mới
- ✅ Auto xóa file khi delete contract
- ✅ Thêm endpoints mới:
  - `GET /api/SalaryContracts/user/{userId}` - Lấy contract theo UserId
  - `GET /api/SalaryContracts` - Lấy tất cả contracts
  - `DELETE /api/SalaryContracts/{id}` - Xóa contract
- 📁 File: `erp_backend\Controllers\SalaryContractsController.cs`

### 6. Migration Database
- ✅ Tạo migration `AddAttachmentToSalaryContracts`
- ✅ Apply migration vào database
- ✅ Thêm 2 cột mới:
  - `AttachmentFileName` (varchar 255, nullable)
  - `AttachmentPath` (varchar 500, nullable)

### 7. Tạo thư mục lưu file
- ✅ Tạo thư mục: `wwwroot/uploads/salary-contracts`
- 📂 Cấu trúc: `wwwroot/uploads/salary-contracts/{userId}/{guid}.ext`

### 8. Documentation
- ✅ Tạo file hướng dẫn chi tiết API
- 📁 File: `erp_backend\SALARY_CONTRACTS_FILE_UPLOAD_API.md`

---

## 🎯 Tính năng chính

### File Upload
- **Định dạng chấp nhận**: PDF, DOC, DOCX, JPG, JPEG, PNG
- **Kích thước tối đa**: 5MB
- **Lưu trữ**: Local tại `wwwroot/uploads/salary-contracts/{userId}/`
- **Tên file**: Tự động generate GUID để tránh trùng lặp

### Validation
- ✅ Kiểm tra file extension
- ✅ Kiểm tra file size
- ✅ Kiểm tra User tồn tại
- ✅ Kiểm tra duplicate contract (1 user = 1 contract)

### Auto Cleanup
- ✅ Xóa file cũ khi upload file mới
- ✅ Xóa file khi xóa contract

---

## 📡 API Endpoints

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/SalaryContracts` | Tạo contract mới (có file) |
| GET | `/api/SalaryContracts/{id}` | Lấy contract theo ID |
| GET | `/api/SalaryContracts/user/{userId}` | Lấy contract theo UserId |
| GET | `/api/SalaryContracts` | Lấy tất cả contracts |
| PUT | `/api/SalaryContracts/{id}` | Cập nhật contract (có file mới) |
| DELETE | `/api/SalaryContracts/{id}` | Xóa contract |

---

## 🧪 Testing với Postman

### 1. Tạo contract với file
```
POST http://localhost:5000/api/SalaryContracts
Content-Type: multipart/form-data
Authorization: Bearer {token}

Body (form-data):
- UserId: 1
- BaseSalary: 20000000
- InsuranceSalary: 0
- ContractType: OFFICIAL
- DependentsCount: 2
- HasCommitment08: false
- Attachment: [Chọn file PDF/DOC]
```

### 2. Cập nhật với file mới
```
PUT http://localhost:5000/api/SalaryContracts/1
Content-Type: multipart/form-data
Authorization: Bearer {token}

Body (form-data):
- BaseSalary: 25000000
- Attachment: [Chọn file mới]
```

### 3. Xem file đã upload
```
GET http://localhost:5000/uploads/salary-contracts/1/{filename}
```

---

## 🔧 Cấu hình

### Thay đổi kích thước file tối đa
Mở `Program.cs` và sửa:
```csharp
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
```

### Thay đổi file extensions cho phép
Mở `SalaryContractsController.cs` và sửa:
```csharp
private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xlsx" };
```

### Thay đổi kích thước tối đa per file
Mở `SalaryContractsController.cs` và sửa:
```csharp
private readonly long _maxFileSizeInMB = 5; // 5MB
```

---

## 📊 Database Schema

### Bảng: SalaryContracts

| Column | Type | Nullable | Mô tả |
|--------|------|----------|-------|
| Id | int | No | Primary Key |
| UserId | int | No | Foreign Key to Users |
| BaseSalary | decimal(18,0) | No | Lương cơ bản |
| InsuranceSalary | decimal(18,0) | No | Lương đóng BH |
| ContractType | varchar | No | OFFICIAL/FREELANCE |
| DependentsCount | int | No | Số người phụ thuộc |
| HasCommitment08 | bool | No | Có cam kết 08? |
| **AttachmentPath** | **varchar(500)** | **Yes** | **Đường dẫn file** |
| **AttachmentFileName** | **varchar(255)** | **Yes** | **Tên file gốc** |
| CreatedAt | timestamp | No | Ngày tạo |
| UpdatedAt | timestamp | Yes | Ngày cập nhật |

---

## 🚀 Frontend Integration (React)

### Upload file
```javascript
const uploadContract = async (formData) => {
  const data = new FormData();
  data.append('UserId', formData.userId);
  data.append('BaseSalary', formData.baseSalary);
  data.append('InsuranceSalary', formData.insuranceSalary);
  data.append('ContractType', formData.contractType);
  data.append('DependentsCount', formData.dependentsCount);
  data.append('HasCommitment08', formData.hasCommitment08);
  data.append('Attachment', formData.file); // File object from input

  const response = await fetch('/api/SalaryContracts', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: data
  });

  return await response.json();
};
```

### Display attachment
```jsx
{contract.attachmentPath && (
  <a 
    href={`${API_URL}${contract.attachmentPath}`}
    target="_blank"
    rel="noopener noreferrer"
    download={contract.attachmentFileName}
  >
    📎 Tải xuống: {contract.attachmentFileName}
  </a>
)}
```

---

## ⚠️ Lưu ý quan trọng

1. **Security**: 
   - Tất cả API đều yêu cầu JWT authentication
   - File được validate extension và size trước khi upload
   - File được lưu với tên GUID để tránh path traversal attack

2. **Performance**:
   - File lưu local tại `wwwroot/uploads`
   - Nếu scale lên production, nên chuyển sang:
     - Azure Blob Storage
     - AWS S3
     - Google Cloud Storage

3. **Permissions**:
   - Đảm bảo thư mục `wwwroot/uploads/salary-contracts` có quyền ghi
   - Windows: Grant quyền cho IIS_IUSRS
   - Linux: chmod 755

4. **Backup**:
   - Nên backup thư mục `wwwroot/uploads` định kỳ
   - Khi restore database, cần restore cả thư mục uploads

---

## 📝 Next Steps (Optional)

### Nâng cao thêm nếu cần:

1. **Multiple Files Upload**
   - Cho phép upload nhiều file (hợp đồng chính + phụ lục)
   - Thêm bảng `SalaryContractAttachments` riêng

2. **File Preview**
   - Thêm API generate thumbnail cho PDF/Images
   - Thêm PDF viewer trong frontend

3. **Cloud Storage**
   - Migrate sang Azure Blob Storage
   - Thêm Azure Storage Service

4. **Audit Log**
   - Log lại khi nào file được upload/delete
   - Ai thực hiện thao tác

5. **Compression**
   - Tự động compress file lớn trước khi lưu
   - Sử dụng thư viện như ImageSharp cho images

---

## 🎉 Kết luận

Bạn đã có đầy đủ chức năng upload file attachment cho Salary Contracts với:
- ✅ Backend API hoàn chỉnh
- ✅ Database đã update
- ✅ Validation đầy đủ
- ✅ Auto cleanup files
- ✅ Documentation chi tiết
- ✅ Build successful

**Có thể test ngay trên Postman hoặc tích hợp vào frontend!**
