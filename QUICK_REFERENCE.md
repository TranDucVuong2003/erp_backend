# ?? Template Editor API - Quick Reference Card

## ?? 5 New Endpoints

```
POST   /api/DocumentTemplates/extract-placeholders      ? Detect {{placeholders}}
GET    /api/DocumentTemplates/with-placeholders/{id}    ? Get template + placeholders
POST   /api/DocumentTemplates/validate/{id}             ? Validate data before render
POST   /api/DocumentTemplates/render/{id}               ? Render by ID
POST   /api/DocumentTemplates/render-by-code/{code}     ? Render by code
```

---

## ?? Quick Test Commands

### 1. Extract Placeholders
```bash
curl -X POST "http://localhost:5000/api/DocumentTemplates/extract-placeholders" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"htmlContent": "<h1>{{Name}}</h1><p>{{Email}}</p>"}'
```

**Expected Response:**
```json
{
  "success": true,
  "placeholders": ["Email", "Name"],
  "count": 2
}
```

---

### 2. Get Template With Placeholders
```bash
curl -X GET "http://localhost:5000/api/DocumentTemplates/with-placeholders/1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 3. Validate Data
```bash
curl -X POST "http://localhost:5000/api/DocumentTemplates/validate/1" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Name": "John Doe", "Email": "john@example.com"}'
```

**Expected Response (Valid):**
```json
{
  "success": true,
  "isValid": true,
  "providedFields": ["Name", "Email"]
}
```

**Expected Response (Invalid):**
```json
{
  "success": false,
  "isValid": false,
  "missingPlaceholders": ["Phone"]
}
```

---

### 4. Render Template by ID
```bash
curl -X POST "http://localhost:5000/api/DocumentTemplates/render/1" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Name": "John Doe", "Email": "john@example.com"}' \
  --output rendered.html
```

---

### 5. Render Template by Code
```bash
curl -X POST "http://localhost:5000/api/DocumentTemplates/render-by-code/SALARY_NOTIFY_V2" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "EmployeeName": "Nguy?n V?n A",
    "BaseSalary": "15,000,000",
    "NetSalary": "13,500,000",
    "Month": "01/2026"
  }' \
  --output salary_notification.html
```

---

## ?? TypeScript Quick Integration

```typescript
import axios from 'axios';

const API_URL = 'http://localhost:5000';
const token = localStorage.getItem('token');

// 1. Extract placeholders
const extractPlaceholders = async (htmlContent: string) => {
  const { data } = await axios.post(
    `${API_URL}/api/DocumentTemplates/extract-placeholders`,
    { htmlContent },
    { headers: { Authorization: `Bearer ${token}` } }
  );
  return data.placeholders;
};

