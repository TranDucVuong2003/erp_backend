# Monthly Attendance (Ch?m công) Implementation Complete

## Overview
?ã hoàn thành vi?c thêm b?ng **MonthlyAttendances** vào h? th?ng ERP ?? qu?n lý ch?m công hàng tháng c?a nhân viên.

---

## MonthlyAttendance (B?ng ch?m công tháng)

### Model Structure
```csharp
public class MonthlyAttendance
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Month { get; set; }                    // 1-12
    public int Year { get; set; }                     // 2020-2100
    public float ActualWorkDays { get; set; }         // S? ngày công th?c t? (User g?i là B)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public User? User { get; set; }
}
```

### Database Configuration
- **Primary Key**: Id
- **Foreign Key**: UserId ? Users.Id (Restrict)
- **Unique Index**: (UserId, Month, Year) - M?i user ch? có 1 b?n ch?m công/tháng
- **Indexes**: 
  - UserId
  - (UserId, Month, Year) - Composite unique
  - (Month, Year) - Composite for period queries
- **Data Type**: ActualWorkDays - float (cho phép 0.5, 20.5 ngày)

---

## API Endpoints

### 1. GET /api/MonthlyAttendances
L?y danh sách t?t c? b?n ch?m công (s?p x?p theo n?m, tháng, tên user)

**Response:**
```json
[
  {
    "id": 1,
    "userId": 5,
    "userName": "Nguy?n V?n A",
    "userEmail": "nguyenvana@example.com",
    "month": 12,
    "year": 2024,
    "actualWorkDays": 20.5,
    "createdAt": "2024-12-22T10:30:00Z",
    "updatedAt": null
  }
]
```

---

### 2. GET /api/MonthlyAttendances/{id}
L?y thông tin b?n ch?m công theo ID

**Example:** `GET /api/MonthlyAttendances/1`

---

### 3. GET /api/MonthlyAttendances/user/{userId}
L?y t?t c? b?n ch?m công c?a m?t user (s?p x?p theo n?m, tháng gi?m d?n)

**Example:** `GET /api/MonthlyAttendances/user/5`

**Response:**
```json
[
  {
    "id": 1,
    "userId": 5,
    "userName": "Nguy?n V?n A",
    "month": 12,
    "year": 2024,
    "actualWorkDays": 20.5,
    ...
  },
  {
    "id": 2,
    "userId": 5,
    "userName": "Nguy?n V?n A",
    "month": 11,
    "year": 2024,
    "actualWorkDays": 22.0,
    ...
  }
]
```

---

### 4. GET /api/MonthlyAttendances/user/{userId}/month/{month}/year/{year}
L?y b?n ch?m công c?a user trong tháng c? th?

**Example:** `GET /api/MonthlyAttendances/user/5/month/12/year/2024`

**Response:**
```json
{
  "id": 1,
  "userId": 5,
  "userName": "Nguy?n V?n A",
  "userEmail": "nguyenvana@example.com",
  "month": 12,
  "year": 2024,
  "actualWorkDays": 20.5,
  "createdAt": "2024-12-22T10:30:00Z",
  "updatedAt": null
}
```

**Error Response (404):**
```json
{
  "message": "Không tìm th?y b?n ch?m công tháng 12/2024 c?a user này"
}
```

---

### 5. GET /api/MonthlyAttendances/month/{month}/year/{year}
L?y t?t c? b?n ch?m công trong tháng (t?t c? nhân viên) + th?ng kê

**Example:** `GET /api/MonthlyAttendances/month/12/year/2024`

**Response:**
```json
{
  "month": 12,
  "year": 2024,
  "attendances": [
    {
      "id": 1,
      "userId": 5,
      "userName": "Nguy?n V?n A",
      "userEmail": "nguyenvana@example.com",
      "department": "IT",
      "position": "Developer",
      "month": 12,
      "year": 2024,
      "actualWorkDays": 20.5,
      "createdAt": "2024-12-22T10:30:00Z",
      "updatedAt": null
    },
    {
      "id": 2,
      "userId": 6,
      "userName": "Tr?n Th? B",
      "department": "IT",
      "position": "Tester",
      "actualWorkDays": 22.0,
      ...
    }
  ],
  "statistics": {
    "totalEmployees": 2,
    "totalWorkDays": 42.5,
    "averageWorkDays": 21.25
  }
}
```

---

### 6. POST /api/MonthlyAttendances
T?o b?n ch?m công m?i

