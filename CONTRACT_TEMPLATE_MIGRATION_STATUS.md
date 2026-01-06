# ? Tr?ng Thái Migration - Contract Templates

## ?? T?ng Quan

Ph?n **Contracts** ?ã ???c chuy?n ??i thành công ?? s? d?ng templates t? database thay vì ??c t? file HTML t?nh.

---

## ? Các Thay ??i ?ã Hoàn Thành

### 1. ContractsController.cs

#### Ph??ng th?c: `PreviewContract(int id)`
- **Tr??c:** ??c file HTML t? `wwwroot/Templates/generate_contract_*.html`
- **Sau:** L?y template t? database d?a vào `DocumentTemplate.Code`

```csharp
// ? ?Ã CHUY?N ??I
var templateCode = customer.CustomerType?.ToLower() == "individual" 
    ? "CONTRACT_INDIVIDUAL" 
    : "CONTRACT_BUSINESS";

var template = await _context.DocumentTemplates
    .Where(t => t.Code == templateCode && t.IsActive)
    .FirstOrDefaultAsync();

if (template == null)
    return NotFound(new { message = $"Không tìm th?y template h?p ??ng trong database: {templateCode}" });

var htmlContent = BindContractDataToTemplate(template.HtmlContent, contract);
```

#### Ph??ng th?c: `ExportContract(int id)`
- **Tr??c:** ??c file HTML t? `wwwroot/Templates/generate_contract_*.html`
- **Sau:** L?y template t? database t??ng t? nh? `PreviewContract()`

```csharp
// ? ?Ã CHUY?N ??I - S? d?ng cùng logic v?i PreviewContract
var templateCode = customer.CustomerType?.ToLower() == "individual"
    ? "CONTRACT_INDIVIDUAL"
    : "CONTRACT_BUSINESS";

var template = await _context.DocumentTemplates
    .Where(t => t.Code == templateCode && t.IsActive)
    .FirstOrDefaultAsync();
```

---

## ?? Template Mapping

### Contract Individual (Khách hàng Cá nhân)

| Thu?c tính | Giá tr? |
|-----------|---------|
| **Template Code** | `CONTRACT_INDIVIDUAL` |
| **File g?c** | `wwwroot/Templates/generate_contract_individual.html` |
| **?i?u ki?n** | `customer.CustomerType == "individual"` |
| **??c ?i?m** | Dành cho khách hàng cá nhân, có thông tin CMND/CCCD, ngày sinh |

### Contract Business (Khách hàng Doanh nghi?p)

| Thu?c tính | Giá tr? |
|-----------|---------|
| **Template Code** | `CONTRACT_BUSINESS` |
| **File g?c** | `wwwroot/Templates/generate_contract_business.html` |
| **?i?u ki?n** | `customer.CustomerType != "individual"` (m?c ??nh) |
| **??c ?i?m** | Dành cho doanh nghi?p, có thông tin mã s? thu?, ng??i ??i di?n, ngày thành l?p |

---

## ?? Placeholders ???c H? Tr?

### Chung cho C? 2 Template

| Placeholder | Mô t? | Ví d? |
|------------|-------|-------|
| `{{ContractNumber}}` | S? h?p ??ng | 128 |
| `{{NumberContract}}` | Alias cho ContractNumber | 128 |
| `{{ContractYear}}` | N?m t?o h?p ??ng | 2024 |
| `{{Day}}` | Ngày hi?n t?i | 31 |
| `{{Month}}` | Tháng hi?n t?i | 12 |
| `{{Year}}` | N?m hi?n t?i | 2024 |
| `{{ContractDate}}` | Ngày t?o h?p ??ng (dd/MM/yyyy) | 31/12/2024 |
| `{{ExpirationDate}}` | Ngày h?t h?n (dd/MM/yyyy) | 31/12/2025 |
| `{{Location}}` | ??a ?i?m | Hà N?i |
| `{{SubTotal}}` | T?ng ti?n ch?a thu? | 10,000,000 |
| `{{TaxAmount}}` | Ti?n thu? VAT | 1,000,000 |
| `{{TotalAmount}}` | T?ng ti?n sau thu? | 11,000,000 |
| `{{AmountInWords}}` | S? ti?n b?ng ch? | M??i m?t tri?u ??ng ch?n |
| `{{PaymentMethod}}` | Ph??ng th?c thanh toán | Chuy?n kho?n |
| `{{Items}}` | B?ng danh sách d?ch v? | (HTML table) |
| `{{UserName}}` | Tên nhân viên ph? trách | Nguy?n V?n A |
| `{{UserPosition}}` | Ch?c v? nhân viên | Sales Manager |

### Dành Riêng cho Contract Individual

| Placeholder | Mô t? | Ví d? |
|------------|-------|-------|
| `{{CustomerBirthDay}}` | Ngày sinh khách hàng | 15 |
| `{{CustomerBirthMonth}}` | Tháng sinh khách hàng | 5 |
| `{{CustomerBirthYear}}` | N?m sinh khách hàng | 1990 |
| `{{CompanyBRepID}}` | S? CMND/CCCD | 001234567890 |

