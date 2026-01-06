# Gi?i Thích Lu?ng Ho?t ??ng H?p ??ng (Contract Template t? Database)

## ?? T?ng Quan Flow

```
Client Request ? Load Contract Data ? 
Get Template from DB (based on CustomerType) ? 
Bind Data to Template ? Generate PDF ? Save & Return
```

---

## ?? Chi Ti?t T?ng B??c

### **B??c 1: Client G?i API**

#### Preview HTML:
```http
GET /api/Contracts/5/preview
Authorization: Bearer {token}
```

#### Export PDF:
```http
GET /api/Contracts/5/export-contract
Authorization: Bearer {token}
```

#### Regenerate PDF (xóa c?, t?o m?i):
```http
POST /api/Contracts/5/regenerate-contract
Authorization: Bearer {token}
```

---

### **B??c 2: Load D? Li?u Contract**

```csharp
var contract = await _context.Contracts
    .Include(c => c.User)                        // ? Nhân viên ph? trách
        .ThenInclude(u => u.Position)            // ? Ch?c v?
    .Include(c => c.SaleOrder)                   // ? ??n hàng
        .ThenInclude(so => so!.Customer)         // ? Khách hàng
    .Include(c => c.SaleOrder)
        .ThenInclude(so => so!.SaleOrderServices)// ? D?ch v? trong ??n hàng
            .ThenInclude(sos => sos.Service)     // ? Chi ti?t d?ch v?
                .ThenInclude(s => s!.Tax)        // ? Thu? c?a d?ch v?
    .Include(c => c.SaleOrder)
        .ThenInclude(so => so!.SaleOrderAddons)  // ? Addon trong ??n hàng
            .ThenInclude(soa => soa.Addon)       // ? Chi ti?t addon
                .ThenInclude(a => a!.Tax)        // ? Thu? c?a addon
    .FirstOrDefaultAsync(c => c.Id == id);
```

**D? li?u sau khi load:**
```json
{
  "Id": 5,
  "NumberContract": 128,
  "Status": "Pending",
  "PaymentMethod": "Chuy?n kho?n",
  "SubTotal": 50000000,
  "TaxAmount": 5000000,
  "TotalAmount": 55000000,
  "Expiration": "2025-12-31",
  "ExtractInvoices": true,
  "User": {
    "Name": "Tr?n V?n B",
    "Email": "tranvanb@company.com",
    "Position": {
      "PositionName": "Sales Manager"
    }
  },
  "SaleOrder": {
    "Title": "Gói d?ch v? Web Development",
    "Customer": {
      "CustomerType": "business",
      "CompanyName": "Công ty TNHH ABC",
      "CompanyAddress": "123 ???ng XYZ, Hà N?i",
      "TaxCode": "0123456789",
      "RepresentativeName": "Nguy?n V?n A",
      "RepresentativePosition": "Giám ??c",
      "RepresentativePhone": "0987654321",
      "RepresentativeEmail": "nguyenvana@abc.com",
      "EstablishedDate": "2010-01-15"
    },
    "SaleOrderServices": [
      {
        "ServiceId": 1,
        "Service": {
          "Name": "Thi?t k? website",
          "Tax": { "Rate": 10 }
        },
        "UnitPrice": 30000000,
        "Quantity": 1,
        "duration": 3,
        "template": "Template A"
      },
      {
        "ServiceId": 2,
        "Service": {
          "Name": "B?o trì h? th?ng",
          "Tax": { "Rate": 10 }
        },
        "UnitPrice": 20000000,
        "Quantity": 1,
        "duration": 12,
        "template": "Template B"
      }
    ],
    "SaleOrderAddons": [
      {
        "AddonId": 1,
        "Addon": {
          "Name": "SSL Certificate",
          "Tax": { "Rate": 10 }
        },
        "UnitPrice": 500000,
        "Quantity": 2,
        "duration": 12,
        "template": "Yearly"
      }
    ]
  }
}
```

---

### **B??c 3: Ch?n Template D?a Vào CustomerType**

