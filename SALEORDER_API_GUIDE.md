# SaleOrder API Documentation

## T?ng quan
SaleOrder API ?� ???c ??ng b? v?i SaleOrder entity m?i v� �p d?ng pattern nh?t qu�n v?i User v� Customer APIs.

## C�c endpoints

### 1. GET /api/saleorders
- **M� t?**: L?y danh s�ch t?t c? sale orders
- **Authentication**: Y�u c?u JWT token
- **Response**: Array of SaleOrder objects

### 2. GET /api/saleorders/{id}
- **M� t?**: L?y sale order theo ID
- **Authentication**: Y�u c?u JWT token
- **Response**: SaleOrder object ho?c 404 Not Found

### 3. GET /api/saleorders/by-customer/{customerId}
- **M� t?**: L?y t?t c? sale orders c?a m?t customer
- **Authentication**: Y�u c?u JWT token
- **Response**: Array of SaleOrder objects

### 4. GET /api/saleorders/statistics
- **M� t?**: Th?ng k� t?ng quan v? sale orders
- **Authentication**: Y�u c?u JWT token
- **Response**: 
```json
{
    "totalSaleOrders": 10,
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

### 5. POST /api/saleorders
- **M� t?**: T?o sale order m?i
- **Authentication**: Y�u c?u JWT token
- **Request Body**:
```json
{
    "title": "Website Development Project",
    "customerId": 1,
    "value": 50000.00,
    "probability": 75,
    "notes": "Optional notes",
    "serviceId": 1,
    "addonId": 1
}
```
- **Validation**:
  - `title`: Required, max 255 characters
  - `customerId`: Required, must exist in database
  - `value`: Required, >= 0
  - `probability`: 0-100
  - `notes`: Optional, max 2000 characters
  - `serviceId`: Optional integer
  - `addonId`: Optional integer

### 6. PUT /api/saleorders/{id}
- **M� t?**: C?p nh?t sale order (partial update)
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
    "message": "C?p nh?t th�ng tin sale order th�nh c�ng",
    "saleOrder": {
        "id": 1,
        "title": "Updated Title",
        "customerId": 1,
        "value": 60000.00,
        "probability": 85,
        "notes": "Existing notes",
        "serviceId": 1,
        "addonId": 1
    },
    "updatedAt": "2024-01-15T10:30:00Z"
}
```

### 7. PATCH /api/saleorders/{id}/probability
- **M� t?**: C?p nh?t ch? x�c su?t sale order
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

### 8. DELETE /api/saleorders/{id}
- **M� t?**: X�a sale order
- **Authentication**: Y�u c?u JWT token
- **Response**:
```json
{
    "message": "X�a sale order th�nh c�ng",
    "deletedSaleOrder": {
        "id": 1,
        "title": "Website Development Project",
        "customerId": 1,
        "value": 50000.00,
        "probability": 75,
        "notes": "Client notes",
        "serviceId": 1,
        "addonId": 1
    },
    "deletedAt": "2024-01-15T10:30:00Z"
}
```

## SaleOrder Entity Structure

```csharp
public class SaleOrder
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
    
    public int? ServiceId { get; set; }
    
    public int? AddonId { get; set; }
    
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

### ServiceId
- **Optional**: Kh�ng b?t bu?c
- **Type**: Integer
- **Foreign Key**: N?u c�, ph?i t?n t?i trong b?ng Services

### AddonId
- **Optional**: Kh�ng b?t bu?c
- **Type**: Integer
- **Foreign Key**: N?u c�, ph?i t?n t?i trong b?ng Addons

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
    "message": "Kh�ng t�m th?y sale order"
}
```

### 500 Internal Server Error
```json
{
    "message": "L?i server khi t?o sale order",
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
7. **Customer Relationship**: Validate customer existence khi t?o/update sale order
8. **Service/Addon Integration**: H? tr? li�n k?t v?i Services v� Addons

## Database Configuration

Entity ???c c?u h�nh trong `ApplicationDbContext`:
- Foreign key relationship v?i Customer, Service, v� Addon
- Indexes cho performance
- Proper column types v� constraints
- Cascade delete cho Customer, SetNull cho Service/Addon

## Changes Made

1. **Renamed Entity**: ?� ??i t�n t? Deal th�nh SaleOrder
2. **Updated Controller**: Apply partial update pattern t? User/Customer controllers
3. **Updated DTOs**: SaleOrderInfo, UpdateSaleOrderResponse, DeleteSaleOrderResponse
4. **Enhanced Validation**: Comprehensive validation cho all fields
5. **Statistics Endpoint**: Endpoint th?ng k� v?i probability ranges
6. **Consistent Error Handling**: Error handling v� logging nh?t qu�n
7. **JWT Authentication**: T?t c? endpoints y�u c?u authentication
8. **Database Migration**: ?� t?o migration ?? ??i t�n b?ng t? Deals th�nh SaleOrders