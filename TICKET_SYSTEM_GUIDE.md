# Ticket Management System API Guide

## Overview
The Ticket Management System provides comprehensive support for handling customer support tickets, including categorization, assignment, logging, and status tracking.

## Entities

### 1. TicketCategory
Manages ticket categories for classification.

**Properties:**
- `Id` (int): Primary key
- `Name` (string, required, max 100): Category name
- `CreatedAt` (DateTime): Creation timestamp
- `UpdatedAt` (DateTime?): Last update timestamp

### 2. Ticket
Main ticket entity for customer support requests.

**Properties:**
- `Id` (int): Primary key
- `Title` (string, required, max 500): Ticket title
- `Description` (string, required): Rich text description
- `CustomerId` (int, required): Related customer ID
- `Priority` (string, required, max 20): Priority level
- `Status` (int, required): Ticket status (numeric)
- `CategoryId` (int, required): Related category ID
- `UrgencyLevel` (int, 1-5): Urgency rating (default: 1)
- `UserId` (int?): Assigned user ID (optional)
- `CreatedById` (int?): Creator user ID (optional)
- `Dateline` (DateTime?): Due date (optional)
- `ClosedAt` (DateTime?): Closure timestamp (auto-set)
- `CreatedAt` (DateTime): Creation timestamp
- `UpdatedAt` (DateTime?): Last update timestamp

**Relationships:**
- `Customer`: Many-to-One (required)
- `Category`: Many-to-One (required)
- `AssignedTo`: Many-to-One User (optional)
- `CreatedBy`: Many-to-One User (optional)

### 3. TicketLog
Audit trail and communication log for tickets.

**Properties:**
- `Id` (int): Primary key
- `TicketId` (int, required): Related ticket ID
- `Content` (string, required): Log entry content
- `UserId` (int, required): User who created the log
- `CreatedAt` (DateTime): Creation timestamp
- `UpdatedAt` (DateTime?): Last update timestamp

**Relationships:**
- `Ticket`: Many-to-One (required)
- `User`: Many-to-One (required)

## API Endpoints

### TicketCategories Controller

#### GET /api/TicketCategories
Get all ticket categories, ordered by name.

**Response:** Array of TicketCategory objects

#### GET /api/TicketCategories/{id}
Get a specific ticket category by ID.

**Parameters:**
- `id` (int): Category ID

**Response:** TicketCategory object or 404

#### POST /api/TicketCategories
Create a new ticket category.

**Request Body:**
```json
{
    "name": "Technical Support"
}
```

**Validation:**
- Name must be unique
- Name is required, max 100 characters

**Response:** Created TicketCategory object

#### PUT /api/TicketCategories/{id}
Update an existing ticket category.

**Parameters:**
- `id` (int): Category ID

**Request Body:** Complete TicketCategory object

**Validation:**
- Name must be unique (excluding current category)

**Response:** 204 No Content or error

#### DELETE /api/TicketCategories/{id}
Delete a ticket category.

**Parameters:**
- `id` (int): Category ID

**Validation:**
- Cannot delete if tickets are using this category

**Response:** 204 No Content or error

### Tickets Controller

#### GET /api/Tickets
Get tickets with optional filtering.

**Query Parameters:**
- `customerId` (int?): Filter by customer
- `categoryId` (int?): Filter by category
- `userId` (int?): Filter by assigned user
- `priority` (string?): Filter by priority
- `status` (int?): Filter by status
- `urgencyLevel` (int?): Filter by urgency level

**Response:** Array of Ticket objects with related data

#### GET /api/Tickets/{id}
Get a specific ticket by ID with all related data.

**Parameters:**
- `id` (int): Ticket ID

**Response:** Ticket object with Customer, Category, AssignedTo, CreatedBy

#### GET /api/Tickets/{id}/logs
Get all logs for a specific ticket.

**Parameters:**
- `id` (int): Ticket ID