```csharp
// ? QUAN TR?NG: Template khác nhau cho t?ng lo?i khách hàng
var templateCode = customer.CustomerType?.ToLower() == "individual" 
    ? "CONTRACT_INDIVIDUAL"      // ? Khách hàng cá nhân
    : "CONTRACT_BUSINESS";       // ? Khách hàng doanh nghi?p

var template = await _context.DocumentTemplates
    .Where(t => t.Code == templateCode && t.IsActive)
    .FirstOrDefaultAsync();
```

**Phân bi?t 2 lo?i template:**

| Lo?i Khách Hàng | Template Code | Thông Tin ??c Bi?t |
|-----------------|---------------|---------------------|
| **Individual** (Cá nhân) | `CONTRACT_INDIVIDUAL` | • Ngày sinh<br>• CMND/CCCD<br>• ??a ch? cá nhân |
| **Business** (Doanh nghi?p) | `CONTRACT_BUSINESS` | • Mã s? thu?<br>• Ngày thành l?p<br>• Ng??i ??i di?n<br>• Ch?c v? ??i di?n |

---

### **B??c 4: Bind D? Li?u vào Template**

#### 4.1. Replace Thông Tin C? B?n

```csharp
template = template
    .Replace("{{ContractNumber}}", contract.NumberContract.ToString())
    .Replace("{{NumberContract}}", contract.NumberContract.ToString())
    .Replace("{{ContractYear}}", contract.CreatedAt.Year.ToString())
    .Replace("{{Day}}", now.Day.ToString())
    .Replace("{{Month}}", now.Month.ToString())
    .Replace("{{Year}}", now.Year.ToString())
    .Replace("{{ContractDate}}", contract.CreatedAt.ToString("dd/MM/yyyy"))
    .Replace("{{ExpirationDate}}", contract.Expiration.ToString("dd/MM/yyyy"))
    .Replace("{{Location}}", "Hà N?i");
```

**K?t qu?:**
```
S? h?p ??ng: 128/2024
Ngày ký: 31/12/2024
Ngày h?t h?n: 31/12/2025
??a ?i?m: Hà N?i
```

#### 4.2. Replace Thông Tin Khách Hàng (Doanh Nghi?p)

```csharp
template = template
    .Replace("{{CompanyBName}}", customer.CompanyName ?? customer.Name ?? "")
    .Replace("{{CompanyBAddress}}", customer.CompanyAddress ?? customer.Address ?? "")
    .Replace("{{CompanyBTaxCode}}", customer.TaxCode ?? "")
    .Replace("{{CompanyBRepName}}", customer.RepresentativeName ?? customer.Name ?? "")
    .Replace("{{CompanyBRepPosition}}", customer.RepresentativePosition ?? "")
    .Replace("{{CompanyBRepID}}", customer.RepresentativeIdNumber ?? customer.IdNumber ?? "")
    .Replace("{{CompanyBPhone}}", customer.RepresentativePhone ?? customer.PhoneNumber ?? "")
    .Replace("{{CompanyBEmail}}", customer.RepresentativeEmail ?? customer.Email ?? "");
```

**K?t qu?:**
```
BÊN B (KHÁCH HÀNG):
Tên công ty: Công ty TNHH ABC
??a ch?: 123 ???ng XYZ, Hà N?i
Mã s? thu?: 0123456789
Ng??i ??i di?n: Nguy?n V?n A
Ch?c v?: Giám ??c
CMND/CCCD: 001234567890
?i?n tho?i: 0987654321
Email: nguyenvana@abc.com
```

#### 4.3. Replace Ngày Thành L?p (Doanh Nghi?p)

```csharp
if (customer.EstablishedDate.HasValue)
{
    template = template
        .Replace("{{CompanyBEstablishedDay}}", customer.EstablishedDate.Value.Day.ToString())
        .Replace("{{CompanyBEstablishedMonth}}", customer.EstablishedDate.Value.Month.ToString())
        .Replace("{{CompanyBEstablishedYear}}", customer.EstablishedDate.Value.Year.ToString())
        .Replace("{{CompanyBEstablishedDate}}", customer.EstablishedDate.Value.ToString("dd/MM/yyyy"));
}
else
{
    // ? N?u không có, thay b?ng chu?i r?ng
    template = template
        .Replace("{{CompanyBEstablishedDay}}", "")
        .Replace("{{CompanyBEstablishedMonth}}", "")
        .Replace("{{CompanyBEstablishedYear}}", "")
        .Replace("{{CompanyBEstablishedDate}}", "");
}
```

