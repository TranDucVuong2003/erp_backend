# ?? Placeholder Schema System - H??ng D?n S? D?ng

## ?? T?ng Quan

H? th?ng **Placeholder Schema** giúp qu?n lý các bi?n ??ng (placeholders) m?t cách có c?u trúc, phân nhóm theo entities (Contract, Customer, Employee...) thay vì qu?n lý theo ki?u flat.

### ? Tính N?ng Chính

- ? **Phân nhóm placeholders** theo domain entities
- ? **H? tr? nested syntax**: `{{Entity.Property}}` VÀ `{{Property}}`
- ? **T? ??ng g?i ý** placeholders d?a trên template type
- ? **Validation schema** - ki?m tra placeholders có h?p l? không
- ? **Type-aware** - bi?t type c?a m?i field (string, number, date...)
- ? **Example values** - g?i ý giá tr? m?u

---

## ??? C?u Trúc Entities

H? th?ng hi?n h? tr? các entities sau:

| Entity | Mô T? | Fields |
|--------|-------|--------|
| **Contract** | H?p ??ng | Id, SaleOrderId, NumberContract, Status, PaymentMethod, TotalAmount, SubTotal, TaxAmount, Expiration, Notes, ExtractInvoices, CreatedAt... |
| **Customer** | Khách hàng | Id, Name, Email, PhoneNumber, CompanyName, CompanyAddress, TaxCode, RepresentativeName, RepresentativeEmail, TechContactName... |
| **SaleOrder** | ??n hàng | Id, Title, CustomerId, Value, Probability, Status, Notes, CreatedAt... |
| **Service** | D?ch v? | Id, Name, Price, Duration, Description... |
| **Addon** | D?ch v? b? sung | Id, Name, Price, Description... |
| **User** | Ng??i dùng | Id, Username, Email, FullName, Role, Department... |

---

## ?? API Endpoints M?i

### 1. Get Available Placeholders by Template Type

**GET** `/api/DocumentTemplates/schema/placeholders?templateType=contract`

L?y t?t c? placeholders có s?n cho m?t lo?i template.

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `templateType` | `string` | ? | `contract` | Lo?i template: `contract`, `quote`, `invoice`, `salary_notification`, `email` |

#### Response Success (200 OK)

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
      },
      {
        "name": "NumberContract",
        "placeholder": "{{Contract.NumberContract}}",
        "type": "number",
        "description": "NumberContract",
        "isRequired": true,
        "example": "123"
      },
      {
        "name": "Status",
        "placeholder": "{{Contract.Status}}",
        "type": "string",
        "description": "Status",
        "isRequired": true,
        "example": "Sample text"
      },
      {
        "name": "TotalAmount",
        "placeholder": "{{Contract.TotalAmount}}",
        "type": "number",
        "description": "TotalAmount (s? ti?n)",
        "isRequired": true,
        "example": "15000000"
      },
      {
        "name": "Expiration",
        "placeholder": "{{Contract.Expiration}}",
        "type": "date",
        "description": "Expiration (??nh d?ng: dd/MM/yyyy)",
        "isRequired": true,
        "example": "01/01/2025"
      }
    ],
    "Customer": [
      {
        "name": "Name",
        "placeholder": "{{Customer.Name}}",
        "type": "string",
        "description": "Name",
        "isRequired": false,
        "example": "Nguy?n V?n A"
      },
      {
        "name": "Email",
        "placeholder": "{{Customer.Email}}",
        "type": "string",
        "description": "Email",
        "isRequired": false,
        "example": "example@company.com"
      },
      {
        "name": "PhoneNumber",
        "placeholder": "{{Customer.PhoneNumber}}",
        "type": "string",
        "description": "PhoneNumber",
        "isRequired": false,
        "example": "0912345678"
      },
      {
        "name": "CompanyName",
        "placeholder": "{{Customer.CompanyName}}",
        "type": "string",
        "description": "CompanyName",
        "isRequired": false,
        "example": "Sample text"
      }
    ],
    "SaleOrder": [...],
    "Service": [...],
    "User": [...]
  },
  "entityCount": 5,
  "totalFields": 87
}
```

#### Curl Example

```bash
curl -X GET "http://localhost:5000/api/DocumentTemplates/schema/placeholders?templateType=contract" \
  -H "Authorization: Bearer {token}"
