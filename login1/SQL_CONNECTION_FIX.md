# 🔧 SQL Server Connection Fix

## ❌ THE PROBLEM

You got this error after merging code:
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server. 
The server was not found or was not accessible. 
(provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
```

---

## 🔍 ROOT CAUSE

You had **TWO different SQL Server instances** in your configuration files:

| File | Server |
|------|--------|
| `appsettings.json` | `ITWW007LAP73981\\MSSQLSERVER1` |
| `appsettings.Development.json` | `ITWW007LAP52616` |

When running in **Development**, the `appsettings.Development.json` is used, which pointed to a **different machine** that either doesn't exist or isn't accessible.

---

## ✅ THE FIX

Both files now use:
```json
"DefaultConnection": "Server=localhost;Database=JwtAuthDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

**Changes:**
1. ✅ Both now point to `localhost` (your local machine)
2. ✅ Removed `Encrypt=True` (not needed for local connections)
3. ✅ Simplified configuration (removed unnecessary instance names)

---

## 🚀 NEXT STEPS

1. **Rebuild the project:**
   ```bash
   dotnet build
   ```

2. **Run the backend:**
   ```bash
   dotnet run
   ```

3. **Verify SQL Server is running:**
   - Open Services (`services.msc`)
   - Look for `SQL Server (INSTANCE_NAME)`
   - Ensure status is **Running**

4. **Test the connection:**
   - Try accessing an endpoint
   - Check Swagger: `https://localhost:7199/swagger`

---

## 🔧 IF IT STILL DOESN'T WORK

Try these alternatives for the connection string:

### **Option 1: Using Dot Notation (Default Instance)**
```json
"Server=.;Database=JwtAuthDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

### **Option 2: Using Named Instance**
Replace `INSTANCE_NAME` with your actual SQL Server instance name:
```json
"Server=localhost\\INSTANCE_NAME;Database=JwtAuthDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

### **Option 3: Using TCP/IP with Port**
```json
"Server=localhost,1433;Database=JwtAuthDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

---

## 📝 HOW TO FIND YOUR SQL SERVER INSTANCE NAME

1. Open **SQL Server Configuration Manager**
2. Go to **SQL Server Services**
3. Look for the running instance name
4. If it says `SQL Server (MSSQLSERVER)`, use `localhost`
5. If it says `SQL Server (CUSTOM_NAME)`, use `localhost\\CUSTOM_NAME`

---

## ✨ SUMMARY

**What was wrong:**
- Merged code had conflicting connection strings
- Development config pointed to wrong server

**What I fixed:**
- Updated both `appsettings.json` and `appsettings.Development.json`
- Both now point to `localhost`
- Simplified and standardized the connection string

**Result:**
- ✅ Connection error should be resolved
- ✅ Backend can now connect to local SQL Server
- ✅ Can test endpoints again

