# ?? QR CODE PAYMENT INTEGRATION GUIDE

## ?? T?ng Quan

H? th?ng ?ã tích h?p tính n?ng t?o mã QR thanh toán cho h?p ??ng s? d?ng Sepay API v?i **3 lo?i thanh toán**:
1. **??t c?c 50%** (Deposit 50%)
2. **Thanh toán n?t 50%** (Final 50%)
3. **Thanh toán 100%** (Full 100%)

Khách hàng có th? quét mã QR ?? thanh toán tr?c ti?p vào tài kho?n ngân hàng c?a công ty, và h? th?ng s? t? ??ng kh?p thanh toán v?i h?p ??ng thông qua webhook.

## ?? C?u Hình

### 1. C?u hình trong `appsettings.json`:

```json
{
  "Sepay": {
    "BankCode": "MB",
    "AccountNumber": "0123456789",
    "AccountName": "CONG TY TNHH ABC",
    "PaymentContentTemplates": {
      "Deposit50": "DatCoc50%HopDong{0}",
      "Final50": "ThanhToan50%HopDong{0}",
      "Full100": "ThanhToanHopDong{0}"
    }
  }
}
```

**L?u ý:** 
- `BankCode`: Mã ngân hàng (VD: MB, VCB, TCB, ACB...)
- `AccountNumber`: S? tài kho?n nh?n ti?n
- `AccountName`: Tên ch? tài kho?n
- **`PaymentContentTemplates`**: Các m?u n?i dung chuy?n kho?n (có th? tùy ch?nh)
  - `{0}` s? ???c thay th? b?ng s? h?p ??ng (VD: 128)
  - **Deposit50**: M?u cho ??t c?c 50% ? `DatCoc50%HopDong128`
  - **Final50**: M?u cho thanh toán n?t 50% ? `ThanhToan50%HopDong128`
  - **Full100**: M?u cho thanh toán 100% ? `ThanhToanHopDong128`

### 2. Ví d? tùy ch?nh n?i dung:

```json
"PaymentContentTemplates": {
  "Deposit50": "DATCOC 50% HD {0}",
  "Final50": "TRA NO HD {0}",
  "Full100": "THANH TOAN HD {0}"
}
```

K?t qu? v?i h?p ??ng #128:
- ??t c?c: `DATCOC 50% HD 128`
- Thanh toán n?t: `TRA NO HD 128`
- Thanh toán 100%: `THANH TOAN HD 128`

## ?? API Endpoint

### **GET /api/Contracts/{id}/qr-code?paymentType={type}**

T?o mã QR thanh toán cho h?p ??ng v?i lo?i thanh toán c? th?.

#### Parameters:

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `id` | int | ? Yes | - | Contract ID |
| `paymentType` | string | ? No | "full100" | Lo?i thanh toán: `full100`, `deposit50`, `final50` |

#### Request Examples:

```http
# Thanh toán 100% (m?c ??nh)
GET /api/Contracts/123/qr-code
Authorization: Bearer {token}

# ??t c?c 50%
GET /api/Contracts/123/qr-code?paymentType=deposit50
Authorization: Bearer {token}

# Thanh toán n?t 50%
GET /api/Contracts/123/qr-code?paymentType=final50
Authorization: Bearer {token}
```

#### Response Success (200 OK):

**??t c?c 50% (deposit50):**
```json
{
  "success": true,
  "contractId": 123,
  "contractNumber": 128,
  "paymentType": "deposit50",
  "paymentTypeDisplay": "??t c?c 50%",
  "qrCodeUrl": "https://qr.sepay.vn/img?acc=0123456789&bank=MB&amount=25000000&des=DatCoc50%25HopDong128",
  "paymentInfo": {
    "bankCode": "MB",
    "bankName": "Ngân hàng TMCP Quân ??i (MB Bank)",
    "accountNumber": "0123456789",
    "accountName": "CONG TY TNHH ABC",
    "amount": 25000000,
    "amountFormatted": "25,000,000",
    "totalAmount": 50000000,
    "totalAmountFormatted": "50,000,000",
    "description": "DatCoc50%HopDong128",
    "status": "Draft"
  }
}
```

**Thanh toán n?t 50% (final50):**
```json
{
  "success": true,
  "contractId": 123,
  "contractNumber": 128,
  "paymentType": "final50",
  "paymentTypeDisplay": "Thanh toán n?t 50%",
  "qrCodeUrl": "https://qr.sepay.vn/img?acc=0123456789&bank=MB&amount=25000000&des=ThanhToan50%25HopDong128",
  "paymentInfo": {
    "bankCode": "MB",
    "bankName": "Ngân hàng TMCP Quân ??i (MB Bank)",
    "accountNumber": "0123456789",
    "accountName": "CONG TY TNHH ABC",
    "amount": 25000000,
    "amountFormatted": "25,000,000",
    "totalAmount": 50000000,
    "totalAmountFormatted": "50,000,000",
    "description": "ThanhToan50%HopDong128",
    "status": "Draft"
  }
}
```