```

---

### 2. Get Placeholders for Specific Entity

**GET** `/api/DocumentTemplates/schema/placeholders/{entityName}`

L?y danh sách fields c?a m?t entity c? th?.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `entityName` | `string` | ? | Tên entity: `Contract`, `Customer`, `SaleOrder`, `Service`, `Addon`, `User` |

#### Response Success (200 OK)

```json
{
  "success": true,
  "entity": "Customer",
  "placeholders": [
    {
      "name": "Id",
      "placeholder": "{{Customer.Id}}",
      "type": "number",
      "description": "Id",
      "isRequired": false,
      "example": "123"
    },
    {
      "name": "Name",
      "placeholder": "{{Customer.Name}}",
      "type": "string",
      "description": "Name",
      "isRequired": false,
      "example": "Nguy?n V?n A"
    },
    {
      "name": "Email",
      "placeholder": "{{Customer.Email}}",
      "type": "string",
      "description": "Email",
      "isRequired": false,
      "example": "example@company.com"
    },
    {
      "name": "CompanyName",
      "placeholder": "{{Customer.CompanyName}}",
      "type": "string",
      "description": "CompanyName",
      "isRequired": false,
      "example": "Sample text"
    },
    {
      "name": "TaxCode",
      "placeholder": "{{Customer.TaxCode}}",
      "type": "string",
      "description": "TaxCode",
      "isRequired": false,
      "example": "Sample text"
    }
  ],
  "count": 25
}
```

#### Response Error (404)

```json
{
  "success": false,
  "message": "Entity 'InvalidEntity' không t?n t?i trong schema"
}
```

#### Curl Example

```bash
curl -X GET "http://localhost:5000/api/DocumentTemplates/schema/placeholders/Customer" \
  -H "Authorization: Bearer {token}"
```

---

### 3. Get All Available Entities

**GET** `/api/DocumentTemplates/schema/entities`

L?y danh sách t?t c? entities có trong h? th?ng.

#### Response Success (200 OK)

```json
{
  "success": true,
  "entities": [
    "Contract",
    "Customer",
    "SaleOrder",
    "Service",
    "Addon",
    "User"
  ],
  "count": 6
}
```

#### Curl Example

```bash
curl -X GET "http://localhost:5000/api/DocumentTemplates/schema/entities" \
  -H "Authorization: Bearer {token}"
```

---

### 4. Validate Placeholders Schema

**POST** `/api/DocumentTemplates/schema/validate-placeholders`

Ki?m tra xem các placeholders có h?p l? v?i template type không.

#### Request Body

```json
{
  "placeholders": [
    "{{Contract.NumberContract}}",
    "{{Customer.Name}}",
    "{{Customer.Email}}",
    "{{InvalidField}}",
    "{{Customer.NonExistentProperty}}"
  ],
  "templateType": "contract"
}
```

#### Response Success - Valid (200 OK)

```json
{
  "success": true,
  "message": "T?t c? placeholders ??u h?p l?",
  "isValid": true,
  "validatedCount": 3
}
```

#### Response Error - Invalid (400 Bad Request)

```json
{
  "success": false,
  "message": "Có placeholders không h?p l?",
  "isValid": false,
  "invalidPlaceholders": [
    "{{InvalidField}}",
    "{{Customer.NonExistentProperty}}"
  ]
}
```

#### Curl Example

```bash
curl -X POST "http://localhost:5000/api/DocumentTemplates/schema/validate-placeholders" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "placeholders": ["{{Contract.NumberContract}}", "{{Customer.Name}}"],
    "templateType": "contract"
  }'
