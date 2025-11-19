# Entity Framework Core Migration Guide - Dashboard Optimization

## ?? Overview
This guide walks you through creating and applying EF Core migrations for the dashboard optimization feature.

---

## ?? Prerequisites

Before running migrations, ensure:
- ? Visual Studio 2022 with Package Manager Console
- ? .NET 9 SDK installed
- ? SQL Server accessible
- ? Connection string configured in `appsettings.json`

---

## ?? Step-by-Step Migration Process

### Step 1: Open Package Manager Console

In Visual Studio:
1. Go to **Tools** ? **NuGet Package Manager** ? **Package Manager Console**
2. Set the **Default project** dropdown to: `ASI.Basecode.Data`

![Package Manager Console](https://user-images.githubusercontent.com/placeholder-pmc.png)

### Step 2: Verify Connection String

Check your `ASI.Basecode.WebApp\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WorkSync_db;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Important**: Update the server name, database name, and credentials to match your environment.

### Step 3: Create the Migration

In Package Manager Console, run:

```powershell
Add-Migration DashboardOptimization -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp
```

**Expected Output:**
```
Build started...
Build succeeded.
To undo this action, use Remove-Migration.
```

This will create migration files in `ASI.Basecode.Data\Migrations\`:
- `{timestamp}_DashboardOptimization.cs`
- `{timestamp}_DashboardOptimization.Designer.cs`

### Step 4: Review the Generated Migration

Open the generated migration file and verify it contains:

```csharp
public partial class DashboardOptimization : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Creates DailySummaries table
        migrationBuilder.CreateTable(
            name: "DailySummaries",
            schema: "ws",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SummaryDate = table.Column<DateTime>(nullable: false),
                // ... other columns
            });

        // Creates HourlyStats table
        // Creates MetricsComputationLog table
        // Creates indexes
    }
}
```

### Step 5: Apply the Migration

Run the migration to update your database:

```powershell
Update-Database -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp
```

**Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20250116123456_DashboardOptimization'.
Done.
```

---

## ? Verify Migration Success

### Option 1: Using SQL Server Management Studio (SSMS)

1. Connect to your SQL Server
2. Expand `Databases` ? `WorkSync_db` ? `Tables`
3. Verify the following tables exist:
   - `ws.DailySummaries`
   - `ws.HourlyStats`
   - `ws.MetricsComputationLog`

### Option 2: Using SQL Query

Run this query in SSMS or Azure Data Studio:

```sql
USE WorkSync_db;
GO

-- Check if tables exist
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'ws'
  AND TABLE_NAME IN ('DailySummaries', 'HourlyStats', 'MetricsComputationLog')
ORDER BY TABLE_NAME;

-- Should return 3 rows
```

### Option 3: Check Migration History

```sql
SELECT * FROM __EFMigrationsHistory
ORDER BY MigrationId DESC;

-- Should see 'DashboardOptimization' migration
```

---

## ?? Troubleshooting

### Error: "Build failed"

**Problem**: Compilation errors in the code.

**Solution**:
```powershell
# In Package Manager Console
dotnet build
```
Fix any compilation errors, then retry the migration.

### Error: "No DbContext was found"

**Problem**: Wrong default project selected.

**Solution**:
1. In Package Manager Console, set **Default project** to `ASI.Basecode.Data`
2. Or explicitly specify the project:
```powershell
Add-Migration DashboardOptimization -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp
```

### Error: "Your target project doesn't match your migrations assembly"

**Problem**: Already fixed in `Startup.cs` by specifying `MigrationsAssembly("ASI.Basecode.Data")`.

**Verify Fix**:
Check `ASI.Basecode.WebApp\Startup.cs` contains:
```csharp
services.AddDbContext<WorkSyncDbContext>(options =>
{
    options.UseSqlServer(
        Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions =>
        {
            sqlServerOptions.CommandTimeout(120);
            sqlServerOptions.MigrationsAssembly("ASI.Basecode.Data");
        });
});
```

### Error: "Cannot open database"

**Problem**: SQL Server not running or wrong connection string.

**Solution**:
1. Start SQL Server
2. Verify connection string in `appsettings.json`
3. Test connection manually

### Error: "Table already exists"

**Problem**: Tables were created manually (e.g., from SQL script).

**Solution Option 1** - Drop tables and re-run migration:
```sql
USE WorkSync_db;
GO

DROP TABLE IF EXISTS ws.HourlyStats;
DROP TABLE IF EXISTS ws.DailySummaries;
DROP TABLE IF EXISTS ws.MetricsComputationLog;
GO
```

Then run `Update-Database` again.

**Solution Option 2** - Mark migration as applied without running it:
```powershell
# This tells EF the migration is already applied
# Only use if tables already exist and match the schema
Add-Migration DashboardOptimization -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp
# Then manually insert into migration history:
```

```sql
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20250116000000_DashboardOptimization', '9.0.0');
```

---

## ?? Rolling Back a Migration

### Undo the Last Migration (Before Update-Database)

If you haven't run `Update-Database` yet:

```powershell
Remove-Migration -Project ASI.Basecode.Data
```

### Revert an Applied Migration

If you've already run `Update-Database`:

```powershell
# Revert to the previous migration
Update-Database -Migration <PreviousMigrationName> -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp

# Or revert all migrations
Update-Database -Migration 0 -Project ASI.Basecode.Data -StartupProject ASI.Basecode.WebApp

# Then remove the migration file
Remove-Migration -Project ASI.Basecode.Data
```

---

## ?? Alternative: Using .NET CLI

If you prefer command line over Package Manager Console:

### Navigate to the Data Project

