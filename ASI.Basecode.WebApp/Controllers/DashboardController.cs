using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Optimized Dashboard controller for ultra-fast admin dashboard metrics and visualizations.
    /// This controller serves precomputed, cached data for maximum performance.
    /// 
    /// Performance Characteristics:
    /// - Response time: &lt;50ms (from cache)
    /// - Data freshness: Updated every 5 minutes by background service
    /// - No expensive real-time calculations
    /// - Horizontal scalability ready
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = "RequireAdmin")] // Only admin and superadmin can access dashboard
    public class DashboardController : ASI.Basecode.WebApp.Mvc.ControllerBase<DashboardController>
    {
        private readonly IDashboardService _dashboardService;
        private readonly IMetricsService _metricsService;

        public DashboardController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IDashboardService dashboardService,
            IMetricsService metricsService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _dashboardService = dashboardService;
            _metricsService = metricsService;
        }

        /// <summary>
        /// Get complete optimized dashboard data (summary + trends + peak usage).
        /// This is the main endpoint for the React frontend dashboard.
        /// Returns precomputed, cached data for ultra-fast response (&lt;50ms).
        /// 
        /// GET: api/Dashboard/GetOptimizedDashboard?date=2024-01-15
        /// </summary>
        /// <param name="date">Optional date (defaults to today)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Complete dashboard data with all metrics</returns>
        [HttpGet]
        public async Task<IActionResult> GetOptimizedDashboard([FromQuery] DateTime? date, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _metricsService.GetDashboardDataAsync(date, cancellationToken);
                
                return Ok(new
                {
                    success = true,
                    data,
                    message = data.FromCache ? "Data retrieved from cache" : "Data retrieved from database"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get optimized dashboard data");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to get dashboard data",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get bookings trend data for a specified date range.
        /// Returns precomputed data from DailySummaries table.
        /// 
        /// GET: api/Dashboard/GetOptimizedTrend?startDate=2024-01-01&endDate=2024-01-31
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOptimizedTrend(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken cancellationToken)
        {
            try
            {
                if (startDate == default || endDate == default)
                {
                    return BadRequest(new { success = false, message = "Both startDate and endDate are required" });
                }

                if ((endDate - startDate).Days > 90)
                {
                    return BadRequest(new { success = false, message = "Date range cannot exceed 90 days" });
                }

                var data = await _metricsService.GetTrendDataAsync(startDate, endDate, cancellationToken);
                
                return Ok(new
                {
                    success = true,
                    data,
                    message = data.FromCache ? "Data retrieved from cache" : "Data retrieved from database"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get optimized trend data");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to get trend data",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get peak usage heatmap data for a specified date.
        /// Returns precomputed data from HourlyStats table.
        /// 
        /// GET: api/Dashboard/GetOptimizedPeakUsage?date=2024-01-15
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOptimizedPeakUsage([FromQuery] DateTime? date, CancellationToken cancellationToken)
        {
            try
            {
                var targetDate = date ?? DateTime.Today;
                var data = await _metricsService.GetPeakUsageDataAsync(targetDate, cancellationToken);
                
                return Ok(new
                {
                    success = true,
                    data,
                    message = data.FromCache ? "Data retrieved from cache" : "Data retrieved from database"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get optimized peak usage data");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to get peak usage data",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Force recompute metrics for a specific date.
        /// This is an admin utility endpoint for manual recalculation.
        /// 
        /// POST: api/Dashboard/RecomputeMetrics
        /// Body: { "date": "2024-01-15" }
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RecomputeMetrics([FromBody] RecomputeMetricsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null || request.Date == default)
                {
                    return BadRequest(new { success = false, message = "Date is required" });
                }

                await _metricsService.ComputeMetricsForDateAsync(request.Date, cancellationToken);
                
                return Ok(new
                {
                    success = true,
                    message = $"Metrics recomputed successfully for {request.Date:yyyy-MM-dd}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recompute metrics");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to recompute metrics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Backfill metrics for a date range.
        /// Useful for initial setup or historical data computation.
        /// 
        /// POST: api/Dashboard/BackfillMetrics
        /// Body: { "startDate": "2024-01-01", "endDate": "2024-01-31" }
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> BackfillMetrics([FromBody] BackfillMetricsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null || request.StartDate == default || request.EndDate == default)
                {
                    return BadRequest(new { success = false, message = "StartDate and EndDate are required" });
                }

                if ((request.EndDate - request.StartDate).Days > 365)
                {
                    return BadRequest(new { success = false, message = "Date range cannot exceed 365 days" });
                }

                await _metricsService.ComputeMetricsForDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);
                
                return Ok(new
                {
                    success = true,
                    message = $"Metrics backfilled successfully for {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to backfill metrics");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to backfill metrics",
                    message = ex.Message
                });
            }
        }

        // ========== Legacy Endpoints (for backward compatibility) =========
        // These endpoints use the old real-time calculation approach
        // Consider deprecating these once frontend is migrated to optimized endpoints

        /// <summary>
        /// Get dashboard summary metrics for KPI cards (LEGACY - use GetOptimizedDashboard instead)
        /// GET: api/Dashboard/Summary?date=2024-01-15
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Summary([FromQuery] DateTime? date, CancellationToken cancellationToken)
        {
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync(date, cancellationToken);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                var errorDetails = new List<string>();
                var currentEx = ex;
                while (currentEx != null)
                {
                    errorDetails.Add(currentEx.Message);
                    currentEx = currentEx.InnerException;
                }

                return StatusCode(500, new { error = "Failed to get dashboard summary", details = errorDetails });
            }
        }

        /// <summary>
        /// Get bookings trend data for trend graph (LEGACY - use GetOptimizedTrend instead)
        /// GET: api/Dashboard/BookingsTrend?startDate=2024-01-01&endDate=2024-01-31
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BookingsTrend(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate required parameters
                if (startDate == default || endDate == default)
                {
                    return BadRequest(new { message = "Both startDate and endDate are required" });
                }

                var trendData = await _dashboardService.GetBookingsTrendAsync(startDate, endDate, cancellationToken);
                return Ok(trendData);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var errorDetails = new List<string>();
                var currentEx = ex;
                while (currentEx != null)
                {
                    errorDetails.Add(currentEx.Message);
                    currentEx = currentEx.InnerException;
                }

                return StatusCode(500, new { error = "Failed to get bookings trend data", details = errorDetails });
            }
        }

        /// <summary>
        /// Get peak usage heatmap data (LEGACY - use GetOptimizedPeakUsage instead)
        /// GET: api/Dashboard/PeakUsage?date=2024-01-15
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PeakUsage([FromQuery] DateTime? date, CancellationToken cancellationToken)
        {
            try
            {
                var peakUsageData = await _dashboardService.GetPeakUsageAsync(date, cancellationToken);
                return Ok(peakUsageData);
            }
            catch (Exception ex)
            {
                var errorDetails = new List<string>();
                var currentEx = ex;
                while (currentEx != null)
                {
                    errorDetails.Add(currentEx.Message);
                    currentEx = currentEx.InnerException;
                }

                return StatusCode(500, new { error = "Failed to get peak usage data", details = errorDetails });
            }
        }

        /// <summary>
        /// Alternative POST endpoint for bookings trend with request body (LEGACY)
        /// POST: api/Dashboard/BookingsTrendPost
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BookingsTrendPost([FromBody] BookingsTrendRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                var trendData = await _dashboardService.GetBookingsTrendAsync(request.StartDate, request.EndDate, cancellationToken);
                return Ok(trendData);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var errorDetails = new List<string>();
                var currentEx = ex;
                while (currentEx != null)
                {
                    errorDetails.Add(currentEx.Message);
                    currentEx = currentEx.InnerException;
                }

                return StatusCode(500, new { error = "Failed to get bookings trend data", details = errorDetails });
            }
        }

        /// <summary>
        /// Alternative POST endpoint for peak usage with request body (LEGACY)
        /// POST: api/Dashboard/PeakUsagePost
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PeakUsagePost([FromBody] PeakUsageRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                var peakUsageData = await _dashboardService.GetPeakUsageAsync(request.Date, cancellationToken);
                return Ok(peakUsageData);
            }
            catch (Exception ex)
            {
                var errorDetails = new List<string>();
                var currentEx = ex;
                while (currentEx != null)
                {
                    errorDetails.Add(currentEx.Message);
                    currentEx = currentEx.InnerException;
                }

                return StatusCode(500, new { error = "Failed to get peak usage data", details = errorDetails });
            }
        }
    }

    // Request DTOs for the new endpoints
    public class RecomputeMetricsRequest
    {
        public DateTime Date { get; set; }
    }

    public class BackfillMetricsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}