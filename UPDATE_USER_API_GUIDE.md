# Hướng dẫn sử dụng UpdateUser API (Partial Update)

## Các cải tiến đã thực hiện

### 1. Partial Update Support
- **Chỉ cập nhật trường được gửi**: API chỉ cập nhật những trường có trong request body
- **Giữ nguyên trường không gửi**: Các trường không có trong request sẽ giữ nguyên giá trị cũ
- **Không bắt buộc trường nào**: Không còn ràng buộc phải gửi tất cả các trường như trước

### 2. Hash Password tự động
- API sẽ tự động hash password bằng BCrypt trước khi lưu vào database
- Nếu không gửi trường `password`, sẽ giữ nguyên password cũ
- Chỉ hash password mới (không phải BCrypt hash)

### 3. Trả về message thành công
- Thay vì trả về `NoContent()`, API giờ trả về thông tin chi tiết
- Bao gồm message thành công, thông tin user và thời gian cập nhật

### 4. Validation cải thiện
- Kiểm tra email trùng lặp với user khác
- Validation đầy đủ cho các trường dữ liệu với length limits
- Error handling tốt hơn với message tiếng Việt

## API Endpoint

```
PUT /api/users/{id}
```

## Request Headers

```
Content-Type: application/json
Authorization: Bearer {JWT_TOKEN}
```

## Request Body Examples

### 1. Cập nhật tất cả trường
```json
{
    "name": "Nguyễn Văn A Updated",
    "email": "nguyen.vana.updated@example.com",
    "password": "newpassword123",
    "position": "Senior Developer",
    "phoneNumber": "0901234567",
    "address": "123 Updated Street, District 1, Ho Chi Minh City",
    "role": "Admin",
    "secondaryEmail": "nguyen.vana.backup@example.com"
}
```

### 2. Partial Update - Chỉ cập nhật tên và chức vụ
```json
{
    "name": "Nguyễn Văn A Updated",
    "position": "Senior Developer"
}
```

### 3. Partial Update - Chỉ cập nhật email
```json
{
    "email": "nguyen.vana.new@example.com"
}
```

### 4. Partial Update - Chỉ cập nhật password
```json
{
    "password": "mynewpassword123"
}
```

### 5. Partial Update - Set empty values (xóa dữ liệu)
```json
{
    "phoneNumber": "",
    "address": "",
    "secondaryEmail": ""
}
```

## Response Examples

### Success Response (200 OK)

```json
{
    "message": "Cập nhật thông tin người dùng thành công",
    "user": {
        "id": 1,
        "name": "Nguyễn Văn A Updated",
        "email": "nguyen.vana.updated@example.com",
        "position": "Senior Developer",
        "role": "Admin"
    },
    "updatedAt": "2024-01-15T10:30:00Z"
}
```

### Error Responses

#### ID Mismatch (400 Bad Request)
```json
{
    "message": "ID không khớp với dữ liệu người dùng"
}
```

#### User Not Found (404 Not Found)
```json
{
    "message": "Không tìm thấy người dùng"
}
```

#### Email Already Exists (400 Bad Request)
```json
{
    "message": "Email đã được sử dụng bởi người dùng khác"
}
```

#### Validation Error (400 Bad Request)
```json
{
    "errors": {
        "Name": ["The Name field is required."],
        "Email": ["The Email field is not a valid e-mail address."]
    }
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
    "message": "Lỗi server khi cập nhật người dùng",
    "error": "Detailed error message"
}
```

## Test Cases

### 1. Partial Update thành công
- Gửi chỉ một số trường cần cập nhật
- Các trường khác giữ nguyên giá trị cũ
- Trả về thông tin thành công

### 2. Update password mới
- Gửi request với trường password
- Password sẽ được hash tự động
- Trả về thông tin thành công

### 3. Không gửi password
- Không gửi trường password trong request
- Password cũ sẽ được giữ nguyên

### 4. Empty request body
- Gửi request với body rỗng `{}`
- Chỉ cập nhật `updatedAt`, các trường khác giữ nguyên

### 5. Lỗi ID không khớp
- ID trong URL khác với ID trong body (nếu gửi)
- Trả về 400 Bad Request

### 6. Lỗi user không tồn tại
- Gửi request với ID không tồn tại
- Trả về 404 Not Found

### 7. Lỗi email trùng lặp
- Gửi email đã được sử dụng bởi user khác
- Trả về 400 Bad Request

### 8. Validation errors
- Gửi dữ liệu không hợp lệ (email sai format, string quá dài)
- Trả về 400 Bad Request với message cụ thể

### 9. Ignore invalid fields
- Gửi các trường không được hỗ trợ
- API sẽ bỏ qua và chỉ cập nhật các trường hợp lệ

## Security Features

1. **JWT Authentication**: Yêu cầu token hợp lệ
2. **Password Hashing**: Tự động hash password bằng BCrypt
3. **Email Validation**: Kiểm tra định dạng email hợp lệ
4. **Data Validation**: Validation đầy đủ theo model
5. **SQL Injection Prevention**: Sử dụng Entity Framework Core

## Lưu ý quan trọng

1. **Partial Update Logic**: 
   - Chỉ gửi các trường muốn cập nhật
   - Các trường không gửi sẽ giữ nguyên giá trị cũ
   - Không còn ràng buộc phải gửi tất cả các trường

2. **Password Field**: 
   - Nếu muốn thay đổi password, gửi trường `password` với giá trị mới
   - Nếu không muốn thay đổi password, không gửi trường `password`
   - Password sẽ được hash tự động, không cần hash trước khi gửi

3. **Required vs Optional**: 
   - Không còn required fields khi update (tất cả đều optional)
   - Chỉ validation format và length limits
   - Có thể set empty string (`""`) cho các trường optional để xóa dữ liệu

4. **String Length Limits**:
   - Name: tối đa 100 ký tự
   - Email: tối đa 150 ký tự
   - Password: tối đa 255 ký tự
   - Position: tối đa 100 ký tự
   - PhoneNumber: tối đa 20 ký tự
   - Address: tối đa 500 ký tự
   - Role: tối đa 50 ký tự
   - SecondaryEmail: tối đa 150 ký tự

5. **Email Validation**:
   - Kiểm tra định dạng email hợp lệ
   - Kiểm tra email không trùng với user khác
   - Áp dụng cho cả `email` và `secondaryEmail`

6. **Ignored Fields**:
   - `id`, `createdAt`, `updatedAt` sẽ bị bỏ qua nếu gửi trong request
   - Các trường không được hỗ trợ khác cũng sẽ bị bỏ qua
   - `UpdatedAt` được tự động set khi update
   - `CreatedAt` giữ nguyên giá trị cũ

7. **Request Format**:
   - Sử dụng `Dictionary<string, object?>` thay vì `User` object
   - Cho phép flexible field selection
   - Case-insensitive field names (name, Name, NAME đều được)

## DELETE USER API

### Endpoint
```
DELETE /api/users/{id}
```

### Request Headers
```
Authorization: Bearer {JWT_TOKEN}
```

### Success Response (200 OK)
```json
{
    "message": "Xóa người dùng thành công",
    "deletedUser": {
        "id": 1,
        "name": "Nguyễn Văn A",
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
    "message": "Không tìm thấy người dùng"
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
    "message": "Lỗi server khi xóa người dùng",
    "error": "Detailed error message"
}
```