#### 4.4. Replace Thông Tin Tài Chính

```csharp
template = template
    .Replace("{{SubTotal}}", contract.SubTotal.ToString("N0"))
    .Replace("{{Discount}}", "0")
    .Replace("{{TaxAmount}}", contract.TaxAmount.ToString("N0"))
    .Replace("{{TotalAmount}}", contract.TotalAmount.ToString("N0"))
    .Replace("{{NetAmount}}", contract.TotalAmount.ToString("N0"))
    .Replace("{{AmountInWords}}", ConvertNumberToWords(contract.TotalAmount))
    .Replace("{{PaymentMethod}}", contract.PaymentMethod ?? "Chuy?n kho?n")
    .Replace("{{Status}}", contract.Status)
    .Replace("{{Notes}}", contract.Notes ?? "");
```

**K?t qu?:**
```
T?ng c?ng (ch?a thu?): 50,000,000 VN?
Thu? VAT (10%):         5,000,000 VN?
Gi?m giá:                       0 VN?
T?NG THANH TOÁN:       55,000,000 VN?
B?ng ch?: N?m m??i l?m tri?u ??ng ch?n

Ph??ng th?c thanh toán: Chuy?n kho?n
Tr?ng thái: Pending
```

#### 4.5. Replace Thông Tin Nhân Viên

```csharp
if (contract.User != null)
{
    template = template
        .Replace("{{UserName}}", contract.User.Name)
        .Replace("{{UserEmail}}", contract.User.Email)
        .Replace("{{UserPhone}}", contract.User.PhoneNumber ?? "")
        .Replace("{{UserPosition}}", contract.User.Position?.PositionName ?? "Nhân viên");
}
```

**K?t qu?:**
```
BÊN A (CÔNG TY):
Ng??i ??i di?n: Tr?n V?n B
Ch?c v?: Sales Manager
Email: tranvanb@company.com
?i?n tho?i: 0912345678
```

#### 4.6. Generate B?ng D?ch V?

```csharp
var itemsHtml = GenerateContractItemsTableFromContract(contract);
template = template.Replace("{{Items}}", itemsHtml);
```

**Logic Generate Items Table:**

```csharp
private string GenerateContractItemsTableFromContract(Contract contract)
{
    var items = new StringBuilder();
    var index = 1;
    decimal subTotal = 0;
    decimal totalTax = 0;

    // ? Loop qua Services
    foreach (var sos in contract.SaleOrder.SaleOrderServices)
    {
        var service = sos.Service;
        var quantity = sos.Quantity ?? (service?.Quantity ?? 1);
        var lineTotal = sos.UnitPrice * quantity;
        subTotal += lineTotal;

        var taxRate = service?.Tax?.Rate ?? 0f;
        var lineTax = taxRate > 0 ? lineTotal * (decimal)taxRate / 100m : 0;
        totalTax += lineTax;

        items.AppendLine($@"
        <tr>
            <td>{index++}</td>
            <td>{service?.Name ?? ""}</td>
            <td>{sos.template ?? ""}</td>
            <td>{taxRate:N2}%</td>
            <td>{sos.duration} tháng</td>
            <td>{sos.UnitPrice:N0}</td>
            <td>{lineTotal:N0}</td>
        </tr>");
    }

    // ? Loop qua Addons
    foreach (var soa in contract.SaleOrder.SaleOrderAddons)
    {
        var addon = soa.Addon;
        var quantity = soa.Quantity ?? (addon?.Quantity ?? 1);
        var lineTotal = soa.UnitPrice * quantity;
        subTotal += lineTotal;

        var taxRate = addon?.Tax?.Rate ?? 0f;
        var lineTax = taxRate > 0 ? lineTotal * (decimal)taxRate / 100m : 0;
        totalTax += lineTax;

        items.AppendLine($@"
        <tr>
            <td>{index++}</td>
            <td>{addon?.Name ?? ""}</td>
            <td>{soa.template ?? ""}</td>
            <td>{taxRate:N2}%</td>
            <td>{soa.duration} tháng</td>
            <td>{soa.UnitPrice:N0}</td>
            <td>{lineTotal:N0}</td>
        </tr>");
    }

    // ? Thêm dòng t?ng k?t
    items.AppendLine($@"
    <tr>
        <td colspan='6'>C?ng (ch?a thu?)</td>
        <td>{subTotal:N0}</td>
    </tr>
    <tr>
        <td colspan='6'>Thu? VAT</td>
        <td>{totalTax:N0}</td>
    </tr>
    <tr>
        <td colspan='6'>T?NG THANH TOÁN</td>
        <td>{(subTotal + totalTax):N0}</td>
    </tr>");

    return items.ToString();
}
```

