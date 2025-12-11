# ? HOÀN THÀNH: C?P NH?T TÍNH KPI CHO DEPOSIT 50%

## ?? TÓM T?T THAY ??I

?ã c?p nh?t h? th?ng ?? **tính KPI ngay khi khách hàng ??t c?c 50%**, không c?n ??i thanh toán 100%.

---

## ?? CÁC FILE ?Ã THAY ??I

### 1. **Contract.cs** - Thêm Navigation Property
**File:** `erp_backend\Models\Contract.cs`

**Thay ??i:**
```csharp
// Navigation properties
public User? User { get; set; }
public SaleOrder? SaleOrder { get; set; }

// ? THÊM M?I
[JsonIgnore]
public ICollection<MatchedTransaction>? MatchedTransactions { get; set; }
```

**Lý do:** Cho phép truy c?p t?t c? transactions c?a m?t contract ?? tính doanh thu chính xác.

---

### 2. **ApplicationDbContext.cs** - C?u hình Relationship
**File:** `erp_backend\Data\ApplicationDbContext.cs`

**Thay ??i:**
```csharp
// Configure MatchedTransaction entity
modelBuilder.Entity<MatchedTransaction>(entity =>
{
    // ...existing config...
    
    // ? C?P NH?T: Two-way relationship
    entity.HasOne(e => e.Contract)
          .WithMany(c => c.MatchedTransactions) // ? THÊM M?I
          .HasForeignKey(e => e.ContractId)
          .OnDelete(DeleteBehavior.SetNull);
    
    // ...rest of config...
});
```

**Lý do:** Thi?t l?p relationship hai chi?u gi?a Contract và MatchedTransactions.

---

### 3. **KpiCalculationService.cs** - Logic Tính KPI M?i
**File:** `erp_backend\Services\KpiCalculationService.cs`

#### Thay ??i 3.1: Thêm "Deposit 50%" vào WHERE clause
```csharp
var contractsQuery = _context.Contracts
    .Include(c => c.SaleOrder)
    .Include(c => c.MatchedTransactions) // ? THÊM M?I
    .Where(c => c.SaleOrder!.CreatedByUserId == userId
        && c.CreatedAt.Month == month
        && c.CreatedAt.Year == year
        && (c.Status == "Deposit 50%"      // ? THÊM M?I
            || c.Status == "Paid" 
            || c.Status == "Completed" 
            || c.Status == "Signed" 
            || c.Status == "Active"));
```

#### Thay ??i 3.2: Tính doanh thu t? MatchedTransactions th?c t?
```csharp
decimal totalPaidAmount = 0;
int totalContracts = contracts.Count;

foreach (var contract in contracts)
{
    // ? ?U TIÊN: L?y t?ng t? MatchedTransactions th?c t?
    var paidAmount = contract.MatchedTransactions?
        .Where(mt => mt.Status == "Matched")
        .Sum(mt => mt.Amount) ?? 0;

    if (paidAmount > 0)
    {
        totalPaidAmount += paidAmount;
    }
    else
    {
        // ? FALLBACK: Tính theo status
        decimal contractRevenue = contract.Status?.ToLower() == "deposit 50%" 
            ? contract.TotalAmount * 0.5m 
            : contract.TotalAmount;
        totalPaidAmount += contractRevenue;
    }
}
```

**Lý do:** 
- **Chính xác h?n:** Tính d?a trên s? ti?n th?c t? ?ã nh?n (t? MatchedTransactions)
- **Linh ho?t:** H? tr? c? deposit 50% và paid 100%
- **?úng nghi?p v?:** N?u khách chuy?n 2 l?n (deposit + final), t?ng doanh thu = t?ng 2 l?n chuy?n

---

### 4. **WebhooksController.cs** - Trigger KPI cho Deposit
**File:** `erp_backend\Controllers\WebhooksController.cs`