### Dành Riêng cho Contract Business

| Placeholder | Mô t? | Ví d? |
|------------|-------|-------|
| `{{CompanyBTaxCode}}` | Mã s? thu? | 0123456789 |
| `{{CompanyBRepName}}` | Tên ng??i ??i di?n | Nguy?n V?n B |
| `{{CompanyBRepPosition}}` | Ch?c v? ng??i ??i di?n | Giám ??c |
| `{{CompanyBRepID}}` | CMND/CCCD ng??i ??i di?n | 001234567890 |
| `{{CompanyBEstablishedDate}}` | Ngày thành l?p công ty | 01/01/2020 |
| `{{CompanyBEstablishedDay}}` | Ngày thành l?p (ngày) | 1 |
| `{{CompanyBEstablishedMonth}}` | Ngày thành l?p (tháng) | 1 |
| `{{CompanyBEstablishedYear}}` | Ngày thành l?p (n?m) | 2020 |

---

## ?? Cách S? D?ng

### B??c 1: Ch?y Migration Script

```http
POST /api/DocumentTemplates/migrate-from-files
Authorization: Bearer {admin_token}
```

**K?t qu?:**
- Template `CONTRACT_INDIVIDUAL` ???c t?o t? `generate_contract_individual.html`
- Template `CONTRACT_BUSINESS` ???c t?o t? `generate_contract_business.html`

### B??c 2: Ki?m Tra Templates ?ã Migrate

```http
GET /api/DocumentTemplates
Authorization: Bearer {admin_token}
```

**Ph?n h?i:**
```json
[
  {
    "id": 3,
    "name": "H?p ??ng D?ch V? (Khách hàng Cá nhân)",
    "templateType": "contract",
    "code": "CONTRACT_INDIVIDUAL",
    "isActive": true,
    "isDefault": false,
    "createdAt": "2024-12-31T10:00:00Z"
  },
  {
    "id": 4,
    "name": "H?p ??ng D?ch V? (Khách hàng Doanh nghi?p)",
    "templateType": "contract",
    "code": "CONTRACT_BUSINESS",
    "isActive": true,
    "isDefault": true,
    "createdAt": "2024-12-31T10:00:00Z"
  }
]
```

### B??c 3: Test Preview Contract

```http
GET /api/Contracts/5/preview
Authorization: Bearer {token}
```

**K?t qu?:**
- T? ??ng ch?n template phù h?p d?a vào `CustomerType`
- Tr? v? HTML ?ã ???c bind d? li?u ?? preview trong browser

### B??c 4: Test Export PDF

```http
GET /api/Contracts/5/export-contract
Authorization: Bearer {token}
```

**K?t qu?:**
- Download file PDF v?i tên d?ng `HopDong_5_20241231_123456.pdf`
- PDF ???c l?u vào `wwwroot/Contracts/YYYY/MM/`
- Thông tin PDF ???c l?u vào database:
  - `ContractPdfPath`: ???ng d?n t??ng ??i
  - `PdfGeneratedAt`: Th?i gian t?o
  - `PdfFileSize`: Kích th??c file (bytes)

---

## ?? Flow X? Lý Contract

### Quy Trình T?o H?p ??ng

```
1. Client g?i API: GET /api/Contracts/{id}/export-contract
   ?
2. Controller load Contract v?i ??y ?? relations:
   - User (Position)
   - SaleOrder (Customer, SaleOrderServices, SaleOrderAddons)
   - Services/Addons (Tax)
   ?
3. Xác ??nh CustomerType c?a khách hàng:
   - "individual" ? CONTRACT_INDIVIDUAL
   - Khác ? CONTRACT_BUSINESS
   ?
4. Load template t? database:
   WHERE Code = templateCode AND IsActive = true
   ?
5. Bind d? li?u vào template:
   - Thông tin h?p ??ng c? b?n
   - Thông tin khách hàng
   - B?ng d?ch v?/addons
   - Tính toán t?ng ti?n, thu?
   ?
6. Convert HTML ? PDF b?ng PuppeteerSharp
   ?
7. L?u file PDF vào wwwroot/Contracts/YYYY/MM/
   ?
8. C?p nh?t thông tin PDF vào database
   ?
9. Tr? v? file PDF cho client
```

---

## ?? L?i Ích

### 1. Qu?n Lý T?p Trung
- ? T?t c? templates ???c qu?n lý trong database
- ? Admin có th? ch?nh s?a templates qua API
- ? Không c?n restart application khi thay ??i template

### 2. Tính Linh Ho?t
- ? Có th? t?o nhi?u phiên b?n template cho cùng lo?i h?p ??ng
- ? D? dàng switch gi?a các templates
- ? A/B testing templates

### 3. Version Control
- ? Track ???c ai t?o/s?a template
- ? Track th?i gian thay ??i
- ? Có th? rollback n?u c?n

