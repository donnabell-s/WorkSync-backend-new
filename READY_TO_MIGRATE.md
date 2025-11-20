# ? READY TO MIGRATE - Final Instructions

## ?? All Issues Fixed!

? **Build**: Successful  
? **DbContext Fix**: Applied (`if (!optionsBuilder.IsConfigured)`)  
? **Startup.cs**: Has `MigrationsAssembly("ASI.Basecode.Data")`  
? **Missing Repositories**: Added (`IRoomLogRepository`, `IMetricsRepository`)  

---

## ?? Step 1: Install EF Core Tools (One Time Only)

Open a terminal and run:

```powershell
dotnet tool install --global dotnet-ef
```

If already installed, update it:

```powershell
dotnet tool update --global dotnet-ef
```

Verify installation:

```powershell
dotnet ef --version
```

Expected output: `Entity Framework Core .NET Command-line Tools 9.0.0` (or similar)

---

## ?? Step 2: Run the Migration

### **Option A: Package Manager Console (Recommended)**

1. Open Visual Studio
2. Go to **Tools** ? **NuGet Package Manager** ? **Package Manager Console**
3. Set **Default project** dropdown to: `ASI.Basecode.Data` ??
4. Run these commands:

```powershell
Add-Migration DashboardOptimization
Update-Database
```

**OR** with explicit parameters:

```powershell
Add-Migration DashboardOptimization -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp
Update-Database -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp
```

### **Option B: .NET CLI (Command Line)**

From your solution root directory:

```powershell
# Navigate to solution root
cd C:\Development\WorkSync\WorkSync-backend-new

# Create migration
dotnet ef migrations add DashboardOptimization --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp --context WorkSyncDbContext

# Apply migration
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp --context WorkSyncDbContext
```

---

## ? Expected Output

### After `Add-Migration`:
```
Build started...
Build succeeded.
Done. To undo this action, use Remove-Migration.
```

**Files created:**
- `ASI.Basecode.Data\Migrations\{timestamp}_DashboardOptimization.cs`
- `ASI.Basecode.Data\Migrations\{timestamp}_DashboardOptimization.Designer.cs`

### After `Update-Database`:
```
Build started...
Build succeeded.
Applying migration '{timestamp}_DashboardOptimization'.
Done.
```

**Tables created in database:**
- `ws.DailySummaries`
- `ws.HourlyStats`
- `ws.MetricsComputationLog`

---

## ?? Verify Success

### Check tables in SSMS or Azure Data Studio:

```sql
USE WorkSync_db;
GO

SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'ws'
  AND TABLE_NAME IN ('DailySummaries', 'HourlyStats', 'MetricsComputationLog')
ORDER BY TABLE_NAME;
```

Expected: 3 rows returned

### Check migration history:

```sql
SELECT * FROM __EFMigrationsHistory
WHERE MigrationId LIKE '%DashboardOptimization%';
```

Expected: 1 row with your migration

---

## ?? Next Steps After Migration

1. ? Build and run the application:
   ```powershell
   cd ASI.Basecode.WebApp
   dotnet run
   ```

2. ? Verify background service starts (check console logs):
   ```
   [INFO] MetricsComputationHostedService is starting.
   ```

3. ? Run initial data backfill (see `QUICK_START.md`):
   ```http
   POST http://localhost:5000/api/Dashboard/BackfillMetrics
   Authorization: Bearer YOUR_SUPERADMIN_TOKEN
   Content-Type: application/json
   
   {
     "startDate": "2024-01-01",
     "endDate": "2024-01-31"
   }
   ```

4. ? Test the optimized endpoint:
   ```http
   GET http://localhost:5000/api/Dashboard/GetOptimizedDashboard
   Authorization: Bearer YOUR_ADMIN_TOKEN
   ```
   
   Expected: Response time < 50ms (on second request - cached) ?

---

## ?? If You Still Get Errors

### Error: "dotnet-ef not found"
**Solution**: Install EF Core tools (Step 1 above)

### Error: "No DbContext was found"
**Solution**: Add `--context WorkSyncDbContext` parameter

### Error: "Unable to resolve service"
**Solution**: Already fixed! Build is successful with all repositories registered

### Error: "Connection error"
**Solution**: 
- Verify SQL Server is running
- Check connection string in `appsettings.json`

---

## ?? Documentation

For more details:
- **Quick Start**: `QUICK_START.md`
- **Detailed Migration Guide**: `EF_CORE_MIGRATION_GUIDE.md`
- **Complete Documentation**: `DASHBOARD_OPTIMIZATION_GUIDE.md`
- **Error Fix Guide**: `MIGRATION_ERROR_FIX_COMPLETE.md`

---

## ?? Summary

**What was fixed:**
1. ? Added `IRoomLogRepository` registration in `Startup.DI.cs`
2. ? Added `IMetricsRepository` registration in `Startup.DI.cs`
3. ? DbContext already has `IsConfigured` check
4. ? Startup.cs already has `MigrationsAssembly`
5. ? Build successful

**What you need to do:**
1. Install EF Core tools (if not already installed)
2. Run `Add-Migration DashboardOptimization`
3. Run `Update-Database`
4. Verify tables created

**You're all set! Run the commands above!** ??

---

## ?? Pro Tip

Save these commands for future migrations:

```powershell
# Package Manager Console (set Default project to ASI.Basecode.Data first)
Add-Migration <MigrationName>
Update-Database

# OR .NET CLI (from solution root)
dotnet ef migrations add <MigrationName> --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```