**Thay ??i:**
```csharp
// 9. ?? T? ??ng tính KPI cho deposit 50% HO?C thanh toán hoàn toàn
// ? LOGIC M?I: Trigger cho c? "Deposit 50%" và "Paid"
var shouldCalculateKpi = (oldStatus?.ToLower() != "deposit 50%" && contract.Status?.ToLower() == "deposit 50%")
                       || (oldStatus?.ToLower() != "paid" && contract.Status?.ToLower() == "paid");

if (shouldCalculateKpi)
{
    var saleUserId = contract.SaleOrder?.CreatedByUserId;
    if (saleUserId.HasValue)
    {
        _logger.LogInformation("?? Triggering KPI calculation for User {UserId} (Status: {OldStatus} ? {NewStatus})...", 
            saleUserId.Value, oldStatus, contract.Status);

        await _kpiCalculationService.CalculateKpiForUserAsync(
            saleUserId.Value,
            contract.CreatedAt.Month,
            contract.CreatedAt.Year);
    }
}
```

**Lý do:** Trigger KPI calculation cho c? 2 tr??ng h?p:
1. Khi contract chuy?n sang "Deposit 50%" ?
2. Khi contract chuy?n sang "Paid" ?

---

### 5. **Database Migration**
**File:** `erp_backend\Migrations\20241211_AddMatchedTransactionsNavigation.cs`

**Thay ??i:** Không có thay ??i schema database (ch? thêm navigation property ? code)

---

## ?? LOGIC M?I - CÁCH TÍNH DOANH THU KPI

### Quy t?c tính doanh thu:

| Status Contract | Có MatchedTransactions? | Cách Tính Doanh Thu | Ví D? (Contract 10,000,000 VN?) |
|-----------------|-------------------------|---------------------|----------------------------------|
| **Deposit 50%** | ? Có | T?ng `MatchedTransactions.Amount` | 5,000,000 VN? (1 transaction) |
| **Deposit 50%** | ? Không | `TotalAmount * 0.5` (Fallback) | 5,000,000 VN? |
| **Paid** | ? Có | T?ng `MatchedTransactions.Amount` | 10,000,000 VN? (2 transactions) |
| **Paid** | ? Không | `TotalAmount` (Fallback) | 10,000,000 VN? |
| **Completed/Signed/Active** | B?t k? | `TotalAmount` | 10,000,000 VN? |

---

## ?? VÍ D? TH?C T?

### Scenario 1: Khách hàng ??t c?c 50%

```
?? Contract #128
- T?ng giá tr?: 10,000,000 VN?
- Status: "Pending" ? "Deposit 50%"

FLOW:
1. Customer chuy?n kho?n: 5,000,000 VN? (n?i dung "ttw deposit 128")
2. Webhook nh?n payment ? T?o MatchedTransaction
3. Contract.Status = "Deposit 50%"
4. ? TRIGGER KPI CALCULATION
5. KPI Service tính:
   - totalPaidAmount = 5,000,000 VN? (t? MatchedTransactions)
   - C?ng vào doanh thu tháng c?a Sale

K?T QU?:
? Sale ???c tính doanh thu: 5,000,000 VN? ngay l?p t?c
? Hoa h?ng ???c tính trên: 5,000,000 VN?
? % KPI ???c c?p nh?t
```

### Scenario 2: Khách hàng thanh toán n?t 50%

```
?? Contract #128 (ti?p theo)
- Status: "Deposit 50%" ? "Paid"

FLOW:
1. Customer chuy?n kho?n thêm: 5,000,000 VN? (n?i dung "ttw final 128")
2. Webhook nh?n payment ? T?o MatchedTransaction m?i
3. Contract.Status = "Paid"
4. ? TRIGGER KPI CALCULATION L?I
5. KPI Service tính:
   - Transaction 1: 5,000,000 VN? (deposit)
   - Transaction 2: 5,000,000 VN? (final)
   - totalPaidAmount = 10,000,000 VN? (t?ng)

K?T QU?:
? Sale ???c c?p nh?t t?ng doanh thu: 10,000,000 VN?
? Hoa h?ng ???c tính l?i trên: 10,000,000 VN?
? % KPI ???c c?p nh?t
```

