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
}