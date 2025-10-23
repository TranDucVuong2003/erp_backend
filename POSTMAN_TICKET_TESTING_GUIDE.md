# H??ng D?n Test API Ticket System B?ng Postman

## Chu?n B?

### 1. Thi?t L?p Environment
T?o Environment trong Postman v?i các bi?n:
- `baseUrl`: `https://localhost:7228` (ho?c port c?a b?n)
- `apiUrl`: `{{baseUrl}}/api`

### 2. D? Li?u C?n Có Tr??c
??m b?o database có:
- Ít nh?t 1 Customer (id: 1)
- Ít nh?t 1 User (id: 1)

## Test Sequence - Th?c Hi?n Theo Th? T?

### B??C 1: Test TicketCategories API

#### 1.1 T?o Ticket Categories
**POST** `{{apiUrl}}/TicketCategories`

**Headers:**
```
Content-Type: application/json
```

**Body (raw JSON):**
```json
{
    "name": "Technical Support"
}
```

**Expected Response:** `201 Created`
```json
{
    "id": 1,
    "name": "Technical Support",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": null
}
```

#### 1.2 T?o Thêm Categories
**POST** `{{apiUrl}}/TicketCategories`

**Body 1:**
```json
{
    "name": "Bug Report"
}
```

**Body 2:**
```json
{
    "name": "Feature Request"
}
```

**Body 3:**
```json
{
    "name": "Account Issues"
}
```

#### 1.3 L?y Danh Sách Categories
**GET** `{{apiUrl}}/TicketCategories`

**Expected Response:** `200 OK`
```json
[
    {
        "id": 4,
        "name": "Account Issues",
        "createdAt": "2024-01-15T10:05:00Z",
        "updatedAt": null
    },
    {
        "id": 2,
        "name": "Bug Report",
        "createdAt": "2024-01-15T10:02:00Z",
        "updatedAt": null
    },
    {
        "id": 3,
        "name": "Feature Request",
        "createdAt": "2024-01-15T10:03:00Z",
        "updatedAt": null
    },
    {
        "id": 1,
        "name": "Technical Support",
        "createdAt": "2024-01-15T10:00:00Z",
        "updatedAt": null
    }
]
```

#### 1.4 L?y Category Theo ID
**GET** `{{apiUrl}}/TicketCategories/1`

**Expected Response:** `200 OK`

#### 1.5 C?p Nh?t Category
**PUT** `{{apiUrl}}/TicketCategories/1`

**Body:**
```json
{
    "id": 1,
    "name": "Technical Support - Updated",
    "createdAt": "2024-01-15T10:00:00Z"
}
```

**Expected Response:** `204 No Content`

### B??C 2: Test Tickets API

#### 2.1 T?o Ticket M?i
**POST** `{{apiUrl}}/Tickets`

**Body:**
```json
{
    "title": "Website Loading Issues",
    "description": "The website is loading very slowly, sometimes taking more than 30 seconds to load. This affects user experience significantly. Need urgent investigation.",
    "customerId": 1,
    "priority": "High",
    "status": 1,
    "categoryId": 1,
    "urgencyLevel": 4,
    "userId": 1,
    "createdById": 1,
    "dateline": "2024-12-31T23:59:59Z"
}
```

**Expected Response:** `201 Created`
```json
{
    "id": 1,
    "title": "Website Loading Issues",
    "description": "The website is loading very slowly...",
    "customerId": 1,
    "customer": {
        "id": 1,
        "name": "Customer Name",
        // ... other customer fields
    },
    "priority": "High",
    "status": 1,
    "categoryId": 1,
    "category": {
        "id": 1,
        "name": "Technical Support - Updated"
    },
    "urgencyLevel": 4,
    "userId": 1,
    "assignedTo": {
        "id": 1,
        "name": "User Name"
    },
    "createdById": 1,
    "createdBy": {
        "id": 1,
        "name": "User Name"
    },
    "dateline": "2024-12-31T23:59:59Z",
    "closedAt": null,
    "createdAt": "2024-01-15T10:10:00Z",
    "updatedAt": null
}
```

