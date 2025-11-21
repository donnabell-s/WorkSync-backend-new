using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Tracks the last computation time for different metric types.
    /// This is used by the MetricsComputationHostedService to determine
    /// which data needs to be recomputed based on changes since last run.
    /// </summary>
    [Table("MetricsComputationLog", Schema = "ws")]
    public class MetricsComputationLog
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Type of metrics computed (e.g., "DailySummary", "HourlyStat")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string MetricType { get; set; }

        /// <summary>
        /// The date for which metrics were computed
        /// </summary>
        [Required]
        public DateTime ComputationDate { get; set; }

        /// <summary>
        /// When the computation started
        /// </summary>
        [Required]
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// When the computation completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Status of the computation (Success, Failed, Running)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        /// <summary>
        /// Number of records processed
        /// </summary>
        public int RecordsProcessed { get; set; }

        /// <summary>
        /// Error message if computation failed
        /// </summary>
        [MaxLength(4000)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Duration of the computation in milliseconds
        /// </summary>
        public long? DurationMs { get; set; }
    }
}