### Scenario 3: Thanh toán 100% luôn

```
?? Contract #129
- T?ng giá tr?: 20,000,000 VN?
- Status: "Pending" ? "Paid"

FLOW:
1. Customer chuy?n kho?n: 20,000,000 VN? (n?i dung "ttw paid 129")
2. Webhook nh?n payment ? T?o MatchedTransaction
3. Contract.Status = "Paid"
4. ? TRIGGER KPI CALCULATION
5. KPI Service tính:
   - totalPaidAmount = 20,000,000 VN? (t? MatchedTransactions)

K?T QU?:
? Sale ???c tính doanh thu: 20,000,000 VN?
? Hoa h?ng ???c tính trên: 20,000,000 VN?
? % KPI ???c c?p nh?t
```

### Scenario 4: Nhi?u contracts trong tháng

```
?? Sale A - Tháng 12/2024:

Contract #128:
- Status: "Deposit 50%"
- MatchedTransaction: 5,000,000 VN?
- ?óng góp KPI: 5,000,000 VN? ?

Contract #129:
- Status: "Paid"
- MatchedTransaction 1: 15,000,000 VN? (deposit)
- MatchedTransaction 2: 15,000,000 VN? (final)
- ?óng góp KPI: 30,000,000 VN? ?

Contract #130:
- Status: "Pending" (ch?a thanh toán)
- ?óng góp KPI: 0 VN? ?

T?NG DOANH THU KPI: 35,000,000 VN?
```

---

## ?? TESTING

### Test Case 1: Webhook Deposit 50%

**Request:**
```bash
POST http://localhost:5000/api/webhooks/sepay-payment
Content-Type: application/json

{
  "id": 123456,
  "gateway": "MB Bank",
  "transactionDate": "2024-12-11 14:30:00",
  "accountNumber": "0375422346",
  "content": "ttw deposit 128",
  "transferType": "in",
  "transferAmount": 5000000,
  "referenceCode": "MBVCB.123456"
}
```

**Expected Response:**
```json
{
  "success": true,
  "processed": true,
  "message": "Payment matched successfully",
  "data": {
    "contractId": 42,
    "contractNumber": 128,
    "transactionId": "SEPAY_123456",
    "paymentType": "deposit50",
    "paymentTypeDescription": "??t c?c 50%",
    "amount": 5000000,
    "contractStatus": "Deposit 50%"
  }
}
```

**Expected Logs:**
```
? Nh?n webhook t? Sepay: ID=123456, Gateway=MB Bank, Amount=5000000
?? Extracted contract number: 128 with payment type: Deposit 50%
? Contract 42 status changed to 'Deposit 50%'
?? ?ã match payment thành công: Contract 42, Transaction SEPAY_123456
?? Sent SignalR notification to group Contract_42
?? Triggering KPI calculation for User 5 (Status: Pending ? Deposit 50%)...
?? User 5: T?ng 1 h?p ??ng, T?ng doanh thu: 5,000,000 VN?
?? User 5: Commission Tier 1, Rate 5%, Amount 250,000 VN?
?? User 5: ??t 10.00% KPI (5,000,000/50,000,000)
? KPI calculated successfully for User 5
```

### Test Case 2: Webhook Final 50%

**Request:**
```bash
POST http://localhost:5000/api/webhooks/sepay-payment
Content-Type: application/json

{
  "id": 123457,
  "gateway": "MB Bank",
  "transactionDate": "2024-12-11 15:00:00",
  "accountNumber": "0375422346",
  "content": "ttw final 128",
  "transferType": "in",
  "transferAmount": 5000000,
  "referenceCode": "MBVCB.123457"
}
```