#### 2.2 T?o Thêm Tickets
**POST** `{{apiUrl}}/Tickets`

**Body 1 (Critical Priority):**
```json
{
    "title": "Login System Down",
    "description": "Users cannot login to the system. Getting 'Authentication failed' error even with correct credentials. This is blocking all user access.",
    "customerId": 1,
    "priority": "Critical",
    "status": 1,
    "categoryId": 2,
    "urgencyLevel": 5,
    "createdById": 1
}
```

**Body 2 (Low Priority):**
```json
{
    "title": "UI Improvement Request",
    "description": "Suggestion to improve the user interface design for better user experience. Not urgent but would be nice to have.",
    "customerId": 1,
    "priority": "Low",
    "status": 1,
    "categoryId": 3,
    "urgencyLevel": 2,
    "createdById": 1
}
```

#### 2.3 L?y Danh Sách Tickets
**GET** `{{apiUrl}}/Tickets`

**Expected Response:** `200 OK` - Array of tickets ordered by CreatedAt descending

#### 2.4 L?y Ticket Theo ID
**GET** `{{apiUrl}}/Tickets/1`

**Expected Response:** `200 OK` - Ticket with all related data

#### 2.5 Filtering Tests

##### L?c Theo Customer
**GET** `{{apiUrl}}/Tickets?customerId=1`

##### L?c Theo Category
**GET** `{{apiUrl}}/Tickets?categoryId=1`

##### L?c Theo Priority
**GET** `{{apiUrl}}/Tickets?priority=Critical`

##### L?c Theo Status
**GET** `{{apiUrl}}/Tickets?status=1`

##### L?c Theo Urgency Level
**GET** `{{apiUrl}}/Tickets?urgencyLevel=5`

##### L?c Theo Assigned User
**GET** `{{apiUrl}}/Tickets?userId=1`

##### Multiple Filters
**GET** `{{apiUrl}}/Tickets?customerId=1&priority=High&status=1`

#### 2.6 C?p Nh?t Ticket
**PUT** `{{apiUrl}}/Tickets/1`

**Body:**
```json
{
    "id": 1,
    "title": "Website Loading Issues - Escalated",
    "description": "The website is loading very slowly, sometimes taking more than 30 seconds to load. This affects user experience significantly. Customer called again today expressing frustration. Need immediate resolution.",
    "customerId": 1,
    "priority": "Critical",
    "status": 2,
    "categoryId": 1,
    "urgencyLevel": 5,
    "userId": 1,
    "createdById": 1,
    "dateline": "2024-12-25T23:59:59Z",
    "createdAt": "2024-01-15T10:10:00Z"
}
```

**Expected Response:** `204 No Content`

#### 2.7 Phân Công Ticket
**PUT** `{{apiUrl}}/Tickets/2/assign`

**Body (raw):**
```json
1
```

**Expected Response:** `204 No Content`

#### 2.8 H?y Phân Công Ticket
**PUT** `{{apiUrl}}/Tickets/2/assign`

**Body (raw):**
```json
null
```

**Expected Response:** `204 No Content`

#### 2.9 C?p Nh?t Status

##### Chuy?n Sang In Progress
**PUT** `{{apiUrl}}/Tickets/1/status`

**Body (raw):**
```json
2
```

##### ?óng Ticket
**PUT** `{{apiUrl}}/Tickets/1/status`

**Body (raw):**
```json
3
```

**Note:** Status 3 s? t? ??ng set `closedAt` timestamp

##### M? L?i Ticket
**PUT** `{{apiUrl}}/Tickets/1/status`

**Body (raw):**
```json
1
```

**Note:** S? clear `closedAt` field

### B??C 3: Test TicketLogs API

#### 3.1 T?o Log Entry
**POST** `{{apiUrl}}/TicketLogs`

**Body:**
```json
{
    "ticketId": 1,
    "content": "Started investigating the issue. Checking server performance metrics and database response times.",
    "userId": 1
}
```

**Expected Response:** `201 Created`

#### 3.2 T?o Thêm Log Entries
**POST** `{{apiUrl}}/TicketLogs`