**Response:** Array of TicketLog objects

#### POST /api/Tickets
Create a new ticket.

**Request Body:**
```json
{
    "title": "Website not loading properly",
    "description": "Detailed description of the issue...",
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

**Validation:**
- Customer must exist
- Category must exist
- Assigned user must exist (if provided)
- Created by user must exist (if provided)
- UrgencyLevel must be 1-5

**Response:** Created Ticket object with related data

#### PUT /api/Tickets/{id}
Update an existing ticket.

**Parameters:**
- `id` (int): Ticket ID

**Request Body:** Complete Ticket object

**Validation:** Same as POST

**Response:** 204 No Content or error

#### PUT /api/Tickets/{id}/assign
Assign or unassign a ticket to/from a user.

**Parameters:**
- `id` (int): Ticket ID

**Request Body:** User ID (int) or null

**Response:** 204 No Content or error

#### PUT /api/Tickets/{id}/status
Update ticket status.

**Parameters:**
- `id` (int): Ticket ID

**Request Body:** Status value (int)

**Special Behavior:**
- Status 3 automatically sets `ClosedAt` timestamp
- Other statuses clear `ClosedAt`

**Response:** 204 No Content or error

#### DELETE /api/Tickets/{id}
Delete a ticket.

**Parameters:**
- `id` (int): Ticket ID

**Validation:**
- Cannot delete if ticket has logs (must delete logs first)

**Response:** 204 No Content or error

### TicketLogs Controller

#### GET /api/TicketLogs
Get all ticket logs with optional filtering.

**Query Parameters:**
- `ticketId` (int?): Filter by ticket

**Response:** Array of TicketLog objects with User and Ticket data

#### GET /api/TicketLogs/{id}
Get a specific ticket log by ID.

**Parameters:**
- `id` (int): Log ID

**Response:** TicketLog object with related data

#### GET /api/TicketLogs/by-ticket/{ticketId}
Get all logs for a specific ticket.

**Parameters:**
- `ticketId` (int): Ticket ID

**Response:** Array of TicketLog objects

#### POST /api/TicketLogs
Create a new ticket log entry.

**Request Body:**
```json
{
    "ticketId": 1,
    "content": "Investigation completed. Issue resolved.",
    "userId": 1
}
```

**Validation:**
- Ticket must exist
- User must exist

**Response:** Created TicketLog object with related data

#### PUT /api/TicketLogs/{id}
Update an existing ticket log.

**Parameters:**
- `id` (int): Log ID

**Request Body:** Complete TicketLog object

**Validation:** Same as POST

**Response:** 204 No Content or error

#### DELETE /api/TicketLogs/{id}
Delete a ticket log entry.

**Parameters:**
- `id` (int): Log ID

**Response:** 204 No Content or error

## Status Values

Common status values (customize as needed):
- `1`: Open/New
- `2`: In Progress
- `3`: Closed/Resolved
- `4`: On Hold
- `5`: Cancelled

## Priority Values

Common priority values:
- "Low"
- "Medium"
- "High" 
- "Critical"

## Urgency Levels

- `1`: Very Low
- `2`: Low
- `3`: Medium
- `4`: High
- `5`: Critical

## Database Configuration

The entities are configured in `ApplicationDbContext` with:
- Proper foreign key relationships
- Cascade and restrict delete behaviors
- Indexes for performance
- Timestamp handling for PostgreSQL

## Error Handling

The API provides comprehensive error handling:
- Validation errors for required fields
- Foreign key validation
- Duplicate name prevention
- Dependency checks before deletion
- Concurrency conflict handling

## Testing

Use the provided `ticket-api-test.http` file to test all endpoints with various scenarios including error cases.

## Integration Notes

- Tickets are linked to existing Customers and Users
- Categories should be created before tickets
- Logs provide audit trail for ticket activities
- Status updates can trigger automatic timestamp updates
- Assignment operations are separate from general updates for flexibility