# ?? PAYROLL CONFIG MODULE - TÀI LI?U H? TH?NG

## ?? T?ng Quan

Module **PayrollConfig** qu?n lý các thông s? c?u hình toàn c?c cho h? th?ng tính l??ng, b?o hi?m và thu? TNCN.

### L?ch S? ??i Tên:
- `SystemConfig` (ban ??u)
- `InsuranceStatus` (t?m th?i)
- **`PayrollConfig`** (hi?n t?i) ?

---

## ?? C?u Trúc D? Li?u

### Model: `PayrollConfig`
```csharp
public class PayrollConfig
{
    [Key]
    public string Key { get; set; }       // Primary Key (unique)
    public string Value { get; set; }     // Giá tr? config
    public string Description { get; set; } // Mô t? config
}
```

### Database Table: `PayrollConfig`
```sql
CREATE TABLE "PayrollConfig" (
    "Key" VARCHAR(100) PRIMARY KEY,
    "Value" VARCHAR(500) NOT NULL,
    "Description" VARCHAR(1000) NULL
);
```

---

## ??? CÁC THÔNG S? C?U HÌNH

### ? NHÓM 1: CÁC M?C L??NG/THU NH?P CHU?N

#### 1. `MIN_WAGE_REGION_1_2026`
- **Giá tr?:** `5,310,000` VN?
- **Mô t?:** L??ng t?i thi?u vùng 1 n?m 2026
- **S? d?ng cho:**
  - Tính **SÀN ?óng b?o hi?m** (BHXH, BHYT, BHTN)
  - Tính **TR?N BHTN** (B?o hi?m th?t nghi?p)
- **Công th?c:**
  ```
  SÀN BHXH = MIN_WAGE_REGION_1_2026 × TRAINED_WORKER_RATE
                = 5,310,000 × 1.07
                = 5,681,700 VN?
  ```

#### 2. `GOV_BASE_SALARY`
- **Giá tr?:** `2,340,000` VN?
- **Mô t?:** L??ng c? s? nhà n??c
- **S? d?ng cho:**
  - Tính **TR?N BHXH** (B?o hi?m xã h?i)
  - Tính **TR?N BHYT** (B?o hi?m y t?)
- **Công th?c:**
  ```
  TR?N BHXH/BHYT = GOV_BASE_SALARY × INSURANCE_CAP_RATIO
                 = 2,340,000 × 20
                 = 46,800,000 VN?
  ```

---

### ?? NHÓM 2: CÁC T? L? & H? S?

#### 3. `TRAINED_WORKER_RATE`
- **Giá tr?:** `1.07`
- **Mô t?:** T? l? c?ng thêm cho lao ??ng qua ?ào t?o
- **Công th?c:** `107% = L??ng vùng + 7%`
- **S? d?ng cho:**
  - Tính l??ng ?óng b?o hi?m cho lao ??ng có ch?ng ch? ?ào t?o
  - Áp d?ng khi `HasCommitment08 = true` trong `SalaryContracts`

#### 4. `INSURANCE_CAP_RATIO`
- **Giá tr?:** `20`
- **Mô t?:** H? s? tr?n b?o hi?m
- **Quy ??nh:** ?óng b?o hi?m t?i ?a trên 20 l?n m?c l??ng chu?n
- **Áp d?ng cho:** BHXH, BHYT

---

### ?? NHÓM 3: THU? THU NH?P CÁ NHÂN (Lu?t 2026)

#### 5. `PERSONAL_DEDUCTION`
- **Giá tr?:** `15,500,000` VN?/tháng
- **Mô t?:** M?c gi?m tr? gia c?nh cho b?n thân
- **Áp d?ng:** T?t c? ng??i n?p thu?
- **S? d?ng trong công th?c:**
  ```
  Thu nh?p tính thu? = Gross Salary - PERSONAL_DEDUCTION - (DEPENDENT_DEDUCTION × s? ng??i ph? thu?c)
  ```

#### 6. `DEPENDENT_DEDUCTION`
- **Giá tr?:** `6,200,000` VN?/tháng/ng??i
- **Mô t?:** M?c gi?m tr? cho m?i ng??i ph? thu?c
- **Áp d?ng:** 
  - Con d??i 18 tu?i
  - Con t? 18-25 tu?i ?ang h?c ??i h?c
  - Cha m?, v?/ch?ng không có thu nh?p
- **L?y t?:** `SalaryContracts.DependentsCount`

