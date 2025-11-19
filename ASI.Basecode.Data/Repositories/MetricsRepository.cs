using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    /// <summary>
    /// Repository implementation for precomputed metrics operations.
    /// Handles efficient storage and retrieval of dashboard summary data.
    /// </summary>
    public class MetricsRepository : BaseRepository, IMetricsRepository
    {
        public MetricsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region DailySummary Operations

        public async Task<DailySummary> GetDailySummaryAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var targetDate = date.Date;
            return await GetDbSet<DailySummary>()
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.SummaryDate == targetDate, cancellationToken);
        }

        public async Task<List<DailySummary>> GetDailySummariesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            
            return await GetDbSet<DailySummary>()
                .AsNoTracking()
                .Where(ds => ds.SummaryDate >= start && ds.SummaryDate <= end)
                .OrderBy(ds => ds.SummaryDate)
                .ToListAsync(cancellationToken);
        }

        public async Task UpsertDailySummaryAsync(DailySummary summary, CancellationToken cancellationToken = default)
        {
            var existing = await GetDbSet<DailySummary>()
                .FirstOrDefaultAsync(ds => ds.SummaryDate == summary.SummaryDate, cancellationToken);

            if (existing != null)
            {
                // Update existing record
                existing.TotalBookings = summary.TotalBookings;
                existing.CompletedBookings = summary.CompletedBookings;
                existing.OngoingBookings = summary.OngoingBookings;
                existing.AvailableRooms = summary.AvailableRooms;
                existing.MaintenanceRooms = summary.MaintenanceRooms;
                existing.TotalBookedMinutes = summary.TotalBookedMinutes;
                existing.TotalAvailableMinutes = summary.TotalAvailableMinutes;
                existing.UtilizationRate = summary.UtilizationRate;
                existing.LastComputedAt = summary.LastComputedAt;
                
                SetEntityState(existing, EntityState.Modified);
            }
            else
            {
                // Insert new record
                await GetDbSet<DailySummary>().AddAsync(summary, cancellationToken);
            }
        }

        public async Task BulkUpsertDailySummariesAsync(List<DailySummary> summaries, CancellationToken cancellationToken = default)
        {
            foreach (var summary in summaries)
            {
                await UpsertDailySummaryAsync(summary, cancellationToken);
            }
        }

        #endregion

        #region HourlyStat Operations

        public async Task<List<HourlyStat>> GetHourlyStatsAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var targetDate = date.Date;
            return await GetDbSet<HourlyStat>()
                .AsNoTracking()
                .Where(hs => hs.StatDate == targetDate)
                .OrderBy(hs => hs.Hour)
                .ThenBy(hs => hs.RoomName)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<HourlyStat>> GetHourlyStatsByRoomAsync(DateTime date, string roomId, CancellationToken cancellationToken = default)
        {
            var targetDate = date.Date;
            return await GetDbSet<HourlyStat>()
                .AsNoTracking()
                .Where(hs => hs.StatDate == targetDate && hs.RoomId == roomId)
                .OrderBy(hs => hs.Hour)
                .ToListAsync(cancellationToken);
        }

        public async Task UpsertHourlyStatAsync(HourlyStat stat, CancellationToken cancellationToken = default)
        {
            var existing = await GetDbSet<HourlyStat>()
                .FirstOrDefaultAsync(hs => hs.StatDate == stat.StatDate && 
                                          hs.Hour == stat.Hour && 
                                          hs.RoomId == stat.RoomId, 
                                    cancellationToken);

            if (existing != null)
            {
                // Update existing record
                existing.RoomName = stat.RoomName;
                existing.BookedMinutes = stat.BookedMinutes;
                existing.OccupancyRate = stat.OccupancyRate;
                existing.BookingCount = stat.BookingCount;
                existing.LastComputedAt = stat.LastComputedAt;
                
                SetEntityState(existing, EntityState.Modified);
            }
            else
            {
                // Insert new record
                await GetDbSet<HourlyStat>().AddAsync(stat, cancellationToken);
            }
        }

        public async Task BulkUpsertHourlyStatsAsync(List<HourlyStat> stats, CancellationToken cancellationToken = default)
        {
            foreach (var stat in stats)
            {
                await UpsertHourlyStatAsync(stat, cancellationToken);
            }
        }

        #endregion

        #region MetricsComputationLog Operations

        public async Task<MetricsComputationLog> GetLastSuccessfulComputationAsync(string metricType, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<MetricsComputationLog>()
                .AsNoTracking()
                .Where(mcl => mcl.MetricType == metricType && mcl.Status == "Success")
                .OrderByDescending(mcl => mcl.CompletedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddComputationLogAsync(MetricsComputationLog log, CancellationToken cancellationToken = default)
        {
            await GetDbSet<MetricsComputationLog>().AddAsync(log, cancellationToken);
        }

        public async Task UpdateComputationLogAsync(MetricsComputationLog log, CancellationToken cancellationToken = default)
        {
            SetEntityState(log, EntityState.Modified);
        }

        public async Task<List<MetricsComputationLog>> GetComputationLogsAsync(string metricType, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<MetricsComputationLog>()
                .AsNoTracking()
                .Where(mcl => mcl.MetricType == metricType && 
                             mcl.ComputationDate >= startDate && 
                             mcl.ComputationDate <= endDate)
                .OrderByDescending(mcl => mcl.StartedAt)
                .ToListAsync(cancellationToken);
        }

        #endregion
    }
}
