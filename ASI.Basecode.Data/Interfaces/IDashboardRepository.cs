using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    /// <summary>
    /// Repository interface for dashboard data operations
    /// </summary>
    public interface IDashboardRepository
    {
        /// <summary>
        /// Get summary metrics for dashboard KPI cards
        /// </summary>
        Task<DashboardSummaryViewModel> GetDashboardSummaryAsync(DateTime date, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get bookings trend data for specified date range
        /// </summary>
        Task<List<BookingsTrendViewModel>> GetBookingsTrendAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get peak usage heatmap data for specified date
        /// </summary>
        Task<List<PeakUsageViewModel>> GetPeakUsageAsync(DateTime date, CancellationToken cancellationToken = default);
    }
}