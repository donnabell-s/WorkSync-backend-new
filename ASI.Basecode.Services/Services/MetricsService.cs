using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service implementation for metrics computation and optimized dashboard data retrieval.
    /// This service computes metrics from raw data and stores them in summary tables.
    /// It uses MemoryCache for ultra-fast dashboard response times (&lt;50ms).
    /// </summary>
    public class MetricsService : IMetricsService
    {
        private readonly IMetricsRepository _metricsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MetricsService> _logger;

        // Cache keys
        private const string DASHBOARD_CACHE_KEY_PREFIX = "Dashboard_";
        private const string TREND_CACHE_KEY_PREFIX = "Trend_";
        private const string PEAK_USAGE_CACHE_KEY_PREFIX = "PeakUsage_";
        
        // Cache expiration (5 minutes to align with hosted service interval)
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

        public MetricsService(
            IMetricsRepository metricsRepository,
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILogger<MetricsService> logger)
        {
            _metricsRepository = metricsRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        #region Computation Methods

        public async Task ComputeMetricsForDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var targetDate = date.Date;
            var logEntry = new MetricsComputationLog
            {
                MetricType = "DailyMetrics",
                ComputationDate = targetDate,
                StartedAt = DateTime.UtcNow,
                Status = "Running"
            };

            try
            {
                _logger.LogInformation("Starting metrics computation for date: {Date}", targetDate);

                await _metricsRepository.AddComputationLogAsync(logEntry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Compute daily summary
                var dailySummary = await ComputeDailySummaryAsync(targetDate, cancellationToken);
                await _metricsRepository.UpsertDailySummaryAsync(dailySummary, cancellationToken);

                // Compute hourly stats
                var hourlyStats = await ComputeHourlyStatsAsync(targetDate, cancellationToken);
                await _metricsRepository.BulkUpsertHourlyStatsAsync(hourlyStats, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Update log entry
                stopwatch.Stop();
                logEntry.Status = "Success";
                logEntry.CompletedAt = DateTime.UtcNow;
                logEntry.DurationMs = stopwatch.ElapsedMilliseconds;
                logEntry.RecordsProcessed = 1 + hourlyStats.Count;

                await _metricsRepository.UpdateComputationLogAsync(logEntry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Clear cache for this date
                ClearCacheForDate(targetDate);

                _logger.LogInformation(
                    "Completed metrics computation for {Date} in {Duration}ms. Processed {Records} records.",
                    targetDate, stopwatch.ElapsedMilliseconds, logEntry.RecordsProcessed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logEntry.Status = "Failed";
                logEntry.CompletedAt = DateTime.UtcNow;
                logEntry.DurationMs = stopwatch.ElapsedMilliseconds;
                logEntry.ErrorMessage = ex.Message;

                try
                {
                    await _metricsRepository.UpdateComputationLogAsync(logEntry, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Failed to update computation log after error");
                }

                _logger.LogError(ex, "Failed to compute metrics for date: {Date}", targetDate);
                throw;
            }
        }

        public async Task ComputeMetricsForDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate.Date;
            var end = endDate.Date;

            _logger.LogInformation("Starting metrics computation for date range: {StartDate} to {EndDate}", start, end);

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Metrics computation cancelled at date: {Date}", date);
                    break;
                }

                await ComputeMetricsForDateAsync(date, cancellationToken);
            }

            _logger.LogInformation("Completed metrics computation for date range: {StartDate} to {EndDate}", start, end);
        }

        /// <summary>
        /// Compute daily summary metrics from raw booking and room data
        /// </summary>
        private async Task<DailySummary> ComputeDailySummaryAsync(DateTime date, CancellationToken cancellationToken)
        {
            var targetDate = date.Date;
            var now = DateTime.Now;

            var db = _unitOfWork.Database;

            // Count available rooms
            var availableRooms = await db.Set<Room>()
                .AsNoTracking()
                .CountAsync(r => r.Status != null && (
                    r.Status.ToLower() == "active" ||
                    r.Status.ToLower() == "available"
                ), cancellationToken);

            // Count maintenance rooms
            var maintenanceRooms = await db.Set<Room>()
                .AsNoTracking()
                .CountAsync(r => r.Status != null && r.Status.ToLower() == "maintenance", cancellationToken);

            // Count total bookings for the date
            var totalBookings = await db.Set<Booking>()
                .AsNoTracking()
                .CountAsync(b => b.StartDatetime.HasValue && b.StartDatetime.Value.Date == targetDate, cancellationToken);

            // Count completed bookings
            var completedBookings = await db.Set<Booking>()
                .AsNoTracking()
                .CountAsync(b => b.EndDatetime.HasValue && b.EndDatetime.Value.Date == targetDate, cancellationToken);

            // Count ongoing bookings (only relevant if computing for today)
            var ongoingBookings = 0;
            if (targetDate.Date == DateTime.Today)
            {
                ongoingBookings = await db.Set<Booking>()
                    .AsNoTracking()
                    .CountAsync(b => b.StartDatetime.HasValue && b.EndDatetime.HasValue &&
                                    b.StartDatetime.Value <= now && b.EndDatetime.Value >= now &&
                                    b.Status != null && b.Status.ToLower() != "declined", cancellationToken);
            }

            // Calculate total booked minutes
            var totalBookedMinutes = await db.Set<Booking>()
                .AsNoTracking()
                .Where(b => b.StartDatetime.HasValue && b.EndDatetime.HasValue &&
                           b.StartDatetime.Value.Date == targetDate &&
                           b.Status != null && b.Status.ToLower() != "declined")
                .SumAsync(b => EF.Functions.DateDiffMinute(b.StartDatetime.Value, b.EndDatetime.Value), cancellationToken);

            // Calculate total available minutes (8 hours per room = 480 minutes)
            var totalRooms = await db.Set<Room>()
                .AsNoTracking()
                .CountAsync(r => r.Status != null && r.Status.ToLower() != "maintenance", cancellationToken);

            var totalAvailableMinutes = totalRooms * 480;
            var utilizationRate = totalAvailableMinutes > 0
                ? Math.Round((double)totalBookedMinutes / totalAvailableMinutes * 100, 2)
                : 0.0;

            return new DailySummary
            {
                SummaryDate = targetDate,
                TotalBookings = totalBookings,
                CompletedBookings = completedBookings,
                OngoingBookings = ongoingBookings,
                AvailableRooms = availableRooms,
                MaintenanceRooms = maintenanceRooms,
                TotalBookedMinutes = totalBookedMinutes,
                TotalAvailableMinutes = totalAvailableMinutes,
                UtilizationRate = utilizationRate,
                LastComputedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Compute hourly statistics for all rooms
        /// </summary>
        private async Task<List<HourlyStat>> ComputeHourlyStatsAsync(DateTime date, CancellationToken cancellationToken)
        {
            var targetDate = date.Date;
            var db = _unitOfWork.Database;

            // Get all active rooms
            var rooms = await db.Set<Room>()
                .AsNoTracking()
                .Where(r => r.Status != null && r.Status.ToLower() != "maintenance")
                .Select(r => new { r.RoomId, r.Name })
                .ToListAsync(cancellationToken);

            // Get all bookings for the date
            var bookings = await db.Set<Booking>()
                .AsNoTracking()
                .Where(b => b.StartDatetime.HasValue && b.EndDatetime.HasValue &&
                           b.StartDatetime.Value.Date == targetDate &&
                           b.Status != null && b.Status.ToLower() != "declined")
                .Select(b => new
                {
                    b.RoomId,
                    StartTime = b.StartDatetime.Value,
                    EndTime = b.EndDatetime.Value
                })
                .ToListAsync(cancellationToken);

            var hourlyStats = new List<HourlyStat>();

            // Calculate stats for each room and each hour
            foreach (var room in rooms)
            {
                var roomBookings = bookings.Where(b => b.RoomId == room.RoomId).ToList();

                for (int hour = 0; hour < 24; hour++)
                {
                    var hourStart = targetDate.AddHours(hour);
                    var hourEnd = hourStart.AddHours(1);

                    // Calculate booked minutes during this hour
                    var bookedMinutes = roomBookings.Sum(b =>
                    {
                        var overlapStart = b.StartTime < hourStart ? hourStart : b.StartTime;
                        var overlapEnd = b.EndTime > hourEnd ? hourEnd : b.EndTime;

                        if (overlapStart >= overlapEnd)
                            return 0;

                        return (int)(overlapEnd - overlapStart).TotalMinutes;
                    });

                    var bookingCount = roomBookings.Count(b =>
                        b.StartTime < hourEnd && b.EndTime > hourStart);

                    var occupancyRate = Math.Round((double)bookedMinutes / 60.0 * 100, 2);

                    hourlyStats.Add(new HourlyStat
                    {
                        StatDate = targetDate,
                        Hour = hour,
                        RoomId = room.RoomId,
                        RoomName = room.Name,
                        BookedMinutes = bookedMinutes,
                        OccupancyRate = occupancyRate,
                        BookingCount = bookingCount,
                        LastComputedAt = DateTime.UtcNow
                    });
                }
            }

            return hourlyStats;
        }

        #endregion

        #region Retrieval Methods (Cached)

        public async Task<DashboardDataViewModel> GetDashboardDataAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            var targetDate = (date ?? DateTime.Today).Date;
            var cacheKey = $"{DASHBOARD_CACHE_KEY_PREFIX}{targetDate:yyyyMMdd}";

            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out DashboardDataViewModel cachedData))
            {
                _logger.LogDebug("Dashboard data retrieved from cache for date: {Date}", targetDate);
                cachedData.FromCache = true;
                return cachedData;
            }

            // If not in cache, get from database
            _logger.LogDebug("Dashboard data not in cache, retrieving from database for date: {Date}", targetDate);

            var dailySummary = await _metricsRepository.GetDailySummaryAsync(targetDate, cancellationToken);
            if (dailySummary == null)
            {
                // If no precomputed data exists, compute it now
                _logger.LogWarning("No precomputed data found for date: {Date}. Computing now...", targetDate);
                await ComputeMetricsForDateAsync(targetDate, cancellationToken);
                dailySummary = await _metricsRepository.GetDailySummaryAsync(targetDate, cancellationToken);
            }

            // Get trend data (last 30 days)
            var trendStartDate = targetDate.AddDays(-29);
            var trends = await _metricsRepository.GetDailySummariesAsync(trendStartDate, targetDate, cancellationToken);

            // Get hourly stats for peak usage
            var hourlyStats = await _metricsRepository.GetHourlyStatsAsync(targetDate, cancellationToken);

            var dashboardData = new DashboardDataViewModel
            {
                Summary = MapToSummaryViewModel(dailySummary),
                Trends = MapToTrendViewModels(trends),
                PeakUsage = MapToPeakUsageViewModels(hourlyStats),
                LastComputedAt = dailySummary?.LastComputedAt ?? DateTime.UtcNow,
                FromCache = false
            };

            // Store in cache
            _cache.Set(cacheKey, dashboardData, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                Priority = CacheItemPriority.High
            });

            return dashboardData;
        }

        public async Task<DashboardTrendDataViewModel> GetTrendDataAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            var cacheKey = $"{TREND_CACHE_KEY_PREFIX}{start:yyyyMMdd}_{end:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out DashboardTrendDataViewModel cachedData))
            {
                _logger.LogDebug("Trend data retrieved from cache for range: {Start} to {End}", start, end);
                cachedData.FromCache = true;
                return cachedData;
            }

            var summaries = await _metricsRepository.GetDailySummariesAsync(start, end, cancellationToken);

            var trendData = new DashboardTrendDataViewModel
            {
                Trends = MapToTrendViewModels(summaries),
                LastComputedAt = summaries.Any() ? summaries.Max(s => s.LastComputedAt) : DateTime.UtcNow,
                FromCache = false
            };

            _cache.Set(cacheKey, trendData, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                Priority = CacheItemPriority.Normal
            });

            return trendData;
        }

        public async Task<DashboardPeakUsageDataViewModel> GetPeakUsageDataAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var targetDate = date.Date;
            var cacheKey = $"{PEAK_USAGE_CACHE_KEY_PREFIX}{targetDate:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out DashboardPeakUsageDataViewModel cachedData))
            {
                _logger.LogDebug("Peak usage data retrieved from cache for date: {Date}", targetDate);
                cachedData.FromCache = true;
                return cachedData;
            }

            var hourlyStats = await _metricsRepository.GetHourlyStatsAsync(targetDate, cancellationToken);

            var peakUsageData = new DashboardPeakUsageDataViewModel
            {
                PeakUsage = MapToPeakUsageViewModels(hourlyStats),
                LastComputedAt = hourlyStats.Any() ? hourlyStats.Max(s => s.LastComputedAt) : DateTime.UtcNow,
                FromCache = false
            };

            _cache.Set(cacheKey, peakUsageData, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                Priority = CacheItemPriority.Normal
            });

            return peakUsageData;
        }

        #endregion

        #region Cache Management

        public void ClearCache()
        {
            _logger.LogInformation("Clearing all dashboard cache");
            // Note: MemoryCache doesn't have a built-in clear all method
            // In production, consider using a cache key registry pattern
        }

        public void ClearCacheForDate(DateTime date)
        {
            var targetDate = date.Date;
            var dashboardKey = $"{DASHBOARD_CACHE_KEY_PREFIX}{targetDate:yyyyMMdd}";
            var peakUsageKey = $"{PEAK_USAGE_CACHE_KEY_PREFIX}{targetDate:yyyyMMdd}";

            _cache.Remove(dashboardKey);
            _cache.Remove(peakUsageKey);

            _logger.LogDebug("Cleared cache for date: {Date}", targetDate);
        }

        #endregion

        #region Mapping Helpers

        private DashboardSummaryViewModel MapToSummaryViewModel(DailySummary summary)
        {
            if (summary == null)
            {
                return new DashboardSummaryViewModel
                {
                    AvailableRooms = 0,
                    RoomsUnderMaintenance = 0,
                    TodaysBookings = 0,
                    OngoingBookings = 0,
                    BookingsCompletedToday = 0,
                    UtilizationRateToday = 0
                };
            }

            return new DashboardSummaryViewModel
            {
                AvailableRooms = summary.AvailableRooms,
                RoomsUnderMaintenance = summary.MaintenanceRooms,
                TodaysBookings = summary.TotalBookings,
                OngoingBookings = summary.OngoingBookings,
                BookingsCompletedToday = summary.CompletedBookings,
                UtilizationRateToday = summary.UtilizationRate
            };
        }

        private List<BookingsTrendViewModel> MapToTrendViewModels(List<DailySummary> summaries)
        {
            return summaries.Select(s => new BookingsTrendViewModel
            {
                Date = s.SummaryDate,
                BookingsCount = s.TotalBookings,
                UtilizationPercentage = s.UtilizationRate
            }).ToList();
        }

        private List<PeakUsageViewModel> MapToPeakUsageViewModels(List<HourlyStat> stats)
        {
            return stats.Select(s => new PeakUsageViewModel
            {
                RoomName = s.RoomName,
                Hour = s.Hour,
                OccupancyRate = s.OccupancyRate
            }).ToList();
        }

        #endregion
    }
}
