# ?? Template System - Complete Solution

## ?? Tài Li?u

| Document | Mô T? |
|----------|-------|
| [PLACEHOLDER_SCHEMA_DOCUMENTATION.md](./PLACEHOLDER_SCHEMA_DOCUMENTATION.md) | ?? Full documentation v? Placeholder Schema System |
| [PLACEHOLDER_SCHEMA_SUMMARY.md](./PLACEHOLDER_SCHEMA_SUMMARY.md) | ?? Tóm t?t nh?ng gì ?ã implement |
| [TEMPLATE_EDITOR_API_DOCUMENTATION.md](./TEMPLATE_EDITOR_API_DOCUMENTATION.md) | ?? API documentation cho Template Editor |
| [FRONTEND_PLACEHOLDER_SELECTOR_EXAMPLE.tsx](./FRONTEND_PLACEHOLDER_SELECTOR_EXAMPLE.tsx) | ?? React component example |

---

## ?? Gi?i Pháp Cho V?n ?? Quá Nhi?u Bi?n

### ? V?n ?? Ban ??u

Khi có quá nhi?u entities (Contract, Customer, SaleOrder, Service, Addon, Contact, Employee...), vi?c qu?n lý placeholders theo ki?u flat s? g?p khó kh?n:

```html
<!-- Khó nh? và d? nh?m l?n -->
{{ContractNumberContract}}
{{CustomerName}}
{{CustomerCompanyName}}
{{CustomerRepresentativeName}}
{{SaleOrderTitle}}
{{ServiceName}}
...hàng tr?m bi?n khác
```

**V?n ??:**
- ? Không bi?t bi?n nào thu?c entity nào
- ? Không có autocomplete/suggestions
- ? Khó validate
- ? Naming conflicts

---

## ? Gi?i Pháp: Placeholder Schema System

### 1. **Phân Nhóm Theo Entity**

```html
<!-- Rõ ràng và có c?u trúc -->
{{Contract.NumberContract}}
{{Customer.Name}}
{{Customer.CompanyName}}
{{Customer.RepresentativeName}}
{{SaleOrder.Title}}
{{Service.Name}}
```

### 2. **Schema-Based Suggestions**

API cung c?p full schema ?? build UI autocomplete:

```typescript
// L?y schema
const schema = await fetch('/api/DocumentTemplates/schema/placeholders?templateType=contract');

// Response:
{
  "Contract": [
    { name: "NumberContract", placeholder: "{{Contract.NumberContract}}", type: "number", example: "123" },
    { name: "TotalAmount", placeholder: "{{Contract.TotalAmount}}", type: "number", example: "15000000" }
  ],
  "Customer": [
    { name: "Name", placeholder: "{{Customer.Name}}", type: "string", example: "Nguy?n V?n A" },
    { name: "Email", placeholder: "{{Customer.Email}}", type: "string", example: "email@example.com" }
  ]
  // ... nhi?u entities khác
}
```

### 3. **Smart Validation**

```typescript
// Validate placeholders có h?p l? không
const result = await validatePlaceholders(
  ["{{Contract.NumberContract}}", "{{InvalidField}}"],
  'contract'
);

// result.invalidPlaceholders = ["{{InvalidField}}"]
```

### 4. **Flexible Data Binding**

```typescript
// Render v?i structured object
const data = {
  Contract: {
    NumberContract: 123,
    TotalAmount: 15000000
  },
  Customer: {
    Name: "Nguy?n V?n A",
    CompanyName: "Công ty ABC"
  }
};

await renderTemplateWithObject(templateId, data);
```

---

## ??? Ki?n Trúc

