# 🚀 Quick Start - Backend URL for Frontend

## ⭐ **What URL should you send to frontend?**

```
https://localhost:7199/api/auth
```

---

## 📍 Complete Endpoint URLs

| Operation | Method | URL | Requires Auth? |
|-----------|--------|-----|----------------|
| Register | POST | `https://localhost:7199/api/auth/register` | ❌ No |
| Login | POST | `https://localhost:7199/api/auth/login` | ❌ No |
| Get Secure Data | GET | `https://localhost:7199/api/auth/secure` | ✅ Yes |
| Refresh Token | POST | `https://localhost:7199/api/auth/refresh` | ❌ No |
| Revoke Token | POST | `https://localhost:7199/api/auth/revoke` | ❌ No |
| Assign Role | PUT | `https://localhost:7199/api/auth/assign-role` | ✅ Yes (Admin) |

---

## 🎯 Copy This for Your Frontend Config

### **For Node.js / React / Vue / Angular:**

```javascript
const API_BASE_URL = 'https://localhost:7199/api/auth';

// Login endpoint
const LOGIN_URL = `${API_BASE_URL}/login`;

// Register endpoint  
const REGISTER_URL = `${API_BASE_URL}/register`;

// Secure data endpoint
const SECURE_URL = `${API_BASE_URL}/secure`;
```

---

## 📝 Login Flow

1. **User submits credentials**
   ```
   POST https://localhost:7199/api/auth/login
   { "employeeId": "EMP001", "password": "password123" }
   ```

2. **Backend returns tokens**
   ```json
   {
     "accessToken": "eyJ...",
     "refreshToken": "abc...",
     "accessTokenExpiresAtUtc": "2025-04-19T06:30:00Z"
   }
   ```

3. **Frontend stores tokens in localStorage**
   ```javascript
   localStorage.setItem('accessToken', data.accessToken);
   localStorage.setItem('refreshToken', data.refreshToken);
   ```

4. **Use accessToken for all protected endpoints**
   ```javascript
   const headers = {
     'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
     'Content-Type': 'application/json'
   };
   ```

---

## ✅ Send This to Your Frontend Team

**Base API URL:** 
```
https://localhost:7199/api/auth
```

**All Endpoints:**
- `POST /register` - Register new user
- `POST /login` - Login and get tokens
- `GET /secure` - Protected endpoint example
- `POST /refresh` - Refresh access token
- `POST /revoke` - Logout/revoke token
- `PUT /assign-role` - Assign role (admin only)

**Token Storage:**
- Store `accessToken` in localStorage
- Store `refreshToken` in localStorage
- Include `accessToken` in `Authorization: Bearer` header

**Token Expiry:**
- Access Token expires in: **60 minutes**
- Refresh Token expires in: **7 days**

See `FRONTEND_INTEGRATION_GUIDE.md` for complete examples!
