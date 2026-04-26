# Frontend Integration Guide

## 🔗 Backend URL Configuration

### Local Development
```
Base URL: https://localhost:7199
OR
Base URL: http://localhost:5289
```

**Note:** Your backend is running on port **7199** (HTTPS) or **5289** (HTTP)

### Production
(To be configured when deployed)

---

## 📍 All Available Endpoints

### 1. **Register User**
```
POST /api/auth/register
```

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "employeeId": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123",
  "preferredLanguage": "english"
}
```

**Response (200):**
```json
{
  "message": "User registered successfully"
}
```

---

### 2. **Login User** ⭐ START HERE
```
POST /api/auth/login
```

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "employeeId": "EMP001",
  "password": "SecurePassword123"
}
```

**Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random-base64-string",
  "accessTokenExpiresAtUtc": "2025-04-19T06:30:00Z"
}
```

**⚠️ Store the `accessToken` in localStorage/sessionStorage for subsequent requests**

---

### 3. **Access Protected Data**
```
GET /api/auth/secure
```

**Headers:**
```
Authorization: Bearer [accessToken]
Content-Type: application/json
```

**Response (200):**
```json
{
  "message": "This is protected data"
}
```

---

### 4. **Refresh Token**
```
POST /api/auth/refresh
```

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "refreshToken": "random-base64-string"
}
```

**Response (200):**
```json
{
  "accessToken": "new-token-string",
  "refreshToken": "new-refresh-token",
  "accessTokenExpiresAtUtc": "2025-04-19T07:30:00Z"
}
```

---

### 5. **Revoke Token**
```
POST /api/auth/revoke
```

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "refreshToken": "random-base64-string"
}
```

**Response (200):**
```json
{
  "message": "Refresh token revoked"
}
```

---

### 6. **Assign Role** (Admin Only)
```
PUT /api/auth/assign-role
```

**Headers:**
```
Authorization: Bearer [adminToken]
Content-Type: application/json
```

**Request Body:**
```json
{
  "employeeId": "EMP001",
  "roleName": "Admin"
}
```

**Response (200):**
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

## 🎯 Frontend Implementation Example

### **JavaScript/TypeScript**

```javascript
const API_BASE_URL = 'https://localhost:7199/api/auth';

// 1. Register
async function register(employeeId, password, firstName, lastName, preferredLanguage) {
  const response = await fetch(`${API_BASE_URL}/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      employeeId,
      password,
      firstName,
      lastName,
      preferredLanguage: preferredLanguage || 'english'
    })
  });
  return response.json();
}

// 2. Login
async function login(employeeId, password) {
  const response = await fetch(`${API_BASE_URL}/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ employeeId, password })
  });
  
  const data = await response.json();
  
  if (response.ok) {
    // Store tokens
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data;
  }
  throw new Error(data.message);
}

// 3. Access Protected Endpoint
async function getSecureData() {
  const token = localStorage.getItem('accessToken');
  
  const response = await fetch(`${API_BASE_URL}/secure`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  
  return response.json();
}

// 4. Refresh Token
async function refreshAccessToken() {
  const refreshToken = localStorage.getItem('refreshToken');
  
  const response = await fetch(`${API_BASE_URL}/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });
  
  const data = await response.json();
  
  if (response.ok) {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data;
  }
  throw new Error('Token refresh failed');
}

// 5. Logout (Revoke Token)
async function logout() {
  const refreshToken = localStorage.getItem('refreshToken');
  
  await fetch(`${API_BASE_URL}/revoke`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });
  
  // Clear storage
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
}

// 6. Assign Role (Admin)
async function assignRole(employeeId, roleName) {
  const token = localStorage.getItem('accessToken');
  
  const response = await fetch(`${API_BASE_URL}/assign-role`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ employeeId, roleName })
  });
  
  return response.json();
}
```

---

## ⚙️ React Example

```javascript
import { useState, useEffect } from 'react';

const API_BASE_URL = 'https://localhost:7199/api/auth';

function LoginPage() {
  const [employeeId, setEmployeeId] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      const response = await fetch(`${API_BASE_URL}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ employeeId, password })
      });

      if (!response.ok) {
        throw new Error('Login failed');
      }

      const data = await response.json();
      
      // Store tokens
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      
      // Redirect to dashboard
      window.location.href = '/dashboard';
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <form onSubmit={handleLogin}>
      <input
        type="text"
        placeholder="Employee ID"
        value={employeeId}
        onChange={(e) => setEmployeeId(e.target.value)}
      />
      <input
        type="password"
        placeholder="Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />
      <button type="submit">Login</button>
      {error && <p style={{ color: 'red' }}>{error}</p>}
    </form>
  );
}

export default LoginPage;
```

---

## 🔐 Security Notes

1. **Store tokens in localStorage or sessionStorage** (not cookies for this setup)
2. **Always include Authorization header** with Bearer token for protected endpoints
3. **Token expires in 60 minutes** (check `appsettings.json` "ExpiryMinutes")
4. **Use refresh token** to get a new access token when it expires
5. **Clear tokens on logout** to prevent unauthorized access

---

## 📋 CORS Configuration (if needed)

If your frontend is on a different domain, add CORS to `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://yourdomain.com")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Before app.UseAuthentication()
app.UseCors("AllowFrontend");
```

---

## 🧪 Testing in Swagger

Visit: `https://localhost:7199/swagger`

All endpoints are documented with request/response examples!

---

## ✅ Quick Checklist for Frontend Setup

- [ ] Backend URL: `https://localhost:7199`
- [ ] Register endpoint: `/api/auth/register`
- [ ] Login endpoint: `/api/auth/login`
- [ ] Store `accessToken` after login
- [ ] Include `Authorization: Bearer [token]` header for protected endpoints
- [ ] Implement token refresh logic
- [ ] Clear tokens on logout

