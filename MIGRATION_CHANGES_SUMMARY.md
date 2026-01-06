# Tóm T?t Các Thay ??i - Template Migration

## ?? M?c ?ích
Chuy?n ??i h? th?ng t? s? d?ng template files hardcoded sang qu?n lý templates trong database ??:
- D? dàng ch?nh s?a templates qua UI
- Version control cho templates
- Qu?n lý t?p trung t?t c? templates
- Không c?n deploy l?i khi thay ??i templates

## ? Các File ?ã ???c C?p Nh?t

### 1. Controllers

#### ? QuotesController.cs (COMPLETED)
- **Ph??ng th?c:** `PreviewQuote()`, `ExportQuotePdf()`
- **Thay ??i:** 
  - Tr??c: ??c template t? file `wwwroot/Templates/QuoteTemplate.html`
  - Sau: L?y template t? database v?i code `QUOTE_DEFAULT`
  ```csharp
  // Tr??c
  var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "QuoteTemplate.html");
  var htmlTemplate = await File.ReadAllTextAsync(templatePath);
  
  // Sau
  var template = await _context.DocumentTemplates
      .Where(t => t.Code == "QUOTE_DEFAULT" && t.IsActive)
      .FirstOrDefaultAsync();
  var htmlContent = BindQuoteDataToTemplate(template.HtmlContent, quote);
  ```

#### ? ContractsController.cs (COMPLETED)
- **Ph??ng th?c:** `PreviewContract()`, `ExportContract()`
- **Thay ??i:**
  - Tr??c: ??c template t? file `generate_contract_individual.html` ho?c `generate_contract_business.html`
  - Sau: L?y template t? database v?i code `CONTRACT_INDIVIDUAL` ho?c `CONTRACT_BUSINESS`
  ```csharp
  // Tr??c
  var templateFileName = customer.CustomerType?.ToLower() == "individual" 
      ? "generate_contract_individual.html" 
      : "generate_contract_business.html";
  var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", templateFileName);
  var htmlTemplate = await File.ReadAllTextAsync(templatePath);
  
  // Sau
  var templateCode = customer.CustomerType?.ToLower() == "individual" 
      ? "CONTRACT_INDIVIDUAL" 
      : "CONTRACT_BUSINESS";
  var template = await _context.DocumentTemplates
      .Where(t => t.Code == templateCode && t.IsActive)
      .FirstOrDefaultAsync();
  var htmlContent = BindContractDataToTemplate(template.HtmlContent, contract);
  ```

### 2. Services

#### ? SalaryReportService.cs (COMPLETED)
- **Ph??ng th?c:** `GenerateHtmlFromTemplateAsync()`
- **Thay ??i:**
  - Tr??c: ??c template t? file `wwwroot/Templates/SalaryReportTemplate.html`
  - Sau: L?y template t? database v?i code `SALARY_REPORT_DEFAULT`
  ```csharp
  // Tr??c
  var templatePath = Path.Combine(_env.WebRootPath, "Templates", "SalaryReportTemplate.html");
  var htmlTemplate = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
  
  // Sau
  var template = await _context.DocumentTemplates
      .Where(t => t.Code == "SALARY_REPORT_DEFAULT" && t.IsActive)
      .FirstOrDefaultAsync();
  var htmlContent = ReplaceTemplatePlaceholders(template.HtmlContent, data);
  ```

## ?? Tr?ng Thái Migration

| Component | Template Code | Status | Notes |
|-----------|--------------|--------|-------|
| **Quotes** | `QUOTE_DEFAULT` | ? COMPLETED | Preview + Export PDF ?ã chuy?n ??i |
| **Contracts (Individual)** | `CONTRACT_INDIVIDUAL` | ? COMPLETED | Preview + Export PDF ?ã chuy?n ??i |
| **Contracts (Business)** | `CONTRACT_BUSINESS` | ? COMPLETED | Preview + Export PDF ?ã chuy?n ??i |
| **Salary Reports** | `SALARY_REPORT_DEFAULT` | ? COMPLETED | Preview + Export PDF ?ã chuy?n ??i |
| **Email Templates** | `EMAIL_*` | ? MIGRATED | ?ã migrate nh?ng ch?a áp d?ng (EmailService v?n dùng hardcode) |

## ??? Các File Template CÓ TH? XÓA (SAU KHI MIGRATE)

**?? L?U Ý:** Ch? xóa các file này SAU KHI:
1. ?ã ch?y migration script thành công
2. ?ã ki?m tra t?t c? templates ho?t ??ng ?úng
3. ?ã backup toàn b? database