#### 7. `FLAT_TAX_THRESHOLD`
- **Giá tr?:** `2,000,000` VN?
- **Mô t?:** Ng??ng thu nh?p vãng lai b?t ??u ph?i kh?u tr? 10%
- **Áp d?ng cho:** 
  - Thu nh?p t? làm thêm, gia công
  - Thu nh?p không th??ng xuyên
- **Quy ??nh:**
  - Thu nh?p ? 2,000,000: Không thu?
  - Thu nh?p > 2,000,000: Kh?u tr? 10%

---

### ??? NHÓM 4: C?U HÌNH M?C ??NH H? TH?NG

#### 8. `DEFAULT_INSURANCE_MODE`
- **Giá tr?:** `"MINIMAL"` ho?c `"FULL"`
- **Mô t?:** Ch? ?? ?óng b?o hi?m m?c ??nh khi t?o nhân viên m?i
- **Các ch? ??:**

##### Mode: `MINIMAL` (?óng m?c sàn)
```
InsuranceSalary = MIN_WAGE_REGION_1_2026 × TRAINED_WORKER_RATE
                = 5,310,000 × 1.07
                = 5,681,700 VN?
```
- **?u ?i?m:** Gi?m chi phí ?óng b?o hi?m cho công ty và nhân viên
- **Nh??c ?i?m:** Quy?n l?i BHXH th?p khi ngh? h?u

##### Mode: `FULL` (?óng full l??ng)
```
InsuranceSalary = BaseSalary (t?i ?a = 46,800,000 VN?)
```
- **?u ?i?m:** Quy?n l?i BHXH cao khi ngh? h?u
- **Nh??c ?i?m:** Chi phí ?óng b?o hi?m cao

---

## ?? API ENDPOINTS

### Base URL: `/api/PayrollConfigs`

#### 1. L?y T?t C? Configs
```http
GET /api/PayrollConfigs
```

**Response 200 OK:**
```json
[
  {
    "key": "MIN_WAGE_REGION_1_2026",
    "value": "5310000",
    "description": "L??ng t?i thi?u vùng 1 n?m 2026. Dùng ?? tính SÀN ?óng BH và TR?N BHTN."
  },
  {
    "key": "GOV_BASE_SALARY",
    "value": "2340000",
    "description": "L??ng c? s? (nhà n??c). Dùng ?? tính TR?N BHXH và BHYT (x20 l?n)."
  }
  // ... 6 configs khác
]
```

---

#### 2. L?y Config Theo Key
```http
GET /api/PayrollConfigs/{key}
```

**Example:**
```http
GET /api/PayrollConfigs/MIN_WAGE_REGION_1_2026
```

**Response 200 OK:**
```json
{
  "key": "MIN_WAGE_REGION_1_2026",
  "value": "5310000",
  "description": "L??ng t?i thi?u vùng 1 n?m 2026. Dùng ?? tính SÀN ?óng BH và TR?N BHTN."
}
```

**Response 404 Not Found:**
```json
{
  "message": "PayrollConfig with key 'INVALID_KEY' not found."
}
```

---

#### 3. T?o Config M?i
```http
POST /api/PayrollConfigs
Content-Type: application/json

{
  "key": "NEW_CONFIG_2025",
  "value": "10000000",
  "description": "Mô t? config m?i"
}
```

**Response 201 Created:**
```json
{
  "key": "NEW_CONFIG_2025",
  "value": "10000000",
  "description": "Mô t? config m?i"
}
```

**Response 409 Conflict:**
```json
{
  "message": "PayrollConfig with key 'NEW_CONFIG_2025' already exists."
}
```

---

#### 4. C?p Nh?t Config
```http
PUT /api/PayrollConfigs/{key}
Content-Type: application/json

{
  "key": "MIN_WAGE_REGION_1_2026",
  "value": "5500000",
  "description": "?ã t?ng l??ng t?i thi?u 2026"
}
```

**Response 204 No Content** (Success)

**Response 400 Bad Request:**
```json
{
  "message": "Key mismatch."
}
```

---

#### 5. Xóa Config
```http
DELETE /api/PayrollConfigs/{key}
```

**Response 204 No Content** (Success)

**Response 404 Not Found:**
```json
{
  "message": "PayrollConfig with key 'INVALID_KEY' not found."
}
```

---

## ?? CÁCH S? D?NG TRONG CODE

