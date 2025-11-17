using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace ASI.Basecode.Data
{
    // Design-time factory for EF Core tools (migrations)
    public class WorkSyncDbContextFactory : IDesignTimeDbContextFactory<WorkSyncDbContext>
    {
        public WorkSyncDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<WorkSyncDbContext>();

            // Allow overriding connection string via environment variable for safety in dev/CI
            var conn = Environment.GetEnvironmentVariable("WORKSYNC_CONNECTIONSTRING");
            if (string.IsNullOrWhiteSpace(conn))
            {
                // Fallback to the same connection used in OnConfiguring
                conn = "Addr=localhost; database=WorkSync_db; Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            }

            builder.UseSqlServer(conn);
            return new WorkSyncDbContext(builder.Options);
        }
    }
}
