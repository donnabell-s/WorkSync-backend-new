using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    /// <summary>
    /// Repository interface for precomputed metrics operations.
    /// This repository handles CRUD operations for summary tables used in dashboard optimization.
    /// </summary>
    public interface IMetricsRepository
    {
        // DailySummary operations
        /// <summary>
        /// Get daily summary for a specific date
        /// </summary>
        Task<DailySummary> GetDailySummaryAsync(DateTime date, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get daily summaries for a date range
        /// </summary>
        Task<List<DailySummary>> GetDailySummariesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Upsert (insert or update) a daily summary
        /// </summary>
        Task UpsertDailySummaryAsync(DailySummary summary, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk upsert daily summaries
        /// </summary>
        Task BulkUpsertDailySummariesAsync(List<DailySummary> summaries, CancellationToken cancellationToken = default);

        // HourlyStat operations
        /// <summary>
        /// Get hourly stats for a specific date
        /// </summary>
        Task<List<HourlyStat>> GetHourlyStatsAsync(DateTime date, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get hourly stats for a specific date and room
        /// </summary>
        Task<List<HourlyStat>> GetHourlyStatsByRoomAsync(DateTime date, string roomId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Upsert hourly stat
        /// </summary>
        Task UpsertHourlyStatAsync(HourlyStat stat, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk upsert hourly stats
        /// </summary>
        Task BulkUpsertHourlyStatsAsync(List<HourlyStat> stats, CancellationToken cancellationToken = default);

        // MetricsComputationLog operations
        /// <summary>
        /// Get the last successful computation log for a specific metric type
        /// </summary>
        Task<MetricsComputationLog> GetLastSuccessfulComputationAsync(string metricType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add a new computation log entry
        /// </summary>
        Task AddComputationLogAsync(MetricsComputationLog log, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing computation log entry
        /// </summary>
        Task UpdateComputationLogAsync(MetricsComputationLog log, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get computation logs for a specific date range and metric type
        /// </summary>
        Task<List<MetricsComputationLog>> GetComputationLogsAsync(string metricType, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
