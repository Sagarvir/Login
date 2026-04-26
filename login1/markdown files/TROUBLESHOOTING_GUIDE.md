# Frontend-Backend Connection Troubleshooting

## ❌ Common Issues & Solutions

### **1. CORS Error**
```
Access to XMLHttpRequest at 'https://localhost:7199/api/auth/login' 
has been blocked by CORS policy
```

**Solution:** Add CORS to your backend `Program.cs`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add this BEFORE app.UseAuthentication()
app.UseCors("AllowFrontend");
```

Replace ports with your frontend ports:
- React default: `http://localhost:3000`
- Vite: `http://localhost:5173`
- Angular: `http://localhost:4200`
- Vue: `http://localhost:8080`

---

### **2. SSL Certificate Error**
```
unable to verify the first certificate
Error: certificate self signed
```

**Solution for Development:**

**JavaScript/Node.js:**
```javascript
const fetch = require('node-fetch');
const https = require('https');

const agent = new https.Agent({ rejectUnauthorized: false });

fetch('https://localhost:7199/api/auth/login', {
  method: 'POST',
  agent: agent,  // Add this
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ ... })
});
```

**Or use the HTTP endpoint:**
```javascript
const API_URL = 'http://localhost:5289/api/auth';  // HTTP instead
```

**React (in development):**
Add to `package.json`:
```json
{
  "proxy": "https://localhost:7199",
  "ignoreHttpsErrors": true
}
```

---

### **3. 401 Unauthorized**
```
Status: 401 Unauthorized
Message: "Invalid token"
```

**Causes & Solutions:**

| Issue | Solution |
|-------|----------|
| Missing Authorization header | Add `Authorization: Bearer [token]` |
| Token expired | Call refresh endpoint to get new token |
| Invalid token format | Make sure to include "Bearer " prefix |
| Token removed from localStorage | Login again to get new token |

---

### **4. 403 Forbidden**
```
Status: 403 Forbidden
Message: "Access denied"
```

**Causes:**
- Trying to call admin-only endpoint without Admin role
- Token doesn't have the required role claim

**Solution:**
- Login as an Admin user
- Admin assigns role to your user
- Your user logs in again to get updated token

---

### **5. 404 Not Found**
```
Status: 404 Not Found
POST https://localhost:7199/api/auth/login → 404
```

**Causes & Solutions:**

| Issue | Solution |
|-------|----------|
| Wrong port number | Check `launchSettings.json`: should be `7199` |
| Wrong route | Make sure it's `/api/auth/login` not `/auth/login` |
| Backend not running | Start the backend: `F5` or `dotnet run` |
| Wrong URL format | Use `https://localhost:7199` not `https://localhost:7199/` |

---

### **6. Connection Refused**
```
Error: connect ECONNREFUSED 127.0.0.1:7199
```

**Solution:**
1. Make sure backend is running
2. Check correct port: `https://localhost:7199`
3. Run backend:
   ```bash
   cd login1
   dotnet run --launch-profile https
   ```

---

### **7. Invalid Token Response**
```
Status: 200 but empty response
OR
{} returned from login
```

**Solution:**
1. Check database connection
2. Verify employee exists in database
3. Check password is correct (case-sensitive)
4. Verify JWT configuration in `appsettings.json`

---

## ✅ Verification Checklist

Before sending to frontend team, verify:

- [ ] Backend is running on `https://localhost:7199`
- [ ] Can access Swagger: `https://localhost:7199/swagger`
- [ ] Swagger shows all 6 endpoints
- [ ] Can test endpoints in Swagger UI
- [ ] CORS is configured (if frontend is separate app)
- [ ] Database migrations are applied
- [ ] `appsettings.json` is configured
- [ ] JWT keys are valid
- [ ] Can register and login users

---

## 🧪 Quick Test Command

```bash
# Test if backend is running
curl -X GET https://localhost:7199/api/auth/secure

# Should return 401 Unauthorized (expected, no token)
# If backend is running, you'll get response
# If backend not running, you'll get connection error
```

---

## 📞 Information to Share with Frontend Team

Send them this:

```
🚀 Backend API Ready!

Base URL: https://localhost:7199/api/auth

Endpoints:
- POST /register
- POST /login
- POST /refresh
- POST /revoke
- GET /secure (requires auth)
- PUT /assign-role (admin only)

Documentation: See FRONTEND_INTEGRATION_GUIDE.md

Token:
- Access token expires in 60 minutes
- Store in localStorage
- Use in Authorization: Bearer [token] header

Roles available:
- Admin
- Translator
- Creator
- Viewer

Test the API: https://localhost:7199/swagger
```

---

## 🔧 How to Enable HTTPS Locally

If frontend needs HTTP instead of HTTPS:

**Use HTTP endpoint in launchSettings.json:**
```
http://localhost:5289/api/auth
```

**Or disable HTTPS requirement for development:**

In `Program.cs`:
```csharp
options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
// This is already in your code!
```

---

## 📝 Common Frontend Code Errors

### ❌ Wrong:
```javascript
const token = data.token;  // Wrong field name
fetch('https://localhost:7199/api/auth/login')  // Missing /api/auth
fetch(`${API_URL}/login?employeeId=EMP001`)  // Query params instead of body
```

### ✅ Correct:
```javascript
const token = data.accessToken;  // Correct field name
fetch('https://localhost:7199/api/auth/login', {  // Full URL
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ employeeId, password })  // Body, not query
})
```

---

## 🆘 Still Having Issues?

1. Check backend console for error messages
2. Enable verbose logging in `appsettings.json`
3. Test endpoints in Swagger first
4. Verify database connection
5. Check JWT configuration