```

---

### 5. Render Template with Object Data (NEW!)

**POST** `/api/DocumentTemplates/render-with-object/{id}`

Render template v?i structured object data (h? tr? nested properties).

Thay vì truy?n flat dictionary, b?n có th? truy?n object có c?u trúc:

#### Request Body

```json
{
  "Contract": {
    "Id": 1,
    "NumberContract": 123,
    "Status": "Active",
    "TotalAmount": 15000000,
    "Expiration": "2025-12-31T00:00:00Z",
    "Notes": "H?p ??ng cung c?p d?ch v? website"
  },
  "Customer": {
    "Id": 45,
    "Name": "Nguy?n V?n A",
    "Email": "nguyenvana@company.com",
    "PhoneNumber": "0912345678",
    "CompanyName": "Công ty TNHH ABC",
    "CompanyAddress": "123 Nguy?n Trãi, Hà N?i",
    "TaxCode": "0123456789",
    "RepresentativeName": "Nguy?n V?n A",
    "RepresentativePosition": "Giám ??c"
  },
  "SaleOrder": {
    "Id": 78,
    "Title": "??n hàng thi?t k? website",
    "Value": 15000000,
    "Status": "Approved"
  },
  "User": {
    "FullName": "Tr?n Th? B",
    "Email": "tranthib@company.com",
    "Department": "Sales"
  }
}
```

#### Template Example

```html
<html>
  <body>
    <h1>H?P ??NG S? {{Contract.NumberContract}}/2025</h1>
    <p>Tr?ng thái: {{Contract.Status}}</p>
    
    <h2>THÔNG TIN KHÁCH HÀNG</h2>
    <p>Tên: {{Customer.Name}}</p>
    <p>Công ty: {{Customer.CompanyName}}</p>
    <p>Email: {{Customer.Email}}</p>
    <p>S? ?i?n tho?i: {{Customer.PhoneNumber}}</p>
    <p>Mã s? thu?: {{Customer.TaxCode}}</p>
    
    <h2>THÔNG TIN ??N HÀNG</h2>
    <p>Tiêu ??: {{SaleOrder.Title}}</p>
    <p>Giá tr?: {{SaleOrder.Value}} VN?</p>
    
    <h2>NHÂN VIÊN PH? TRÁCH</h2>
    <p>{{User.FullName}} - {{User.Department}}</p>
    
    <p>T?ng ti?n: {{Contract.TotalAmount}} VN?</p>
    <p>H?n h?p ??ng: {{Contract.Expiration}}</p>
  </body>
</html>
```

#### Response Success (200 OK)

**Content-Type:** `text/html; charset=utf-8`

```html
<html>
  <body>
    <h1>H?P ??NG S? 123/2025</h1>
    <p>Tr?ng thái: Active</p>
    
    <h2>THÔNG TIN KHÁCH HÀNG</h2>
    <p>Tên: Nguy?n V?n A</p>
    <p>Công ty: Công ty TNHH ABC</p>
    <p>Email: nguyenvana@company.com</p>
    <p>S? ?i?n tho?i: 0912345678</p>
    <p>Mã s? thu?: 0123456789</p>
    
    <h2>THÔNG TIN ??N HÀNG</h2>
    <p>Tiêu ??: ??n hàng thi?t k? website</p>
    <p>Giá tr?: 15,000,000 VN?</p>
    
    <h2>NHÂN VIÊN PH? TRÁCH</h2>
    <p>Tr?n Th? B - Sales</p>
    
    <p>T?ng ti?n: 15,000,000 VN?</p>
    <p>H?n h?p ??ng: 31/12/2025</p>
  </body>
</html>
```

#### Curl Example

```bash
curl -X POST "http://localhost:5000/api/DocumentTemplates/render-with-object/5" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "Contract": {
      "NumberContract": 123,
      "Status": "Active",
      "TotalAmount": 15000000
    },
    "Customer": {
      "Name": "Nguy?n V?n A",
      "CompanyName": "Công ty ABC"
    }
  }' \
  --output rendered.html
