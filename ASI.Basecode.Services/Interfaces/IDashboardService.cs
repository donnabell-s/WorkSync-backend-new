using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Service interface for dashboard operations
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Get summary metrics for dashboard KPI cards
        /// </summary>
        Task<DashboardSummaryViewModel> GetDashboardSummaryAsync(DateTime? date = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get bookings trend data for specified date range
        /// </summary>
        Task<List<BookingsTrendViewModel>> GetBookingsTrendAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get peak usage heatmap data for specified date
        /// </summary>
        Task<List<PeakUsageViewModel>> GetPeakUsageAsync(DateTime? date = null, CancellationToken cancellationToken = default);
    }
}