### ? ?ã Migrate và CÓ TH? XÓA:
```
erp_backend/wwwroot/Templates/
??? QuoteTemplate.html                    ? CÓ TH? XÓA (?ã migrate -> QUOTE_DEFAULT)
??? SalaryReportTemplate.html             ? CÓ TH? XÓA (?ã migrate -> SALARY_REPORT_DEFAULT)
??? generate_contract_individual.html     ? CÓ TH? XÓA (?ã migrate -> CONTRACT_INDIVIDUAL)
??? generate_contract_business.html       ? CÓ TH? XÓA (?ã migrate -> CONTRACT_BUSINESS)
??? Email_AccountCreation.html            ?? ?Ã MIGRATE nh?ng ch?a áp d?ng (EmailService ch?a dùng DB)
??? Email_PasswordResetOTP.html           ?? ?Ã MIGRATE nh?ng ch?a áp d?ng (EmailService ch?a dùng DB)
??? Email_Notification.html               ?? ?Ã MIGRATE nh?ng ch?a áp d?ng (EmailService ch?a dùng DB)
??? Email_PaymentSuccess.html             ?? ?Ã MIGRATE nh?ng ch?a áp d?ng (EmailService ch?a dùng DB)
```

### ? Files KHÔNG XÓA:
```
erp_backend/wwwroot/Templates/assets/     ? GI? L?I (các file CSS, images, etc.)
```

## ?? Quy Trình Migration

### B??c 1: Ch?y Migration Script
```http
POST /api/DocumentTemplates/migrate-from-files
Authorization: Bearer {admin_token}
```

### B??c 2: Ki?m Tra Templates ?ã Migrate
```http
GET /api/DocumentTemplates
Authorization: Bearer {admin_token}
```

Ki?m tra các template code sau ?ã t?n t?i:
- ? `QUOTE_DEFAULT`
- ? `SALARY_REPORT_DEFAULT`
- ? `CONTRACT_INDIVIDUAL`
- ? `CONTRACT_BUSINESS`
- ?? `EMAIL_ACCOUNT_CREATION` (?ã migrate nh?ng ch?a s? d?ng)
- ?? `EMAIL_PASSWORD_RESET_OTP` (?ã migrate nh?ng ch?a s? d?ng)
- ?? `EMAIL_NOTIFICATION` (?ã migrate nh?ng ch?a s? d?ng)
- ?? `EMAIL_PAYMENT_SUCCESS` (?ã migrate nh?ng ch?a s? d?ng)

### B??c 3: Test Các Ch?c N?ng

#### ? Test Báo Giá
```http
# Preview
GET /api/Quotes/5/preview
Authorization: Bearer {token}

# Export PDF
GET /api/Quotes/5/export-pdf
Authorization: Bearer {token}
```

#### ? Test H?p ??ng
```http
# Preview (Individual)
GET /api/Contracts/5/preview
Authorization: Bearer {token}

# Preview (Business)
GET /api/Contracts/6/preview
Authorization: Bearer {token}

# Export PDF
GET /api/Contracts/5/export-contract
Authorization: Bearer {token}
```

#### ? Test Báo Cáo L??ng
```http
# Preview
POST /api/Payslips/preview-salary-report
Authorization: Bearer {token}
Content-Type: application/json

{
  "month": 12,
  "year": 2024,
  "departmentId": null,
  "createdByName": "Admin"
}

# Export PDF
POST /api/Payslips/export-salary-report
Authorization: Bearer {token}
Content-Type: application/json

{
  "month": 12,
  "year": 2024,
  "departmentId": null,
  "createdByName": "Admin"
}
```

### B??c 4: Backup Tr??c Khi Xóa
```bash
# Backup toàn b? th? m?c Templates
cp -r wwwroot/Templates wwwroot/Templates.backup

# Ho?c t?o archive
tar -czf templates_backup_$(date +%Y%m%d).tar.gz wwwroot/Templates/
```

### B??c 5: Xóa Template Files (TÙY CH?N)
```bash
# Xóa các template ?Ã CHUY?N ??I thành công
rm wwwroot/Templates/QuoteTemplate.html
rm wwwroot/Templates/SalaryReportTemplate.html
rm wwwroot/Templates/generate_contract_individual.html
rm wwwroot/Templates/generate_contract_business.html

# ?? CH?A XÓA các email templates (EmailService ch?a chuy?n ??i)
# rm wwwroot/Templates/Email_*.html  # KHÔNG XÓA
```

## ?? Rollback Plan

N?u g?p v?n ??, có th? rollback b?ng cách:

### Option 1: Revert Code Changes
```bash
git revert <commit-hash>
```

### Option 2: Restore Template Files
```bash
# Restore t? backup
cp -r wwwroot/Templates.backup/* wwwroot/Templates/
```

### Option 3: Disable Templates in Database
```sql
-- Disable t?t c? templates ?ã migrate
UPDATE document_templates SET IsActive = 0 
WHERE Code IN (
    'QUOTE_DEFAULT', 
    'SALARY_REPORT_DEFAULT', 
    'CONTRACT_INDIVIDUAL', 
    'CONTRACT_BUSINESS'
);
```

## ? L?i Ích C?a Thay ??i

### 1. Qu?n Lý D? Dàng
- Admin có th? ch?nh s?a templates qua API/UI
- Không c?n truy c?p server ho?c code
- Không c?n deploy l?i application

