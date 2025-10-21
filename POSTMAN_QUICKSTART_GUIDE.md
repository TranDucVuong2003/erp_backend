# ?? Quick Start Guide - Ticket System API Testing

## Cách Import vào Postman

### B??c 1: Import Collection
1. M? Postman
2. Click **Import** (góc trái trên)
3. Drag & drop file `Ticket_System_API_Collection.postman_collection.json`
4. Click **Import**

### B??c 2: Import Environment  
1. Click **Import** l?n n?a
2. Drag & drop file `Ticket_System_Environment.postman_environment.json`
3. Click **Import**

### B??c 3: Activate Environment
1. Click dropdown ? góc ph?i trên (Environment selector)
2. Ch?n **"Ticket System Environment"**

### B??c 4: C?p Nh?t Base URL (n?u c?n)
1. Click vào Environment name
2. Thay ??i `baseUrl` t? `https://localhost:7228` thành URL c?a b?n
3. Click **Save**

## ?? Test Workflow ???c Recommend

### Phase 1: Setup (CH?Y TR??C TIÊN)
```
1. Setup ? Create Technical Support Category
2. Setup ? Create Bug Report Category  
3. Setup ? Create Feature Request Category
```

### Phase 2: Basic CRUD
```
4. Ticket Categories ? Get All Categories
5. Tickets CRUD ? Create High Priority Ticket
6. Tickets CRUD ? Create Critical Priority Ticket
7. Tickets CRUD ? Get All Tickets
8. Tickets CRUD ? Get Ticket by ID
```

### Phase 3: Management Operations
```
9. Ticket Management ? Assign Ticket to User
10. Ticket Management ? Update Status to In Progress
11. Ticket Logs ? Create Log Entry
12. Ticket Logs ? Create Another Log Entry
13. Ticket Management ? Close Ticket
```

### Phase 4: Advanced Features
```
14. Filtering & Search ? Filter by Priority
15. Filtering & Search ? Multiple Filters
16. Ticket Logs ? Get Logs via Tickets Endpoint
17. Ticket Logs ? Update Log Entry
```

### Phase 5: Error Testing
```
18. Error Cases ? Create Ticket - Invalid Customer
19. Error Cases ? Create Duplicate Category
20. Error Cases ? Assign Invalid User
21. Error Cases ? Get Non-existent Ticket
```

## ?? Pre-requisites

??m b?o database có d? li?u c? b?n:

### Customers Table
```sql
INSERT INTO "Customers" ("CustomerType", "Name", "Email", "CreatedAt") 
VALUES ('individual', 'John Doe', 'john@example.com', NOW());
```

### Users Table  
```sql
INSERT INTO "Users" ("Name", "Email", "Password", "Role", "CreatedAt")
VALUES ('Admin User', 'admin@example.com', 'hashed_password', 'Admin', NOW());
```

## ?? Customization

### Thay ??i Port
N?u API ch?y port khác, update bi?n `baseUrl` trong Environment:
- Development: `https://localhost:7228`
- Staging: `https://api-staging.yoursite.com` 
- Production: `https://api.yoursite.com`

### Thêm Authentication
N?u c?n authentication, thêm vào Collection level:
1. Click vào Collection name
2. Tab **Authorization**
3. Ch?n type (Bearer Token, API Key, etc.)
4. C?u hình credentials

## ?? Auto-Testing Features

Collection ?ã có s?n tests t? ??ng:

### Response Validation
- Status codes (200, 201, 400, 404)
- Response structure validation
- Error message verification

### Variable Management
- Auto-save IDs t? responses
- Reuse IDs trong subsequent requests
- Timestamp generation

### Test Results
Sau khi ch?y, check tab **Test Results** ?? xem:
- ? Passed tests
- ? Failed tests  
- Response times
- Status codes

## ?? Collection Runner

?? ch?y toàn b? collection:

1. Right-click vào Collection name
2. Ch?n **Run collection**
3. Ch?n requests mu?n ch?y
4. Set **Delay** (recommend: 500ms)
5. Click **Run Ticket System API**

## ?? Monitoring & Reports

### Newman CLI
Ch?y collection t? command line:
```bash
npm install -g newman
newman run Ticket_System_API_Collection.postman_collection.json -e Ticket_System_Environment.postman_environment.json --reporters html
```

### CI/CD Integration
S? d?ng trong pipeline:
```yaml
# GitHub Actions example
- name: Run API Tests
  run: |
    newman run Ticket_System_API_Collection.postman_collection.json \
    -e Ticket_System_Environment.postman_environment.json \
    --reporters junit,html
```

## ?? Common Issues & Solutions

### SSL Certificate Issues
```bash
# Disable SSL verification for localhost testing
newman run collection.json --insecure
```

### Port Already in Use
- Check if API server is running
- Verify correct port in baseUrl
- Try different port

### Database Connection
- Ensure database is running
- Check connection string
- Verify required tables exist

### Foreign Key Violations
- Run Setup folder first
- Ensure Customer and User records exist
- Check IDs in environment variables

## ?? Reference

### Status Codes
- `1`: Open/New
- `2`: In Progress  
- `3`: Closed
- `4`: On Hold
- `5`: Cancelled

### Priority Levels
- `"Low"`, `"Medium"`, `"High"`, `"Critical"`

### Urgency Scale
- `1-5` (1 = Very Low, 5 = Critical)

---
**Happy Testing! ??**