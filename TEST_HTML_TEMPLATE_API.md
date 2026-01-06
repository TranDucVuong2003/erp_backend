# Test API Báo Cáo L??ng v?i HTML Template

## Prerequisites

1. Server ?ang ch?y: `https://localhost:7139` ho?c `http://localhost:5000`
2. Có JWT token h?p l?
3. Có d? li?u phi?u l??ng trong database

## Test 1: Preview HTML trong Browser

### Request
```http
POST https://localhost:7139/api/Payslips/preview-salary-report
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

{
  "month": 12,
  "year": 2024,
  "departmentId": null,
  "createdByName": "Admin"
}
```

### cURL
```bash
curl -X POST "https://localhost:7139/api/Payslips/preview-salary-report" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "month": 12,
    "year": 2024,
    "departmentId": null,
    "createdByName": "Admin"
  }'
```

### Expected Response
HTML content hi?n th? báo cáo l??ng (có th? m? tr?c ti?p trong browser)

---

## Test 2: Xu?t PDF v?i HTML Template (M?c ??nh)

### Request
```http
POST https://localhost:7139/api/Payslips/export-salary-report?useTemplate=true
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

{
  "month": 12,
  "year": 2024,
  "departmentId": null,
  "createdByName": "Admin"
}
```

### cURL
```bash
curl -X POST "https://localhost:7139/api/Payslips/export-salary-report?useTemplate=true" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "month": 12,
    "year": 2024,
    "departmentId": null,
    "createdByName": "Admin"
  }' \
  --output BaoCaoLuong_Template.pdf
```

### PowerShell
```powershell
$token = "YOUR_JWT_TOKEN"
$body = @{
    month = 12
    year = 2024
    departmentId = $null
    createdByName = "Admin"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7139/api/Payslips/export-salary-report?useTemplate=true" `
  -Method Post `
  -Headers @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
  } `
  -Body $body `
  -OutFile "BaoCaoLuong_Template.pdf"
```

### Expected Response
- **Status**: 200 OK
- **Content-Type**: application/pdf
- **Headers**: Content-Disposition: attachment; filename=BaoCaoLuong_12_2024.pdf
- **Body**: PDF file binary data

---

## Test 3: Xu?t PDF v?i QuestPDF

### Request
```http
POST https://localhost:7139/api/Payslips/export-salary-report?useTemplate=false
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

{
  "month": 12,
  "year": 2024,
  "departmentId": null,
  "createdByName": "Admin"
}
```

### cURL
```bash
curl -X POST "https://localhost:7139/api/Payslips/export-salary-report?useTemplate=false" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "month": 12,
    "year": 2024,
    "departmentId": null,
    "createdByName": "Admin"
  }' \
  --output BaoCaoLuong_QuestPDF.pdf
```

### Expected Response
- **Status**: 200 OK
- **Content-Type**: application/pdf
- PDF v?i layout c?a QuestPDF

---

## Test 4: Filter theo Phòng Ban

### Request
```http
POST https://localhost:7139/api/Payslips/export-salary-report?useTemplate=true
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

{
  "month": 12,
  "year": 2024,
  "departmentId": 1,
  "createdByName": "Admin"
}
```

### Expected Response
PDF ch? ch?a nhân viên c?a phòng ban có ID = 1

---

## Test 5: Test Error Cases

### Test 5.1: Tháng không h?p l?

```http
POST https://localhost:7139/api/Payslips/export-salary-report
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

{
  "month": 13,
  "year": 2024,
  "departmentId": null,
  "createdByName": "Admin"
}
```

**Expected**: 400 Bad Request
```json
{
  "message": "Tháng ph?i t? 1-12"
}
```

### Test 5.2: Không có d? li?u

```http
POST https://localhost:7139/api/Payslips/export-salary-report
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

{
  "month": 1,
  "year": 2020,
  "departmentId": null,
  "createdByName": "Admin"
}
```

**Expected**: 404 Not Found
```json
{
  "message": "Không có d? li?u phi?u l??ng tháng 1/2020"
}
```

---

## So Sánh K?t Qu?

### HTML Template (IronPDF)
**?u ?i?m:**
- ? Giao di?n gi?ng v?i template HTML
- ? D? tùy ch?nh (ch?nh CSS)
- ? Có logo và styling ??p
- ? Có ch? ký

**Nh??c ?i?m:**
- ?? Ch?m h?n (c?n render HTML)
- ?? Ph? thu?c IronPDF license

