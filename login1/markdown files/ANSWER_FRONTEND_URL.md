# 🎯 FINAL ANSWER - Frontend Connection

## ⭐ THE ANSWER TO YOUR QUESTION

**"What URL or link should I send to the front end code so that it gets connected to my backend login?"**

---

## 📌 SEND THIS TO YOUR FRONTEND TEAM

```
Base API URL: https://localhost:7199/api/auth
```

---

## 🔗 Complete Endpoint URLs

```
Login Endpoint:
https://localhost:7199/api/auth/login

Register Endpoint:
https://localhost:7199/api/auth/register

Secure Data Endpoint:
https://localhost:7199/api/auth/secure

Refresh Token Endpoint:
https://localhost:7199/api/auth/refresh

Revoke Token Endpoint:
https://localhost:7199/api/auth/revoke

Assign Role Endpoint:
https://localhost:7199/api/auth/assign-role
```

---

## 📋 COPY THIS CODE TO YOUR FRONTEND

```javascript
// Configuration
const API_BASE_URL = 'https://localhost:7199/api/auth';

// Login function
async function login(employeeId, password) {
  const response = await fetch(`${API_BASE_URL}/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ employeeId, password })
  });
  
  const data = await response.json();
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
  
  return data;
}

// Use protected endpoint
async function getSecureData() {
  const token = localStorage.getItem('accessToken');
  
  const response = await fetch(`${API_BASE_URL}/secure`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  
  return response.json();
}
```

---

## ✅ Step-by-Step Setup

### Step 1: Configure Base URL
```javascript
const API_BASE_URL = 'https://localhost:7199/api/auth';
```

### Step 2: Handle Login
```javascript
POST /api/auth/login
Body: { "employeeId": "EMP001", "password": "password123" }
Response: { "accessToken": "...", "refreshToken": "..." }
```

### Step 3: Store Tokens
```javascript
localStorage.setItem('accessToken', data.accessToken);
localStorage.setItem('refreshToken', data.refreshToken);
```

### Step 4: Use Token for Protected Requests
```javascript
Headers: {
  'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
  'Content-Type': 'application/json'
}
```

---

## 🚀 Quick Verification

1. **Start Backend**
   ```bash
   dotnet run
   ```

2. **Test Swagger**
   Visit: `https://localhost:7199/swagger`

3. **Test Login Endpoint**
   ```bash
   curl -X POST https://localhost:7199/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"employeeId":"EMP001","password":"password123"}'
   ```

4. **If you get a response** → Backend is working ✅

---

## 📚 Documentation Files Created

| File | Purpose |
|------|---------|
| `README_FRONTEND_SETUP.md` | Complete setup guide |
| `FRONTEND_INTEGRATION_GUIDE.md` | Detailed implementation |
| `BACKEND_URL_FOR_FRONTEND.md` | Quick reference |
| `COPY_PASTE_FRONTEND_CODE.md` | Ready-to-use code |
| `TROUBLESHOOTING_GUIDE.md` | Common issues & fixes |
| `ASSIGN_ROLE_GUIDE.md` | Admin role assignment |

---

## 🎁 What You Give to Frontend Team

```
🚀 Backend API Ready!

Base URL: https://localhost:7199/api/auth

Key Endpoints:
1. POST /register - Create account
2. POST /login - Login (returns token)
3. GET /secure - Protected data
4. POST /refresh - Refresh token
5. POST /revoke - Logout
6. PUT /assign-role - Assign roles (admin only)

How to Use:
1. Call login endpoint with employeeId + password
2. Store accessToken from response
3. Include token in Authorization header: Bearer [token]
4. Use for all protected requests

Token Info:
- Access Token expires: 60 minutes
- Refresh Token expires: 7 days
- Use refresh endpoint to get new access token

Test Here: https://localhost:7199/swagger
```

---

## 💻 Minimal Frontend Example

```html
<!DOCTYPE html>
<html>
<body>
  <input id="employeeId" placeholder="Employee ID" />
  <input id="password" type="password" placeholder="Password" />
  <button onclick="handleLogin()">Login</button>
  <div id="result"></div>

  <script>
    const API_URL = 'https://localhost:7199/api/auth';

    async function handleLogin() {
      const employeeId = document.getElementById('employeeId').value;
      const password = document.getElementById('password').value;

      const response = await fetch(`${API_URL}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ employeeId, password })
      });

      const data = await response.json();
      localStorage.setItem('accessToken', data.accessToken);
      
      document.getElementById('result').innerText = 
        'Logged in! Token: ' + data.accessToken.substring(0, 20) + '...';
    }
  </script>
</body>
</html>
```

---

## ⚙️ Configuration

**For Development (Local):**
```javascript
API_URL = 'https://localhost:7199/api/auth'
```

**For Production:**
```javascript
API_URL = 'https://your-domain.com/api/auth'
```

---

## 🔐 Important Security Notes

1. **Always use HTTPS** in production
2. **Store tokens securely** (localStorage for this setup)
3. **Include token in all protected requests**
4. **Refresh token before expiry** (60 minutes)
5. **Clear tokens on logout**
6. **Never expose JWT in public**

---

## 🆘 If Something Doesn't Work

1. **Check backend is running:**
   ```bash
   dotnet run
   ```

2. **Test in Swagger:**
   `https://localhost:7199/swagger`

3. **Check CORS:**
   See `TROUBLESHOOTING_GUIDE.md`

4. **Check database:**
   Make sure migrations are applied

5. **Enable logging:**
   Check `appsettings.json`

---

## ✨ Summary

**To connect your frontend to backend, use:**

```
https://localhost:7199/api/auth
```

**Login:**
```
POST /login
Body: { employeeId, password }
Returns: { accessToken, refreshToken, accessTokenExpiresAtUtc }
```

**Protected Requests:**
```
Header: Authorization: Bearer [accessToken]
```

**That's it! 🎉**

For detailed examples, see `COPY_PASTE_FRONTEND_CODE.md`