```
???????????????????????????????????????????????????????????
?                    FRONTEND (React)                      ?
???????????????????????????????????????????????????????????
?                                                           ?
?  ???????????????????      ??????????????????????????    ?
?  ? Template Editor ?      ? Placeholder Selector   ?    ?
?  ?                 ???????? (v?i autocomplete)     ?    ?
?  ???????????????????      ??????????????????????????    ?
?           ?                          ?                   ?
?           ?                          ?                   ?
?           ?                          ?                   ?
?  ???????????????????????????????????????????????????    ?
?  ?         TemplateService (API calls)              ?    ?
?  ???????????????????????????????????????????????????    ?
?           ?                                              ?
????????????????????????????????????????????????????????????
            ?
            ? HTTP Requests
            ?
????????????????????????????????????????????????????????????
?                    BACKEND (.NET 8)                       ?
????????????????????????????????????????????????????????????
?                                                           ?
?  ????????????????????????????????????????????????????    ?
?  ?    DocumentTemplatesController                   ?    ?
?  ?  - GET /schema/placeholders?templateType=X       ?    ?
?  ?  - GET /schema/placeholders/{entity}             ?    ?
?  ?  - POST /schema/validate-placeholders            ?    ?
?  ?  - POST /render-with-object/{id}                 ?    ?
?  ????????????????????????????????????????????????????    ?
?           ?                          ?                   ?
?           ?                          ?                   ?
?  ????????????????????      ??????????????????????????   ?
?  ?PlaceholderSchema ?      ? TemplateRenderService  ?   ?
?  ?Service           ?      ? - Flatten objects      ?   ?
?  ?- Get schema      ?      ? - Replace placeholders ?   ?
?  ?- Validate        ?      ? - Format values        ?   ?
?  ????????????????????      ??????????????????????????   ?
?           ?                                              ?
?           ?                                              ?
?  ????????????????????????????????????????????????????   ?
?  ?         Models (via Reflection)                  ?   ?
?  ?  - Contract, Customer, SaleOrder, Service...     ?   ?
?  ????????????????????????????????????????????????????   ?
?                                                           ?
?????????????????????????????????????????????????????????????
```

---

## ?? Quick Start Guide

### Backend Setup (? Already Done!)

```bash
# Build ?ã successful
dotnet build
```

### Frontend Integration

#### 1. Install Dependencies

```bash
npm install axios
```

#### 2. Copy Template Service

Sao chép code t? `FRONTEND_PLACEHOLDER_SELECTOR_EXAMPLE.tsx`:
- `PlaceholderSelector` component
- `TemplateService` class

#### 3. Use in Your App

```typescript
import PlaceholderSelector from './components/PlaceholderSelector';

function TemplateEditor() {
  const [showSelector, setShowSelector] = useState(false);

  const handleInsert = (placeholder: string) => {
    // Insert vào editor
    insertAtCursor(placeholder);
  };

  return (
    <div>
      <button onClick={() => setShowSelector(true)}>
        ?? Chèn Bi?n
      </button>
      
      {showSelector && (
        <PlaceholderSelector
          templateType="contract"
          onInsertPlaceholder={handleInsert}
          onClose={() => setShowSelector(false)}
        />
      )}
    </div>
  );
}
```

---

## ?? Entities & Fields

### Contract (15+ fields)
```
{{Contract.Id}}
{{Contract.NumberContract}}
{{Contract.Status}}
{{Contract.PaymentMethod}}
{{Contract.TotalAmount}}
{{Contract.SubTotal}}
{{Contract.TaxAmount}}
{{Contract.Expiration}}
{{Contract.Notes}}
{{Contract.CreatedAt}}
...
```

### Customer (25+ fields)
```
{{Customer.Id}}
{{Customer.Name}}
{{Customer.Email}}
{{Customer.PhoneNumber}}
{{Customer.CompanyName}}
{{Customer.CompanyAddress}}
{{Customer.TaxCode}}
{{Customer.RepresentativeName}}
{{Customer.RepresentativeEmail}}
{{Customer.RepresentativePhone}}
{{Customer.TechContactName}}
{{Customer.TechContactEmail}}
...
```

