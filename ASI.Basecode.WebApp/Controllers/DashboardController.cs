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
    /// Dashboard controller for admin dashboard metrics and visualizations
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = "RequireAdmin")] // Only admin and superadmin can access dashboard
    public class DashboardController : ASI.Basecode.WebApp.Mvc.ControllerBase<DashboardController>
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IDashboardService dashboardService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get dashboard summary metrics for KPI cards
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
        /// Get bookings trend data for trend graph
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
        /// Get peak usage heatmap data
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
        /// Alternative POST endpoint for bookings trend with request body
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
        /// Alternative POST endpoint for peak usage with request body
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
}