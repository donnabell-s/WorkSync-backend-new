using ASI.Basecode.Data.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Service interface for metrics computation and retrieval.
    /// This service is responsible for computing dashboard metrics from raw data
    /// and storing them in optimized summary tables. It also provides cached access
    /// to precomputed metrics for fast dashboard rendering.
    /// </summary>
    public interface IMetricsService
    {
        /// <summary>
        /// Compute and store metrics for a specific date.
        /// This method reads raw booking and room data, computes all metrics,
        /// and stores them in the summary tables.
        /// </summary>
        /// <param name="date">The date to compute metrics for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ComputeMetricsForDateAsync(DateTime date, CancellationToken cancellationToken = default);

        /// <summary>
        /// Compute and store metrics for a date range.
        /// Useful for backfilling historical data or initial setup.
        /// </summary>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ComputeMetricsForDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get optimized dashboard data for a specific date.
        /// This method returns precomputed, cached data for ultra-fast response times.
        /// </summary>
        /// <param name="date">The date to get dashboard data for (defaults to today)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Complete dashboard data including summary, trends, and peak usage</returns>
        Task<DashboardDataViewModel> GetDashboardDataAsync(DateTime? date = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get bookings trend data for a date range.
        /// Returns precomputed data from DailySummaries table.
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<DashboardTrendDataViewModel> GetTrendDataAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get peak usage heatmap data for a specific date.
        /// Returns precomputed data from HourlyStats table.
        /// </summary>
        /// <param name="date">The date to get peak usage data for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<DashboardPeakUsageDataViewModel> GetPeakUsageDataAsync(DateTime date, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clear all cached dashboard data.
        /// Useful when manual recalculation is needed.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Clear cache for a specific date.
        /// </summary>
        void ClearCacheForDate(DateTime date);
    }
}
