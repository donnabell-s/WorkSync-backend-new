using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Dashboard service implementation
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IDashboardRepository dashboardRepository,
            IUnitOfWork unitOfWork,
            ILoggerFactory loggerFactory)
        {
            _dashboardRepository = dashboardRepository;
            _unitOfWork = unitOfWork;
            _logger = loggerFactory.CreateLogger<DashboardService>();
        }

        /// <summary>
        /// Get summary metrics for dashboard KPI cards
        /// </summary>
        public async Task<DashboardSummaryViewModel> GetDashboardSummaryAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var targetDate = date ?? DateTime.Today;
                return await _dashboardRepository.GetDashboardSummaryAsync(targetDate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary for date {Date}", date);
                throw;
            }
        }

        /// <summary>
        /// Get bookings trend data for specified date range
        /// </summary>
        public async Task<List<BookingsTrendViewModel>> GetBookingsTrendAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate date range
                if (startDate > endDate)
                    throw new ArgumentException("Start date cannot be greater than end date");

                // Limit to reasonable range to prevent performance issues
                var daysDiff = (endDate - startDate).Days;
                if (daysDiff > 90) // Max 3 months
                    throw new ArgumentException("Date range cannot exceed 90 days");

                return await _dashboardRepository.GetBookingsTrendAsync(startDate, endDate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings trend for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        /// <summary>
        /// Get peak usage heatmap data for specified date
        /// </summary>
        public async Task<List<PeakUsageViewModel>> GetPeakUsageAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var targetDate = date ?? DateTime.Today;
                return await _dashboardRepository.GetPeakUsageAsync(targetDate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting peak usage for date {Date}", date);
                throw;
            }
        }
    }
}