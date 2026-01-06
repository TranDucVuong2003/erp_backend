# Cách HTML ???c l?u tr? trong Database

## ? Tình tr?ng hi?n t?i: HTML ?Ã ???c l?u THU?N trong Database

### 1. Xác nh?n: Database l?u HTML thu?n (Plain HTML)

Khi b?n ch?y migration script:

```csharp
var htmlContent = await File.ReadAllTextAsync(filePath);

var template = new DocumentTemplate
{
    HtmlContent = htmlContent,  // ? ?ây là HTML thu?n, không có escape sequences
    // ...
};

_context.DocumentTemplates.Add(template);
await _context.SaveChangesAsync();
```

**Entity Framework Core** t? ??ng l?u string vào SQL Server **AS-IS** (nguyên tr?ng), không có b?t k? escape nào.

### 2. T?i sao b?n th?y `\r\n` và `\"` trong Postman?

#### ?? ?i?u quan tr?ng: ?ây là JSON Encoding, KHÔNG ph?i data trong database!

Khi b?n g?i API:
```http
GET /api/DocumentTemplates
```

ASP.NET Core **PH?I** encode HTML thành JSON theo chu?n JSON specification:

| Ký t? trong HTML | Trong JSON Response |
|------------------|---------------------|
| Xu?ng dòng `\n`  | `\\n` |
| Carriage return `\r` | `\\r` |
| D?u ngo?c kép `"` | `\\"` |
| Backslash `\` | `\\\\` |

**Ví d?:**

**File HTML g?c** (`SalaryReportTemplate.html`):
```html
<!DOCTYPE html>
<html lang="vi">
<head>
    <title>Báo Cáo</title>
```

**Trong SQL Server** (column `HtmlContent`):
```html
<!DOCTYPE html>
<html lang="vi">
<head>
    <title>Báo Cáo</title>
```

**API JSON Response** (Postman hi?n th?):
```json
{
  "htmlContent": "<!DOCTYPE html>\\r\\n<html lang=\\"vi\\">\\r\\n<head>\\r\\n    <title>Báo Cáo</title>"
}
```

**Trong C# code** (khi b?n ??c t? database):
```csharp
var template = await _context.DocumentTemplates.FirstAsync();
Console.WriteLine(template.HtmlContent);

// OUTPUT (chính xác nh? file g?c):
// <!DOCTYPE html>
// <html lang="vi">
// <head>
//     <title>Báo Cáo</title>
```

---

## ?? Cách ki?m tra HTML th?c s? trong Database

### Ph??ng pháp 1: Truy v?n SQL tr?c ti?p

M? **SQL Server Management Studio** và ch?y:

```sql
SELECT TOP 1
    Code,
    LEFT(HtmlContent, 200) AS HTMLPreview,
    LEN(HtmlContent) AS ContentLength
FROM document_templates
WHERE Code = 'SALARY_REPORT_DEFAULT'
```

**K?t qu? mong ??i:**
```
Code                  | HTMLPreview                                    | ContentLength
----------------------|------------------------------------------------|--------------
SALARY_REPORT_DEFAULT | <!DOCTYPE html>\r\n<html lang="vi">\r\n<head> | 5432
```

**Chú ý:** Trong SQL Server Management Studio, b?n s? th?y:
- Xu?ng dòng hi?n th? nh? xu?ng dòng th?t (không có ký t? `\n`)
- D?u ngo?c kép `"` hi?n th? bình th??ng
- Không có d?u backslash `\\` th?a

### Ph??ng pháp 2: S? d?ng endpoint `/raw-html`

Tôi ?ã t?o endpoint này ?? b?n xem HTML thu?n:

```http
GET https://localhost:7210/api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT/raw-html
```

**K?t qu?:** Trình duy?t s? hi?n th? HTML tr?c ti?p (không có JSON wrapping, không có escape sequences).

### Ph??ng pháp 3: Debug trong Visual Studio

??t breakpoint t?i file `SalaryReportService.cs`:

```csharp
private async Task<string> GenerateHtmlFromTemplateAsync(SalaryReportDto data)
{
    var template = await _context.DocumentTemplates
        .Where(t => t.Code == "SALARY_REPORT_DEFAULT" && t.IsActive)
        .FirstOrDefaultAsync();

    // ? ??T BREAKPOINT ? ?ÂY
    var htmlContent = template.HtmlContent;
    
    // Ki?m tra trong "Watch" window:
    // htmlContent[0] = '<'
    // htmlContent[15] = '\n'  (NOT '\\n')
    // htmlContent[30] = '"'   (NOT '\\"')
}
```

