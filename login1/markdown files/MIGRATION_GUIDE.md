# Database Migration Steps

The migration files have been created to update your database schema:

**Migration Files Created:**
- `Migrations/20250414000000_UpdateUserSchema.cs` - Contains the UP and DOWN migration logic
- `Migrations/20250414000000_UpdateUserSchema.Designer.cs` - Designer metadata
- `Migrations/AppDbContextModelSnapshot.cs` - Updated snapshot

## What the Migration Does:

### UP (Apply Changes):
1. Drops the old `Username` column from the `Users` table
2. Adds new columns:
   - `EmployeeId` (string, required)
   - `FirstName` (string, required)
   - `LastName` (string, required)
3. Changes `Role_id` from non-nullable `int` to nullable `int?`

### DOWN (Rollback):
Reverses all the above changes if needed

## How to Apply the Migration:

### Option 1: Using Visual Studio Package Manager Console
```
Update-Database
```

### Option 2: Using .NET CLI
```
dotnet ef database update
```

### Option 3: If you have issues with the running application
1. **Stop the application** (Ctrl+F5 in Visual Studio or kill the process)
2. **Then run**: `dotnet ef database update`
3. **Rebuild**: `dotnet build`
4. **Restart** the application

## Verification:

After running the migration, verify that:
1. ✅ The `Users` table no longer has a `Username` column
2. ✅ The `Users` table now has `EmployeeId`, `FirstName`, `LastName` columns
3. ✅ The `Role_id` column is now nullable

## API Endpoints (Updated):

### Register (POST /api/auth/register)
```json
{
  "employeeId": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123"
}
```

### Login (POST /api/auth/login)
```json
{
  "employeeId": "EMP001",
  "password": "SecurePassword123"
}
```
