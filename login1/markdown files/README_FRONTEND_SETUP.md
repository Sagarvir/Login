# 📊 Frontend-Backend Integration Summary

## ✨ What You Need to Know

### **The Backend URL**
```
https://localhost:7199/api/auth
```

This is the **base URL** that connects your frontend to the backend.

---

## 🎯 The 5-Minute Setup

### **Step 1: Start Backend**
```bash
dotnet run --launch-profile https
```
✅ Backend running on: `https://localhost:7199`

### **Step 2: Test Backend is Working**
Visit: `https://localhost:7199/swagger`

You should see 6 endpoints listed

### **Step 3: Copy Base URL to Frontend**
```javascript
const API_BASE_URL = 'https://localhost:7199/api/auth';
```

### **Step 4: Implement Login**
```javascript
async function login(employeeId, password) {
  const response = await fetch(`${API_BASE_URL}/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ employeeId, password })
  });
  
  const data = await response.json();
  localStorage.setItem('accessToken', data.accessToken);
  return data;
}
```

### **Step 5: Use Token for Protected Endpoints**
```javascript
const token = localStorage.getItem('accessToken');

fetch(`${API_BASE_URL}/secure`, {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});
```

---

## 📚 All 6 Endpoints

```
1. Register User
   POST /api/auth/register
   
2. Login User ⭐ START HERE
   POST /api/auth/login
   
3. Access Protected Data
   GET /api/auth/secure
   Requires: Authorization header with token
   
4. Refresh Token
   POST /api/auth/refresh
   When access token expires
   
5. Logout/Revoke Token
   POST /api/auth/revoke
   
6. Assign Role (Admin Only)
   PUT /api/auth/assign-role
   Requires: Admin token
```

---

## 🔄 Login Flow Diagram

```
┌─────────────────┐
│   User Clicks   │
│  Login Button   │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────┐
│ Frontend sends POST /api/auth/login  │
│ Body: {employeeId, password}        │
│ URL: https://localhost:7199/api/auth│
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│ Backend validates credentials       │
│ Returns: {accessToken, refreshToken}│
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│ Frontend stores tokens in localStorage
│ localStorage.setItem('accessToken',)│
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│ Frontend uses token for all requests │
│ Header: Authorization: Bearer [token]
└─────────────────────────────────────┘
```

---

## 📋 Essential Headers

For protected endpoints, always include:

```javascript
{
  'Authorization': `Bearer ${accessToken}`,
  'Content-Type': 'application/json'
}
```

---

## ⏰ Token Timing

| Token | Expires In | What to Do |
|-------|-----------|-----------|
| Access Token | 60 minutes | Use for all requests |
| Refresh Token | 7 days | Get new access token when it expires |

**When Access Token Expires:**
1. Call `POST /api/auth/refresh` with refreshToken
2. Get new accessToken
3. Update localStorage
4. Continue requests

---

## 🗂️ File Structure for Frontend

```
frontend/
├── config/
│   └── api.js              ← API_BASE_URL here
├── services/
│   ├── auth.service.js     ← Login/Register functions
│   └── api.service.js      ← HTTP wrapper with token
├── components/
│   ├── LoginPage.jsx
│   ├── RegisterPage.jsx
│   └── Dashboard.jsx
└── utils/
    └── auth.utils.js       ← Token management
```

---

## 🔐 Security Checklist for Frontend

- [ ] Store tokens in localStorage (not cookies for this API)
- [ ] Never log or display tokens in console
- [ ] Always include Authorization header for protected endpoints
- [ ] Clear tokens on logout
- [ ] Implement token refresh before expiry
- [ ] Validate token exists before making requests
- [ ] Handle 401 Unauthorized by redirecting to login
- [ ] Use HTTPS in production (HTTP OK for local dev)

---

## ✅ Before Sending to Frontend Team

Make sure:

1. **Backend is running**
   ```bash
   dotnet run
   ```

2. **Swagger UI works**
   `https://localhost:7199/swagger`

3. **Database is seeded with test data**
   ```sql
   -- Add a test employee
   INSERT INTO Users VALUES ('EMP001', 'hashed_password', 'John', 'Doe', NULL, 'english')
   
   -- Add a test admin (for testing assign-role)
   INSERT INTO Users VALUES ('ADMIN001', 'hashed_password', 'Admin', 'User', 1, 'english')
   INSERT INTO Roles VALUES (1, 'Admin')
   ```

4. **CORS is enabled** (if frontend is separate)
   See: `TROUBLESHOOTING_GUIDE.md`

5. **appsettings.json is correct**
   Database connection, JWT keys, etc.

---

## 📞 Share This With Frontend Team

```
🎯 Backend API Information

Base URL: https://localhost:7199/api/auth

Register:
POST /register
{ employeeId, firstName, lastName, password, preferredLanguage }

Login:
POST /login
{ employeeId, password }
Returns: { accessToken, refreshToken, accessTokenExpiresAtUtc }

Protected Data:
GET /secure
Headers: { Authorization: Bearer [token] }

Refresh Token:
POST /refresh
{ refreshToken }

Logout:
POST /revoke
{ refreshToken }

Assign Role (Admin):
PUT /assign-role
{ employeeId, roleName }
Headers: { Authorization: Bearer [adminToken] }

Test all endpoints: https://localhost:7199/swagger

Documentation: See FRONTEND_INTEGRATION_GUIDE.md
```

---

## 🚀 Quick References

| Need | See File |
|------|----------|
| Detailed implementation examples | `FRONTEND_INTEGRATION_GUIDE.md` |
| CORS issues, SSL errors, etc | `TROUBLESHOOTING_GUIDE.md` |
| Assign role endpoint details | `ASSIGN_ROLE_GUIDE.md` |
| All endpoints with responses | `BACKEND_URL_FOR_FRONTEND.md` |
| Database migrations | `MIGRATION_GUIDE.md` |

---

## 💡 Pro Tips

1. **Test in Swagger first** before coding frontend
2. **Use Postman/Insomnia** to test API independently
3. **Check browser console** for CORS errors
4. **Use localStorage.setItem()** to store tokens
5. **Always validate token exists** before using it
6. **Implement auto-logout** when token expires
7. **Use try-catch** for all fetch calls
8. **Log API responses** during development

---

## 📝 Summary

| Item | Value |
|------|-------|
| **Backend URL** | `https://localhost:7199/api/auth` |
| **HTTP Method** | REST (GET, POST, PUT, DELETE) |
| **Content Type** | `application/json` |
| **Authentication** | JWT Bearer Token |
| **Token Expiry** | 60 minutes |
| **Refresh Expiry** | 7 days |
| **Environment** | Local Development |

