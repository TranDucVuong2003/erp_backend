# ?? Summary: Placeholder Schema System Implementation

## ? Nh?ng Gì ?ã Hoàn Thành

### 1. **T?o PlaceholderSchemaService** (`Services/PlaceholderSchemaService.cs`)
- ? Service qu?n lý schema c?a các entities (Contract, Customer, SaleOrder, Service, Addon, User)
- ? T? ??ng phát hi?n available fields t? Models b?ng Reflection
- ? G?i ý placeholders theo template type
- ? Validate placeholders có h?p l? không
- ? Cung c?p metadata: type, description, example values

### 2. **Nâng C?p TemplateRenderService** (`Services/TemplateRenderService.cs`)
- ? H? tr? **nested placeholders**: `{{Entity.Property}}` VÀ `{{Property}}`
- ? Render v?i **structured object data** (không ch? flat dictionary)
- ? T? ??ng flatten nested objects
- ? Format values theo type (date ? dd/MM/yyyy, decimal ? with commas, boolean ? Có/Không)
- ? **Backward compatible** - v?n support flat dictionary nh? c?

### 3. **Thêm API Endpoints M?i** (`Controllers/DocumentTemplatesController.cs`)

#### Schema Management APIs:
- ? `GET /api/DocumentTemplates/schema/placeholders?templateType=contract` - L?y available placeholders
- ? `GET /api/DocumentTemplates/schema/placeholders/{entityName}` - L?y fields c?a m?t entity
- ? `GET /api/DocumentTemplates/schema/entities` - L?y t?t c? entities
- ? `POST /api/DocumentTemplates/schema/validate-placeholders` - Validate placeholders

#### Rendering v?i Object:
- ? `POST /api/DocumentTemplates/render-with-object/{id}` - Render v?i structured object
- ? `POST /api/DocumentTemplates/render-with-object-by-code/{code}` - Render by code v?i object

### 4. **Register Services** (`Program.cs`)
- ? ?ã register `IPlaceholderSchemaService` trong DI container

### 5. **Documentation**
- ? T?o `PLACEHOLDER_SCHEMA_DOCUMENTATION.md` - Full documentation v?i examples

---

## ?? C?i Ti?n Chính

### Tr??c ?ây (Flat Placeholders):
```html
<!-- Template -->
<h1>H?p ??ng {{ContractNumberContract}}</h1>
<p>Khách hàng: {{CustomerName}}</p>
<p>Email: {{CustomerEmail}}</p>

<!-- Data -->
{
  "ContractNumberContract": "123",
  "CustomerName": "Nguy?n V?n A",
  "CustomerEmail": "email@example.com",
  "CustomerCompanyName": "...",
  "CustomerTaxCode": "...",
  // R?t nhi?u fields khác, khó qu?n lý
}
```

**V?n ??:**
- ? Không bi?t field nào thu?c entity nào
- ? Không có g?i ý khi vi?t template
- ? Khó validate
- ? Naming inconsistent

### Bây Gi? (Structured Placeholders):
```html
<!-- Template - Rõ ràng h?n -->
<h1>H?p ??ng {{Contract.NumberContract}}</h1>
<p>Khách hàng: {{Customer.Name}}</p>
<p>Email: {{Customer.Email}}</p>
<p>Công ty: {{Customer.CompanyName}}</p>
<p>MST: {{Customer.TaxCode}}</p>

<!-- Data - Có c?u trúc -->
{
  "Contract": {
    "NumberContract": 123,
    "TotalAmount": 15000000,
    "Status": "Active"
  },
  "Customer": {
    "Name": "Nguy?n V?n A",
    "Email": "email@example.com",
    "CompanyName": "Công ty ABC",
    "TaxCode": "0123456789"
  }
}
```

**L?i ích:**
- ? Rõ ràng: `{{Contract.NumberContract}}` - bi?t ngay thu?c entity nào
- ? Auto-suggest: API cung c?p schema ?? build autocomplete
- ? Validation: Ki?m tra ???c placeholders có h?p l? không
- ? Type-aware: Bi?t type c?a field (string, number, date...)
- ? Flexible: V?n support `{{NumberContract}}` n?u mu?n ng?n g?n

---

## ?? Entity Schema

| Entity | Fields | Example Placeholders |
|--------|--------|---------------------|
| **Contract** | 15+ fields | `{{Contract.NumberContract}}`, `{{Contract.TotalAmount}}`, `{{Contract.Expiration}}` |
| **Customer** | 25+ fields | `{{Customer.Name}}`, `{{Customer.CompanyName}}`, `{{Customer.TaxCode}}` |
| **SaleOrder** | 10+ fields | `{{SaleOrder.Title}}`, `{{SaleOrder.Value}}`, `{{SaleOrder.Status}}` |
| **Service** | 8+ fields | `{{Service.Name}}`, `{{Service.Price}}`, `{{Service.Duration}}` |
| **Addon** | 6+ fields | `{{Addon.Name}}`, `{{Addon.Price}}` |
| **User** | 12+ fields | `{{User.FullName}}`, `{{User.Email}}`, `{{User.Department}}` |

