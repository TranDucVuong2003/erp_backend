# H??ng d?n s? d?ng DeleteCustomer API

## M� t?
API DeleteCustomer ?� ???c c?i ti?n ?? tr? v? th�ng tin chi ti?t v? customer v?a b? x�a thay v� ch? tr? v? status `NoContent()`.

## Endpoint
```
DELETE /api/customers/{id}
```

## Request Headers
```
Authorization: Bearer {JWT_TOKEN}
```

## Parameters
- `id` (path parameter): ID c?a customer c?n x�a

## Response Examples

### Success Response (200 OK)
```json
{
    "message": "X�a kh�ch h�ng th�nh c�ng",
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
    "message": "X�a kh�ch h�ng th�nh c�ng",
    "deletedCustomer": {
        "id": 2,
        "name": null,
        "email": null,
        "phoneNumber": null,
        "companyName": "C�ng ty ABC",
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
    "message": "Kh�ng t�m th?y kh�ch h�ng"
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
    "message": "L?i server khi x�a kh�ch h�ng",
    "error": "Detailed error message"
}
```

## Test Cases

### 1. Delete th�nh c�ng - Individual Customer
- G?i request v?i ID c?a individual customer t?n t?i
- Customer ???c x�a kh?i database
- Tr? v? th�ng tin customer v?a x�a v?i `customerType: "individual"`

### 2. Delete th�nh c�ng - Company Customer
- G?i request v?i ID c?a company customer t?n t?i
- Customer ???c x�a kh?i database
- Tr? v? th�ng tin customer v?a x�a v?i `customerType: "company"`

### 3. Customer kh�ng t?n t?i
- G?i request v?i ID kh�ng t?n t?i
- Tr? v? 404 Not Found

### 4. Unauthorized
- G?i request kh�ng c� JWT token ho?c token kh�ng h?p l?
- Tr? v? 401 Unauthorized

## CustomerInfo Structure

### Th�ng tin tr? v? trong `deletedCustomer`:
- **id**: ID c?a customer
- **name**: T�n (cho individual customer)
- **email**: Email (cho individual customer)
- **phoneNumber**: S? ?i?n tho?i (cho individual customer)
- **companyName**: T�n c�ng ty (cho company customer)
- **representativeName**: T�n ng??i ??i di?n (cho company customer)
- **representativeEmail**: Email ng??i ??i di?n (cho company customer)
- **customerType**: Lo?i kh�ch h�ng ("individual" ho?c "company")
- **isActive**: Tr?ng th�i ho?t ??ng
- **status**: Tr?ng th�i kh�ch h�ng

## So s�nh Before/After

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
    "message": "X�a kh�ch h�ng th�nh c�ng",
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
            console.log(result.message); // "X�a kh�ch h�ng th�nh c�ng"
            console.log('Deleted customer:', result.deletedCustomer);
            
            // Show success notification with deleted customer info
            const customerName = result.deletedCustomer.customerType === 'individual' 
                ? result.deletedCustomer.name 
                : result.deletedCustomer.companyName;
            
            showNotification(`${customerName} ?� ???c x�a th�nh c�ng`);
        } else {
            const error = await response.json();
            console.error('Delete failed:', error.message);
        }
    } catch (error) {
        console.error('Network error:', error);
    }
}
```

## L?u � quan tr?ng

1. **Authentication Required**: C?n JWT token h?p l? ?? th?c hi?n delete
2. **Permanent Delete**: Customer s? b? x�a v?nh vi?n kh?i database
3. **Response Data**: API tr? v? th�ng tin customer v?a b? x�a ?? frontend c� th? hi?n th? confirmation
4. **Error Handling**: C� error handling ??y ?? v?i logging
5. **Security**: Ch? user c� quy?n m?i c� th? th?c hi?n delete
6. **Customer Types**: Response kh�c nhau t�y thu?c v�o lo?i customer (individual/company)

## L?i �ch c?a c?i ti?n

1. **Better UX**: Frontend c� th? hi?n th? th�ng tin customer v?a x�a
2. **Confirmation**: Ng??i d�ng bi?t ch�nh x�c customer n�o ?� b? x�a  
3. **Audit Trail**: C� timestamp ch�nh x�c v? th?i ?i?m x�a
4. **Consistency**: API style nh?t qu�n v?i Delete User API
5. **Error Handling**: Better error messages v� logging
6. **Flexibility**: H? tr? c? individual v� company customers v?i th�ng tin ph� h?p