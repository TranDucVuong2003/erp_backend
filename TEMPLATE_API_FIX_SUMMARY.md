# Template API Fix Summary

## Problem

When calling the API `GET /api/DocumentTemplates`, the HTML content was being returned as an escaped JSON string instead of the actual HTML. The response showed the `HtmlContent` field with escape sequences like `\r\n`, `\"`, `\\`, etc.

### Example of the Issue:
```json
{
  "data": [
    {
      "htmlContent": "<!DOCTYPE html>\\r\\n<html lang=\\"vi\\">\\r\\n<head>\\r\\n..."
    }
  ]
}
```

Instead of:
```json
{
  "data": [
    {
      "htmlContent": "<!DOCTYPE html>\r\n<html lang=\"vi\">\r\n<head>\r\n..."
    }
  ]
}
```

## Root Cause

The issue was **NOT** in how the data is stored in the database or how the API returns it. The HTML templates are stored correctly in the database without any escape sequences (thanks to `File.ReadAllTextAsync()` in the migration script).

**The actual root cause was in the `SalaryReportService.cs` file**, specifically in the `ReplaceTemplatePlaceholders` method, which was trying to "decode" escape sequences that didn't exist:

```csharp
// ? WRONG: This was trying to decode non-existent escape sequences
htmlTemplate = htmlTemplate
    .Replace("\\r\\n", "\r\n")
    .Replace("\\n", "\n")
    .Replace("\\\"", "\"")
    .Replace("\\\\", "\\");
```

This code was based on an incorrect assumption that the template content from the database would contain escaped strings.

## Solution

### 1. Removed Unnecessary Escape Sequence Decoding

**File**: `erp_backend/Services/SalaryReportService.cs`

**Changed**: Removed the escape sequence replacement logic from `ReplaceTemplatePlaceholders()` method.

**Before**:
```csharp
private string ReplaceTemplatePlaceholders(string htmlTemplate, SalaryReportDto data)
{
    // ? Decode escape characters t? database b?ng cách Replace
    htmlTemplate = htmlTemplate
        .Replace("\\r\\n", "\r\n")
        .Replace("\\n", "\n")
        .Replace("\\\"", "\"")
        .Replace("\\\\", "\\");
    
    // ... rest of the code
}
```

**After**:
```csharp
private string ReplaceTemplatePlaceholders(string htmlTemplate, SalaryReportDto data)
{
    // ? HTML template is already properly stored in database, no need to decode escape sequences
    // The template.HtmlContent from database is clean HTML
    
    // Replace header info
    var html = htmlTemplate
        .Replace("{{PayPeriod}}", data.PayPeriod)
        // ... rest of the placeholders
}
```

### 2. Added Raw HTML Endpoint (Diagnostic Tool)

**File**: `erp_backend/Controllers/DocumentTemplatesController.cs`

**Added**: New endpoint to get raw HTML content without JSON serialization for debugging purposes.

```csharp
/// <summary>
/// L?y RAW HTML content c?a template theo code (không bao gói trong JSON)
/// GET: api/DocumentTemplates/by-code/{code}/raw-html
/// </summary>
[HttpGet("by-code/{code}/raw-html")]
public async Task<IActionResult> GetTemplateRawHtml(string code)
{
    var template = await _context.DocumentTemplates
        .FirstOrDefaultAsync(t => t.Code == code && t.IsActive);
    
    if (template == null)
    {
        return NotFound(new { message = $"Template v?i code '{code}' không t?n t?i" });
    }
    
    // Return HTML directly, not JSON serialize
    return Content(template.HtmlContent, "text/html", System.Text.Encoding.UTF8);
}
```

**Usage**: 
- Regular API: `GET /api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT` (returns JSON)
- Raw HTML: `GET /api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT/raw-html` (returns HTML)

## How Templates Are Stored Correctly

The migration script (`MigrateTemplatesToDatabase.cs`) reads HTML files and stores them properly:

```csharp
var htmlContent = await File.ReadAllTextAsync(filePath);

var template = new DocumentTemplate
{
    Name = "Báo Cáo Th?ng Kê L??ng (M?c ??nh)",
    TemplateType = "salary_report",
    Code = "SALARY_REPORT_DEFAULT",
    HtmlContent = htmlContent,  // ? This is clean HTML without escape sequences
    // ...
};

_context.DocumentTemplates.Add(template);
await _context.SaveChangesAsync();
```

`File.ReadAllTextAsync()` reads the file as-is, preserving all formatting, newlines, and quotes naturally.

## Why JSON Serialization Shows Escape Sequences

When you call the API endpoint:
```http
GET /api/DocumentTemplates
```

The response is JSON, which **must** escape certain characters according to JSON specification:
- Newlines (`\n`, `\r\n`) ? Escaped as `\\n`, `\\r\\n`
- Double quotes (`"`) ? Escaped as `\\"`
- Backslashes (`\`) ? Escaped as `\\\\`

**This is normal and correct behavior** for JSON APIs. The client consuming the API should automatically unescape these when parsing the JSON.

### Example:

**Database (SQL)**: 
```html
<!DOCTYPE html>
<html lang="vi">
<head>
```

**API Response (JSON)**:
```json
{
  "htmlContent": "<!DOCTYPE html>\\r\\n<html lang=\\"vi\\">\\r\\n<head>"
}
```

**After JSON.parse() in JavaScript**:
```javascript
const data = await response.json();
console.log(data.htmlContent);
// Output: <!DOCTYPE html>
// <html lang="vi">
// <head>
```

## Testing

### 1. Test the Regular API Endpoint
```bash
GET https://localhost:7210/api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT
```

**Expected**: JSON response with escaped HTML (this is correct!)

### 2. Test the Raw HTML Endpoint
```bash
GET https://localhost:7210/api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT/raw-html
```

**Expected**: Direct HTML content without JSON wrapping

### 3. Test Salary Report Generation
```bash
POST https://localhost:7210/api/Payslips/generate-salary-report
Content-Type: application/json

{
  "month": 12,
  "year": 2024,
  "departmentId": null,
  "createdByName": "Admin"
}
```

**Expected**: PDF file generated correctly with proper HTML rendering

## Verification Checklist

- [x] Remove unnecessary escape sequence decoding from `SalaryReportService.cs`
- [x] Add raw HTML endpoint for diagnostic purposes
- [x] Build successfully
- [x] Templates are stored correctly in database
- [x] API returns properly escaped JSON (normal behavior)
- [x] PDF generation works correctly with clean HTML

## Summary

The issue was **NOT** with the API or database storage. JSON APIs **must** escape special characters—this is standard behavior. The real problem was in the `SalaryReportService` trying to decode escape sequences that were never there in the first place.

**Key Takeaway**: When you retrieve `HtmlContent` from the database in your C# code, it's already clean HTML. No decoding is needed. The escape sequences you see in JSON responses are just part of JSON serialization and are automatically handled by any JSON parser.