### QuestPDF
**?u ?i?m:**
- ? Nhanh
- ? Không c?n license (Community)
- ? Type-safe

**Nh??c ?i?m:**
- ?? Không có logo
- ?? Layout ??n gi?n h?n
- ?? Khó tùy ch?nh (c?n code C#)

---

## Postman Collection

### Collection Structure
```json
{
  "info": {
    "name": "Salary Report API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Preview HTML",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          },
          {
            "key": "Authorization",
            "value": "Bearer {{jwt_token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"month\": 12,\n  \"year\": 2024,\n  \"departmentId\": null,\n  \"createdByName\": \"Admin\"\n}"
        },
        "url": {
          "raw": "{{base_url}}/api/Payslips/preview-salary-report",
          "host": ["{{base_url}}"],
          "path": ["api", "Payslips", "preview-salary-report"]
        }
      }
    },
    {
      "name": "Export PDF (Template)",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          },
          {
            "key": "Authorization",
            "value": "Bearer {{jwt_token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"month\": 12,\n  \"year\": 2024,\n  \"departmentId\": null,\n  \"createdByName\": \"Admin\"\n}"
        },
        "url": {
          "raw": "{{base_url}}/api/Payslips/export-salary-report?useTemplate=true",
          "host": ["{{base_url}}"],
          "path": ["api", "Payslips", "export-salary-report"],
          "query": [
            {
              "key": "useTemplate",
              "value": "true"
            }
          ]
        }
      }
    },
    {
      "name": "Export PDF (QuestPDF)",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          },
          {
            "key": "Authorization",
            "value": "Bearer {{jwt_token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"month\": 12,\n  \"year\": 2024,\n  \"departmentId\": null,\n  \"createdByName\": \"Admin\"\n}"
        },
        "url": {
          "raw": "{{base_url}}/api/Payslips/export-salary-report?useTemplate=false",
          "host": ["{{base_url}}"],
          "path": ["api", "Payslips", "export-salary-report"],
          "query": [
            {
              "key": "useTemplate",
              "value": "false"
            }
          ]
        }
      }
    }
  ],
  "variable": [
    {
      "key": "base_url",
      "value": "https://localhost:7139"
    },
    {
      "key": "jwt_token",
      "value": "YOUR_JWT_TOKEN_HERE"
    }
  ]
}
```

---

## Verification Checklist

### ? HTML Template PDF
- [ ] File PDF ???c t?i xu?ng thành công
- [ ] Logo hi?n th? ?úng
- [ ] Tiêu ?? "BÁO CÁO TH?NG KÊ L??NG"
- [ ] Thông tin k? l??ng, phòng ban hi?n th? ?úng
- [ ] B?ng nhân viên có d? li?u
- [ ] S? ti?n format ?úng (có d?u ph?y ng?n cách)
- [ ] Dòng t?ng c?ng có background màu vàng
- [ ] Ph?n t?ng k?t hi?n th? ??y ??
- [ ] Có 3 ch? ký (Ng??i l?p bi?u, K? toán tr??ng, Giám ??c)
- [ ] Footer có th?i gian t?o

### ? QuestPDF
- [ ] File PDF ???c t?i xu?ng thành công
- [ ] Tiêu ?? "BÁO CÁO L??NG NHÂN VIÊN"
- [ ] B?ng có alternating row colors
- [ ] S? ti?n format ?úng
- [ ] Page numbers trong footer
- [ ] T?ng c?ng hi?n th? ?úng

### ? HTML Preview
- [ ] HTML hi?n th? trong browser
- [ ] CSS ???c apply ?úng
- [ ] Responsive (n?u c?n)
- [ ] Print preview OK

---

## Performance Benchmark

Test v?i 100 nhân viên:

| Method | Time | PDF Size |
|--------|------|----------|
| HTML Template (IronPDF) | ~2-3s | ~150KB |
| QuestPDF | ~0.5-1s | ~80KB |

---

## Notes

1. **Default behavior**: API m?c ??nh s? d?ng HTML template (`useTemplate=true`)
2. **Browser caching**: Preview HTML có th? b? cache b?i browser
3. **File naming**: PDF filename format: `BaoCaoLuong_{MM}_{YYYY}.pdf`
4. **Encoding**: Template s? d?ng UTF-8 encoding
5. **Font**: DejaVu Sans h? tr? ti?ng Vi?t t?t