**Trong Watch window**, b?n s? th?y:
```
htmlContent = "<!DOCTYPE html>\r\n<html lang=\"vi\">\r\n..."
```

**Chú ý:**
- `\r\n` trong Watch window là **ký t? xu?ng dòng th?c**, không ph?i chu?i `"\\r\\n"`
- `\"` trong Watch window là **d?u ngo?c kép th?c**, không ph?i chu?i `"\\\""`

---

## ?? So sánh: JSON Encoding vs Database Storage

| N?i | Xu?ng dòng | D?u ngo?c kép | HTML Entity |
|-----|-----------|---------------|-------------|
| **File g?c** | `\n` (ASCII 10) | `"` (ASCII 34) | `<` |
| **Database** | `\n` (ASCII 10) | `"` (ASCII 34) | `<` |
| **C# string** | `\n` (ASCII 10) | `"` (ASCII 34) | `<` |
| **JSON API** | `\\n` (escaped) | `\\"` (escaped) | `<` |
| **JSON.parse()** | `\n` (ASCII 10) | `"` (ASCII 34) | `<` |

**K?t lu?n:** Ch? có **JSON API response** m?i có escape sequences. T?t c? n?i khác ??u là HTML thu?n.

---

## ? Sai l?m th??ng g?p

### 1. Ngh? r?ng c?n ph?i "decode" HTML t? database

```csharp
// ? SAI - Không c?n làm ?i?u này!
var html = template.HtmlContent
    .Replace("\\r\\n", "\r\n")
    .Replace("\\\"", "\"");
```

**T?i sao sai?** 
- Database ?ã l?u HTML thu?n r?i
- `template.HtmlContent` trong C# ?ã là HTML s?ch
- Không có `\\r\\n` hay `\\"` nào c?n replace c?!

### 2. Lo l?ng v? escape sequences trong Postman

```json
// Th?y trong Postman
{
  "htmlContent": "<!DOCTYPE html>\\r\\n..."
}
```