```bash
cd C:\Development\WorkSync\WorkSync-backend-new\ASI.Basecode.Data
```

### Create Migration

```bash
dotnet ef migrations add DashboardOptimization --startup-project ..\ASI.Basecode.WebApp --verbose
```

### Apply Migration

```bash
dotnet ef database update --startup-project ..\ASI.Basecode.WebApp --verbose
```

### List Migrations

```bash
dotnet ef migrations list --startup-project ..\ASI.Basecode.WebApp
```

### Remove Last Migration

```bash
dotnet ef migrations remove --startup-project ..\ASI.Basecode.WebApp
```

---

## ?? Expected Database Schema

After successful migration, your tables should have this structure:

### DailySummaries Table

```sql
CREATE TABLE [ws].[DailySummaries] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SummaryDate] DATETIME2 NOT NULL,
    [TotalBookings] INT NOT NULL,
    [CompletedBookings] INT NOT NULL,
    [OngoingBookings] INT NOT NULL,
    [AvailableRooms] INT NOT NULL,
    [MaintenanceRooms] INT NOT NULL,
    [TotalBookedMinutes] INT NOT NULL,
    [TotalAvailableMinutes] INT NOT NULL,
    [UtilizationRate] FLOAT NOT NULL,
    [LastComputedAt] DATETIME2 NOT NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT UQ_DailySummaries_SummaryDate UNIQUE (SummaryDate)
);

CREATE INDEX IX_DailySummaries_SummaryDate ON [ws].[DailySummaries] (SummaryDate);
```

### HourlyStats Table

```sql
CREATE TABLE [ws].[HourlyStats] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [StatDate] DATETIME2 NOT NULL,
    [Hour] INT NOT NULL CHECK ([Hour] >= 0 AND [Hour] <= 23),
    [RoomId] VARCHAR(50) NOT NULL,
    [RoomName] NVARCHAR(200) NULL,
    [BookedMinutes] INT NOT NULL,
    [OccupancyRate] FLOAT NOT NULL,
    [BookingCount] INT NOT NULL,
    [LastComputedAt] DATETIME2 NOT NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT FK_HourlyStats_Rooms FOREIGN KEY (RoomId) 
        REFERENCES [ws].[Rooms](RoomId) ON DELETE CASCADE,
    CONSTRAINT UQ_HourlyStats_StatDate_Room_Hour 
        UNIQUE (StatDate, RoomId, Hour)
);

CREATE INDEX IX_HourlyStats_StatDate ON [ws].[HourlyStats] (StatDate);
CREATE INDEX IX_HourlyStats_StatDate_RoomId ON [ws].[HourlyStats] (StatDate, RoomId);
```

### MetricsComputationLog Table

```sql
CREATE TABLE [ws].[MetricsComputationLog] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MetricType] NVARCHAR(100) NOT NULL,
    [ComputationDate] DATETIME2 NOT NULL,
    [StartedAt] DATETIME2 NOT NULL,
    [CompletedAt] DATETIME2 NULL,
    [Status] NVARCHAR(50) NOT NULL,
    [RecordsProcessed] INT NOT NULL,
    [ErrorMessage] NVARCHAR(4000) NULL,
    [DurationMs] BIGINT NULL
);

CREATE INDEX IX_MetricsComputationLog_MetricType_Date_Status 
    ON [ws].[MetricsComputationLog] (MetricType, ComputationDate, Status);
```

---

## ?? Next Steps After Migration

1. ? **Verify Tables Created** - Use SSMS or SQL query above
2. ? **Build Application** - `dotnet build`
3. ? **Run Application** - `dotnet run`
4. ? **Check Background Service** - Look for "MetricsComputationHostedService is starting" in logs
5. ? **Backfill Initial Data** - Use the `/api/Dashboard/BackfillMetrics` endpoint
6. ? **Test Optimized Endpoint** - Call `/api/Dashboard/GetOptimizedDashboard`

---

## ?? Additional Resources

### EF Core Migrations Documentation
- [Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Managing Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing)
- [Applying Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)

### Package Manager Console Commands Reference

| Command | Description |
|---------|-------------|
| `Add-Migration <Name>` | Create a new migration |
| `Update-Database` | Apply pending migrations |
| `Update-Database -Migration <Name>` | Migrate to specific version |
| `Remove-Migration` | Remove last migration (before Update-Database) |
| `Script-Migration` | Generate SQL script for migrations |
| `Get-Migration` | List all migrations |
| `Drop-Database` | Drop the database |

### .NET CLI Commands Reference

| Command | Description |
|---------|-------------|
| `dotnet ef migrations add <Name>` | Create a new migration |
| `dotnet ef database update` | Apply pending migrations |
| `dotnet ef database update <Name>` | Migrate to specific version |
| `dotnet ef migrations remove` | Remove last migration |
| `dotnet ef migrations script` | Generate SQL script |
| `dotnet ef migrations list` | List all migrations |
| `dotnet ef database drop` | Drop the database |

---

## ? Success Checklist

After completing the migration, verify:

- [ ] Migration created successfully in `ASI.Basecode.Data\Migrations\`
- [ ] `Update-Database` completed without errors
- [ ] Three new tables exist in database (DailySummaries, HourlyStats, MetricsComputationLog)
- [ ] Indexes created on tables
- [ ] Foreign keys properly configured
- [ ] Migration recorded in `__EFMigrationsHistory` table
- [ ] Application builds without errors
- [ ] Background service starts successfully

---

## ?? You're Ready!

Once the migration is successful, continue with the Quick Start Guide to:
1. Run initial data backfill
2. Test the optimized endpoints
3. Verify cache performance

**See**: `QUICK_START.md` for next steps.