### 2. Version Control
- M?i template có version number
- Track ???c l?ch s? thay ??i
- Có th? rollback v? version c?

### 3. Multi-Template Support
- Có th? có nhi?u templates cho cùng lo?i document
- D? dàng A/B testing
- Customize theo t?ng khách hàng n?u c?n

### 4. Audit Trail
- Track ???c ai t?o/s?a template
- Track th?i gian thay ??i
- CreatedByUserId, CreatedAt, UpdatedAt

### 5. Deployment
- Không c?n copy files khi deploy
- Ch? c?n migrate database
- D? dàng sync gi?a các environment

## ?? Related Documentation

### Migration Documentation
- `erp_backend/TEMPLATE_MIGRATION_README.md` - Migration guide t?ng quát
- `erp_backend/MIGRATION_CHANGES_SUMMARY.md` - File này
- `erp_backend/CONTRACT_TEMPLATE_MIGRATION_STATUS.md` - Chi ti?t v? Contract migration

### Technical Documentation
- `erp_backend/CONTRACT_FLOW_EXPLANATION.md` - Gi?i thích flow x? lý Contract

### Database Model
- `erp_backend/Models/DocumentTemplate.cs`
- Table: `document_templates`

### Migration Script
- `erp_backend/Migrations/Scripts/MigrateTemplatesToDatabase.cs`
- Endpoint: `POST /api/DocumentTemplates/migrate-from-files`

### Controllers
- `erp_backend/Controllers/DocumentTemplatesController.cs` - CRUD operations
- `erp_backend/Controllers/QuotesController.cs` - S? d?ng QUOTE_DEFAULT
- `erp_backend/Controllers/ContractsController.cs` - S? d?ng CONTRACT_INDIVIDUAL/CONTRACT_BUSINESS
- `erp_backend/Controllers/PayslipsController.cs` - S? d?ng SalaryReportService

### Services
- `erp_backend/Services/SalaryReportService.cs` - S? d?ng SALARY_REPORT_DEFAULT

## ?? Important Notes

### EmailService Templates
- `EmailService.cs` v?n ?ang s? d?ng hardcoded HTML templates
- **CH?A ???c chuy?n ??i** vì:
  - Email templates có nhi?u customization
  - C?n test k? h?n v?i các email providers
  - Có th? migrate sau n?u c?n thi?t
- Email templates **?ã ???c migrate vào database** nh?ng ch?a áp d?ng trong code

### Template Placeholders
- M?i template có list `AvailablePlaceholders` ?? document
- Giúp developer bi?t có nh?ng placeholder nào
- Example: `{{CustomerName}}`, `{{TotalAmount}}`, etc.

### Performance
- Templates ???c cache trong database
- Query performance t?t v?i index trên Code
- Không ?nh h??ng ?áng k? ??n response time

### Contract Templates - Logic ??c Bi?t
Contracts có 2 template khác nhau d?a vào `CustomerType`:
```csharp
var templateCode = customer.CustomerType?.ToLower() == "individual" 
    ? "CONTRACT_INDIVIDUAL"  // Khách hàng cá nhân
    : "CONTRACT_BUSINESS";   // Khách hàng doanh nghi?p
```

## ?? Next Steps

### Immediate (?ã Hoàn Thành)
- ? Deploy code changes
- ? Run migration script
- ? Test Quotes features
- ? Test Contracts features (both individual & business)
- ? Test Salary Reports features

### Short Term
- [ ] Update user documentation
- [ ] Train users on template management
- [ ] Monitor production for issues
- [ ] Collect feedback from users

### Long Term (Optional)
- [ ] Migrate EmailService templates (if needed)
- [ ] Implement template caching mechanism
- [ ] Create admin UI for template management
- [ ] Implement template versioning system
- [ ] Add template preview in admin panel
- [ ] Implement template validation rules

## ?? Training Materials Needed

1. **For Administrators:**
   - How to view templates via API
   - How to update templates
   - How to create new template versions
   - Troubleshooting guide

2. **For Developers:**
   - Template placeholder documentation
   - How to add new template types
   - How to customize binding logic
   - Performance considerations

3. **For Users:**
   - Where to find generated documents
   - How to regenerate documents
   - What to do if document generation fails

## ?? Support

N?u g?p v?n ??:
1. Check logs trong Output window
2. Verify database có templates: `GET /api/DocumentTemplates`
3. Check `IsActive = true` cho templates c?n dùng
4. Verify JWT token có role Admin (cho migration)
5. Check CustomerType khi test Contracts
6. Contact development team n?u v?n ?? v?n t?n t?i

---
**Last Updated:** 2024-12-31  
**Migration Version:** 1.0  
**Status:** ? **COMPLETED** (Quotes, Contracts, Salary Reports)  
**Pending:** ?? EmailService (?ã migrate nh?ng ch?a áp d?ng)