### 1. L?y Config Value
```csharp
// Trong Service ho?c Controller
public class PayrollService
{
    private readonly ApplicationDbContext _context;
    
    public async Task<decimal> GetMinWage()
    {
        var config = await _context.PayrollConfigs
            .FindAsync("MIN_WAGE_REGION_1_2026");
        
        return decimal.Parse(config.Value);
    }
    
    public async Task<decimal> CalculateInsuranceFloor()
    {
        var minWage = await GetConfigValue("MIN_WAGE_REGION_1_2026");
        var trainedRate = await GetConfigValue("TRAINED_WORKER_RATE");
        
        return minWage * trainedRate;
        // = 5,310,000 × 1.07 = 5,681,700
    }
    
    private async Task<decimal> GetConfigValue(string key)
    {
        var config = await _context.PayrollConfigs.FindAsync(key);
        return decimal.Parse(config.Value);
    }
}
```

### 2. Tính L??ng ?óng B?o Hi?m
```csharp
public async Task<decimal> CalculateInsuranceSalary(SalaryContracts contract)
{
    var minWage = await GetConfigValue("MIN_WAGE_REGION_1_2026");
    var trainedRate = await GetConfigValue("TRAINED_WORKER_RATE");
    var capRatio = await GetConfigValue("INSURANCE_CAP_RATIO");
    var govBaseSalary = await GetConfigValue("GOV_BASE_SALARY");
    
    // Tính SÀN
    decimal floor = minWage * trainedRate; // 5,681,700
    
    // Tính TR?N
    decimal cap = govBaseSalary * capRatio; // 46,800,000
    
    // L?y l??ng ?óng BH
    decimal insuranceSalary = contract.IsStandardInsuranceMode 
        ? floor  // MINIMAL mode
        : Math.Min(contract.BaseSalary, cap); // FULL mode
    
    return insuranceSalary;
}
```

### 3. Tính Thu? TNCN
```csharp
public async Task<decimal> CalculateTaxableIncome(
    decimal grossSalary, 
    int dependentsCount)
{
    var personalDeduction = await GetConfigValue("PERSONAL_DEDUCTION");
    var dependentDeduction = await GetConfigValue("DEPENDENT_DEDUCTION");
    
    decimal totalDeduction = personalDeduction + 
                            (dependentDeduction * dependentsCount);
    
    decimal taxableIncome = grossSalary - totalDeduction;
    
    return taxableIncome > 0 ? taxableIncome : 0;
}
```

---

## ?? K?CH B?N C?P NH?T

### Khi Thay ??i L??ng T?i Thi?u (Ví d?: N?m 2027)

#### B??c 1: T?o Config M?i
```http
POST /api/PayrollConfigs
{
  "key": "MIN_WAGE_REGION_1_2027",
  "value": "5600000",
  "description": "L??ng t?i thi?u vùng 1 n?m 2027"
}
```

#### B??c 2: C?p Nh?t Code S? D?ng
```csharp
// Thay vì hardcode key
var minWage = await GetConfigValue("MIN_WAGE_REGION_1_2026");

// Dùng key ??ng theo n?m
var year = DateTime.Now.Year;
var key = $"MIN_WAGE_REGION_1_{year}";
var minWage = await GetConfigValue(key);
```

#### B??c 3: Migration (Optional)
```csharp
// T?o migration ?? seed config m?i
migrationBuilder.InsertData(
    table: "PayrollConfig",
    columns: new[] { "Key", "Value", "Description" },
    values: new object[] 
    { 
        "MIN_WAGE_REGION_1_2027", 
        "5600000", 
        "L??ng t?i thi?u vùng 1 n?m 2027" 
    });
```

---

## ?? L?U Ý QUAN TR?NG

### 1. Validation
- `Key` ph?i unique và không ???c r?ng
- `Value` nên validate theo ki?u d? li?u (number, string, boolean)
- Không xóa các key ?ang ???c s? d?ng trong code

### 2. B?o M?t
- Ch? Admin ho?c HR Manager m?i ???c phép c?p nh?t configs
- Log m?i thay ??i ?? audit

### 3. Performance
- Cache configs th??ng xuyên s? d?ng
- Refresh cache khi có update

### 4. Testing
- Test k? các công th?c tính toán khi thay ??i config
- Verify k?t qu? trên môi tr??ng staging tr??c khi deploy production

---

## ?? LIÊN H? & H? TR?

**Module Owner:** Backend Development Team  
**Created Date:** December 23, 2024  
**Last Updated:** December 23, 2024  
**Version:** 1.0.0

---

**?? Document Status:** ? **COMPLETED & REVIEWED**
