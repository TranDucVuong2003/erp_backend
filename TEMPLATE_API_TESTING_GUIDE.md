# Quick Testing Guide for Template API Fix

## Test 1: Verify Templates in Database
```http
GET https://localhost:7210/api/DocumentTemplates?type=salary_report
```

**Expected Response**:
```json
{
  "success": true,
  "totalCount": 1,
  "data": [
    {
      "id": 2,
      "name": "Báo Cáo Th?ng Kê L??ng (M?c ??nh)",
      "templateType": "salary_report",
      "code": "SALARY_REPORT_DEFAULT",
      "htmlContent": "<!DOCTYPE html>\\r\\n<html>\\r\\n...",
      "isActive": true,
      "isDefault": true
    }
  ]
}
```

**Note**: The `\\r\\n` and `\\"` you see are **normal JSON escaping**. This is correct!

---

## Test 2: Get Raw HTML (No JSON Escaping)
```http
GET https://localhost:7210/api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT/raw-html
```

**Expected**: Direct HTML content displayed in browser
```html
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <title>Báo Cáo L??ng Tháng</title>
...
```

---

## Test 3: Generate Salary Report Preview (HTML)
```http
POST https://localhost:7210/api/Payslips/generate-salary-report-preview
Content-Type: application/json

{
  "month": 12,
  "year": 2024,
  "departmentId": null,
  "createdByName": "Admin"
}
```

**Expected**: HTML string with replaced placeholders

---

## Test 4: Generate Salary Report PDF
```http
POST https://localhost:7210/api/Payslips/generate-salary-report
Content-Type: application/json

{
  "month": 12,
  "year": 2024,
  "departmentId": null,
  "createdByName": "Admin"
}
```

**Expected**: PDF file download with properly rendered salary report

---

## Understanding JSON Escaping

### What You See in Postman/API:
```json
{
  "htmlContent": "<!DOCTYPE html>\\r\\n<html lang=\\"vi\\">\\r\\n"
}
```

### What Your Code Actually Receives (C#):
```csharp
var content = template.HtmlContent;
// Value: "<!DOCTYPE html>\r\n<html lang=\"vi\">\r\n"
```

### What JavaScript Receives After JSON.parse():
```javascript
const data = await response.json();
console.log(data.htmlContent);
// Output: <!DOCTYPE html>
// <html lang="vi">
```

---

## Common Mistakes to Avoid

### ? WRONG: Manual Unescaping
```csharp
// Don't do this!
htmlContent = htmlContent
    .Replace("\\r\\n", "\r\n")
    .Replace("\\\"", "\"");
```

**Why it's wrong**: The HTML from database is already clean. JSON escaping only happens during API serialization, not in your C# code.

### ? CORRECT: Use As-Is
```csharp
// Just use it directly
var htmlContent = template.HtmlContent;
var pdf = await _pdfService.ConvertHtmlToPdfAsync(htmlContent);
```

---

## Diagnostic Tools

### Check Database Directly (SQL Server Management Studio)
```sql
SELECT TOP 1
    Code,
    SUBSTRING(HtmlContent, 1, 200) AS Preview,
    LEN(HtmlContent) AS ContentLength
FROM document_templates
WHERE Code = 'SALARY_REPORT_DEFAULT'
```

**Expected**: Clean HTML without `\r\n` or `\"` in the database

### Check API Response (Use /raw-html endpoint)
```bash
# This endpoint returns HTML directly (no JSON)
curl https://localhost:7210/api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT/raw-html

# Should output clean HTML
```

---

## If You Still See Issues

1. **Check if templates were migrated correctly**
   ```http
   POST https://localhost:7210/api/DocumentTemplates/migrate-from-files
   ```

2. **Verify template exists**
   ```http
   GET https://localhost:7210/api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT
   ```

3. **Test with raw HTML endpoint**
   ```http
   GET https://localhost:7210/api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT/raw-html
   ```

4. **Check logs** for any errors in PDF generation

---

## Summary

? **Template stored correctly** ? No escape sequences in database  
? **API returns JSON correctly** ? Escape sequences in JSON response are normal  
? **C# code receives clean HTML** ? No manual unescaping needed  
? **PDF generates correctly** ? HTML is properly formatted

The key insight: **JSON escaping is transparent**. Your C# code never sees `\\r\\n` or `\\"` - the JSON serializer handles that automatically.
