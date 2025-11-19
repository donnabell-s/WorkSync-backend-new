# ?? EF Core Migration Quick Reference Card

## ?? Your Project Setup
- **DbContext Project**: `ASI.Basecode.Data`
- **Startup Project**: `ASI.Basecode.WebApp`
- **Migration Name**: `DashboardOptimization`

---

## ?? Quick Commands

### Package Manager Console (Visual Studio)

**Set Default Project**: `ASI.Basecode.Data` ?? Important!

```powershell
# Create migration
Add-Migration DashboardOptimization -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp

# Apply migration
Update-Database -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp

# Verify migration
Get-Migration -Project ASI.Basecode.Data
```

### .NET CLI (Command Line)

```bash
# Navigate to Data project
cd ASI.Basecode.Data

# Create migration
dotnet ef migrations add DashboardOptimization --startup-project ..\ASI.Basecode.WebApp --verbose

# Apply migration
dotnet ef database update --startup-project ..\ASI.Basecode.WebApp --verbose

# List migrations
dotnet ef migrations list --startup-project ..\ASI.Basecode.WebApp
```

---

## ? Verify Migration Success

### SQL Query to Check Tables
```sql
USE WorkSync_db;
GO

SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'ws'
  AND TABLE_NAME IN ('DailySummaries', 'HourlyStats', 'MetricsComputationLog')
ORDER BY TABLE_NAME;
```

**Expected**: 3 rows returned

### Check Migration History
```sql
SELECT * FROM __EFMigrationsHistory
WHERE MigrationId LIKE '%DashboardOptimization%';
```

---

## ?? Common Operations

### Undo Last Migration (Not Applied Yet)
```powershell
Remove-Migration -Project ASI.Basecode.Data
```

### Revert Applied Migration
```powershell
# Revert to previous state
Update-Database -Migration <PreviousMigrationName> -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp

# Then remove the migration file
Remove-Migration -Project ASI.Basecode.Data
```

### Generate SQL Script (Instead of Applying)
```powershell
Script-Migration -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp
```

---

## ?? Quick Troubleshooting

### Error: "Build failed"
```powershell
dotnet build
# Fix compilation errors, then retry
```

### Error: "No DbContext was found"
**Solution**: Set **Default project** to `ASI.Basecode.Data` in PMC

### Error: "Your target project doesn't match"
**Solution**: Already fixed! `Startup.cs` now has:
```csharp
sqlServerOptions.MigrationsAssembly("ASI.Basecode.Data");
```

### Error: "Table already exists"
```sql
-- Drop tables and rerun migration
DROP TABLE IF EXISTS ws.HourlyStats;
DROP TABLE IF EXISTS ws.DailySummaries;
DROP TABLE IF EXISTS ws.MetricsComputationLog;
```

---

## ?? Full Process Checklist

- [ ] Open Package Manager Console
- [ ] Set Default Project to `ASI.Basecode.Data`
- [ ] Run `Add-Migration DashboardOptimization`
- [ ] Verify migration files created in `Migrations` folder
- [ ] Run `Update-Database`
- [ ] Check database - 3 new tables exist
- [ ] Verify migration in `__EFMigrationsHistory`
- [ ] Build and run application
- [ ] Verify background service starts

---

## ?? Need More Help?

- **Detailed Guide**: `EF_CORE_MIGRATION_GUIDE.md`
- **Quick Start**: `QUICK_START.md`
- **Full Documentation**: `DASHBOARD_OPTIMIZATION_GUIDE.md`

---

## ?? Pro Tips

1. **Always build before creating migrations** - Ensures clean state
2. **Review generated migration** - Make sure it looks correct
3. **Test in development first** - Before applying to production
4. **Backup database** - Before running Update-Database
5. **Use verbose flag** - For detailed output: `--verbose`

---

**Print this card and keep it handy!** ??