**Thanh toán 100% (full100):**
```json
{
  "success": true,
  "contractId": 123,
  "contractNumber": 128,
  "paymentType": "full100",
  "paymentTypeDisplay": "Thanh toán 100%",
  "qrCodeUrl": "https://qr.sepay.vn/img?acc=0123456789&bank=MB&amount=50000000&des=ThanhToanHopDong128",
  "paymentInfo": {
    "bankCode": "MB",
    "bankName": "Ngân hàng TMCP Quân ??i (MB Bank)",
    "accountNumber": "0123456789",
    "accountName": "CONG TY TNHH ABC",
    "amount": 50000000,
    "amountFormatted": "50,000,000",
    "totalAmount": 50000000,
    "totalAmountFormatted": "50,000,000",
    "description": "ThanhToanHopDong128",
    "status": "Draft"
  }
}
```

## ?? Frontend Implementation

### 1. **React/Vue Component Example:**

```javascript
// Component hi?n th? các nút thanh toán
const ContractPayment = ({ contractId }) => {
  const [qrData, setQrData] = useState(null);
  const [paymentType, setPaymentType] = useState('full100');

  const generateQR = async (type) => {
    const response = await fetch(
      `/api/Contracts/${contractId}/qr-code?paymentType=${type}`,
      {
        headers: { 'Authorization': `Bearer ${token}` }
      }
    );
    const data = await response.json();
    setQrData(data);
    setPaymentType(type);
  };

  return (
    <div className="payment-section">
      <h3>Ch?n lo?i thanh toán</h3>
      
      <div className="payment-buttons">
        <button onClick={() => generateQR('deposit50')}>
          ??t c?c 50% ({formatMoney(totalAmount * 0.5)} VN?)
        </button>
        <button onClick={() => generateQR('final50')}>
          Thanh toán n?t 50% ({formatMoney(totalAmount * 0.5)} VN?)
        </button>
        <button onClick={() => generateQR('full100')}>
          Thanh toán 100% ({formatMoney(totalAmount)} VN?)
        </button>
      </div>

      {qrData && (
        <div className="qr-display">
          <h4>{qrData.paymentTypeDisplay}</h4>
          <img src={qrData.qrCodeUrl} alt="QR Code Payment" />
          
          <div className="payment-info">
            <p><strong>Ngân hàng:</strong> {qrData.paymentInfo.bankName}</p>
            <p><strong>S? TK:</strong> {qrData.paymentInfo.accountNumber}</p>
            <p><strong>Ch? TK:</strong> {qrData.paymentInfo.accountName}</p>
            <p><strong>S? ti?n:</strong> {qrData.paymentInfo.amountFormatted} VN?</p>
            <p><strong>N?i dung:</strong> {qrData.paymentInfo.description}</p>
          </div>
          
          <button onClick={() => copyToClipboard(qrData.paymentInfo.description)}>
            ?? Copy N?i Dung
          </button>
        </div>
      )}
    </div>
  );
};
```

### 2. **Axios/Fetch API Calls:**

```javascript
// Service ?? g?i API
const PaymentService = {
  // ??t c?c 50%
  getDepositQR: async (contractId) => {
    return axios.get(`/api/Contracts/${contractId}/qr-code?paymentType=deposit50`, {
      headers: { Authorization: `Bearer ${token}` }
    });
  },

  // Thanh toán n?t 50%
  getFinalPaymentQR: async (contractId) => {
    return axios.get(`/api/Contracts/${contractId}/qr-code?paymentType=final50`, {
      headers: { Authorization: `Bearer ${token}` }
    });
  },

  // Thanh toán 100%
  getFullPaymentQR: async (contractId) => {
    return axios.get(`/api/Contracts/${contractId}/qr-code?paymentType=full100`, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }
};
```

## ?? Quy Trình T? ??ng v?i Webhook

### 1. **Flow Diagram:**

