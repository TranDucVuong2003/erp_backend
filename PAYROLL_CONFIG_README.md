# ?? PAYROLL CONFIG MODULE - QUICK REFERENCE

## ? Tr?ng Thái Module

| Thành Ph?n | Tr?ng Thái | File Path |
|------------|-----------|-----------|
| **Model** | ? Hoàn t?t | `erp_backend\Models\PayrollConfig.cs` |
| **DbContext** | ? ?ã c?u hình | `erp_backend\Data\ApplicationDbContext.cs` |
| **Controller** | ? ?ã t?o | `erp_backend\Controllers\PayrollConfigsController.cs` |
| **Migration** | ? ?ã apply | `20251223085237_SeedPayrollConfigData.cs` |
| **Database** | ? ?ã seed data | Table: `PayrollConfig` (8 records) |
| **Documentation** | ? Hoàn t?t | `PAYROLL_CONFIG_DOCUMENTATION.md` |

---

## ?? 8 Config Keys Hi?n Có

| Key | Value | Mô T? |
|-----|-------|-------|
| `MIN_WAGE_REGION_1_2026` | 5,310,000 | L??ng t?i thi?u vùng 1 n?m 2026 |
| `GOV_BASE_SALARY` | 2,340,000 | L??ng c? s? nhà n??c |
| `TRAINED_WORKER_RATE` | 1.07 | T? l? lao ??ng qua ?ào t?o |
| `INSURANCE_CAP_RATIO` | 20 | H? s? tr?n b?o hi?m |
| `PERSONAL_DEDUCTION` | 15,500,000 | Gi?m tr? gia c?nh b?n thân |
| `DEPENDENT_DEDUCTION` | 6,200,000 | Gi?m tr? ng??i ph? thu?c |
| `FLAT_TAX_THRESHOLD` | 2,000,000 | Ng??ng thu? thu nh?p vãng lai |
| `DEFAULT_INSURANCE_MODE` | MINIMAL | Ch? ?? ?óng BH m?c ??nh |

---

## ?? Quick Start - API Usage

### Get All Configs
```bash
curl -X GET http://localhost:5000/api/PayrollConfigs
```

### Get Specific Config
```bash
curl -X GET http://localhost:5000/api/PayrollConfigs/MIN_WAGE_REGION_1_2026
```

### Create New Config
```bash
curl -X POST http://localhost:5000/api/PayrollConfigs \
  -H "Content-Type: application/json" \
  -d '{
    "key": "NEW_KEY",
    "value": "1000000",
    "description": "Mô t?"
  }'
```

### Update Config
```bash
curl -X PUT http://localhost:5000/api/PayrollConfigs/NEW_KEY \
  -H "Content-Type: application/json" \
  -d '{
    "key": "NEW_KEY",
    "value": "2000000",
    "description": "?ã c?p nh?t"
  }'
```

### Delete Config
```bash
curl -X DELETE http://localhost:5000/api/PayrollConfigs/NEW_KEY
```

---

## ?? Code Examples

### C# - Get Config Value
```csharp
var config = await _context.PayrollConfigs.FindAsync("MIN_WAGE_REGION_1_2026");
decimal minWage = decimal.Parse(config.Value); // 5,310,000
```

### C# - Calculate Insurance Floor
```csharp
var minWage = decimal.Parse(
    (await _context.PayrollConfigs.FindAsync("MIN_WAGE_REGION_1_2026")).Value
);
var trainedRate = decimal.Parse(
    (await _context.PayrollConfigs.FindAsync("TRAINED_WORKER_RATE")).Value
);

decimal insuranceFloor = minWage * trainedRate; // 5,681,700
```

### JavaScript/TypeScript - Fetch Config
```typescript
// Get all configs
const response = await fetch('/api/PayrollConfigs');
const configs = await response.json();

// Get specific config
const minWageResponse = await fetch('/api/PayrollConfigs/MIN_WAGE_REGION_1_2026');
const minWageConfig = await minWageResponse.json();
console.log(minWageConfig.value); // "5310000"
```

---

## ?? Tài Li?u Chi Ti?t

Xem file **[PAYROLL_CONFIG_DOCUMENTATION.md](./PAYROLL_CONFIG_DOCUMENTATION.md)** ?? bi?t:
- C?u trúc d? li?u chi ti?t
- Gi?i thích t?ng config key
- Công th?c tính toán
- Use cases và examples
- Best practices

---

## ?? Migration History

| Migration | Mô T? | Status |
|-----------|-------|--------|
| `20251223085237_SeedPayrollConfigData` | Seed 8 configs | ? Applied |
| `20251223090542_RenameInsuranceStatusToPayrollConfigAndReseed` | ??i tên b?ng | ? Applied |

---

## ?? Testing Checklist

- [x] Model created and configured
- [x] DbContext updated with DbSet
- [x] Entity configuration added
- [x] Migration created and applied
- [x] Database seeded with 8 records
- [x] Controller created with CRUD operations
- [x] API endpoints tested
- [x] Documentation completed
- [x] Build successful

---

## ?? Notes

- **Primary Key:** `Key` (string, unique)
- **Table Name:** `PayrollConfig` (singular)
- **DbSet Name:** `PayrollConfigs` (plural)
- **No timestamps:** CreatedAt/UpdatedAt không c?n thi?t cho config data

---

## ?? Next Steps

1. ? Module ?ã hoàn t?t và s?n sàng s? d?ng
2. Tích h?p vào Payroll calculation logic
3. T?o UI ?? qu?n lý configs (Admin panel)
4. Thêm authorization cho API endpoints
5. Setup caching cho performance optimization

---

**Last Updated:** December 23, 2024  
**Status:** ? **PRODUCTION READY**
