# H??ng d?n s? d?ng DeleteUser API

## M� t?
API DeleteUser ?� ???c c?i ti?n ?? tr? v? th�ng tin chi ti?t v? user v?a b? x�a thay v� ch? tr? v? status `NoContent()`.

## Endpoint
```
DELETE /api/users/{id}
```

## Request Headers
```
Authorization: Bearer {JWT_TOKEN}
```

## Parameters
- `id` (path parameter): ID c?a user c?n x�a

## Response Examples

### Success Response (200 OK)
```json
{
    "message": "X�a ng??i d�ng th�nh c�ng",
    "deletedUser": {
        "id": 1,
        "name": "Nguy?n V?n A",
        "email": "nguyen.vana@example.com",
        "position": "Developer",
        "role": "User"
    },
    "deletedAt": "2024-01-15T10:30:00Z"
}
```

### Error Responses

#### User Not Found (404 Not Found)
```json
{
    "message": "Kh�ng t�m th?y ng??i d�ng"
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
    "message": "L?i server khi x�a ng??i d�ng",
    "error": "Detailed error message"
}
```

## Test Cases

### 1. Delete th�nh c�ng
- G?i request v?i ID t?n t?i
- User ???c x�a kh?i database
- Tr? v? th�ng tin user v?a x�a

### 2. User kh�ng t?n t?i
- G?i request v?i ID kh�ng t?n t?i
- Tr? v? 404 Not Found

### 3. Unauthorized
- G?i request kh�ng c� JWT token ho?c token kh�ng h?p l?
- Tr? v? 401 Unauthorized

## L?u � quan tr?ng

1. **Authentication Required**: C?n JWT token h?p l? ?? th?c hi?n delete
2. **Permanent Delete**: User s? b? x�a v?nh vi?n kh?i database
3. **Response Data**: API tr? v? th�ng tin user v?a b? x�a ?? frontend c� th? hi?n th? confirmation
4. **Error Handling**: C� error handling ??y ?? v?i logging
5. **Security**: Ch? user c� quy?n m?i c� th? th?c hi?n delete

## So s�nh Before/After

### Before
```
DELETE /api/users/1
Response: 204 No Content (empty body)
```

### After  
```
DELETE /api/users/1
Response: 200 OK
{
    "message": "X�a ng??i d�ng th�nh c�ng",
    "deletedUser": {
        "id": 1,
        "name": "Nguy?n V?n A", 
        "email": "nguyen.vana@example.com",
        "position": "Developer",
        "role": "User"
    },
    "deletedAt": "2024-01-15T10:30:00Z"
}
```

## Frontend Usage Example

```javascript
// Example with JavaScript fetch
async function deleteUser(userId) {
    try {
        const response = await fetch(`/api/users/${userId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        
        if (response.ok) {
            const result = await response.json();
            console.log(result.message); // "X�a ng??i d�ng th�nh c�ng"
            console.log('Deleted user:', result.deletedUser);
            
            // Show success notification with deleted user info
            showNotification(`${result.deletedUser.name} ?� ???c x�a th�nh c�ng`);
        } else {
            const error = await response.json();
            console.error('Delete failed:', error.message);
        }
    } catch (error) {
        console.error('Network error:', error);
    }
}
```

## L?i �ch c?a c?i ti?n

1. **Better UX**: Frontend c� th? hi?n th? th�ng tin user v?a x�a
2. **Confirmation**: Ng??i d�ng bi?t ch�nh x�c user n�o ?� b? x�a  
3. **Audit Trail**: C� timestamp ch�nh x�c v? th?i ?i?m x�a
4. **Consistency**: API style nh?t qu�n v?i Update v� Create
5. **Error Handling**: Better error messages v� logging