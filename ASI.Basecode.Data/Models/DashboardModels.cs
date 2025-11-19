using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Dashboard summary metrics for KPI cards
    /// </summary>
    public class DashboardSummaryViewModel
    {
        public int AvailableRooms { get; set; }
        public int RoomsUnderMaintenance { get; set; }
        public int TodaysBookings { get; set; }
        public int OngoingBookings { get; set; }
        public int BookingsCompletedToday { get; set; }
        public double UtilizationRateToday { get; set; }
    }

    /// <summary>
    /// Bookings trend data point for trend graph
    /// </summary>
    public class BookingsTrendViewModel
    {
        public DateTime Date { get; set; }
        public int BookingsCount { get; set; }
        public double UtilizationPercentage { get; set; }
    }

    /// <summary>
    /// Peak usage data point for heatmap
    /// </summary>
    public class PeakUsageViewModel
    {
        public string RoomName { get; set; }
        public int Hour { get; set; }
        public double OccupancyRate { get; set; }
    }

    /// <summary>
    /// Request model for trend graph with date range
    /// </summary>
    public class BookingsTrendRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Request model for peak usage heatmap
    /// </summary>
    public class PeakUsageRequest
    {
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Complete dashboard data view model with all precomputed metrics.
    /// This is the main response model for the optimized dashboard endpoint.
    /// </summary>
    public class DashboardDataViewModel
    {
        /// <summary>
        /// Summary metrics for the requested date
        /// </summary>
        public DashboardSummaryViewModel Summary { get; set; }

        /// <summary>
        /// Trend data (last 30 days by default)
        /// </summary>
        public List<BookingsTrendViewModel> Trends { get; set; }

        /// <summary>
        /// Peak usage heatmap data
        /// </summary>
        public List<PeakUsageViewModel> PeakUsage { get; set; }

        /// <summary>
        /// When this data was last computed
        /// </summary>
        public DateTime LastComputedAt { get; set; }

        /// <summary>
        /// Whether the data came from cache
        /// </summary>
        public bool FromCache { get; set; }
    }

    /// <summary>
    /// Trend data response model
    /// </summary>
    public class DashboardTrendDataViewModel
    {
        public List<BookingsTrendViewModel> Trends { get; set; }
        public DateTime LastComputedAt { get; set; }
        public bool FromCache { get; set; }
    }

    /// <summary>
    /// Peak usage data response model
    /// </summary>
    public class DashboardPeakUsageDataViewModel
    {
        public List<PeakUsageViewModel> PeakUsage { get; set; }
        public DateTime LastComputedAt { get; set; }
        public bool FromCache { get; set; }
    }
}