```

---

### 6. Render Template by Code with Object

**POST** `/api/DocumentTemplates/render-with-object-by-code/{code}`

T??ng t? endpoint trên nh?ng render theo template code thay vì ID.

---

## ?? Use Cases

### Use Case 1: Template Editor - Hi?n Th? Danh Sách Placeholders

```typescript
// 1. L?y danh sách entities
const entitiesResponse = await fetch('/api/DocumentTemplates/schema/entities', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const { entities } = await entitiesResponse.json();

// 2. L?y placeholders cho t?ng entity
const placeholdersData = {};
for (const entity of entities) {
  const response = await fetch(
    `/api/DocumentTemplates/schema/placeholders/${entity}`,
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  const { placeholders } = await response.json();
  placeholdersData[entity] = placeholders;
}

// 3. Hi?n th? trong UI theo nhóm
console.log(placeholdersData);
/*
{
  "Contract": [
    { name: "NumberContract", placeholder: "{{Contract.NumberContract}}", ... },
    { name: "TotalAmount", placeholder: "{{Contract.TotalAmount}}", ... }
  ],
  "Customer": [
    { name: "Name", placeholder: "{{Customer.Name}}", ... },
    { name: "Email", placeholder: "{{Customer.Email}}", ... }
  ]
}
*/
```

---

### Use Case 2: Auto-suggest Placeholders Khi Ng??i Dùng Gõ

```typescript
// Khi ng??i dùng gõ "{{" trong editor
const templateType = 'contract'; // L?y t? form

const response = await fetch(
  `/api/DocumentTemplates/schema/placeholders?templateType=${templateType}`,
  { headers: { 'Authorization': `Bearer ${token}` } }
);

const { data } = await response.json();

// Flatten t?t c? placeholders ?? hi?n th? trong autocomplete dropdown
const allPlaceholders = [];
for (const [entity, fields] of Object.entries(data)) {
  for (const field of fields) {
    allPlaceholders.push({
      label: field.placeholder,
      detail: `${entity}.${field.name} (${field.type})`,
      example: field.example,
      insertText: field.placeholder
    });
  }
}

// Hi?n th? trong autocomplete dropdown c?a editor
console.log(allPlaceholders);
```

---

### Use Case 3: Validate Template Tr??c Khi L?u

```typescript
// Sau khi ng??i dùng nh?p xong HTML template
const htmlContent = `
  <html>
    <body>
      <h1>H?p ??ng {{Contract.NumberContract}}</h1>
      <p>Khách hàng: {{Customer.Name}}</p>
      <p>Invalid: {{SomeInvalidField}}</p>
    </body>
  </html>
`;

// 1. Extract placeholders t? HTML
const extractResponse = await fetch('/api/DocumentTemplates/extract-placeholders', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ htmlContent })
});
const { placeholders } = await extractResponse.json();

// 2. Validate placeholders v?i schema
const validateResponse = await fetch('/api/DocumentTemplates/schema/validate-placeholders', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    placeholders: placeholders,
    templateType: 'contract'
  })
});

const validation = await validateResponse.json();