**HTML ???c generate:**

```html
<table>
  <thead>
    <tr>
      <th>STT</th>
      <th>Tên d?ch v?</th>
      <th>Template</th>
      <th>Thu?</th>
      <th>Th?i gian</th>
      <th>??n giá</th>
      <th>Thành ti?n</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>1</td>
      <td>Thi?t k? website</td>
      <td>Template A</td>
      <td>10.00%</td>
      <td>3 tháng</td>
      <td>30,000,000</td>
      <td>30,000,000</td>
    </tr>
    <tr>
      <td>2</td>
      <td>B?o trì h? th?ng</td>
      <td>Template B</td>
      <td>10.00%</td>
      <td>12 tháng</td>
      <td>20,000,000</td>
      <td>20,000,000</td>
    </tr>
    <tr>
      <td>3</td>
      <td>SSL Certificate</td>
      <td>Yearly</td>
      <td>10.00%</td>
      <td>12 tháng</td>
      <td>500,000</td>
      <td>1,000,000</td>
    </tr>
    <tr>
      <td colspan='6'>C?ng (ch?a thu?)</td>
      <td>51,000,000</td>
    </tr>
    <tr>
      <td colspan='6'>Thu? VAT</td>
      <td>5,100,000</td>
    </tr>
    <tr>
      <td colspan='6'>T?NG THANH TOÁN</td>
      <td>56,100,000</td>
    </tr>
  </tbody>
</table>
```

---

### **B??c 5: X? Lý K?t Qu?**

#### **5A. N?u Preview (tr? v? HTML):**

```csharp
[HttpGet("{id}/preview")]
public async Task<IActionResult> PreviewContract(int id)
{
    // ... load data & get template ...
    
    var htmlContent = BindContractDataToTemplate(template.HtmlContent, contract);
    
    // ? Tr? v? HTML ?? preview
    return Content(htmlContent, "text/html");
}
```

#### **5B. N?u Export PDF:**

```csharp
[HttpGet("{id}/export-contract")]
public async Task<IActionResult> ExportContract(int id)
{
    // ? Ki?m tra cache: n?u ?ã có PDF, tr? v? luôn
    if (!string.IsNullOrEmpty(contract.ContractPdfPath))
    {
        var existingFilePath = Path.Combine("wwwroot", contract.ContractPdfPath);
        if (File.Exists(existingFilePath))
        {
            var pdfBytes = await File.ReadAllBytesAsync(existingFilePath);
            return File(pdfBytes, "application/pdf", Path.GetFileName(existingFilePath));
        }
    }
    
    // ? Ch?a có PDF, t?o m?i
    var htmlContent = BindContractDataToTemplate(template.HtmlContent, contract);
    var pdfBytes = await _pdfService.ConvertHtmlToPdfAsync(htmlContent);
    
    // ? L?u file v?i c?u trúc: Contracts/YYYY/MM/filename.pdf
    var fileName = $"HopDong_{contract.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
    var yearMonth = $"{DateTime.Now:yyyy}/{DateTime.Now:MM}";
    var relativePath = Path.Combine("Contracts", yearMonth, fileName);
    var absolutePath = Path.Combine("wwwroot", relativePath);
    
    Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
    await File.WriteAllBytesAsync(absolutePath, pdfBytes);
    
    // ? C?p nh?t database
    contract.ContractPdfPath = relativePath.Replace("\\", "/");
    contract.PdfGeneratedAt = DateTime.UtcNow;
    contract.PdfFileSize = pdfBytes.Length;
    await _context.SaveChangesAsync();
    
    return File(pdfBytes, "application/pdf", fileName);
}
```

