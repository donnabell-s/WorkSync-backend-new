using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Daily summary table for precomputed dashboard metrics.
    /// This table stores aggregated data per day to avoid expensive real-time calculations.
    /// Updated by the MetricsComputationHostedService every 5 minutes.
    /// </summary>
    [Table("DailySummaries", Schema = "ws")]
    public class DailySummary
    {
        /// <summary>
        /// Unique identifier for the daily summary record
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The date for which this summary is calculated (date only, no time component)
        /// </summary>
        [Required]
        public DateTime SummaryDate { get; set; }

        /// <summary>
        /// Total number of bookings created on this date
        /// </summary>
        public int TotalBookings { get; set; }

        /// <summary>
        /// Number of bookings completed on this date
        /// </summary>
        public int CompletedBookings { get; set; }

        /// <summary>
        /// Number of bookings currently ongoing (calculated at computation time)
        /// </summary>
        public int OngoingBookings { get; set; }

        /// <summary>
        /// Number of rooms marked as available/active on this date
        /// </summary>
        public int AvailableRooms { get; set; }

        /// <summary>
        /// Number of rooms under maintenance on this date
        /// </summary>
        public int MaintenanceRooms { get; set; }

        /// <summary>
        /// Total minutes of all bookings on this date
        /// </summary>
        public int TotalBookedMinutes { get; set; }

        /// <summary>
        /// Total available minutes (rooms * operating hours)
        /// </summary>
        public int TotalAvailableMinutes { get; set; }

        /// <summary>
        /// Utilization rate percentage (0-100)
        /// Calculated as: (TotalBookedMinutes / TotalAvailableMinutes) * 100
        /// </summary>
        public double UtilizationRate { get; set; }

        /// <summary>
        /// When this summary was last computed
        /// </summary>
        public DateTime LastComputedAt { get; set; }

        /// <summary>
        /// Version number for optimistic concurrency control
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
