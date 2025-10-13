# H??ng d?n s? d?ng DeleteUser API

## Mô t?
API DeleteUser ?ã ???c c?i ti?n ?? tr? v? thông tin chi ti?t v? user v?a b? xóa thay vì ch? tr? v? status `NoContent()`.

## Endpoint
```
DELETE /api/users/{id}
```

## Request Headers
```
Authorization: Bearer {JWT_TOKEN}
```

## Parameters
- `id` (path parameter): ID c?a user c?n xóa

## Response Examples

### Success Response (200 OK)
```json
{
    "message": "Xóa ng??i dùng thành công",
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
    "message": "Không tìm th?y ng??i dùng"
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
    "message": "L?i server khi xóa ng??i dùng",
    "error": "Detailed error message"
}
```

## Test Cases

### 1. Delete thành công
- G?i request v?i ID t?n t?i
- User ???c xóa kh?i database
- Tr? v? thông tin user v?a xóa

### 2. User không t?n t?i
- G?i request v?i ID không t?n t?i
- Tr? v? 404 Not Found

### 3. Unauthorized
- G?i request không có JWT token ho?c token không h?p l?
- Tr? v? 401 Unauthorized

## L?u ý quan tr?ng

1. **Authentication Required**: C?n JWT token h?p l? ?? th?c hi?n delete
2. **Permanent Delete**: User s? b? xóa v?nh vi?n kh?i database
3. **Response Data**: API tr? v? thông tin user v?a b? xóa ?? frontend có th? hi?n th? confirmation
4. **Error Handling**: Có error handling ??y ?? v?i logging
5. **Security**: Ch? user có quy?n m?i có th? th?c hi?n delete

## So sánh Before/After

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
    "message": "Xóa ng??i dùng thành công",
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
            console.log(result.message); // "Xóa ng??i dùng thành công"
            console.log('Deleted user:', result.deletedUser);
            
            // Show success notification with deleted user info
            showNotification(`${result.deletedUser.name} ?ã ???c xóa thành công`);
        } else {
            const error = await response.json();
            console.error('Delete failed:', error.message);
        }
    } catch (error) {
        console.error('Network error:', error);
    }
}
```

## L?i ích c?a c?i ti?n

1. **Better UX**: Frontend có th? hi?n th? thông tin user v?a xóa
2. **Confirmation**: Ng??i dùng bi?t chính xác user nào ?ã b? xóa  
3. **Audit Trail**: Có timestamp chính xác v? th?i ?i?m xóa
4. **Consistency**: API style nh?t quán v?i Update và Create
5. **Error Handling**: Better error messages và logging