if (!validation.isValid) {
  alert(`Invalid placeholders: ${validation.invalidPlaceholders.join(', ')}`);
  // Highlight các placeholders không h?p l? trong editor
} else {
  // Cho phép l?u template
  console.log('Template is valid!');
}
```

---

### Use Case 4: Render Contract v?i Data T? Database

```typescript
// Backend: L?y data t? DB và render
async function renderContractPdf(contractId: number) {
  // 1. L?y contract v?i related entities
  const contract = await db.contracts.findOne({
    where: { id: contractId },
    include: [
      { model: 'Customer' },
      { model: 'SaleOrder', include: ['Service', 'Addon'] },
      { model: 'User' }
    ]
  });

  // 2. Chu?n b? object data
  const data = {
    Contract: {
      Id: contract.id,
      NumberContract: contract.numberContract,
      Status: contract.status,
      TotalAmount: contract.totalAmount,
      SubTotal: contract.subTotal,
      TaxAmount: contract.taxAmount,
      Expiration: contract.expiration,
      Notes: contract.notes
    },
    Customer: {
      Name: contract.saleOrder.customer.name,
      Email: contract.saleOrder.customer.email,
      PhoneNumber: contract.saleOrder.customer.phoneNumber,
      CompanyName: contract.saleOrder.customer.companyName,
      CompanyAddress: contract.saleOrder.customer.companyAddress,
      TaxCode: contract.saleOrder.customer.taxCode,
      RepresentativeName: contract.saleOrder.customer.representativeName,
      RepresentativeEmail: contract.saleOrder.customer.representativeEmail
    },
    SaleOrder: {
      Id: contract.saleOrder.id,
      Title: contract.saleOrder.title,
      Value: contract.saleOrder.value,
      Status: contract.saleOrder.status
    },
    User: {
      FullName: contract.user.fullName,
      Email: contract.user.email,
      Department: contract.user.department
    }
  };

  // 3. Render template
  const response = await fetch(
    `/api/DocumentTemplates/render-with-object-by-code/CONTRACT_DEFAULT`,
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    }
  );

  const html = await response.text();
  
  // 4. Convert to PDF (n?u c?n)
  // ...
  
  return html;
}
```

---

## ?? Frontend Component Example

### Placeholder Selector Component

```typescript
import React, { useState, useEffect } from 'react';

interface PlaceholderField {
  name: string;
  placeholder: string;
  type: string;
  description: string;
  isRequired: boolean;
  example: string;
}

interface Props {
  templateType: string;
  onInsertPlaceholder: (placeholder: string) => void;
}

