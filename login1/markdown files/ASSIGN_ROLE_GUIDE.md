# Assign Role API - Complete Guide

## 🔐 Overview
The **Assign Role** endpoint allows **ONLY Admin users** to assign roles to employees. This is protected with JWT authentication and role-based authorization.

---

## 📋 How It Works

### 1. **Authorization Flow**
```
User Login → Get JWT Token → If Role = "Admin" → Can call Assign Role API
                           → If Role = NULL or other → Access Denied (403)
```

### 2. **JWT Claims with Roles**
When a user logs in, the JWT token includes:
```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
new Claim(ClaimTypes.Name, user.EmployeeId),
new Claim(ClaimTypes.Role, user.Role?.Name ?? "")
```

When role is assigned, **user must log in again** to get a new token with the role claim.

---

## 🛠️ API Details

### Endpoint
```
PUT /api/auth/assign-role
```

### Authentication Required
✅ **YES** - Must provide a valid JWT token in the header

### Authorization Required
✅ **YES** - User must have "Admin" role

### Request Body (JSON)
```json
{
  "employeeId": "EMP001",
  "roleName": "Admin"
}
```

### Request Parameters
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `employeeId` | string | ✅ Yes | The employee ID of the user to assign role to |
| `roleName` | string | ✅ Yes | The name of the role to assign (e.g., "Admin", "Translator", "Creator", "Viewer") |

---

## 📤 Response Examples

### Success Response (200 OK)
```json
{
  "message": "Role assigned successfully",
  "employeeId": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "assignedRole": "Admin"
}
```

### Error Responses

**400 Bad Request** - Missing required fields
```json
{
  "message": "EmployeeId and RoleName are required"
}
```

**404 Not Found** - Employee not found
```json
{
  "message": "User with EmployeeId 'EMP999' not found"
}
```

**404 Not Found** - Role not found
```json
{
  "message": "Role 'InvalidRole' not found"
}
```

**403 Forbidden** - User is not an Admin
```json
{
  "message": "Unauthorized"
}
```

---

## 🔄 Complete Flow Example

### Step 1: Admin Login
**POST /api/auth/login**
```json
{
  "employeeId": "ADMIN001",
  "password": "AdminPassword123"
}
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random-base64-string",
  "accessTokenExpiresAtUtc": "2025-04-15T12:30:00Z"
}
```

### Step 2: Use Token to Assign Role
**PUT /api/auth/assign-role**

Headers:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

Body:
```json
{
  "employeeId": "EMP001",
  "roleName": "Admin"
}
```

Response:
```json
{
  "message": "Role assigned successfully",
  "employeeId": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "assignedRole": "Admin"
}
```

### Step 3: Employee Must Login Again
The employee at EMP001 must now **login again** to get a new JWT token with the "Admin" role claim.

---

## ✅ Available Roles
Based on `Role.cs` comments, the available roles are:
- `Admin` - Full administrative access
- `Translator` - Can translate content
- `Creator` - Can create content
- `Viewer` - Can only view content

---

## 🔍 Important Notes

### 1. **Claims are Updated on Next Login**
- Role assignment updates the database immediately
- But the JWT token is **not** updated until the user logs in again
- New login = new token with updated role claim

### 2. **Only Admins Can Assign Roles**
- The endpoint checks: `[Authorize(Roles = "Admin")]`
- If user doesn't have "Admin" role, returns 403 Forbidden

### 3. **Database Relationship**
- Users table has `RoleId` (foreign key) pointing to Roles table
- When you assign a role, you're setting `user.RoleId = role.Id`

### 4. **Input Validation**
- Both `employeeId` and `roleName` are required
- If either is null/empty, returns 400 Bad Request

---

## 🧪 Testing with Swagger/Postman

### Postman Setup

1. **Get Admin Token**
   - Method: POST
   - URL: `https://localhost:7000/api/auth/login` (adjust port)
   - Body:
   ```json
   {
     "employeeId": "ADMIN001",
     "password": "AdminPassword123"
   }
   ```
   - Copy the `accessToken` from response

2. **Call Assign Role**
   - Method: PUT
   - URL: `https://localhost:7000/api/auth/assign-role`
   - Headers:
     - `Authorization: Bearer [paste-token-here]`
     - `Content-Type: application/json`
   - Body:
   ```json
   {
     "employeeId": "EMP001",
     "roleName": "Admin"
   }
   ```

---

## 🚨 Troubleshooting

| Issue | Solution |
|-------|----------|
| **403 Forbidden** | Make sure you're logged in as an Admin user |
| **404 User not found** | Check the employeeId spelling - it's case-sensitive |
| **404 Role not found** | Role name must exist in Roles table (Admin, Translator, Creator, Viewer) |
| **400 Bad Request** | Both employeeId and roleName must be provided in request body |
| **Missing Authorization Header** | Add: `Authorization: Bearer [your-jwt-token]` |

---

## 📝 Summary

✅ **Correct Input Format:**
```json
{
  "employeeId": "EMP001",
  "roleName": "Admin"
}
```

✅ **Headers Required:**
```
Authorization: Bearer [JWT_TOKEN]
Content-Type: application/json
```

✅ **User Must Have:** Admin role in their JWT claims
