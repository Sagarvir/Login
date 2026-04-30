# ✅ DATABASE SETUP - WHAT TO DO NOW

## 🎯 Current Status

Your database is **FULLY CONFIGURED** ✅

All migrations have been applied:
1. ✅ InitialCreate - Users, RefreshTokens, Roles
2. ✅ AddLanguages - Languages table with 5 seed values
3. ✅ AddPreferredLanguageToUser - Converted string to foreign key

---

## 📋 Your Database Has These Tables

```
✅ Users
   - Id, EmployeeId, FirstName, LastName, Password
   - PreferredLanguageId (FK to Languages)
   - RoleId (FK to Roles)

✅ RefreshTokens
   - Id, UserId, TokenHash, ExpiresAtUtc, CreatedAtUtc, etc.

✅ Roles
   - Id, Name

✅ Languages (pre-seeded with 5 languages)
   - 1: English
   - 2: Spanish
   - 3: French
   - 4: German
   - 5: Japanese
```

---

## ⚠️ IMPORTANT - Missing Seed Data

Your **Roles table is EMPTY**. You need to add roles:

### Option 1: Add via SQL Query (Quick)

```sql
INSERT INTO Roles (Name) VALUES ('Admin')
INSERT INTO Roles (Name) VALUES ('Translator')
INSERT INTO Roles (Name) VALUES ('Creator')
INSERT INTO Roles (Name) VALUES ('Viewer')
```

### Option 2: Create a Migration (Proper Way)

1. **Create migration file:**
   ```bash
   cd login1
   dotnet ef migrations add SeedRoles
   ```

2. **Edit the generated migration file** (look for `20260428XXXXXX_SeedRoles.cs`):

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "Roles",
        columns: new[] { "Name" },
        values: new object[,]
        {
            { "Admin" },
            { "Translator" },
            { "Creator" },
            { "Viewer" }
        });
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DeleteData(
        table: "Roles",
        keyColumn: "Id",
        keyValue: 1);
    migrationBuilder.DeleteData(
        table: "Roles",
        keyColumn: "Id",
        keyValue: 2);
    migrationBuilder.DeleteData(
        table: "Roles",
        keyColumn: "Id",
        keyValue: 3);
    migrationBuilder.DeleteData(
        table: "Roles",
        keyColumn: "Id",
        keyValue: 4);
}
```

3. **Apply the migration:**
   ```bash
   dotnet ef database update
   ```

---

## 🧪 Verify Your Database (SQL Query)

```sql
-- Check all tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'

-- Check Languages (should have 5 rows)
SELECT * FROM Languages

-- Check Roles (should be empty until you seed)
SELECT * FROM Roles

-- Check Users (should be empty until registration)
SELECT * FROM Users

-- Check schema of Users table
EXEC sp_columns Users
```

---

## 📊 What Each Column Means

### **Users Table**

| Column | Type | Meaning |
|--------|------|---------|
| Id | INT | Auto-generated unique ID |
| EmployeeId | VARCHAR | Employee's unique ID (used for login) |
| FirstName | VARCHAR | User's first name |
| LastName | VARCHAR | User's last name |
| Password | VARCHAR | Hashed password (never plain text!) |
| PreferredLanguageId | INT | Foreign key to Languages table (1-5) |
| RoleId | INT | Foreign key to Roles table (can be NULL) |

### **Languages Table (Pre-seeded)**

| Id | Name |
|----|------|
| 1 | English |
| 2 | Spanish |
| 3 | French |
| 4 | German |
| 5 | Japanese |

### **Roles Table (Empty - You need to seed)**

| Id | Name |
|----|------|
| - | - |

---

## ✨ Next Steps (Choose One)

### ✅ **QUICK START** (Use SQL Query)

1. Open SQL Server Management Studio
2. Connect to your database
3. Run:
   ```sql
   INSERT INTO Roles (Name) VALUES ('Admin')
   INSERT INTO Roles (Name) VALUES ('Translator')
   INSERT INTO Roles (Name) VALUES ('Creator')
   INSERT INTO Roles (Name) VALUES ('Viewer')
   ```
4. Run `SELECT * FROM Roles` to verify
5. Done! Your backend is ready.

### ✅ **PROPER WAY** (Use Migration)

1. Open PowerShell in your project
2. Run:
   ```bash
   dotnet ef migrations add SeedRoles
   dotnet ef database update
   ```
3. Your Roles table will be seeded automatically
4. Done! Plus you have a migration record.

---

## 🧪 Test Your Backend Now

### 1. Start Backend
```bash
dotnet run
```

### 2. Open Swagger
```
https://localhost:7199/swagger
```

### 3. Register a User
```json
POST /api/auth/register
{
  "employeeId": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "password": "TestPassword123",
  "preferredLanguage": "english"
}
```

Response: `{ "message": "User registered successfully" }`

### 4. Login
```json
POST /api/auth/login
{
  "employeeId": "EMP001",
  "password": "TestPassword123"
}
```

Response:
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "base64...",
  "accessTokenExpiresAtUtc": "2025-04-28T..."
}
```

### 5. Assign Role (as Admin)
```json
PUT /api/auth/assign-role
Authorization: Bearer [admin-token]
{
  "employeeId": "EMP001",
  "roleName": "Admin"
}
```

---

## 📝 Database Connection Info

Your connection string is in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=ITWWLAPPF2CAX6D\\MSSQLSERVER1;Database=JwtAuthDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
}
```

- **Server:** ITWWLAPPF2CAX6D\MSSQLSERVER1
- **Database:** JwtAuthDb
- **Authentication:** Windows Authentication

---

## 🚀 You're Ready When:

- [ ] Roles table is seeded with 4 roles
- [ ] Backend starts without errors
- [ ] Swagger UI loads
- [ ] Can register a user
- [ ] Can login a user
- [ ] Can assign role to user
- [ ] Frontend is connected

---

## 📌 Summary

**What to do RIGHT NOW:**

1. **Seed the Roles table** (SQL or migration)
   ```sql
   INSERT INTO Roles (Name) VALUES ('Admin'), ('Translator'), ('Creator'), ('Viewer')
   ```

2. **Verify it worked**
   ```sql
   SELECT * FROM Roles
   ```

3. **Test your backend** with Swagger

4. **Connect frontend** using: `https://localhost:7199/api/auth`

---

## ❓ FAQ

**Q: Why is Roles table empty?**
A: Roles are reference data that you define. Languages are pre-seeded because they're standard, but roles are specific to your system.

**Q: Do I need test users?**
A: Not required. Users are created via the `/register` endpoint.

**Q: Can I run without seeding Roles?**
A: Yes, but you won't be able to assign roles. Seeding is optional but recommended.

**Q: Is PreferredLanguage required?**
A: No, it defaults to English if not provided during registration.

**Q: What if I need to add a new language?**
A: Add directly to Languages table via SQL, or create a new migration.

---

## ✅ CHECKLIST - DO THIS NOW

- [ ] Seed Roles table with: Admin, Translator, Creator, Viewer
- [ ] Verify roles exist: `SELECT * FROM Roles`
- [ ] Run backend: `dotnet run`
- [ ] Test in Swagger: `https://localhost:7199/swagger`
- [ ] Register a test user
- [ ] Login with test user
- [ ] Assign admin role to test user
- [ ] Frontend ready to integrate: `https://localhost:7199/api/auth`