**Expected Response:**
```json
{
  "success": true,
  "processed": true,
  "message": "Payment matched successfully",
  "data": {
    "contractId": 42,
    "contractNumber": 128,
    "transactionId": "SEPAY_123457",
    "paymentType": "final50",
    "paymentTypeDescription": "Thanh toán n?t 50%",
    "amount": 5000000,
    "contractStatus": "Paid"
  }
}
```

**Expected Logs:**
```
? Nh?n webhook t? Sepay: ID=123457, Gateway=MB Bank, Amount=5000000
?? Extracted contract number: 128 with payment type: Final 50%
? Contract 42 status changed to 'Paid' (Final 50%)
?? ?ã match payment thành công: Contract 42, Transaction SEPAY_123457
?? Sent SignalR notification to group Contract_42
?? Triggering KPI calculation for User 5 (Status: Deposit 50% ? Paid)...
Contract 42 (#128): ?ã thanh toán 10,000,000 VN? (t? transactions)
?? User 5: T?ng 1 h?p ??ng, T?ng doanh thu: 10,000,000 VN?
?? User 5: Commission Tier 1, Rate 5%, Amount 500,000 VN?
?? User 5: ??t 20.00% KPI (10,000,000/50,000,000)
? KPI calculated successfully for User 5
```

---

## ?? L?U Ý

### 1. Migration Database
```bash
# ?ã t?o migration
dotnet ef migrations add AddMatchedTransactionsNavigation

# Ch?a apply vào database - C?N CH?Y:
dotnet ef database update
```

### 2. Data Consistency
- **MatchedTransactions là ngu?n truth chính** - luôn ?u tiên s? li?u t? ?ây
- Fallback v? tính theo status ch? khi không có transactions
- Tránh tính trùng l?p (m?i transaction ch? tính 1 l?n)

### 3. Performance
- `Include(c => c.MatchedTransactions)` có th? ?nh h??ng performance n?u contract có nhi?u transactions
- ?? xu?t: Thêm index cho `MatchedTransactions.ContractId` và `MatchedTransactions.Status`

### 4. Edge Cases

#### Case 1: Contract có nhi?u transactions nh?
```
Contract #128:
- Transaction 1: 2,000,000 VN?
- Transaction 2: 3,000,000 VN?
? Total: 5,000,000 VN? ?
```

#### Case 2: Contract ch?a có transaction
```
Contract #129:
- Status: "Deposit 50%"
- MatchedTransactions: [] (r?ng)
? Fallback: TotalAmount * 0.5 = 5,000,000 VN? ?
```

#### Case 3: Contract có 3 l?n thanh toán
```
Contract #130:
- Transaction 1: 3,000,000 VN?
- Transaction 2: 3,000,000 VN?
- Transaction 3: 4,000,000 VN?
? Total: 10,000,000 VN? ?
```

---

## ? CHECKLIST TRI?N KHAI

- [x] C?p nh?t Contract model (thêm navigation property)
- [x] C?p nh?t ApplicationDbContext (configure relationship)
- [x] C?p nh?t KpiCalculationService (logic tính KPI m?i)
- [x] C?p nh?t WebhooksController (trigger cho deposit)
- [x] T?o migration
- [x] Build thành công
- [ ] **C?N LÀM: Apply migration vào database** (`dotnet ef database update`)
- [ ] **C?N LÀM: Test v?i webhook th?c t?**
- [ ] **C?N LÀM: Ki?m tra KPI dashboard**

---

## ?? K?T LU?N

H? th?ng ?ã ???c c?p nh?t thành công ??:
1. ? Tính KPI ngay khi khách hàng ??t c?c 50%
2. ? Tính doanh thu chính xác d?a trên s? ti?n th?c t? ?ã nh?n
3. ? H? tr? c? deposit 50% và thanh toán 100%
4. ? T? ??ng c?p nh?t KPI khi có thanh toán m?i

**Sale gi? ???c h??ng doanh thu ngay khi có deposit, không c?n ??i thanh toán 100%!** ??