```
???????????????    QR Code      ????????????????
?  Customer   ? ??????????????  ?  Mobile Bank ?
???????????????                 ????????????????
                                       ?
                                       ? Scan & Pay
                                       ?
                                ????????????????
                                ?    Sepay     ?
                                ?   Webhook    ?
                                ????????????????
                                       ?
                                       ? POST /api/webhooks/sepay-payment
                                       ?
                                ????????????????
                                ?   Backend    ?
                                ?   Webhook    ?
                                ????????????????
                                       ?
                    ???????????????????????????????????????
                    ?                  ?                  ?
                    ?                  ?                  ?
            ????????????????   ????????????????   ????????????????
            ?Parse Content ?   ?Find Contract ?   ? Match Amount ?
            ????????????????   ????????????????   ????????????????
                    ?                  ?                  ?
                    ???????????????????????????????????????
                                       ?
                            ???????????????????????
                            ? Create Transaction  ?
                            ? Update Contract     ?
                            ? Calculate KPI       ?
                            ???????????????????????
```

### 2. **Webhook Processing Logic:**

Webhook t? ??ng nh?n di?n lo?i thanh toán t? n?i dung chuy?n kho?n:

| N?i dung CK | Lo?i thanh toán | S? ti?n | C?p nh?t Status |
|-------------|-----------------|---------|-----------------|
| `DatCoc50%HopDong128` | ??t c?c 50% | 50% Total | Gi? nguyên (ho?c "PartiallyPaid") |
| `ThanhToan50%HopDong128` | Thanh toán n?t 50% | 50% Total | ? ? "Paid" |
| `ThanhToanHopDong128` | Thanh toán 100% | 100% Total | ? ? "Paid" |

### 3. **Webhook Response Examples:**

**??t c?c 50% - Success:**
```json
{
  "message": "Payment matched successfully",
  "processed": true,
  "contractId": 123,
  "contractNumber": 128,
  "transactionId": "TXN123456",
  "paymentType": "deposit50",
  "paymentTypeDescription": "??t c?c 50%",
  "amount": 25000000,
  "contractStatus": "Draft"
}
```

**Thanh toán n?t 50% - Success:**
```json
{
  "message": "Payment matched successfully",
  "processed": true,
  "contractId": 123,
  "contractNumber": 128,
  "transactionId": "TXN123457",
  "paymentType": "final50",
  "paymentTypeDescription": "Thanh toán n?t 50%",
  "amount": 25000000,
  "contractStatus": "Paid"
}
```

**Thanh toán 100% - Success:**
```json
{
  "message": "Payment matched successfully",
  "processed": true,
  "contractId": 123,
  "contractNumber": 128,
  "transactionId": "TXN123458",
  "paymentType": "full100",
  "paymentTypeDescription": "Thanh toán 100%",
  "amount": 50000000,
  "contractStatus": "Paid"
}
```

## ?? Use Cases & Scenarios

### Scenario 1: ??t c?c tr??c

```
1. Khách hàng xem h?p ??ng #128 (50,000,000 VN?)
2. Ch?n "??t c?c 50%"
3. Quét QR ? Thanh toán 25,000,000 VN?
4. N?i dung: "DatCoc50%HopDong128"
5. Webhook nh?n ? T?o transaction ? Status gi? nguyên
6. Khách hàng v?n có th? thanh toán n?t 50%
```

### Scenario 2: Thanh toán ?? sau ??t c?c

```
1. ?ã ??t c?c 50% ? Scenario 1
2. Ch?n "Thanh toán n?t 50%"
3. Quét QR ? Thanh toán 25,000,000 VN?
4. N?i dung: "ThanhToan50%HopDong128"
5. Webhook nh?n ? T?o transaction ? Status = "Paid"
6. ? KPI ???c tính t? ??ng
```

### Scenario 3: Thanh toán 100% m?t l?n

```
1. Khách hàng xem h?p ??ng #129 (80,000,000 VN?)
2. Ch?n "Thanh toán 100%"
3. Quét QR ? Thanh toán 80,000,000 VN?
4. N?i dung: "ThanhToanHopDong129"
5. Webhook nh?n ? T?o transaction ? Status = "Paid"
6. ? KPI ???c tính t? ??ng
```

## ?? UI/UX Recommendations

### Desktop View:

```
??????????????????????????????????????????????
?         THANH TOÁN H?P ??NG #128          ?
?         T?ng giá tr?: 50,000,000 VN?      ?
??????????????????????????????????????????????
?                                            ?
?  [ ??t c?c 50% ]  [ Thanh toán 50% ]      ?
?      25tr             25tr                 ?
?                                            ?
?         [ Thanh toán 100% - 50tr ]        ?
?                                            ?
??????????????????????????????????????????????
?              [QR CODE IMAGE]               ?
?                                            ?
?  Lo?i: ??t c?c 50%                        ?
?  Ngân hàng: MB Bank                        ?
?  S? TK: 0123456789                         ?
?  S? ti?n: 25,000,000 VN?                  ?
?  N?i dung: DatCoc50%HopDong128            ?
?                                            ?
?  [?? Copy N?i Dung]  [?? T?i QR]         ?
??????????????????????????????????????????????
```

