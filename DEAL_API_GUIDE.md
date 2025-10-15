# Deal API Documentation

## T?ng quan
Deal API ?ã ???c ??ng b? v?i Deal entity m?i và áp d?ng pattern nh?t quán v?i User và Customer APIs.

## Các endpoints

### 1. GET /api/deals
- **Mô t?**: L?y danh sách t?t c? deals
- **Authentication**: Yêu c?u JWT token
- **Response**: Array of Deal objects

### 2. GET /api/deals/{id}
- **Mô t?**: L?y deal theo ID
- **Authentication**: Yêu c?u JWT token
- **Response**: Deal object ho?c 404 Not Found

### 3. GET /api/deals/by-customer/{customerId}
- **Mô t?**: L?y t?t c? deals c?a m?t customer
- **Authentication**: Yêu c?u JWT token
- **Response**: Array of Deal objects

### 4. GET /api/deals/statistics
- **Mô t?**: Th?ng kê t?ng quan v? deals
- **Authentication**: Yêu c?u JWT token
- **Response**: 
```json
{
    "totalDeals": 10,
    "totalValue": 500000.00,
    "averageProbability": 65.5,
    "probabilityRanges": [
        {
            "probabilityRange": "Low (0-25%)",
            "count": 2,
            "totalValue": 50000.00
        },
        {
            "probabilityRange": "Medium (26-50%)",
            "count": 3,
            "totalValue": 120000.00
        }
    ]
}
```

### 5. POST /api/deals
- **Mô t?**: T?o deal m?i
- **Authentication**: Yêu c?u JWT token
- **Request Body**:
```json
{
    "title": "Website Development Project",
    "customerId": 1,
    "value": 50000.00,
    "probability": 75,
    "notes": "Optional notes",
    "services": 1
}
```
- **Validation**:
  - `title`: Required, max 255 characters
  - `customerId`: Required, must exist in database
  - `value`: Required, >= 0
  - `probability`: 0-100
  - `notes`: Optional, max 2000 characters
  - `services`: Optional integer

### 6. PUT /api/deals/{id}
- **Mô t?**: C?p nh?t deal (partial update)
- **Authentication**: Yêu c?u JWT token
- **Request Body**: Ch? g?i các tr??ng c?n c?p nh?t
```json
{
    "title": "Updated Title",
    "value": 60000.00,
    "probability": 85
}
```
- **Response**:
```json
{
    "message": "C?p nh?t thông tin deal thành công",
    "deal": {
        "id": 1,
        "title": "Updated Title",
        "customerId": 1,
        "value": 60000.00,
        "probability": 85,
        "notes": "Existing notes",
        "services": 1
    },
    "updatedAt": "2024-01-15T10:30:00Z"
}
```

### 7. PATCH /api/deals/{id}/probability
- **Mô t?**: C?p nh?t ch? xác su?t deal
- **Authentication**: Yêu c?u JWT token
- **Request Body**:
```json
{
    "probability": 90
}
```
- **Response**:
```json
{
    "message": "C?p nh?t xác su?t thành công",
    "id": 1,
    "probability": 90,
    "updatedAt": "2024-01-15T10:30:00Z"
}
```

### 8. DELETE /api/deals/{id}
- **Mô t?**: Xóa deal
- **Authentication**: Yêu c?u JWT token
- **Response**:
```json
{
    "message": "Xóa deal thành công",
    "deletedDeal": {
        "id": 1,
        "title": "Website Development Project",
        "customerId": 1,
        "value": 50000.00,
        "probability": 75,
        "notes": "Client notes",
        "services": 1
    },
    "deletedAt": "2024-01-15T10:30:00Z"
}
```

## Deal Entity Structure

```csharp
public class Deal
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Value { get; set; }
    
    [Range(0, 100)]
    public int Probability { get; set; } = 0;
    
    [StringLength(2000)]
    public string? Notes { get; set; }
    
    public int? Services { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}
```

## Validation Rules

### Title
- **Required**: B?t bu?c
- **Max Length**: 255 ký t?

### CustomerId
- **Required**: B?t bu?c
- **Foreign Key**: Ph?i t?n t?i trong b?ng Customers

### Value
- **Required**: B?t bu?c
- **Range**: >= 0
- **Type**: Decimal (15,2)

### Probability
- **Range**: 0-100%
- **Default**: 0

### Notes
- **Optional**: Không b?t bu?c
- **Max Length**: 2000 ký t?

### Services
- **Optional**: Không b?t bu?c
- **Type**: Integer

## Error Responses

### 400 Bad Request
```json
{
    "message": "Customer không t?n t?i"
}
```

### 401 Unauthorized
```json
{
    "message": "Unauthorized"
}
```

### 404 Not Found
```json
{
    "message": "Không tìm th?y deal"
}
```

### 500 Internal Server Error
```json
{
    "message": "L?i server khi t?o deal",
    "error": "Detailed error message"
}
```

## Key Features

1. **Partial Update**: PUT endpoint h? tr? partial update
2. **Comprehensive Validation**: Validation ??y ?? cho t?t c? fields
3. **Error Handling**: Proper error handling v?i logging
4. **JWT Authentication**: T?t c? endpoints yêu c?u authentication
5. **Statistics**: Endpoint th?ng kê v?i probability ranges
6. **Consistent Response**: Response format nh?t quán v?i User/Customer APIs
7. **Customer Relationship**: Validate customer existence khi t?o/update deal

## Database Configuration

Entity ???c c?u hình trong `ApplicationDbContext`:
- Foreign key relationship v?i Customer
- Indexes cho performance
- Proper column types và constraints

## Changes Made

1. **Simplified Entity**: Lo?i b? các properties không c?n thi?t (Stage, Priority, AssignedTo, etc.)
2. **Updated Controller**: Apply partial update pattern t? User/Customer controllers
3. **Added DTOs**: DealInfo, UpdateDealResponse, DeleteDealResponse
4. **Enhanced Validation**: Comprehensive validation cho all fields
5. **Statistics Endpoint**: Thay th? pipeline endpoint b?ng statistics endpoint phù h?p h?n
6. **Consistent Error Handling**: Error handling và logging nh?t quán
7. **JWT Authentication**: T?t c? endpoints yêu c?u authentication