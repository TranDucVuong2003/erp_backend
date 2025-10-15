# Deal API Documentation

## T?ng quan
Deal API ?� ???c ??ng b? v?i Deal entity m?i v� �p d?ng pattern nh?t qu�n v?i User v� Customer APIs.

## C�c endpoints

### 1. GET /api/deals
- **M� t?**: L?y danh s�ch t?t c? deals
- **Authentication**: Y�u c?u JWT token
- **Response**: Array of Deal objects

### 2. GET /api/deals/{id}
- **M� t?**: L?y deal theo ID
- **Authentication**: Y�u c?u JWT token
- **Response**: Deal object ho?c 404 Not Found

### 3. GET /api/deals/by-customer/{customerId}
- **M� t?**: L?y t?t c? deals c?a m?t customer
- **Authentication**: Y�u c?u JWT token
- **Response**: Array of Deal objects

### 4. GET /api/deals/statistics
- **M� t?**: Th?ng k� t?ng quan v? deals
- **Authentication**: Y�u c?u JWT token
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
- **M� t?**: T?o deal m?i
- **Authentication**: Y�u c?u JWT token
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
- **M� t?**: C?p nh?t deal (partial update)
- **Authentication**: Y�u c?u JWT token
- **Request Body**: Ch? g?i c�c tr??ng c?n c?p nh?t
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
    "message": "C?p nh?t th�ng tin deal th�nh c�ng",
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
- **M� t?**: C?p nh?t ch? x�c su?t deal
- **Authentication**: Y�u c?u JWT token
- **Request Body**:
```json
{
    "probability": 90
}
```
- **Response**:
```json
{
    "message": "C?p nh?t x�c su?t th�nh c�ng",
    "id": 1,
    "probability": 90,
    "updatedAt": "2024-01-15T10:30:00Z"
}
```

### 8. DELETE /api/deals/{id}
- **M� t?**: X�a deal
- **Authentication**: Y�u c?u JWT token
- **Response**:
```json
{
    "message": "X�a deal th�nh c�ng",
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
- **Max Length**: 255 k� t?

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
- **Optional**: Kh�ng b?t bu?c
- **Max Length**: 2000 k� t?

### Services
- **Optional**: Kh�ng b?t bu?c
- **Type**: Integer

## Error Responses

### 400 Bad Request
```json
{
    "message": "Customer kh�ng t?n t?i"
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
    "message": "Kh�ng t�m th?y deal"
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
4. **JWT Authentication**: T?t c? endpoints y�u c?u authentication
5. **Statistics**: Endpoint th?ng k� v?i probability ranges
6. **Consistent Response**: Response format nh?t qu�n v?i User/Customer APIs
7. **Customer Relationship**: Validate customer existence khi t?o/update deal

## Database Configuration

Entity ???c c?u h�nh trong `ApplicationDbContext`:
- Foreign key relationship v?i Customer
- Indexes cho performance
- Proper column types v� constraints

## Changes Made

1. **Simplified Entity**: Lo?i b? c�c properties kh�ng c?n thi?t (Stage, Priority, AssignedTo, etc.)
2. **Updated Controller**: Apply partial update pattern t? User/Customer controllers
3. **Added DTOs**: DealInfo, UpdateDealResponse, DeleteDealResponse
4. **Enhanced Validation**: Comprehensive validation cho all fields
5. **Statistics Endpoint**: Thay th? pipeline endpoint b?ng statistics endpoint ph� h?p h?n
6. **Consistent Error Handling**: Error handling v� logging nh?t qu�n
7. **JWT Authentication**: T?t c? endpoints y�u c?u authentication