**C?u trúc file ???c l?u:**
```
wwwroot/
??? Contracts/
    ??? 2024/
        ??? 12/
            ??? HopDong_5_20241231_143000.pdf
            ??? HopDong_6_20241231_144500.pdf
            ??? HopDong_7_20241231_150000.pdf
```

#### **5C. Regenerate PDF (xóa c?, t?o m?i):**

```csharp
[HttpPost("{id}/regenerate-contract")]
public async Task<IActionResult> RegenerateContract(int id)
{
    var contract = await _context.Contracts.FindAsync(id);
    
    // ? Xóa file c?
    if (!string.IsNullOrEmpty(contract.ContractPdfPath))
    {
        var oldPath = Path.Combine("wwwroot", contract.ContractPdfPath);
        if (File.Exists(oldPath))
        {
            File.Delete(oldPath);
        }
    }
    
    // ? Reset metadata trong database
    contract.ContractPdfPath = null;
    contract.PdfGeneratedAt = null;
    contract.PdfFileSize = null;
    await _context.SaveChangesAsync();
    
    // ? Redirect sang ExportContract ?? t?o m?i
    return await ExportContract(id);
}
```

---

## ?? So Sánh 2 Lo?i Template

### **CONTRACT_INDIVIDUAL (Cá Nhân)**

**Placeholders ??c bi?t:**
```
{{CustomerBirthDay}}           ? 15
{{CustomerBirthMonth}}         ? 01
{{CustomerBirthYear}}          ? 1990
{{CompanyBName}}               ? Nguy?n V?n A (tên cá nhân)
{{CompanyBAddress}}            ? 456 ???ng ABC, Hà N?i (??a ch? cá nhân)
{{CompanyBRepID}}              ? 001234567890 (CMND/CCCD)
```

**Không có:**
- `{{CompanyBTaxCode}}`
- `{{CompanyBEstablishedDate}}`
- `{{CompanyBRepPosition}}`

---

### **CONTRACT_BUSINESS (Doanh Nghi?p)**

**Placeholders ??c bi?t:**
```
{{CompanyBName}}               ? Công ty TNHH ABC
{{CompanyBAddress}}            ? 123 ???ng XYZ, Hà N?i
{{CompanyBTaxCode}}            ? 0123456789
{{CompanyBRepName}}            ? Nguy?n V?n A (ng??i ??i di?n)
{{CompanyBRepPosition}}        ? Giám ??c
{{CompanyBEstablishedDay}}     ? 15
{{CompanyBEstablishedMonth}}   ? 01
{{CompanyBEstablishedYear}}    ? 2010
{{CompanyBEstablishedDate}}    ? 15/01/2010
```

**Không có:**
- `{{CustomerBirthDay}}`
- `{{CustomerBirthMonth}}`
- `{{CustomerBirthYear}}`

---

## ?? Flow Chart

```
???????????????????
?  Client Request ?
???????????????????
         ?
         ?
???????????????????????????
? Load Contract + All     ?
? Relations (User,        ?
? SaleOrder, Customer,    ?
? Services, Addons, Tax)  ?
???????????????????????????
         ?
         ?
???????????????????????????
? Determine CustomerType  ?
? • individual            ?
? • business              ?
???????????????????????????
         ?
         ?
???????????????????????????
? Query Template from DB  ?
? • CONTRACT_INDIVIDUAL   ?
?   OR                    ?
? • CONTRACT_BUSINESS     ?
? WHERE IsActive = true   ?
???????????????????????????
         ?
         ?
???????????????????????????
? Bind Data to Template   ?
? • Basic info            ?
? • Customer info         ?
? • Financial info        ?
? • User info             ?
? • Generate items table  ?
? • Convert amount words  ?
???????????????????????????
         ?
         ?
    ??????????????
    ?  Action?   ?
    ?            ?
    ?            ?
??????????   ????????????????
?Preview ?   ?  Export PDF  ?
?        ?   ?              ?
?Return  ?   ? Check Cache? ?
?HTML    ?   ????????????????
??????????          ?
                    ??Yes?? Return Cached PDF
                    ?
                    ??No??? Generate New PDF
                            ?
                            ?
                    ????????????????????
                    ? PuppeteerSharp   ?
                    ? Convert HTML?PDF ?
                    ????????????????????
                            ?
                            ?
                    ????????????????????
                    ? Save PDF File    ?
                    ? wwwroot/Contracts?
                    ? /YYYY/MM/file.pdf?
                    ????????????????????
                            ?
                            ?
                    ????????????????????
                    ? Update Database  ?
                    ? • ContractPdfPath?
                    ? • PdfGeneratedAt ?
                    ? • PdfFileSize    ?
                    ????????????????????
                            ?
                            ?
                    ????????????????????
                    ? Return PDF File  ?
                    ????????????????????
```