### SaleOrder, Service, Addon, User...
Xem full list trong [PLACEHOLDER_SCHEMA_DOCUMENTATION.md](./PLACEHOLDER_SCHEMA_DOCUMENTATION.md)

---

## ?? Common Use Cases

### Use Case 1: Create Contract Template

```html
<!DOCTYPE html>
<html>
<head>
    <title>H?p ??ng {{Contract.NumberContract}}/2025</title>
</head>
<body>
    <h1>H?P ??NG CUNG C?P D?CH V?</h1>
    <p>S?: {{Contract.NumberContract}}/2025-TTWS</p>
    
    <h2>BÊN A: {{Customer.CompanyName}}</h2>
    <p>??a ch?: {{Customer.CompanyAddress}}</p>
    <p>MST: {{Customer.TaxCode}}</p>
    <p>Ng??i ??i di?n: {{Customer.RepresentativeName}}</p>
    
    <h2>GIÁ TR? H?P ??NG</h2>
    <p>Ch?a thu?: {{Contract.SubTotal}} VN?</p>
    <p>Thu? VAT: {{Contract.TaxAmount}} VN?</p>
    <p>T?ng thanh toán: {{Contract.TotalAmount}} VN?</p>
    
    <p>Nhân viên: {{User.FullName}}</p>
</body>
</html>
```

### Use Case 2: Render Contract PDF

```typescript
// Backend code
async function generateContractPdf(contractId: number) {
  // 1. Fetch data with relations
  const contract = await db.contracts.findOne({
    where: { id: contractId },
    include: ['Customer', 'SaleOrder', 'User']
  });

  // 2. Prepare structured data
  const data = {
    Contract: {
      NumberContract: contract.numberContract,
      TotalAmount: contract.totalAmount,
      SubTotal: contract.subTotal,
      TaxAmount: contract.taxAmount,
      Status: contract.status
    },
    Customer: {
      Name: contract.customer.name,
      CompanyName: contract.customer.companyName,
      TaxCode: contract.customer.taxCode,
      RepresentativeName: contract.customer.representativeName
    },
    User: {
      FullName: contract.user.fullName,
      Email: contract.user.email
    }
  };

  // 3. Render
  const html = await templateService.renderTemplateWithObjectByCode(
    'CONTRACT_DEFAULT',
    data
  );

  // 4. Convert to PDF
  const pdf = await pdfService.convertHtmlToPdf(html);
  
  return pdf;
}
```

---

## ? Testing APIs

### Test v?i curl:

```bash
# 1. L?y schema
curl -X GET "http://localhost:5000/api/DocumentTemplates/schema/placeholders?templateType=contract" \
  -H "Authorization: Bearer {token}"

# 2. Validate placeholders
curl -X POST "http://localhost:5000/api/DocumentTemplates/schema/validate-placeholders" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "placeholders": ["{{Contract.NumberContract}}", "{{Customer.Name}}"],
    "templateType": "contract"
  }'

# 3. Render v?i object
curl -X POST "http://localhost:5000/api/DocumentTemplates/render-with-object/5" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "Contract": {"NumberContract": 123, "TotalAmount": 15000000},
    "Customer": {"Name": "Nguy?n V?n A", "CompanyName": "Công ty ABC"}
  }' \
  --output rendered.html
```

---

## ?? Migration Path

### Không C?n Migration!

H? th?ng **100% backward compatible**:

? Templates c? v?n ho?t ??ng  
? API c? v?n ho?t ??ng  
? Flat dictionary v?n ???c h? tr?  

### N?u Mu?n Upgrade:

1. **Update UI** - Add `PlaceholderSelector` component
2. **New templates** - Use nested syntax `{{Entity.Property}}`
3. **Rendering** - Use `/render-with-object` endpoints

---

## ?? Project Structure