**Request:**
```json
{
  "userId": 5,
  "month": 12,
  "year": 2024,
  "actualWorkDays": 20.5
}
```

**Response:**
```json
{
  "message": "T?o b?n ch?m công thành công",
  "attendance": {
    "id": 1,
    "userId": 5,
    "userName": "Nguy?n V?n A",
    "userEmail": "nguyenvana@example.com",
    "month": 12,
    "year": 2024,
    "actualWorkDays": 20.5,
    "createdAt": "2024-12-22T10:30:00Z",
    "updatedAt": null
  }
}
```

**Validations:**
- ? UserId ph?i t?n t?i trong b?ng Users
- ? Month ph?i t? 1-12
- ? Year ph?i t? 2020-2100
- ? ActualWorkDays ph?i t? 0-31
- ? M?t user ch? có th? có 1 b?n ch?m công/tháng (unique constraint)

**Error Response (400):**
```json
{
  "message": "User này ?ã có b?n ch?m công tháng 12/2024. Vui lòng s? d?ng ph??ng th?c c?p nh?t."
}
```

---

### 7. PUT /api/MonthlyAttendances/{id}
C?p nh?t b?n ch?m công (h? tr? partial update)

**Example:** `PUT /api/MonthlyAttendances/1`

**Request:**
```json
{
  "actualWorkDays": 21.5
}
```

**Full Update Request:**
```json
{
  "userId": 5,
  "month": 12,
  "year": 2024,
  "actualWorkDays": 21.5
}
```

**Response:**
```json
{
  "message": "C?p nh?t b?n ch?m công thành công",
  "attendance": {
    "id": 1,
    "userId": 5,
    "userName": "Nguy?n V?n A",
    "month": 12,
    "year": 2024,
    "actualWorkDays": 21.5,
    "updatedAt": "2024-12-22T11:00:00Z"
  }
}
```

**Note:** Khi c?p nh?t userId, month, ho?c year, h? th?ng s? ki?m tra unique constraint ?? tránh trùng l?p.

---

### 8. DELETE /api/MonthlyAttendances/{id}
Xóa b?n ch?m công

**Example:** `DELETE /api/MonthlyAttendances/1`

**Response:**
```json
{
  "message": "Xóa b?n ch?m công thành công",
  "deletedAttendance": {
    "id": 1,
    "userId": 5,
    "userName": "Nguy?n V?n A",
    "month": 12,
    "year": 2024,
    "actualWorkDays": 20.5,
    "createdAt": "2024-12-22T10:30:00Z"
  }
}
```

---

## Use Cases

### 1. Nh?p ch?m công cho nhân viên tháng m?i
```
POST /api/MonthlyAttendances
{
  "userId": 5,
  "month": 12,
  "year": 2024,
  "actualWorkDays": 20.5
}
```

### 2. S?a s? ngày công c?a nhân viên
```
PUT /api/MonthlyAttendances/1
{
  "actualWorkDays": 21.5
}
```

### 3. Xem ch?m công c?a m?t nhân viên
```
GET /api/MonthlyAttendances/user/5
? Tr? v? t?t c? các tháng ?ã ch?m công
```

### 4. Xem ch?m công tháng c?a t?t c? nhân viên
```
GET /api/MonthlyAttendances/month/12/year/2024
? Tr? v? danh sách + th?ng kê (t?ng nhân viên, trung bình ngày công)
```

### 5. Tính l??ng theo ngày công
```
1. GET /api/SalaryBases/user/{userId}
   ? L?y BaseSalary (P1)

2. GET /api/MonthlyAttendances/user/{userId}/month/{month}/year/{year}
   ? L?y ActualWorkDays (B)

3. Công th?c tính l??ng theo công:
   SalaryByWorkDays = (BaseSalary / 26) * ActualWorkDays
   (Gi? s? 26 ngày công chu?n/tháng)
```

---

## Integration v?i Salary Module

### K?t h?p v?i SalaryBase và SalaryComponent
```
L??ng tháng = (BaseSalary / 26) * ActualWorkDays 
            + TotalBonus 
            - TotalDeduction 
            - (BaseSalary * InsurancePercent / 100)
```

**Trong ?ó:**
- `BaseSalary` t? **SalaryBase** (P1)
- `ActualWorkDays` t? **MonthlyAttendance** (B)
- `TotalBonus` (type="in") t? **SalaryComponent**
- `TotalDeduction` (type="out") t? **SalaryComponent**
- `InsurancePercent` t? **SalaryBase**