**Body 1:**
```json
{
    "ticketId": 1,
    "content": "Found the root cause: Database queries are not optimized. Working on fixing the slow queries.",
    "userId": 1
}
```

**Body 2:**
```json
{
    "ticketId": 1,
    "content": "Applied database optimizations. Website loading speed improved significantly. Monitoring for 24 hours before closing.",
    "userId": 1
}
```

**Body 3:**
```json
{
    "ticketId": 2,
    "content": "Investigating authentication system. Checking user credentials validation process.",
    "userId": 1
}
```

#### 3.3 L?y T?t C? Logs
**GET** `{{apiUrl}}/TicketLogs`

#### 3.4 L?y Logs Theo Ticket ID (Query Parameter)
**GET** `{{apiUrl}}/TicketLogs?ticketId=1`

#### 3.5 L?y Logs Theo Ticket ID (Dedicated Endpoint)
**GET** `{{apiUrl}}/TicketLogs/by-ticket/1`

#### 3.6 L?y Logs Qua Tickets Endpoint
**GET** `{{apiUrl}}/Tickets/1/logs`

#### 3.7 L?y Log Theo ID
**GET** `{{apiUrl}}/TicketLogs/1`

#### 3.8 C?p Nh?t Log
**PUT** `{{apiUrl}}/TicketLogs/1`

**Body:**
```json
{
    "id": 1,
    "ticketId": 1,
    "content": "Started investigating the issue. Checking server performance metrics, database response times, and network latency. Initial findings suggest database bottleneck.",
    "userId": 1,
    "createdAt": "2024-01-15T10:20:00Z"
}
```

#### 3.9 Xóa Log
**DELETE** `{{apiUrl}}/TicketLogs/1`

**Expected Response:** `204 No Content`

### B??C 4: Test Error Cases

#### 4.1 Ticket v?i Customer Không T?n T?i
**POST** `{{apiUrl}}/Tickets`

**Body:**
```json
{
    "title": "Test Ticket",
    "description": "Test description",
    "customerId": 999,
    "priority": "Low",
    "status": 1,
    "categoryId": 1,
    "urgencyLevel": 1
}
```

**Expected Response:** `400 Bad Request`
```json
"Customer không t?n t?i."
```

#### 4.2 Ticket v?i Category Không T?n T?i
**POST** `{{apiUrl}}/Tickets`

**Body:**
```json
{
    "title": "Test Ticket",
    "description": "Test description",
    "customerId": 1,
    "priority": "Low",
    "status": 1,
    "categoryId": 999,
    "urgencyLevel": 1
}
```

**Expected Response:** `400 Bad Request`
```json
"TicketCategory không t?n t?i."
```

#### 4.3 Log v?i Ticket Không T?n T?i
**POST** `{{apiUrl}}/TicketLogs`

**Body:**
```json
{
    "ticketId": 999,
    "content": "Test log",
    "userId": 1
}
```

**Expected Response:** `400 Bad Request`
```json
"Ticket không t?n t?i."
```

#### 4.4 Log v?i User Không T?n T?i
**POST** `{{apiUrl}}/TicketLogs`

**Body:**
```json
{
    "ticketId": 1,
    "content": "Test log",
    "userId": 999
}
```

**Expected Response:** `400 Bad Request`
```json
"User không t?n t?i."
```

#### 4.5 T?o Category Trùng Tên
**POST** `{{apiUrl}}/TicketCategories`

**Body:**
```json
{
    "name": "Technical Support"
}
```

**Expected Response:** `400 Bad Request`
```json
"Tên danh m?c ?ã t?n t?i."
```

#### 4.6 Phân Công User Không T?n T?i
**PUT** `{{apiUrl}}/Tickets/1/assign`

**Body:**
```json
999
```

**Expected Response:** `400 Bad Request`
```json
"User không t?n t?i."
```

#### 4.7 L?y Ticket Không T?n T?i
**GET** `{{apiUrl}}/Tickets/999`

**Expected Response:** `404 Not Found`

