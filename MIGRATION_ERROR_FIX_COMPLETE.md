# ?? MIGRATION ERROR FIX - Missing Dependencies

## ? Error Encountered

```
No DbContext was found in assembly 'ASI.Basecode.WebApp'. 
Ensure that you're using the correct assembly and that the type is neither abstract nor generic.
```

AND

```
Could not execute because the specified command or file was not found.
dotnet-ef does not exist.
```

---

## ?? Root Causes

1. **Missing EF Core Tools** - The `dotnet ef` command is not installed
2. **Wrong Assembly** - EF Core is looking in WebApp instead of Data project
3. **Missing Service Registrations** - Some repositories not registered (RoomLogRepository)

---

## ? COMPLETE FIX (Step-by-Step)

### Step 1: Install EF Core Tools (Required!)

Open a terminal and run:

```powershell
# Install globally (recommended)
dotnet tool install --global dotnet-ef

# OR update if already installed
dotnet tool update --global dotnet-ef
```

**Verify installation:**
```powershell
dotnet ef --version
```

You should see something like: `Entity Framework Core .NET Command-line Tools 9.0.0`

---

### Step 2: Add Missing Repository Registration

The error mentions `IRoomLogRepository` is not registered. Let me check if it needs to be added to `Startup.DI.cs`.

**UPDATE**: Check if you have these in `Startup.DI.cs`:

```csharp
// In ConfigureOtherServices() method

// Repositories
this._services.AddScoped<IUserRepository, UserRepository>();
this._services.AddScoped<IBookingRepository, BookingRepository>();
this._services.AddScoped<IBookingLogRepository, BookingLogRepository>();
this._services.AddScoped<IRoomRepository, RoomRepository>();
this._services.AddScoped<IRoomLogRepository, RoomLogRepository>(); // ? Make sure this exists!
this._services.AddScoped<ISessionRepository, SessionRepository>();
this._services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
this._services.AddScoped<IDashboardRepository, DashboardRepository>();
this._services.AddScoped<IMetricsRepository, MetricsRepository>();
```

---

### Step 3: Run Migration with Correct Parameters

Now that tools are installed, run the migration.

#### **Option A: Package Manager Console (Visual Studio)**

1. Open **Tools** ? **NuGet Package Manager** ? **Package Manager Console**
2. **IMPORTANT**: Set **Default project** dropdown to: `ASI.Basecode.Data`
3. Run:

```powershell
# Simple command (relies on Default project setting)
Add-Migration DashboardOptimization
Update-Database
```

**OR with explicit parameters:**

```powershell
Add-Migration DashboardOptimization -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp -Context WorkSyncDbContext
Update-Database -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp -Context WorkSyncDbContext
```

#### **Option B: .NET CLI (Command Line)**

From your solution root directory (`C:\Development\WorkSync\WorkSync-backend-new`):

```powershell
# Navigate to solution root
cd C:\Development\WorkSync\WorkSync-backend-new

# Create migration
dotnet ef migrations add DashboardOptimization --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp --context WorkSyncDbContext --verbose

# Apply migration
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp --context WorkSyncDbContext --verbose
```

---

## ?? Why These Parameters Matter

| Parameter | Value | Why? |
|-----------|-------|------|
| `--project` | `ASI.Basecode.Data` | Where DbContext lives and where migrations will be created |
| `--startup-project` | `ASI.Basecode.WebApp` | Where `appsettings.json` and DI configuration is |
| `--context` | `WorkSyncDbContext` | Explicit DbContext name (in case multiple exist) |
| `--verbose` | (optional) | Shows detailed output for debugging |

---

## ? Expected Success Output

### After `Add-Migration`:
```
Build started...
Build succeeded.
Done. To undo this action, use Remove-Migration.
```

**Files created in** `ASI.Basecode.Data\Migrations\`:
- `{timestamp}_DashboardOptimization.cs`
- `{timestamp}_DashboardOptimization.Designer.cs`

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

## ?? Troubleshooting Specific Errors

### Error: "dotnet-ef command not found"
**Solution**: Install EF Core tools (Step 1 above)

### Error: "No DbContext was found in assembly 'ASI.Basecode.WebApp'"
**Solution**: Use `--project ASI.Basecode.Data` parameter explicitly

### Error: "Unable to resolve service for type 'IRoomLogRepository'"
**Solution**: 
1. Check if `IRoomLogRepository` and `RoomLogRepository` exist in your codebase
2. Verify they're registered in `Startup.DI.cs`
3. If missing, this is a separate issue from the dashboard migration

**Workaround for now**: You can temporarily comment out the RoomLogRepository service registration errors, but this needs to be fixed properly later.

### Error: "Build failed"
**Solution**: 
```powershell
# Build the solution first
dotnet build

# Check for errors
```

### Error: "A connection was successfully established with the server, but then an error occurred"
**Solution**: 
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Test connection manually in SSMS

---

## ?? Complete Checklist

Before running migration:
- [ ] ? EF Core tools installed (`dotnet ef --version` works)
- [ ] ? Solution builds successfully (`dotnet build`)
- [ ] ? SQL Server is running
- [ ] ? Connection string correct in `appsettings.json`
- [ ] ? All repositories registered in `Startup.DI.cs`

Running migration:
- [ ] Use `--project ASI.Basecode.Data` parameter
- [ ] Use `--startup-project ASI.Basecode.WebApp` parameter
- [ ] Use `--context WorkSyncDbContext` parameter
- [ ] Run from solution root directory

After migration:
- [ ] Migration files exist in `ASI.Basecode.Data\Migrations\`
- [ ] No error messages in console
- [ ] Tables exist in database (verify with SQL query)
- [ ] Entry in `__EFMigrationsHistory` table

---

## ?? Quick Command Reference

### Install Tools (One Time)
```powershell
dotnet tool install --global dotnet-ef
```

### Create Migration
```powershell
# From solution root
cd C:\Development\WorkSync\WorkSync-backend-new
dotnet ef migrations add DashboardOptimization --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp --context WorkSyncDbContext
```

### Apply Migration
```powershell
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp --context WorkSyncDbContext
```

### Verify Migration
```sql
-- Check tables
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'ws'
  AND TABLE_NAME IN ('DailySummaries', 'HourlyStats', 'MetricsComputationLog');

-- Check migration history
SELECT * FROM __EFMigrationsHistory
WHERE MigrationId LIKE '%DashboardOptimization%';
```

---

## ?? Pro Tips

1. **Always use verbose flag** when debugging: `--verbose`
2. **Check EF Core version match** between tools and packages
3. **Run from solution root** for consistency
4. **Set Default project in PMC** if using Package Manager Console
5. **Keep a backup** of your database before applying migrations

---

## ?? Summary

The main issues were:
1. ? Missing `dotnet ef` tools ? ? Install with `dotnet tool install --global dotnet-ef`
2. ? Wrong project path ? ? Use `--project ASI.Basecode.Data`
3. ? Missing context parameter ? ? Use `--context WorkSyncDbContext`

**Now try running the migration again with the corrected commands!** ??
