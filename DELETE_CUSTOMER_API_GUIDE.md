# H??ng d?n s? d?ng DeleteCustomer API

## Mô t?
API DeleteCustomer ?ã ???c c?i ti?n ?? tr? v? thông tin chi ti?t v? customer v?a b? xóa thay vì ch? tr? v? status `NoContent()`.

## Endpoint
```
DELETE /api/customers/{id}
```

## Request Headers
```
Authorization: Bearer {JWT_TOKEN}
```

## Parameters
- `id` (path parameter): ID c?a customer c?n xóa

## Response Examples

### Success Response (200 OK)
```json
{
    "message": "Xóa khách hàng thành công",
    "deletedCustomer": {
        "id": 1,
        "name": "Nguy?n V?n A",
        "email": "nguyenvana@example.com",
        "phoneNumber": "0901234567",
        "companyName": null,
        "representativeName": null,
        "representativeEmail": null,
        "customerType": "individual",
        "isActive": true,
        "status": "active"
    },
    "deletedAt": "2024-01-15T10:30:00Z"
}
```

### Success Response - Company Customer (200 OK)
```json
{
    "message": "Xóa khách hàng thành công",
    "deletedCustomer": {
        "id": 2,
        "name": null,
        "email": null,
        "phoneNumber": null,
        "companyName": "Công ty ABC",
        "representativeName": "Tr?n V?n B",
        "representativeEmail": "tranvanb@abc.com",
        "customerType": "company",
        "isActive": true,
        "status": "active"
    },
    "deletedAt": "2024-01-15T10:30:00Z"
}
```

### Error Responses

#### Customer Not Found (404 Not Found)
```json
{
    "message": "Không tìm th?y khách hàng"
}
```

#### Unauthorized (401 Unauthorized)
```json
{
    "message": "Unauthorized"
}
```

#### Server Error (500 Internal Server Error)
```json
{
    "message": "L?i server khi xóa khách hàng",
    "error": "Detailed error message"
}
```

## Test Cases

### 1. Delete thành công - Individual Customer
- G?i request v?i ID c?a individual customer t?n t?i
- Customer ???c xóa kh?i database
- Tr? v? thông tin customer v?a xóa v?i `customerType: "individual"`

### 2. Delete thành công - Company Customer
- G?i request v?i ID c?a company customer t?n t?i
- Customer ???c xóa kh?i database
- Tr? v? thông tin customer v?a xóa v?i `customerType: "company"`

### 3. Customer không t?n t?i
- G?i request v?i ID không t?n t?i
- Tr? v? 404 Not Found

### 4. Unauthorized
- G?i request không có JWT token ho?c token không h?p l?
- Tr? v? 401 Unauthorized

## CustomerInfo Structure

### Thông tin tr? v? trong `deletedCustomer`:
- **id**: ID c?a customer
- **name**: Tên (cho individual customer)
- **email**: Email (cho individual customer)
- **phoneNumber**: S? ?i?n tho?i (cho individual customer)
- **companyName**: Tên công ty (cho company customer)
- **representativeName**: Tên ng??i ??i di?n (cho company customer)
- **representativeEmail**: Email ng??i ??i di?n (cho company customer)
- **customerType**: Lo?i khách hàng ("individual" ho?c "company")
- **isActive**: Tr?ng thái ho?t ??ng
- **status**: Tr?ng thái khách hàng

## So sánh Before/After

### Before
```
DELETE /api/customers/1
Response: 204 No Content (empty body)
```

### After  
```
DELETE /api/customers/1
Response: 200 OK
{
    "message": "Xóa khách hàng thành công",
    "deletedCustomer": {
        "id": 1,
        "name": "Nguy?n V?n A",
        "email": "nguyenvana@example.com",
        "phoneNumber": "0901234567",
        "customerType": "individual",
        "isActive": true,
        "status": "active"
    },
    "deletedAt": "2024-01-15T10:30:00Z"
}
```

## Frontend Usage Example

```javascript
// Example with JavaScript fetch
async function deleteCustomer(customerId) {
    try {
        const response = await fetch(`/api/customers/${customerId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        
        if (response.ok) {
            const result = await response.json();
            console.log(result.message); // "Xóa khách hàng thành công"
            console.log('Deleted customer:', result.deletedCustomer);
            
            // Show success notification with deleted customer info
            const customerName = result.deletedCustomer.customerType === 'individual' 
                ? result.deletedCustomer.name 
                : result.deletedCustomer.companyName;
            
            showNotification(`${customerName} ?ã ???c xóa thành công`);
        } else {
            const error = await response.json();
            console.error('Delete failed:', error.message);
        }
    } catch (error) {
        console.error('Network error:', error);
    }
}
```

## L?u ý quan tr?ng

1. **Authentication Required**: C?n JWT token h?p l? ?? th?c hi?n delete
2. **Permanent Delete**: Customer s? b? xóa v?nh vi?n kh?i database
3. **Response Data**: API tr? v? thông tin customer v?a b? xóa ?? frontend có th? hi?n th? confirmation
4. **Error Handling**: Có error handling ??y ?? v?i logging
5. **Security**: Ch? user có quy?n m?i có th? th?c hi?n delete
6. **Customer Types**: Response khác nhau tùy thu?c vào lo?i customer (individual/company)

## L?i ích c?a c?i ti?n

1. **Better UX**: Frontend có th? hi?n th? thông tin customer v?a xóa
2. **Confirmation**: Ng??i dùng bi?t chính xác customer nào ?ã b? xóa  
3. **Audit Trail**: Có timestamp chính xác v? th?i ?i?m xóa
4. **Consistency**: API style nh?t quán v?i Delete User API
5. **Error Handling**: Better error messages và logging
6. **Flexibility**: H? tr? c? individual và company customers v?i thông tin phù h?p