```
erp_backend/
??? Controllers/
?   ??? DocumentTemplatesController.cs      [UPDATED] ??
?       - Added schema APIs
?       - Added render-with-object endpoints
?
??? Services/
?   ??? PlaceholderSchemaService.cs         [NEW] ?
?   ?   - GetAvailablePlaceholders()
?   ?   - GetPlaceholdersForEntity()
?   ?   - ValidatePlaceholders()
?   ?
?   ??? TemplateRenderService.cs            [UPDATED] ??
?       - RenderTemplateWithObjectAsync()
?       - FlattenObject() helper
?       - Format values by type
?
??? Models/
?   ??? Contract.cs
?   ??? Customer.cs
?   ??? SaleOrder.cs
?   ??? Service.cs
?   ??? Addon.cs
?   ??? User.cs
?
??? Program.cs                              [UPDATED] ??
?   - Registered PlaceholderSchemaService
?
??? Documentation/
    ??? PLACEHOLDER_SCHEMA_DOCUMENTATION.md     ?? Full docs
    ??? PLACEHOLDER_SCHEMA_SUMMARY.md          ?? Summary
    ??? TEMPLATE_EDITOR_API_DOCUMENTATION.md   ?? API docs (old)
    ??? FRONTEND_PLACEHOLDER_SELECTOR_EXAMPLE.tsx ?? React example
    ??? README_TEMPLATE_SYSTEM.md              ?? This file
```

---

## ?? Best Practices

### 1. ? Dùng Nested Syntax Trong Templates M?i
```html
<!-- Good -->
{{Contract.NumberContract}}
{{Customer.Name}}

<!-- Acceptable (backward compat) -->
{{NumberContract}}
{{Name}}
```

### 2. ? Validate Tr??c Khi L?u
```typescript
const { isValid, invalidPlaceholders } = await validatePlaceholders(
  placeholders,
  templateType
);

if (!isValid) {
  showError(`Invalid: ${invalidPlaceholders.join(', ')}`);
  return;
}
```

### 3. ? Hi?n Th? Schema Trong UI
```typescript
// Cho ng??i dùng bi?t có nh?ng placeholders nào available
const schema = await getAvailablePlaceholders('contract');
displayPlaceholdersGroupedByEntity(schema);
```

### 4. ? Structured Data Khi Render
```typescript
// Good - structured
const data = {
  Contract: { ... },
  Customer: { ... }
};

// Acceptable - flat (backward compat)
const data = {
  "Contract.NumberContract": "123",
  "Customer.Name": "..."
};
```

---

## ?? Roadmap

### Phase 2 (Future):
- [ ] Loop placeholders: `{{#foreach Items}}...{{/foreach}}`
- [ ] Conditional: `{{#if Status == 'Active'}}...{{/if}}`
- [ ] Formatters: `{{TotalAmount | currency}}`
- [ ] Nested entities: `{{Contract.SaleOrder.Customer.Name}}`

### Phase 3 (Future):
- [ ] Visual template builder (drag & drop)
- [ ] Template versioning
- [ ] Template preview v?i sample data
- [ ] Multi-language support

---

## ?? Support

### Tài li?u:
- [Full Documentation](./PLACEHOLDER_SCHEMA_DOCUMENTATION.md)
- [Summary](./PLACEHOLDER_SCHEMA_SUMMARY.md)
- [API Docs](./TEMPLATE_EDITOR_API_DOCUMENTATION.md)

### Examples:
- [React Component](./FRONTEND_PLACEHOLDER_SELECTOR_EXAMPLE.tsx)
- API examples in documentation

---

## ? Status

- [x] ? Backend implementation complete
- [x] ? Build successful
- [x] ? Documentation complete
- [x] ? Frontend examples provided
- [ ] ?? Frontend integration (your next step)
- [ ] ?? End-to-end testing
- [ ] ?? Production deployment

---

**Last Updated:** 2024-12-31  
**Version:** 3.0  
**Status:** ? Ready for Frontend Integration

**Gi?i pháp hoàn ch?nh cho v?n ?? qu?n lý placeholders khi có nhi?u entities!** ??