**?i?u này là ?ÚNG và B?T BU?C!**
- JSON spec yêu c?u escape các ký t? ??c bi?t
- Client (JavaScript, C#, Python...) t? ??ng unescape khi parse JSON
- N?u không escape, JSON s? không h?p l? (invalid)

---

## ? Cách s? d?ng ?úng trong Code

### 1. Trong Service (C#)

```csharp
public class SalaryReportService
{
    private async Task<string> GenerateHtmlFromTemplateAsync(SalaryReportDto data)
    {
        // ? L?y template t? database
        var template = await _context.DocumentTemplates
            .Where(t => t.Code == "SALARY_REPORT_DEFAULT")
            .FirstOrDefaultAsync();

        // ? Dùng tr?c ti?p, không c?n decode gì c?!
        var html = template.HtmlContent;

        // ? Replace placeholders
        html = html.Replace("{{PayPeriod}}", data.PayPeriod);
        html = html.Replace("{{TotalEmployees}}", data.TotalEmployees.ToString());

        return html;
    }

    public async Task<byte[]> GeneratePdfAsync(SalaryReportDto data)
    {
        var html = await GenerateHtmlFromTemplateAsync(data);
        
        // ? HTML s?ch, s?n sàng t?o PDF
        var pdf = await _pdfService.ConvertHtmlToPdfAsync(html);
        
        return pdf;
    }
}
```

### 2. Trong JavaScript Client

```javascript
// ? Fetch t? API
const response = await fetch('/api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT');
const data = await response.json();

// ? JSON.parse() t? ??ng unescape
console.log(data.data.htmlContent);
// OUTPUT: <!DOCTYPE html>
// <html lang="vi">
// <head>

// ? S? d?ng tr?c ti?p
document.getElementById('preview').innerHTML = data.data.htmlContent;
```

### 3. Trong React Client

```tsx
function TemplatePreview() {
    const [html, setHtml] = useState('');

    useEffect(() => {
        fetch('/api/DocumentTemplates/by-code/SALARY_REPORT_DEFAULT')
            .then(res => res.json())
            .then(data => {
                // ? data.data.htmlContent ?ã là HTML s?ch
                setHtml(data.data.htmlContent);
            });
    }, []);

    return (
        <div dangerouslySetInnerHTML={{ __html: html }} />
    );
}
```

---

## ?? Test ?? xác nh?n

### Test 1: Ki?m tra ?? dài string

```sql
-- Trong SQL Server
SELECT 
    Code,
    LEN(HtmlContent) AS LengthInDB,
    DATALENGTH(HtmlContent) AS BytesInDB
FROM document_templates
WHERE Code = 'SALARY_REPORT_DEFAULT'
```

**K?t qu? mong ??i:**
- `LengthInDB`: ~5000-6000 (s? ký t? HTML)
- `BytesInDB`: G?p ?ôi (vì SQL Server dùng NVARCHAR - UTF-16)

### Test 2: Ki?m tra ký t? ??u tiên

```sql
SELECT 
    LEFT(HtmlContent, 1) AS FirstChar,
    ASCII(LEFT(HtmlContent, 1)) AS FirstCharCode
FROM document_templates
WHERE Code = 'SALARY_REPORT_DEFAULT'
```

**K?t qu? mong ??i:**
- `FirstChar`: `<`
- `FirstCharCode`: `60` (ASCII code c?a `<`)

**N?u database l?u escaped HTML, k?t qu? sai s? là:**
- `FirstChar`: `\`
- `FirstCharCode`: `92` (ASCII code c?a `\`)

### Test 3: ??m s? xu?ng dòng th?c

```sql
SELECT 
    Code,
    (LEN(HtmlContent) - LEN(REPLACE(HtmlContent, CHAR(10), ''))) AS NumberOfNewlines
FROM document_templates
WHERE Code = 'SALARY_REPORT_DEFAULT'
```

**K?t qu? mong ??i:** ~100-200 xu?ng dòng

**N?u database l?u escaped `\\n`, k?t qu? sai s? là:** 0 xu?ng dòng

---

## ?? Checklist: Xác nh?n HTML thu?n trong Database

- [ ] Ch?y SQL query ki?m tra `LEFT(HtmlContent, 50)` ? Ph?i th?y `<!DOCTYPE html>`
- [ ] Test endpoint `/raw-html` ? Trình duy?t hi?n th? HTML ?úng
- [ ] Debug C# code ? `template.HtmlContent[0]` ph?i là `'<'`, không ph?i `'\\'`
- [ ] Ki?m tra ?? dài ? ~5000-6000 ký t?, không ph?i 10000+ ký t? (n?u escaped s? g?p ?ôi)
- [ ] Generate PDF thành công ? HTML render ?úng trong PDF

---

## ?? K?t lu?n

### ? HTML trong Database c?a b?n ?ã THU?N r?i!

1. **Migration script ?úng:** `File.ReadAllTextAsync()` ? L?u plain HTML
2. **Entity Framework ?úng:** T? ??ng l?u string AS-IS vào SQL Server
3. **API JSON ?úng:** Escape sequences trong JSON response là B?T BU?C theo spec
4. **Service code ?úng:** ?ã b? ph?n decode không c?n thi?t

### ?? ?i?u quan tr?ng nh?t

**KHÔNG BAO GI? c?n decode/unescape HTML t? database trong C# code!**

N?u b?n th?y `\\r\\n` hay `\\"` thì ?ó ch? là:
- Cách JSON hi?n th? trong Postman/browser
- Cách C# string literal hi?n th? trong debugger
- **KHÔNG PH?I** cách data ???c l?u trong database

---

## ?? N?u v?n còn nghi ng?

Ch?y l?nh này trong C# (??t breakpoint):

```csharp
var template = await _context.DocumentTemplates.FirstAsync();
var firstChar = template.HtmlContent[0];  // Ph?i là '<'
var secondChar = template.HtmlContent[1]; // Ph?i là '!'
var hasRealNewline = template.HtmlContent.Contains("\n"); // Ph?i là true
var hasEscapedNewline = template.HtmlContent.Contains("\\n"); // Ph?i là false

Console.WriteLine($"First char: {firstChar} (ASCII {(int)firstChar})");
Console.WriteLine($"Second char: {secondChar} (ASCII {(int)secondChar})");
Console.WriteLine($"Has real newline: {hasRealNewline}");
Console.WriteLine($"Has escaped newline: {hasEscapedNewline}");
```

**K?t qu? mong ??i:**
```
First char: < (ASCII 60)
Second char: ! (ASCII 33)
Has real newline: True
Has escaped newline: False
```

**N?u database l?u escaped HTML (sai), k?t qu? s? là:**
```
First char: \ (ASCII 92)
Second char: < (ASCII 60)
Has real newline: False
Has escaped newline: True
```

Nh?ng v?i code c?a b?n, k?t qu? s? là **?úng** (case th? nh?t) vì HTML ?ã ???c l?u thu?n trong database!
