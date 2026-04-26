# Copy-Paste Frontend Code

## 📋 Copy-Paste Ready Code Snippets

---

## 1️⃣ Configuration File

**`src/config/api.js`**
```javascript
// Backend API Configuration
export const API_CONFIG = {
  BASE_URL: 'https://localhost:7199/api/auth',
  TIMEOUT: 10000,
  HEADERS: {
    'Content-Type': 'application/json'
  }
};

// Endpoints
export const ENDPOINTS = {
  REGISTER: '/register',
  LOGIN: '/login',
  SECURE: '/secure',
  REFRESH: '/refresh',
  REVOKE: '/revoke',
  ASSIGN_ROLE: '/assign-role'
};
```

---

## 2️⃣ Authentication Service

**`src/services/authService.js`**
```javascript
import { API_CONFIG, ENDPOINTS } from '../config/api';

class AuthService {
  // Register new user
  async register(employeeId, firstName, lastName, password, preferredLanguage = 'english') {
    try {
      const response = await fetch(
        `${API_CONFIG.BASE_URL}${ENDPOINTS.REGISTER}`,
        {
          method: 'POST',
          headers: API_CONFIG.HEADERS,
          body: JSON.stringify({
            employeeId,
            firstName,
            lastName,
            password,
            preferredLanguage
          })
        }
      );

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Registration failed');
      }

      return await response.json();
    } catch (error) {
      console.error('Registration error:', error);
      throw error;
    }
  }

  // Login user
  async login(employeeId, password) {
    try {
      const response = await fetch(
        `${API_CONFIG.BASE_URL}${ENDPOINTS.LOGIN}`,
        {
          method: 'POST',
          headers: API_CONFIG.HEADERS,
          body: JSON.stringify({
            employeeId,
            password
          })
        }
      );

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Login failed');
      }

      const data = await response.json();

      // Store tokens
      this.saveTokens(data.accessToken, data.refreshToken);

      return data;
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  // Get secure data
  async getSecureData() {
    try {
      const token = this.getAccessToken();
      
      if (!token) {
        throw new Error('No access token found');
      }

      const response = await fetch(
        `${API_CONFIG.BASE_URL}${ENDPOINTS.SECURE}`,
        {
          method: 'GET',
          headers: {
            ...API_CONFIG.HEADERS,
            'Authorization': `Bearer ${token}`
          }
        }
      );

      if (response.status === 401) {
        // Token expired, try refresh
        await this.refreshToken();
        return this.getSecureData(); // Retry
      }

      if (!response.ok) {
        throw new Error('Failed to get secure data');
      }

      return await response.json();
    } catch (error) {
      console.error('Secure data error:', error);
      throw error;
    }
  }

  // Refresh token
  async refreshToken() {
    try {
      const refreshToken = this.getRefreshToken();
      
      if (!refreshToken) {
        throw new Error('No refresh token found');
      }

      const response = await fetch(
        `${API_CONFIG.BASE_URL}${ENDPOINTS.REFRESH}`,
        {
          method: 'POST',
          headers: API_CONFIG.HEADERS,
          body: JSON.stringify({ refreshToken })
        }
      );

      if (!response.ok) {
        // Refresh failed, clear tokens and redirect to login
        this.logout();
        throw new Error('Token refresh failed');
      }

      const data = await response.json();
      this.saveTokens(data.accessToken, data.refreshToken);

      return data;
    } catch (error) {
      console.error('Token refresh error:', error);
      throw error;
    }
  }

  // Revoke/Logout
  async logout() {
    try {
      const refreshToken = this.getRefreshToken();
      
      if (refreshToken) {
        await fetch(
          `${API_CONFIG.BASE_URL}${ENDPOINTS.REVOKE}`,
          {
            method: 'POST',
            headers: API_CONFIG.HEADERS,
            body: JSON.stringify({ refreshToken })
          }
        );
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      // Clear tokens regardless of response
      this.clearTokens();
    }
  }

  // Assign role (admin only)
  async assignRole(employeeId, roleName) {
    try {
      const token = this.getAccessToken();
      
      if (!token) {
        throw new Error('No access token found');
      }

      const response = await fetch(
        `${API_CONFIG.BASE_URL}${ENDPOINTS.ASSIGN_ROLE}`,
        {
          method: 'PUT',
          headers: {
            ...API_CONFIG.HEADERS,
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify({
            employeeId,
            roleName
          })
        }
      );

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Failed to assign role');
      }

      return await response.json();
    } catch (error) {
      console.error('Assign role error:', error);
      throw error;
    }
  }

  // Token management helpers
  saveTokens(accessToken, refreshToken) {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }

  getAccessToken() {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken() {
    return localStorage.getItem('refreshToken');
  }

  clearTokens() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }

  isAuthenticated() {
    return !!this.getAccessToken();
  }
}

export default new AuthService();
```

---

## 3️⃣ React Login Component