### API Flow ?? tính l??ng tháng:
```
1. GET /api/SalaryBases/user/{userId}
2. GET /api/MonthlyAttendances/user/{userId}/month/{month}/year/{year}
3. GET /api/SalaryComponents/user/{userId}/month/{month}/year/{year}
4. Tính toán: L??ng = f(BaseSalary, ActualWorkDays, Bonus, Deduction, Insurance)
```

---

## Database Migration

### Run Migration Command
```bash
cd erp_backend
dotnet ef migrations add AddMonthlyAttendanceTable
dotnet ef database update
```

### Migration Details
**Table Name**: MonthlyAttendances

**Columns:**
- Id (PK, Identity)
- UserId (FK ? Users)
- Month (int, 1-12)
- Year (int, 2020-2100)
- ActualWorkDays (float)
- CreatedAt (timestamp with time zone, default: CURRENT_TIMESTAMP)
- UpdatedAt (timestamp with time zone, nullable)

**Constraints:**
- UNIQUE (UserId, Month, Year)
- FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE RESTRICT

**Indexes:**
- UserId
- (UserId, Month, Year) - Unique
- (Month, Year)

---

## Features Summary

### ? Core Features
- Qu?n lý s? ngày công th?c t? c?a nhân viên theo tháng
- H? tr? s? ngày công d?ng float (0.5, 20.5 ngày)
- ??m b?o m?i user ch? có 1 b?n ch?m công/tháng (unique constraint)
- CRUD ??y ?? v?i validation

### ? Query Features
- L?c theo user
- L?c theo kho?ng th?i gian (month/year)
- Th?ng kê t? ??ng (t?ng nhân viên, trung bình ngày công)
- K?t h?p thông tin Department và Position

### ? Validation Features
- Ki?m tra user t?n t?i
- Ki?m tra month (1-12)
- Ki?m tra year (2020-2100)
- Ki?m tra actualWorkDays (0-31)
- Ki?m tra trùng l?p khi t?o/c?p nh?t

### ? Integration Ready
- Tích h?p v?i SalaryBase ?? tính l??ng theo công
- Tích h?p v?i SalaryComponent ?? tính t?ng l??ng cu?i cùng
- H? tr? báo cáo ch?m công theo phòng ban

---

## Error Handling

### Common Error Responses

#### 400 Bad Request
```json
{
  "message": "User này ?ã có b?n ch?m công tháng 12/2024. Vui lòng s? d?ng ph??ng th?c c?p nh?t."
}
```

#### 404 Not Found
```json
{
  "message": "Không tìm th?y b?n ch?m công tháng 12/2024 c?a user này"
}
```

#### 500 Internal Server Error
```json
{
  "message": "L?i server khi t?o b?n ch?m công",
  "error": "Chi ti?t l?i..."
}
```

---

## Business Logic Notes

1. **Unique Constraint**: M?i user ch? có 1 b?n ch?m công/tháng ?? tránh trùng l?p.

2. **Float Data Type**: S? d?ng float cho ActualWorkDays ?? h? tr?:
   - N?a ngày công: 0.5
   - Ngh? phép có l??ng: 0.5
   - Làm thêm gi? quy ??i ngày: 1.5

3. **Standard Work Days**: M?c ??nh 26 ngày công/tháng (có th? ?i?u ch?nh theo quy ??nh công ty).

4. **Cascade Delete**: Không xóa khi xóa User (Restrict) ?? gi? l?i d? li?u l?ch s?.

5. **Audit Trail**: Có CreatedAt và UpdatedAt ?? theo dõi l?ch s? thay ??i.

---

## Next Steps (Optional Enhancements)

1. **Leave Management Integration**: Tích h?p v?i module ngh? phép
2. **Overtime Calculation**: Tính toán làm thêm gi?
3. **Holiday Management**: Qu?n lý ngày l?, ngh? phép
4. **Auto Calculate**: T? ??ng tính s? ngày công t? check-in/check-out
5. **Approval Workflow**: Quy trình phê duy?t ch?m công
6. **Bulk Import**: Nh?p hàng lo?t ch?m công t? Excel
7. **Report Generation**: Báo cáo ch?m công theo phòng ban/tháng/quý
8. **Notification**: Thông báo khi ch?a ch?m công ??

---

## Implementation Date
December 22, 2024

## Developer
GitHub Copilot with .NET 8 & EF Core

---

**Status**: ? **HOÀN THÀNH** - ?ã build thành công, s?n sàng t?o migration và s? d?ng.