### Mobile View:

```
???????????????????????????
?  H?P ??NG #128         ?
?  50,000,000 VN?        ?
???????????????????????????
?                         ?
?   CH?N LO?I THANH TOÁN ?
?                         ?
?  ???????????????????   ?
?  ? ??t c?c 50%     ?   ?
?  ?  25,000,000 VN? ?   ?
?  ???????????????????   ?
?                         ?
?  ???????????????????   ?
?  ? Thanh toán 50%  ?   ?
?  ?  25,000,000 VN? ?   ?
?  ???????????????????   ?
?                         ?
?  ???????????????????   ?
?  ? Thanh toán 100% ?   ?
?  ?  50,000,000 VN? ?   ?
?  ???????????????????   ?
?                         ?
???????????????????????????
?                         ?
?     [QR CODE IMAGE]     ?
?                         ?
?  ?? M? App Ngân Hàng   ?
?  ?? Copy N?i Dung      ?
?                         ?
???????????????????????????
```

## ?? B?o M?t & Validation

### Backend Validation:

1. **Amount Matching:**
   - Cho phép sai l?ch 1% (tolerance)
   - ??t c?c 50%: Ki?m tra = TotalAmount × 0.5
   - Thanh toán 50%: Ki?m tra = TotalAmount × 0.5
   - Thanh toán 100%: Ki?m tra = TotalAmount

2. **Duplicate Prevention:**
   - Check TransactionId ?ã t?n t?i ch?a
   - Không x? lý l?i transaction ?ã matched

3. **Contract Validation:**
   - Ki?m tra h?p ??ng t?n t?i
   - Parse ?úng s? h?p ??ng t? n?i dung

### Frontend Validation:

```javascript
// Ki?m tra tr?ng thái h?p ??ng tr??c khi t?o QR
const canGeneratePayment = (contract, paymentType) => {
  if (contract.status === 'Paid') {
    return { allowed: false, message: 'H?p ??ng ?ã thanh toán ??' };
  }
  
  if (paymentType === 'final50' && !hasDepositPaid(contract)) {
    return { allowed: false, message: 'Ch?a ??t c?c 50%' };
  }
  
  return { allowed: true, message: 'OK' };
};
```

## ?? Troubleshooting

### L?i th??ng g?p:

| L?i | Nguyên nhân | Gi?i pháp |
|-----|-------------|-----------|
| Amount mismatch | S? ti?n không kh?p v?i lo?i thanh toán | Ki?m tra paymentType parameter |
| Contract not found | S? h?p ??ng sai ho?c không t?n t?i | Verify contract number |
| Transaction already processed | Webhook g?i l?i | B? qua, ?ã x? lý r?i |
| Cannot extract contract number | Format n?i dung sai | Check PaymentContentTemplates |

### Debug Webhook:

```bash
# Xem logs webhook
tail -f logs/webhook.log | grep "sepay-payment"

# Test webhook locally
curl -X POST http://localhost:5000/api/webhooks/sepay-payment \
  -H "Content-Type: application/json" \
  -d '{
    "TransactionId": "TEST001",
    "Amount": 25000000,
    "Content": "DatCoc50%HopDong128",
    "TransactionDate": "2024-01-15T10:30:00Z"
  }'
```

## ?? Support & Resources

### Configuration Files:
- **Backend Config:** `appsettings.json` ? `Sepay` section
- **Templates:** Customize `PaymentContentTemplates`
- **Webhook Handler:** `WebhooksController.cs` ? `SepayPaymentWebhook`

### Logging:
```csharp
// Logs ???c ghi t? ??ng t?i:
_logger.LogInformation("Generated QR code for Contract {ContractId}, PaymentType: {PaymentType}");
_logger.LogInformation("Webhook matched payment: Type={Type}, Amount={Amount}");
```

### Testing Checklist:

- [ ] C?u hình bank info trong appsettings.json
- [ ] Test API `/qr-code` v?i 3 lo?i payment
- [ ] Verify QR code URL ?úng format
- [ ] Test webhook v?i mock data
- [ ] Ki?m tra Contract Status update ?úng
- [ ] Verify KPI calculation trigger
- [ ] Test v?i transaction th?t

---

**Version:** 2.0 (Multi-Payment Support)  
**Last Updated:** 2024  
**Author:** ERP Development Team

**Changelog v2.0:**
- ? Added 3 payment types (deposit50, final50, full100)
- ? Configurable payment content templates
- ? Enhanced webhook to detect payment type
- ? Smart amount validation per payment type
- ? Improved status update logic
