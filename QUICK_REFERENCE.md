# Quick Reference - Assign Role API

## 📌 Input Format (JSON Body)
```json
{
  "employeeId": "EMP001",
  "roleName": "Admin"
}
```

## 🔧 Example: Assign Admin Role to Employee

**Request:**
```
PUT /api/auth/assign-role
Authorization: Bearer [JWT_TOKEN]
Content-Type: application/json

{
  "employeeId": "EMP001",
  "roleName": "Admin"
}
```

**Success Response (200):**
```json
{
  "message": "Role assigned successfully",
  "employeeId": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "assignedRole": "Admin"
}
```

---

## ✅ Key Points

1. **Only Admins can call this API** - You must be logged in as Admin
2. **JWT token required** - Add `Authorization: Bearer [token]` header
3. **User must re-login** - After role assignment, employee needs to login again to get updated JWT with new role
4. **Role must exist** - Available roles: Admin, Translator, Creator, Viewer
5. **EmployeeId must exist** - Employee must be registered first

---

## 🔐 How Claims Work

When user logs in:
```
JWT Token includes:
- EmployeeId (ClaimTypes.Name)
- UserId (ClaimTypes.NameIdentifier)
- Role (ClaimTypes.Role)  ← This is checked by [Authorize(Roles = "Admin")]
```

So the flow is:
1. **Admin logs in** → Gets token with Role = "Admin"
2. **Admin calls assign-role** → Updates database
3. **Employee logs in** → Gets NEW token with Role = "Admin" (updated from DB)
4. **Now employee has Admin claims** → Can call admin-only endpoints
