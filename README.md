# ERP Backend - PostgreSQL Configuration

This project is configured to use PostgreSQL 17 as the database.

## Database Configuration

### Connection String
The application connects to PostgreSQL with the following configuration:
- **Server**: localhost
- **Port**: 5432
- **Database**: erp
- **Username**: postgres
- **Password**: 1234

### Prerequisites
1. PostgreSQL 17 installed and running on localhost:5432
2. Database named `erp` created
3. User `postgres` with password `1234` has access to the database

## Setup Instructions

### 1. Create Database
Connect to PostgreSQL and create the database:
```sql
CREATE DATABASE erp;
```

### 2. Update Database Schema
Run the following command to apply migrations and create tables:
```bash
dotnet ef database update
```

### 3. Run the Application
```bash
dotnet run
```

## Available Endpoints

The application includes a sample Users controller with the following endpoints:

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get a specific user
- `POST /api/users` - Create a new user
- `PUT /api/users/{id}` - Update an existing user  
- `DELETE /api/users/{id}` - Delete a user

## Configuration Files

- `appsettings.json` - Production configuration
- `appsettings.Development.json` - Development configuration with detailed EF logging

## Entity Framework Commands

- Create migration: `dotnet ef migrations add [MigrationName]`
- Update database: `dotnet ef database update`
- Remove last migration: `dotnet ef migrations remove`
- List migrations: `dotnet ef migrations list`

## Sample User Model

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```