---

## ?? Key Features

### 1. **Template Selection Logic**
```csharp
var templateCode = customer.CustomerType?.ToLower() == "individual" 
    ? "CONTRACT_INDIVIDUAL" 
    : "CONTRACT_BUSINESS";
```
- **Dynamic**: Ch?n template t? ??ng d?a vào lo?i khách hàng
- **Flexible**: D? dàng thêm lo?i khách hàng m?i

### 2. **PDF Caching**
```csharp
if (!string.IsNullOrEmpty(contract.ContractPdfPath))
{
    if (File.Exists(existingFilePath))
    {
        return File(pdfBytes, "application/pdf", fileName);
    }
}
```
- **Performance**: Không regenerate PDF n?u ?ã có
- **Storage**: L?u metadata trong database

### 3. **PDF Invalidation**
```csharp
bool needsRegenerate = 
    request.SaleOrderId != existingContract.SaleOrderId ||
    request.UserId != existingContract.UserId ||
    request.Status != existingContract.Status ||
    request.Expiration != existingContract.Expiration ||
    request.ExtractInvoices != existingContract.ExtractInvoices;

if (needsRegenerate)
{
    File.Delete(oldFilePath);
    contract.ContractPdfPath = null;
}
```
- **Smart**: Xóa PDF c? khi có thay ??i quan tr?ng
- **Consistent**: ??m b?o PDF luôn sync v?i d? li?u

### 4. **Organized File Structure**
```
wwwroot/Contracts/YYYY/MM/filename.pdf
```
- **Scalable**: D? qu?n lý khi có nhi?u file
- **Searchable**: Tìm file theo n?m/tháng

### 5. **Convert Number to Vietnamese Words**
```csharp
ConvertNumberToWords(55000000)
// ? "N?m m??i l?m tri?u ??ng ch?n"
```
- **Legal**: C?n thi?t cho h?p ??ng
- **Vietnamese**: H? tr? ti?ng Vi?t ??y ??

---

## ?? Best Practices

### ? DO:
- Cache PDF ?? tránh regenerate không c?n thi?t
- Xóa PDF c? khi có thay ??i quan tr?ng
- L?u metadata (path, size, timestamp) trong database
- T? ch?c file theo n?m/tháng
- Validate d? li?u tr??c khi generate
- Log chi ti?t ?? debug

### ? DON'T:
- Regenerate PDF m?i l?n request
- L?u PDF path tuy?t ??i
- Quên xóa file c? khi update
- Hardcode template trong code
- Ignore l?i khi generate PDF

---

## ?? Troubleshooting

### L?i "Template not found"
```
? Check: Template có trong database không?
? Check: IsActive = true?
? Check: Code ?úng không?
? Check: CustomerType mapping ?úng không?
```

### PDF b? l?i font ti?ng Vi?t
```
? Check: Template HTML có font support ti?ng Vi?t?
? Check: PuppeteerSharp config
```

### File PDF không t?n t?i
```
? Check: wwwroot/Contracts folder exists?
? Check: Permission ?? ghi file?
? Check: ContractPdfPath trong database
```

---

**Last Updated:** 2024-12-31  
**Version:** 2.0  
**Status:** ? Completed - Contract Template Migration
