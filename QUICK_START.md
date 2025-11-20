# ?? Quick Start Guide - Dashboard Optimization

## Prerequisites
- SQL Server with WorkSync_db database
- .NET 9 SDK
- Visual Studio 2022 or VS Code
- Admin/SuperAdmin account for testing

## Setup Steps (10 minutes)

### 1. Run Database Migration (EF Core)

**Important**: We use Entity Framework Core migrations, not SQL scripts.

#### Option A: Using Package Manager Console (Visual Studio) - **RECOMMENDED**

1. Open **Tools** ? **NuGet Package Manager** ? **Package Manager Console**
2. **Set Default project** dropdown to: `ASI.Basecode.Data` ?? **CRITICAL STEP**
3. Run these commands:

```powershell
# Create the migration (with explicit project parameters)
Add-Migration DashboardOptimization -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp

# Apply the migration to database
Update-Database -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp
```

**OR** if you've set the Default project dropdown correctly:

```powershell
# Create the migration (simpler - relies on Default project setting)
Add-Migration DashboardOptimization

# Apply the migration
Update-Database
```

#### Option B: Using .NET CLI (Command Line)

Open a terminal and navigate to your solution folder, then:

```bash
# Run from solution root directory
dotnet ef migrations add DashboardOptimization --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp

# Apply the migration to database
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```

**Expected Result**: 
- Migration files created in `ASI.Basecode.Data\Migrations\`
- Three new tables created in your database:
  - `ws.DailySummaries`
  - `ws.HourlyStats`
  - `ws.MetricsComputationLog`

**Troubleshooting**: If you still see the "target project doesn't match" error:
1. Make sure you've set **Default project** to `ASI.Basecode.Data` in Package Manager Console
2. OR always use the `-Project` and `-StartupProject` parameters explicitly
3. See detailed troubleshooting: **`EF_CORE_MIGRATION_GUIDE.md`**

### 2. Build and Run Backend
```bash
cd ASI.Basecode.WebApp
dotnet build
dotnet run
```

### 3. Verify Background Service Started
Check console output for:
```
[INFO] MetricsComputationHostedService is starting.
```

### 4. Backfill Initial Data
Open Postman/curl and send:

```http
POST http://localhost:5000/api/Dashboard/BackfillMetrics
Authorization: Bearer YOUR_SUPERADMIN_TOKEN
Content-Type: application/json

{
  "startDate": "2024-01-01",
  "endDate": "2024-01-31"
}
```

**Wait ~1-2 minutes for completion**

### 5. Test Optimized Endpoint
```http
GET http://localhost:5000/api/Dashboard/GetOptimizedDashboard
Authorization: Bearer YOUR_ADMIN_TOKEN
```

Expected response time: **< 50ms** ?

### 6. Update React Frontend

#### Install dependencies (if needed)
```bash
npm install axios @tanstack/react-query
```

#### Update your dashboard service
```typescript
// services/dashboardService.ts
export const getDashboard = async () => {
  const response = await axios.get(
    'http://localhost:5000/api/Dashboard/GetOptimizedDashboard',
    {
      headers: { Authorization: `Bearer ${token}` }
    }
  );
  return response.data;
};
```

#### Update your component
```typescript
// components/Dashboard.tsx
const { data } = useQuery({
  queryKey: ['dashboard'],
  queryFn: getDashboard,
  refetchInterval: 5 * 60 * 1000, // Auto-refresh every 5 min
});

// Use data.data.summary, data.data.trends, data.data.peakUsage
```

## Verify It's Working

### Check 1: Tables Created
```sql
-- Check if migration tables exist
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'ws'
  AND TABLE_NAME IN ('DailySummaries', 'HourlyStats', 'MetricsComputationLog')
ORDER BY TABLE_NAME;
-- Should return 3 rows
```

### Check 2: Background Service
```sql
-- Should see recent computations (after waiting 5 minutes or running backfill)
SELECT TOP 5 * 
FROM ws.MetricsComputationLog 
ORDER BY StartedAt DESC;
```

### Check 3: Summary Data
```sql
-- Should have data for recent dates (after running backfill)
SELECT TOP 5 * 
FROM ws.DailySummaries 
ORDER BY SummaryDate DESC;
```

### Check 4: Cache Hit
Call the optimized endpoint twice in a row. Second call should return:
```json
{
  "success": true,
  "data": {
    ...
    "fromCache": true  // ? This means it's working!
  },
  "message": "Data retrieved from cache"
}
```

### Check 5: Performance
Time the API call:
- **Cached**: < 50ms ?
- **Database**: < 200ms ??
- **Old endpoint**: > 2000ms ?? (for comparison)

## Troubleshooting

### Problem: Migration error "target project doesn't match migrations assembly"
**Fix**: This is already fixed in `Startup.cs`. If you still see this error, verify `Startup.cs` contains:
```csharp
sqlServerOptions.MigrationsAssembly("ASI.Basecode.Data");
```

### Problem: "No DbContext was found"
**Fix**: Make sure you set the **Default project** to `ASI.Basecode.Data` in Package Manager Console

### Problem: Tables already exist
**Fix**: If you manually created tables before, drop them first:
```sql
DROP TABLE IF EXISTS ws.HourlyStats;
DROP TABLE IF EXISTS ws.DailySummaries;
DROP TABLE IF EXISTS ws.MetricsComputationLog;
```
Then run `Update-Database` again.

### Problem: Background service not starting
**Fix**: Check `appsettings.json` has correct connection string

### Problem: No data in summary tables
**Fix**: Run the backfill endpoint or wait 5 minutes for next auto-computation

### Problem: Still slow responses
**Fix**: Verify cache is enabled and `fromCache` is `true` on second request

## What Happens Now?

1. ? Background service runs every **5 minutes**
2. ? Metrics auto-update for today and yesterday
3. ? Dashboard loads in **< 50ms** from cache
4. ? Data is always fresh (< 5 minutes old)
5. ? Zero infrastructure cost (uses built-in .NET features)

## Need More Help?

- **Migration Issues**: See `EF_CORE_MIGRATION_GUIDE.md`
- **Complete Guide**: See `DASHBOARD_OPTIMIZATION_GUIDE.md`
- **React Integration**: See `REACT_INTEGRATION_COMPLETE.md`

## Success Checklist

- [ ] Database migration completed (3 tables created)
- [ ] Backend builds without errors
- [ ] Background service logs show "Starting metrics computation"
- [ ] Backfill endpoint returns success
- [ ] Optimized endpoint returns data in < 50ms
- [ ] `fromCache: true` on second request
- [ ] React app fetches data successfully
- [ ] Dashboard loads visibly faster

**You're done!** ?? Enjoy your 50x faster dashboard!