// 2. Validate data
const validateData = async (templateId: number, data: Record<string, string>) => {
  try {
    await axios.post(
      `${API_URL}/api/DocumentTemplates/validate/${templateId}`,
      data,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    return { isValid: true };
  } catch (error: any) {
    return {
      isValid: false,
      missing: error.response?.data?.missingPlaceholders
    };
  }
};

// 3. Render template
const renderTemplate = async (templateId: number, data: Record<string, string>) => {
  const { data: html } = await axios.post(
    `${API_URL}/api/DocumentTemplates/render/${templateId}`,
    data,
    {
      headers: { Authorization: `Bearer ${token}` },
      responseType: 'text'
    }
  );
  return html;
};

// Usage example
const preview = async () => {
  const placeholders = await extractPlaceholders('<h1>{{Name}}</h1>');
  console.log('Detected:', placeholders); // ["Name"]
  
  const validation = await validateData(1, { Name: 'John' });
  if (validation.isValid) {
    const html = await renderTemplate(1, { Name: 'John' });
    // Display html in iframe
  }
};
```

---

## ?? Complete Workflow Example

```typescript
// Step 1: User types HTML in editor
const htmlContent = `
  <html>
    <body>
      <h1>Hello {{Name}}</h1>
      <p>Email: {{Email}}</p>
    </body>
  </html>
`;

// Step 2: Auto-detect placeholders
const placeholders = await extractPlaceholders(htmlContent);
// ["Email", "Name"]

// Step 3: User provides sample data
const sampleData = {
  Name: "John Doe",
  Email: "john@example.com"
};

// Step 4: Validate before preview
const validation = await validateData(templateId, sampleData);
if (!validation.isValid) {
  alert(`Missing: ${validation.missing.join(', ')}`);
  return;
}

// Step 5: Render preview
const previewHtml = await renderTemplate(templateId, sampleData);

// Step 6: Show in modal
setPreviewHtml(previewHtml);
```

---

## ?? React Component Snippet

```tsx
const TemplateEditor = () => {
  const [html, setHtml] = useState('');
  const [placeholders, setPlaceholders] = useState<string[]>([]);
  const [previewHtml, setPreviewHtml] = useState('');

  // Auto-detect on typing (debounced)
  useEffect(() => {
    const timer = setTimeout(async () => {
      if (html) {
        const detected = await extractPlaceholders(html);
        setPlaceholders(detected);
      }
    }, 500);
    return () => clearTimeout(timer);
  }, [html]);

  const handlePreview = async () => {
    const sampleData = {
      Name: 'Sample Name',
      Email: 'sample@example.com'
    };

    const validation = await validateData(templateId, sampleData);
    if (!validation.isValid) {
      alert(`Missing: ${validation.missing.join(', ')}`);
      return;
    }

    const rendered = await renderTemplate(templateId, sampleData);
    setPreviewHtml(rendered);
  };

  return (
    <div>
      <textarea value={html} onChange={e => setHtml(e.target.value)} />
      
      <div>
        <h3>Detected Placeholders ({placeholders.length})</h3>
        {placeholders.map(p => (
          <div key={p}>{{p}}</div>
        ))}
      </div>

      <button onClick={handlePreview}>Preview</button>

      {previewHtml && (
        <iframe srcDoc={previewHtml} />
      )}
    </div>
  );
};
```

---

## ?? Common Placeholder Examples

### Salary Notification
```json
{
  "EmployeeName": "Nguy?n V?n A",
  "BaseSalary": "15,000,000",
  "NetSalary": "13,500,000",
  "Month": "01/2026",
  "Department": "IT Department",
  "CurrentDate": "31/12/2024",
  "CompanyName": "Công ty ABC"
}
```

### Email Template
```json
{
  "Username": "john_doe",
  "Email": "john@example.com",
  "OTPCode": "123456",
  "PasswordResetLink": "https://app.com/reset?token=xxx"
}
```

### Contract Template
```json
{
  "CustomerName": "Công ty XYZ",
  "CustomerAddress": "123 ???ng ABC",
  "ContractNumber": "HD-2024-001",
  "ContractDate": "31/12/2024",
  "TotalAmount": "50,000,000",
  "ServiceName": "Phát tri?n ph?n m?m"
}
```

---

## ? Performance Tips

1. **Debounce Extract Placeholders**
   ```typescript
   useEffect(() => {
     const timer = setTimeout(() => extractPlaceholders(), 500);
     return () => clearTimeout(timer);
   }, [htmlContent]);
   ```

2. **Cache Templates**
   ```typescript
   const [templateCache, setTemplateCache] = useState({});
   ```

3. **Lazy Load Preview**
   ```typescript
   const [isPreviewOpen, setIsPreviewOpen] = useState(false);
   // Only render when modal opens
   ```

---

## ??? Troubleshooting

### Error: "Template không t?n t?i"
- ? Check template ID/code is correct
- ? Check template.IsActive = true

### Error: "Missing placeholders"
- ? Run validate endpoint first
- ? Check placeholder names match (case-insensitive)
- ? Ensure all {{}} placeholders have data

### Error: "Unauthorized"
- ? Check JWT token is valid
- ? Check token not expired
- ? Check Authorization header format: `Bearer {token}`

---

## ?? HTTP Status Codes

| Code | Meaning | Action |
|------|---------|--------|
| 200 | Success | Process response |
| 400 | Bad Request | Check request body |
| 401 | Unauthorized | Refresh token |
| 404 | Not Found | Check template ID/code |
| 500 | Server Error | Check logs, contact admin |

---

## ?? Full Documentation

- **API Reference:** [TEMPLATE_EDITOR_API_DOCUMENTATION.md](./TEMPLATE_EDITOR_API_DOCUMENTATION.md)
- **Recovery Summary:** [RECOVERY_SUMMARY.md](./RECOVERY_SUMMARY.md)
- **Frontend Guide:** [FRONTEND_TEMPLATE_EDITOR_GUIDE.md](./FRONTEND_TEMPLATE_EDITOR_GUIDE.md)

---

**Quick Start:**
```bash
# 1. Start backend
cd erp_backend
dotnet run

# 2. Test endpoint
curl http://localhost:5000/api/DocumentTemplates/extract-placeholders \
  -X POST \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"htmlContent": "<h1>{{Test}}</h1>"}'

# 3. Build frontend integration
npm install axios
# Copy TypeScript code above
```

**Status:** ? Ready to Use

---

**Last Updated:** 2024-12-31