### 4. Deployment
- ? Không c?n copy files template khi deploy
- ? Ch? c?n migrate database
- ? D? dàng sync gi?a environments

---

## ?? L?u Ý Quan Tr?ng

### 1. Template Files G?c
Các file sau **CÓ TH? XÓA** sau khi migration thành công:
- ? `wwwroot/Templates/generate_contract_individual.html`
- ? `wwwroot/Templates/generate_contract_business.html`

**Khuy?n ngh?:**
- Backup files tr??c khi xóa
- Ki?m tra k? t?t c? ch?c n?ng tr??c khi xóa
- Có th? gi? l?i ?? tham kh?o

### 2. Logic Ch?n Template
```csharp
// Logic quan tr?ng: D?a vào CustomerType
var templateCode = customer.CustomerType?.ToLower() == "individual" 
    ? "CONTRACT_INDIVIDUAL" 
    : "CONTRACT_BUSINESS";
```

**N?u mu?n thêm lo?i template m?i:**
1. T?o template code m?i (VD: `CONTRACT_VIP`)
2. Update logic ch?n template trong `ContractsController`
3. T?o template trong database

### 3. X? Lý L?i
```csharp
if (template == null)
    return NotFound(new { 
        message = $"Không tìm th?y template h?p ??ng trong database: {templateCode}" 
    });
```

**Các tr??ng h?p l?i có th? x?y ra:**
- Template ch?a ???c migrate
- Template b? vô hi?u hóa (IsActive = false)
- Template b? xóa kh?i database

---

## ?? Test Cases

### Test Case 1: Preview Contract (Individual)
**Setup:**
- T?o Contract v?i Customer có `CustomerType = "individual"`
- ??m b?o template `CONTRACT_INDIVIDUAL` t?n t?i và active

**Expected:**
- API tr? v? HTML content
- HTML có ??y ?? thông tin cá nhân (ngày sinh, CMND/CCCD)

### Test Case 2: Preview Contract (Business)
**Setup:**
- T?o Contract v?i Customer có `CustomerType = "business"`
- ??m b?o template `CONTRACT_BUSINESS` t?n t?i và active

**Expected:**
- API tr? v? HTML content
- HTML có ??y ?? thông tin doanh nghi?p (mã s? thu?, ng??i ??i di?n)

### Test Case 3: Export PDF
**Setup:**
- T?o Contract h?p l?
- Template t??ng ?ng t?n t?i

**Expected:**
- Download file PDF thành công
- PDF ???c l?u vào `wwwroot/Contracts/YYYY/MM/`
- Database ???c update v?i `ContractPdfPath`, `PdfGeneratedAt`, `PdfFileSize`

### Test Case 4: Template Not Found
**Setup:**
- Vô hi?u hóa template: `UPDATE document_templates SET IsActive = 0`

**Expected:**
- API tr? v? 404 Not Found
- Message: "Không tìm th?y template h?p ??ng trong database: ..."

### Test Case 5: Regenerate Contract
**Setup:**
- Contract ?ã có PDF c?

**Expected:**
- File PDF c? b? xóa
- T?o file PDF m?i
- Database ???c c?p nh?t v?i thông tin PDF m?i

---

## ?? So Sánh Tr??c/Sau Migration

| Tiêu chí | Tr??c Migration | Sau Migration |
|----------|----------------|---------------|
| **Ngu?n Template** | Files trong `wwwroot/Templates/` | Database table `document_templates` |
| **Ch?nh s?a Template** | S?a file, commit, deploy l?i | G?i API, không c?n deploy |
| **Version Control** | Git (code level) | Database (CreatedAt, UpdatedAt) |
| **Rollback** | Git revert | Update `IsActive` ho?c switch template |
| **Multi-template** | Không h? tr? | D? dàng t?o nhi?u template |
| **Environment Sync** | Copy files | Migrate database |
| **Deployment** | Ph?i copy files | Không c?n (ch? DB migration) |

---

## ?? Next Steps

### 1. Testing
- [ ] Test v?i khách hàng cá nhân
- [ ] Test v?i khách hàng doanh nghi?p
- [ ] Test regenerate PDF
- [ ] Test khi template không t?n t?i

### 2. Documentation
- [ ] C?p nh?t API documentation
- [ ] H??ng d?n admin qu?n lý templates
- [ ] Training cho team

### 3. Optimization (Optional)
- [ ] Cache templates trong memory
- [ ] Implement template versioning
- [ ] Thêm preview template trong admin UI
- [ ] Implement template validation

---

## ?? Support

N?u g?p v?n ??:
1. Ki?m tra logs trong Output window
2. Verify templates ?ã migrate: `GET /api/DocumentTemplates`
3. Verify `IsActive = true` cho templates
4. Check CustomerType c?a Customer
5. Liên h? development team

---

**Last Updated:** 2024-12-31  
**Status:** ? **COMPLETED**  
**Migration Version:** 1.0  
**Controller:** `ContractsController.cs`
