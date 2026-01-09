# Changelog: Xóa tr??ng AvailablePlaceholders

## Ngày: 09/01/2026

## T?ng quan
?ã xóa tr??ng `AvailablePlaceholders` kh?i model `DocumentTemplate` và database vì tr??ng này là **redundant** (d? th?a). Thông tin v? placeholders hi?n ???c cung c?p ??ng thông qua `PlaceholderSchemaService`.

## Lý do xóa
1. **Không ???c s? d?ng trong logic nghi?p v?**: Controller ch? l?u và tr? v? nh?ng không validate hay x? lý
2. **?ã có PlaceholderSchemaService**: Service t? ??ng phát hi?n placeholders t? Models qua reflection
3. **API thay th? s?n có**: `/api/DocumentTemplates/schema/placeholders` cung c?p thông tin real-time
4. **Gi?m data redundancy**: Không c?n l?u tr? metadata t?nh khi có th? generate ??ng

## Các thay ??i

### 1. Model - DocumentTemplate.cs
**?ã xóa:**
```csharp
// Metadata cho placeholders (JSON array: ["{{CustomerName}}", "{{TotalAmount}}"])
public string? AvailablePlaceholders { get; set; }
```

**K?t qu?:** Model g?n h?n, ch? gi? d? li?u thi?t y?u

### 2. Controller - DocumentTemplatesController.cs
**Dòng 297: Xóa assignment trong UpdateTemplate:**
```csharp
// ?ã xóa: existing.AvailablePlaceholders = template.AvailablePlaceholders;
```

**K?t qu?:** Controller không còn x? lý tr??ng này

### 3. Migration Script - MigrateTemplatesToDatabase.cs
**Xóa t?t c? assignments c?a AvailablePlaceholders trong các ph??ng th?c:**
- `MigrateQuoteTemplateAsync()`
- `MigrateSalaryReportTemplateAsync()`
- `MigrateContractIndividualTemplateAsync()`
- `MigrateContractBusinessTemplateAsync()`
- `MigrateEmailAccountCreationTemplateAsync()`
- `MigrateEmailPasswordResetOTPTemplateAsync()`
- `MigrateEmailNotificationTemplateAsync()`
- `MigrateEmailPaymentSuccessTemplateAsync()`

**K?t qu?:** Migration scripts ??n gi?n h?n

### 4. ApplicationDbContext.cs
**Dòng 1204: Xóa configuration:**
```csharp
// ?ã xóa: entity.Property(e => e.AvailablePlaceholders).HasColumnType("text");
```

**?ã s?a TemplateType:**
```csharp
// Thay ??i t? .IsRequired() thành nullable
entity.Property(e => e.TemplateType).HasMaxLength(50);
```

**K?t qu?:** EF Core configuration c?p nh?t

### 5. Database Migration
**Migration Name:** `20260109072422_RemoveAvailablePlaceholdersColumn`

**SQL th?c thi:**
```sql
ALTER TABLE document_templates DROP COLUMN "AvailablePlaceholders";
ALTER TABLE document_templates ALTER COLUMN "TemplateType" DROP NOT NULL;
```

**K?t qu?:** 
- Column `AvailablePlaceholders` ?ã b? xóa kh?i database
- Column `TemplateType` hi?n là nullable

## API thay th?

Thay vì l?u `AvailablePlaceholders` trong database, s? d?ng các API sau:

### L?y placeholders theo template type
```http
GET /api/DocumentTemplates/schema/placeholders?templateType=contract
```

**Response:**
```json
{
  "success": true,
  "templateType": "contract",
  "data": {
    "Contract": [
      {
        "name": "Id",
        "placeholder": "{{Contract.Id}}",
        "type": "number",
        "description": "Id",
        "isRequired": false,
        "example": "123"
      }
    ],
    "Customer": [...]
  },
  "entityCount": 5,
  "totalFields": 45
}
```

### L?y placeholders cho entity c? th?
```http
GET /api/DocumentTemplates/schema/placeholders/Contract
```

### Auto-detect placeholders t? HTML
```http
POST /api/DocumentTemplates/extract-placeholders
Content-Type: application/json

{
  "htmlContent": "<html>{{Contract.Id}} {{Customer.Name}}</html>"
}
```

## Testing

? **Build successful**
? **Migration applied successfully**
? **All compilation errors fixed**

### Test checklist:
- [x] Model compiles
- [x] Controller compiles
- [x] Migration scripts compile
- [x] ApplicationDbContext compiles
- [x] Migration created
- [x] Migration applied to database
- [x] Final build successful

## Breaking Changes

### ?? Cho Frontend/API Consumers:

1. **Khi GET template:** Response không còn tr? v? `availablePlaceholders`
   
   **Tr??c:**
   ```json
   {
     "id": 1,
     "name": "Template",
     "availablePlaceholders": "[\"{{Field1}}\", \"{{Field2}}\"]"
   }
   ```
   
   **Sau:**
   ```json
   {
     "id": 1,
     "name": "Template"
     // availablePlaceholders không còn
   }
   ```

2. **Khi POST/PUT template:** Không c?n g?i `availablePlaceholders` n?a
   
   **Tr??c:**
   ```json
   {
     "name": "New Template",
     "code": "TEMP_001",
     "htmlContent": "<html>...</html>",
     "availablePlaceholders": "[\"{{Field1}}\"]"
   }
   ```
   
   **Sau:**
   ```json
   {
     "name": "New Template",
     "code": "TEMP_001",
     "htmlContent": "<html>...</html>"
     // Không c?n availablePlaceholders
   }
   ```

3. **?? l?y placeholders:** S? d?ng API m?i
   ```javascript
   // Thay vì l?y t? template.availablePlaceholders
   const response = await fetch('/api/DocumentTemplates/schema/placeholders?templateType=contract');
   const { data } = await response.json();
   // data ch?a t?t c? placeholders grouped by entity
   ```

## Rollback (n?u c?n)

N?u c?n rollback migration:

```bash
dotnet ef database update <previous_migration_name> --project erp_backend
dotnet ef migrations remove --project erp_backend
```

Sau ?ó restore code t? git:
```bash
git checkout HEAD~1 -- erp_backend/Models/DocumentTemplate.cs
git checkout HEAD~1 -- erp_backend/Controllers/DocumentTemplatesController.cs
git checkout HEAD~1 -- erp_backend/Data/ApplicationDbContext.cs
git checkout HEAD~1 -- erp_backend/Migrations/Scripts/MigrateTemplatesToDatabase.cs
```

## Tác gi?
- **Ng??i th?c hi?n:** GitHub Copilot
- **Ng??i yêu c?u:** Tran Duc Vuong
- **Ngày:** 09/01/2026

## Files thay ??i
1. `erp_backend/Models/DocumentTemplate.cs` - Removed field
2. `erp_backend/Controllers/DocumentTemplatesController.cs` - Removed assignment
3. `erp_backend/Migrations/Scripts/MigrateTemplatesToDatabase.cs` - Removed all references
4. `erp_backend/Data/ApplicationDbContext.cs` - Removed configuration
5. `erp_backend/Migrations/20260109072422_RemoveAvailablePlaceholdersColumn.cs` - New migration

---

**Status:** ? **COMPLETED SUCCESSFULLY**