const PlaceholderSelector: React.FC<Props> = ({ templateType, onInsertPlaceholder }) => {
  const [placeholders, setPlaceholders] = useState<Record<string, PlaceholderField[]>>({});
  const [selectedEntity, setSelectedEntity] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetchPlaceholders();
  }, [templateType]);

  const fetchPlaceholders = async () => {
    try {
      const response = await fetch(
        `/api/DocumentTemplates/schema/placeholders?templateType=${templateType}`,
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );
      const result = await response.json();
      setPlaceholders(result.data);
      
      // Auto-select first entity
      const firstEntity = Object.keys(result.data)[0];
      setSelectedEntity(firstEntity);
    } catch (error) {
      console.error('Error fetching placeholders:', error);
    }
  };

  const filteredPlaceholders = selectedEntity && placeholders[selectedEntity]
    ? placeholders[selectedEntity].filter(p =>
        p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.placeholder.toLowerCase().includes(searchTerm.toLowerCase())
      )
    : [];

  return (
    <div className="placeholder-selector">
      <h3>?? Chèn Bi?n ??ng</h3>
      
      {/* Entity Tabs */}
      <div className="entity-tabs">
        {Object.keys(placeholders).map(entity => (
          <button
            key={entity}
            className={selectedEntity === entity ? 'active' : ''}
            onClick={() => setSelectedEntity(entity)}
          >
            {entity} ({placeholders[entity].length})
          </button>
        ))}
      </div>

      {/* Search */}
      <input
        type="text"
        placeholder="?? Tìm ki?m placeholder..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        className="search-input"
      />

      {/* Placeholder List */}
      <div className="placeholder-list">
        {filteredPlaceholders.map(field => (
          <div
            key={field.name}
            className="placeholder-item"
            onClick={() => onInsertPlaceholder(field.placeholder)}
          >
            <div className="placeholder-name">
              <code>{field.placeholder}</code>
              {field.isRequired && <span className="required">*</span>}
            </div>
            <div className="placeholder-meta">
              <span className="type">{field.type}</span>
              <span className="example">VD: {field.example}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default PlaceholderSelector;
```

---

## ?? So Sánh: C? vs M?i

### ? Cách C? (Flat Dictionary)

```typescript
// Render API c? - khó qu?n lý khi có nhi?u entities
const data = {
  "ContractNumberContract": "123",
  "ContractTotalAmount": "15000000",
  "CustomerName": "Nguy?n V?n A",
  "CustomerEmail": "email@example.com",
  "CustomerCompanyName": "Công ty ABC",
  "SaleOrderTitle": "??n hàng XYZ",
  "UserFullName": "Tr?n Th? B"
  // ... R?t nhi?u fields khác, khó nh?
};

// Template ph?i dùng flat placeholders
<h1>{{ContractNumberContract}}</h1>
<p>{{CustomerName}}</p>
```

**V?n ??:**
- ? Khó nh? tên bi?n (ContractNumberContract hay NumberContract?)
- ? Không bi?t field nào thu?c entity nào
- ? Không có autocomplete/suggestions
- ? Khó validate

---

### ? Cách M?i (Structured + Schema)

```typescript
// Render API m?i - structured object
const data = {
  Contract: {
    NumberContract: 123,
    TotalAmount: 15000000
  },
  Customer: {
    Name: "Nguy?n V?n A",
    Email: "email@example.com",
    CompanyName: "Công ty ABC"
  },
  SaleOrder: {
    Title: "??n hàng XYZ"
  },
  User: {
    FullName: "Tr?n Th? B"
  }
};

// Template dùng nested placeholders (rõ ràng h?n)
<h1>{{Contract.NumberContract}}</h1>
<p>{{Customer.Name}}</p>
<p>{{Customer.Email}}</p>

// HO?C v?n dùng flat format cho backward compatibility
<h1>{{NumberContract}}</h1>
<p>{{Name}}</p>
```

**?u ?i?m:**
- ? Rõ ràng: `{{Contract.NumberContract}}` - bi?t ngay thu?c entity nào
- ? Auto-suggest: Gõ `{{Contract.` s? hi?n t?t c? fields c?a Contract
- ? Validation: Bi?t ???c field nào h?p l?, field nào không
- ? Type-aware: Bi?t type c?a field ?? format ?úng (date, number...)
- ? Backward compatible: V?n h? tr? `{{FieldName}}` n?u không mu?n dùng prefix

---

## ?? Template Type Mappings

| Template Type | Relevant Entities | Use Cases |
|---------------|-------------------|-----------|
| `contract` | Contract, Customer, SaleOrder, Service, User | H?p ??ng cung c?p d?ch v? |
| `quote` | Customer, SaleOrder, Service, Addon, User | Báo giá |
| `invoice` | Contract, Customer, SaleOrder, Service | Hóa ??n |
| `salary_notification` | User | Thông báo l??ng |
| `email` | Customer, User | Email marketing, thông báo |
| `notification` | User, Customer | Push notifications |

---

## ?? Migration Guide

### N?u B?n ?ang Dùng API C?

H? th?ng **hoàn toàn backward compatible**. B?n có th?:

1. **Ti?p t?c dùng flat dictionary** v?i `/render/{id}` và `/render-by-code/{code}`
2. **Upgrade t? t?** - chuy?n sang structured object v?i endpoints m?i
3. **Dùng c? hai cùng lúc** - templates c? dùng flat, templates m?i dùng nested

### Upgrade Steps

```typescript
// B??C 1: Thêm schema endpoints vào service
class TemplateService {
  async getAvailablePlaceholders(templateType: string) {
    const response = await axios.get(
      `${API_URL}/api/DocumentTemplates/schema/placeholders`,
      { params: { templateType } }
    );
    return response.data;
  }

  async validatePlaceholders(placeholders: string[], templateType: string) {
    const response = await axios.post(
      `${API_URL}/api/DocumentTemplates/schema/validate-placeholders`,
      { placeholders, templateType }
    );
    return response.data;
  }
}

// B??C 2: Update UI ?? hi?n th? placeholders theo nhóm
// (Xem component example phía trên)

// B??C 3: Khi render, dùng endpoint m?i v?i structured object
async function renderContract(contractId: number) {
  // L?y data t? backend
  const contractData = await fetchContractData(contractId);
  
  // Render v?i object
  const response = await axios.post(
    `${API_URL}/api/DocumentTemplates/render-with-object-by-code/CONTRACT_DEFAULT`,
    contractData
  );
  
  return response.data;
}
```

---

## ? Best Practices

1. **Dùng nested syntax trong templates m?i**: `{{Entity.Property}}` rõ ràng h?n `{{Property}}`
2. **Validate tr??c khi l?u template**: Tránh typo và placeholders không t?n t?i
3. **Hi?n th? schema trong UI**: Giúp ng??i dùng bi?t có nh?ng field nào available
4. **Cung c?p examples**: Hi?n th? giá tr? m?u cho m?i placeholder
5. **Group theo entity**: D? tìm ki?m h?n là list dài không có c?u trúc

---

## ?? Roadmap

- [ ] H? tr? array/loop placeholders: `{{#foreach Items}}...{{/foreach}}`
- [ ] Conditional rendering: `{{#if Contract.Status == 'Active'}}...{{/if}}`
- [ ] Custom formatters: `{{Contract.TotalAmount | currency}}`
- [ ] Multi-language support cho field descriptions
- [ ] Visual template builder (drag & drop)

---

**Last Updated:** 2024-12-31  
**API Version:** 3.0  
**Status:** ? Ready for Production

---

## ?? Quick Examples

### Example 1: Contract Template

```html
<!DOCTYPE html>
<html>
<head>
    <title>H?p ??ng {{Contract.NumberContract}}/2025</title>
</head>
<body>
    <h1>H?P ??NG CUNG C?P D?CH V?</h1>
    <p>S?: {{Contract.NumberContract}}/2025-TTWS</p>
    <p>Ngày: {{Contract.CreatedAt}}</p>
    
    <h2>BÊN A: {{Customer.CompanyName}}</h2>
    <p>??a ch?: {{Customer.CompanyAddress}}</p>
    <p>Mã s? thu?: {{Customer.TaxCode}}</p>
    <p>Ng??i ??i di?n: {{Customer.RepresentativeName}}</p>
    <p>Ch?c v?: {{Customer.RepresentativePosition}}</p>
    <p>Email: {{Customer.RepresentativeEmail}}</p>
    <p>?i?n tho?i: {{Customer.RepresentativePhone}}</p>
    
    <h2>BÊN B: Công ty Tr??ng Thành Web</h2>
    <p>Ng??i ph? trách: {{User.FullName}}</p>
    <p>Email: {{User.Email}}</p>
    
    <h2>N?I DUNG H?P ??NG</h2>
    <p>Tiêu ??: {{SaleOrder.Title}}</p>
    <p>T?ng ti?n ch?a thu?: {{Contract.SubTotal}} VN?</p>
    <p>Thu? VAT: {{Contract.TaxAmount}} VN?</p>
    <p>T?ng thanh toán: {{Contract.TotalAmount}} VN?</p>
    <p>H?n h?p ??ng: {{Contract.Expiration}}</p>
    <p>Ghi chú: {{Contract.Notes}}</p>
</body>
</html>
```

### Example 2: Email Notification Template

```html
<!DOCTYPE html>
<html>
<body>
    <h1>Xin chào {{Customer.Name}}!</h1>
    
    <p>C?m ?n b?n ?ã s? d?ng d?ch v? c?a chúng tôi.</p>
    
    <p>
        Email này ???c g?i b?i: <strong>{{User.FullName}}</strong>
        t? phòng ban {{User.Department}}.
    </p>
    
    <p>
        N?u có b?t k? th?c m?c nào, vui lòng liên h?:<br>
        Email: {{User.Email}}<br>
        Ho?c: {{Customer.TechContactEmail}}
    </p>
</body>
</html>
```

---

Bây gi? b?n có m?t h? th?ng qu?n lý placeholders hoàn ch?nh và có c?u trúc! ??