---

## ?? Use Cases Th?c T?

### Use Case 1: Template Editor UI
```typescript
// G?i API l?y schema
const response = await fetch('/api/DocumentTemplates/schema/placeholders?templateType=contract');
const { data } = await response.json();

// data = {
//   "Contract": [ {name: "NumberContract", placeholder: "{{Contract.NumberContract}}", ...}, ... ],
//   "Customer": [ {name: "Name", placeholder: "{{Customer.Name}}", ...}, ... ],
//   ...
// }

// Hi?n th? trong UI theo nhóm
// - Contract (15 fields)
//   - NumberContract (number) - VD: 123
//   - TotalAmount (number) - VD: 15000000
//   - Status (string) - VD: Active
// - Customer (25 fields)
//   - Name (string) - VD: Nguy?n V?n A
//   - Email (string) - VD: email@company.com
//   ...
```

### Use Case 2: Autocomplete Khi Gõ
```typescript
// Khi user gõ "{{" trong editor
// ? Hi?n dropdown v?i t?t c? placeholders
// Khi user gõ "{{Contract." 
// ? Hi?n danh sách fields c?a Contract
```

### Use Case 3: Validate Template Tr??c Khi L?u
```typescript
// 1. Extract placeholders t? HTML
const { placeholders } = await extractPlaceholders(htmlContent);

// 2. Validate v?i schema
const { isValid, invalidPlaceholders } = await validatePlaceholders(
  placeholders, 
  'contract'
);

if (!isValid) {
  alert(`Invalid: ${invalidPlaceholders.join(', ')}`);
}
```

### Use Case 4: Render Contract PDF
```typescript
// Backend: L?y data t? DB
const contract = await db.contracts.findOne({
  where: { id: contractId },
  include: ['Customer', 'SaleOrder', 'User']
});

// Chu?n b? structured data
const data = {
  Contract: {
    NumberContract: contract.numberContract,
    TotalAmount: contract.totalAmount,
    Status: contract.status,
    Expiration: contract.expiration
  },
  Customer: {
    Name: contract.customer.name,
    Email: contract.customer.email,
    CompanyName: contract.customer.companyName,
    TaxCode: contract.customer.taxCode
  },
  User: {
    FullName: contract.user.fullName,
    Email: contract.user.email
  }
};

// Render
const html = await renderTemplateWithObject(templateId, data);
```

---

## ?? Migration Guide

### Không C?n Migration!
H? th?ng **100% backward compatible**:

1. ? **Templates c? v?n ho?t ??ng** - không c?n thay ??i gì
2. ? **API c? v?n ho?t ??ng** - `/render/{id}` và `/render-by-code/{code}` không ??i
3. ? **Flat dictionary v?n work** - truy?n `{"Name": "..."}` v?n ???c

### N?u Mu?n Upgrade:
1. Update UI ?? hi?n th? placeholders theo nhóm (s? d?ng schema APIs)
2. Khi t?o templates m?i, dùng nested syntax `{{Entity.Property}}`
3. Khi render, dùng endpoints m?i `/render-with-object/{id}`

---

## ?? Files Changed

```
erp_backend/
??? Services/
?   ??? PlaceholderSchemaService.cs          [NEW] ?
?   ??? TemplateRenderService.cs             [UPDATED] ??
??? Controllers/
?   ??? DocumentTemplatesController.cs       [UPDATED] ??
??? Program.cs                                [UPDATED] ??
??? PLACEHOLDER_SCHEMA_DOCUMENTATION.md       [NEW] ??
??? PLACEHOLDER_SCHEMA_SUMMARY.md            [NEW] ??
```

---

## ?? Next Steps

### Frontend Implementation:
1. T?o `PlaceholderSelector` component
2. Integrate autocomplete trong editor
3. Add validation UI tr??c khi save template
4. Update render logic ?? dùng structured data

### Backend Enhancement (Future):
1. Support array/loop: `{{#foreach Items}}...{{/foreach}}`
2. Conditional rendering: `{{#if Status == 'Active'}}...{{/if}}`
3. Custom formatters: `{{TotalAmount | currency}}`
4. Nested entities: `{{Contract.SaleOrder.Customer.Name}}`

---

## ? Testing Checklist

- [x] Build successful
- [ ] Test schema APIs v?i Postman
- [ ] Test render-with-object APIs
- [ ] Frontend integration
- [ ] End-to-end test v?i real contract data

---

## ?? Support

N?u có câu h?i, tham kh?o:
- `PLACEHOLDER_SCHEMA_DOCUMENTATION.md` - Full documentation
- `TEMPLATE_EDITOR_API_DOCUMENTATION.md` - API c?
- Examples trong documentation

**Last Updated:** 2024-12-31  
**Status:** ? Build Successful - Ready for Integration