**`src/components/LoginPage.jsx`**
```javascript
import { useState } from 'react';
import authService from '../services/authService';

function LoginPage() {
  const [employeeId, setEmployeeId] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleLogin = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      await authService.login(employeeId, password);
      // Redirect to dashboard
      window.location.href = '/dashboard';
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: '400px', margin: '100px auto' }}>
      <h2>Login</h2>
      <form onSubmit={handleLogin}>
        <div style={{ marginBottom: '15px' }}>
          <input
            type="text"
            placeholder="Employee ID"
            value={employeeId}
            onChange={(e) => setEmployeeId(e.target.value)}
            disabled={loading}
            style={{ width: '100%', padding: '8px' }}
          />
        </div>
        <div style={{ marginBottom: '15px' }}>
          <input
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            disabled={loading}
            style={{ width: '100%', padding: '8px' }}
          />
        </div>
        <button 
          type="submit" 
          disabled={loading}
          style={{ width: '100%', padding: '10px' }}
        >
          {loading ? 'Logging in...' : 'Login'}
        </button>
      </form>
      {error && <p style={{ color: 'red', marginTop: '15px' }}>{error}</p>}
    </div>
  );
}

export default LoginPage;
```

---

## 4️⃣ React Protected Route

**`src/components/ProtectedRoute.jsx`**
```javascript
import { Navigate } from 'react-router-dom';
import authService from '../services/authService';

function ProtectedRoute({ children }) {
  if (!authService.isAuthenticated()) {
    return <Navigate to="/login" />;
  }

  return children;
}

export default ProtectedRoute;
```

---

## 5️⃣ React Dashboard Component

**`src/components/Dashboard.jsx`**
```javascript
import { useState, useEffect } from 'react';
import authService from '../services/authService';

function Dashboard() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      try {
        const result = await authService.getSecureData();
        setData(result);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleLogout = () => {
    authService.logout();
    window.location.href = '/login';
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div style={{ color: 'red' }}>Error: {error}</div>;

  return (
    <div style={{ padding: '20px' }}>
      <h1>Dashboard</h1>
      <p>{data?.message}</p>
      <button onClick={handleLogout}>Logout</button>
    </div>
  );
}

export default Dashboard;
```

---

## 6️⃣ HTTP Interceptor (Optional but Recommended)

**`src/utils/httpClient.js`**
```javascript
import authService from '../services/authService';

export async function apiCall(url, options = {}) {
  const token = authService.getAccessToken();

  const headers = {
    'Content-Type': 'application/json',
    ...options.headers
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  let response = await fetch(url, {
    ...options,
    headers
  });

  // If 401, try refreshing token
  if (response.status === 401 && authService.getRefreshToken()) {
    try {
      await authService.refreshToken();
      const newToken = authService.getAccessToken();
      headers['Authorization'] = `Bearer ${newToken}`;

      response = await fetch(url, {
        ...options,
        headers
      });
    } catch (error) {
      authService.logout();
      window.location.href = '/login';
      throw error;
    }
  }

  return response;
}
```

---

## 7️⃣ App Router Setup

**`src/App.jsx`**
```javascript
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './components/LoginPage';
import Dashboard from './components/Dashboard';
import ProtectedRoute from './components/ProtectedRoute';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/dashboard" />} />
      </Routes>
    </Router>
  );
}

export default App;
```

---

## 8️⃣ Environment Variables

**`.env`**
```
VITE_API_URL=https://localhost:7199/api/auth
VITE_ENV=development
```

**`src/config/api.js` (updated)**
```javascript
export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_URL || 'https://localhost:7199/api/auth',
  TIMEOUT: 10000,
  HEADERS: {
    'Content-Type': 'application/json'
  }
};
```

---

## 9️⃣ Package.json Scripts

```json
{
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview",
    "lint": "eslint src"
  },
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^6.0.0"
  }
}
```

---

## 🔟 Usage Examples

### Login
```javascript
import authService from './services/authService';

// Call this when user submits login form
await authService.login('EMP001', 'password123');
// Tokens are automatically stored
```

### Use Protected Endpoint
```javascript
// Token is automatically included in Authorization header
const data = await authService.getSecureData();
```

### Logout
```javascript
await authService.logout();
// Tokens are cleared
```

### Check Authentication Status
```javascript
if (authService.isAuthenticated()) {
  // User is logged in
}
```

---

## ✅ Installation Steps

1. **Create React App**
   ```bash
   npm create vite@latest my-app -- --template react
   cd my-app
   npm install
   ```

2. **Install React Router**
   ```bash
   npm install react-router-dom
   ```

3. **Copy files**
   - Copy all the above code snippets to your project
   - Maintain the folder structure

4. **Run**
   ```bash
   npm run dev
   ```

5. **Access**
   - Frontend: `http://localhost:5173`
   - Backend: `https://localhost:7199`

---

## 🚀 You're Ready!

All code is copy-paste ready. Just adjust paths based on your project structure.