#### 4.8 Xóa Category ?ang ???c S? D?ng
**DELETE** `{{apiUrl}}/TicketCategories/1`

**Expected Response:** `400 Bad Request`
```json
"Không th? xóa danh m?c ?ang ???c s? d?ng b?i ticket."
```

#### 4.9 Xóa Ticket Có Logs
**DELETE** `{{apiUrl}}/Tickets/1`

**Expected Response:** `400 Bad Request`
```json
"Không th? xóa ticket có logs. Hãy xóa logs tr??c."
```

### B??C 5: Test Cleanup (Delete Operations)

#### 5.1 Xóa Logs Tr??c
**DELETE** `{{apiUrl}}/TicketLogs/2`
**DELETE** `{{apiUrl}}/TicketLogs/3`

#### 5.2 Xóa Tickets
**DELETE** `{{apiUrl}}/Tickets/1`
**DELETE** `{{apiUrl}}/Tickets/2`
**DELETE** `{{apiUrl}}/Tickets/3`

#### 5.3 Xóa Categories
**DELETE** `{{apiUrl}}/TicketCategories/1`
**DELETE** `{{apiUrl}}/TicketCategories/2`
**DELETE** `{{apiUrl}}/TicketCategories/3`
**DELETE** `{{apiUrl}}/TicketCategories/4`

## Status Codes và Ý Ngh?a

### Ticket Status Values:
- `1`: Open/New
- `2`: In Progress  
- `3`: Closed/Resolved
- `4`: On Hold
- `5`: Cancelled

### Priority Values:
- "Low"
- "Medium" 
- "High"
- "Critical"

### Urgency Levels:
- `1`: Very Low
- `2`: Low
- `3`: Medium
- `4`: High
- `5`: Critical

## Tips cho Testing

### 1. Postman Collection Structure
T?o các Folders:
- **Setup** (Create categories, users, customers)
- **Tickets CRUD**
- **Ticket Management** (assign, status updates)
- **Logs Management**
- **Filtering & Search**
- **Error Cases**
- **Cleanup**

### 2. S? d?ng Variables
```javascript
// Trong Tests tab c?a request t?o ticket
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Save ticket ID", function () {
    var responseJson = pm.response.json();
    pm.environment.set("ticketId", responseJson.id);
});
```

### 3. Pre-request Scripts
```javascript
// ?? t? ??ng set timestamp
pm.environment.set("currentTimestamp", new Date().toISOString());
```

### 4. Tests Scripts Examples
```javascript
// Verify response structure
pm.test("Response has required fields", function () {
    var responseJson = pm.response.json();
    pm.expect(responseJson).to.have.property("id");
    pm.expect(responseJson).to.have.property("title");
    pm.expect(responseJson).to.have.property("customer");
    pm.expect(responseJson.customer).to.have.property("name");
});

// Verify filtering works
pm.test("Filtered results contain only specified category", function () {
    var responseJson = pm.response.json();
    responseJson.forEach(function(ticket) {
        pm.expect(ticket.categoryId).to.eql(1);
    });
});
```

## Workflow Testing Scenarios

### Scenario 1: Complete Ticket Lifecycle
1. T?o category
2. T?o ticket (status: Open)
3. Phân công cho user
4. Thêm log "Started investigation"
5. C?p nh?t status thành "In Progress"
6. Thêm log "Found solution"
7. C?p nh?t status thành "Closed"
8. Verify `closedAt` ???c set

### Scenario 2: Escalation Workflow
1. T?o ticket priority "Low"
2. Thêm log "Initial assessment"
3. C?p nh?t priority thành "Critical"
4. C?p nh?t urgency level thành 5
5. Phân công l?i cho senior user
6. Thêm log "Escalated to senior team"

### Scenario 3: Bulk Operations
1. T?o nhi?u tickets v?i filters khác nhau
2. Test filtering theo t?ng criteria
3. Test multiple filters cùng lúc
4. Verify sorting và ordering

H??ng d?n này cung c?p workflow hoàn ch?nh ?? test toàn b? Ticket System API b?ng Postman m?t cách có h? th?ng.