using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Hourly statistics table for detailed time-based analytics.
    /// Stores room occupancy rates for each hour of each day.
    /// Used for heatmap visualizations and peak usage analysis.
    /// Updated by the MetricsComputationHostedService every 5 minutes.
    /// </summary>
    [Table("HourlyStats", Schema = "ws")]
    public class HourlyStat
    {
        /// <summary>
        /// Unique identifier for the hourly stat record
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The date for which this stat is recorded
        /// </summary>
        [Required]
        public DateTime StatDate { get; set; }

        /// <summary>
        /// Hour of the day (0-23)
        /// </summary>
        [Required]
        [Range(0, 23)]
        public int Hour { get; set; }

        /// <summary>
        /// Room identifier
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RoomId { get; set; }

        /// <summary>
        /// Room code (denormalized for faster queries and frontend display)
        /// </summary>
        [MaxLength(50)]
        public string RoomCode { get; set; }

        /// <summary>
        /// Room name (denormalized for faster queries)
        /// </summary>
        [MaxLength(200)]
        public string RoomName { get; set; }

        /// <summary>
        /// Number of minutes the room was booked during this hour (0-60)
        /// </summary>
        public int BookedMinutes { get; set; }

        /// <summary>
        /// Occupancy rate percentage for this hour (0-100)
        /// Calculated as: (BookedMinutes / 60) * 100
        /// </summary>
        public double OccupancyRate { get; set; }

        /// <summary>
        /// Number of bookings during this hour
        /// </summary>
        public int BookingCount { get; set; }

        /// <summary>
        /// When this stat was last computed
        /// </summary>
        public DateTime LastComputedAt { get; set; }

        /// <summary>
        /// Navigation property to Room
        /// </summary>
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        /// <summary>
        /// Version number for optimistic concurrency control
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
