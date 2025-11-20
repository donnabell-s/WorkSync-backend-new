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
using System.Text.Json;
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
        
        // Cache expiration (30 seconds to align with hosted service interval for demonstration)
        // NOTE: Change to TimeSpan.FromMinutes(5) for production use
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromSeconds(30);

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
                StartedAt = DateTime.Now,  // Changed from DateTime.UtcNow
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
                logEntry.CompletedAt = DateTime.Now;  // Changed from DateTime.UtcNow
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
                logEntry.CompletedAt = DateTime.Now;  // Changed from DateTime.UtcNow
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

            // Count available rooms (using standardized status: "Available")
            var availableRooms = await db.Set<Room>()
                .AsNoTracking()
                .CountAsync(r => r.Status == "available" || r.Status == "active", cancellationToken);

            // Count maintenance rooms (using standardized status: "Under Maintenance")
            var maintenanceRooms = await db.Set<Room>()
                .AsNoTracking()
                .CountAsync(r => r.Status == "maintenance", cancellationToken);

            // Get all approved bookings (including recurring ones)
            var allBookings = await db.Set<Booking>()
                .AsNoTracking()
                .Where(b => b.Status == "approved" && b.StartDatetime.HasValue && b.EndDatetime.HasValue)
                .Select(b => new
                {
                    b.BookingId,
                    b.StartDatetime,
                    b.EndDatetime,
                    b.Recurrence
                })
                .ToListAsync(cancellationToken);

            // Expand recurring bookings into individual occurrences for the target date
            var expandedBookings = new List<(DateTime Start, DateTime End)>();
            foreach (var booking in allBookings)
            {
                var occurrences = GenerateOccurrencesForDate(
                    booking.StartDatetime.Value, 
                    booking.EndDatetime.Value, 
                    booking.Recurrence, 
                    targetDate);
                expandedBookings.AddRange(occurrences);
            }

            // Count total bookings for the date (after expansion)
            var totalBookings = expandedBookings.Count(b => b.Start.Date == targetDate);

            // Count completed bookings (bookings that have ended)
            var completedBookings = expandedBookings.Count(b => 
                b.End.Date == targetDate && b.End <= now);

            // Count ongoing bookings (only relevant if computing for today)
            var ongoingBookings = 0;
            if (targetDate.Date == DateTime.Today)
            {
                ongoingBookings = expandedBookings.Count(b => 
                    b.Start <= now && b.End >= now);
            }

            // Calculate total booked minutes (only for bookings on target date)
            var totalBookedMinutes = expandedBookings
                .Where(b => b.Start.Date == targetDate)
                .Sum(b => (int)(b.End - b.Start).TotalMinutes);

            // Calculate total available minutes (8 hours per room = 480 minutes)
            // Only count rooms that are Available or Occupied (exclude Under Maintenance)
            var totalRooms = await db.Set<Room>()
                .AsNoTracking()
                .CountAsync(r => r.Status == "available" || r.Status == "active", cancellationToken);

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
                LastComputedAt = DateTime.Now  // Changed from DateTime.UtcNow
            };
        }

        /// <summary>
        /// Compute hourly statistics for all rooms
        /// </summary>
        private async Task<List<HourlyStat>> ComputeHourlyStatsAsync(DateTime date, CancellationToken cancellationToken)
        {
            var targetDate = date.Date;
            var db = _unitOfWork.Database;

            // Get all available and occupied rooms (exclude Under Maintenance)
            var rooms = await db.Set<Room>()
                .AsNoTracking()
                .Where(r => r.Status == "available" || r.Status == "active")
                .Select(r => new { r.RoomId, r.Code, r.Name })
                .ToListAsync(cancellationToken);

            // Get all approved bookings (including recurring)
            var allBookings = await db.Set<Booking>()
                .AsNoTracking()
                .Where(b => b.Status == "approved" && 
                           b.StartDatetime.HasValue && 
                           b.EndDatetime.HasValue)
                .Select(b => new
                {
                    b.RoomId,
                    b.StartDatetime,
                    b.EndDatetime,
                    b.Recurrence
                })
                .ToListAsync(cancellationToken);

            // Expand recurring bookings by room
            var bookingsByRoom = new Dictionary<string, List<(DateTime Start, DateTime End)>>();
            foreach (var booking in allBookings)
            {
                if (!bookingsByRoom.ContainsKey(booking.RoomId))
                    bookingsByRoom[booking.RoomId] = new List<(DateTime, DateTime)>();

                var occurrences = GenerateOccurrencesForDate(
                    booking.StartDatetime.Value,
                    booking.EndDatetime.Value,
                    booking.Recurrence,
                    targetDate);
                
                bookingsByRoom[booking.RoomId].AddRange(occurrences);
            }

            var hourlyStats = new List<HourlyStat>();

            // Calculate stats for each room and each hour
            foreach (var room in rooms)
            {
                var roomBookings = bookingsByRoom.ContainsKey(room.RoomId) 
                    ? bookingsByRoom[room.RoomId] 
                    : new List<(DateTime, DateTime)>();

                for (int hour = 0; hour < 24; hour++)
                {
                    var hourStart = targetDate.AddHours(hour);
                    var hourEnd = hourStart.AddHours(1);

                    // Calculate booked minutes during this hour
                    var bookedMinutes = roomBookings.Sum(b =>
                    {
                        var overlapStart = b.Item1 < hourStart ? hourStart : b.Item1;
                        var overlapEnd = b.Item2 > hourEnd ? hourEnd : b.Item2;

                        if (overlapStart >= overlapEnd)
                            return 0;

                        return (int)(overlapEnd - overlapStart).TotalMinutes;
                    });

                    var bookingCount = roomBookings.Count(b =>
                        b.Item1 < hourEnd && b.Item2 > hourStart);

                    var occupancyRate = Math.Round((double)bookedMinutes / 60.0 * 100, 2);

                    hourlyStats.Add(new HourlyStat
                    {
                        StatDate = targetDate,
                        Hour = hour,
                        RoomId = room.RoomId,
                        RoomCode = room.Code,
                        RoomName = room.Name,
                        BookedMinutes = bookedMinutes,
                        OccupancyRate = occupancyRate,
                        BookingCount = bookingCount,
                        LastComputedAt = DateTime.Now  // Changed from DateTime.UtcNow
                    });
                }
            }

            return hourlyStats;
        }

        /// <summary>
        /// Generate all occurrences of a booking (including recurring ones) that fall on the target date.
        /// </summary>
        /// <param name="originalStart">Original booking start datetime</param>
        /// <param name="originalEnd">Original booking end datetime</param>
        /// <param name="recurrenceJson">JSON string containing recurrence information</param>
        /// <param name="targetDate">The date to generate occurrences for</param>
        /// <returns>List of booking occurrences (start, end) on the target date</returns>
        private List<(DateTime Start, DateTime End)> GenerateOccurrencesForDate(
            DateTime originalStart, 
            DateTime originalEnd, 
            string recurrenceJson, 
            DateTime targetDate)
        {
            var occurrences = new List<(DateTime, DateTime)>();
            var targetDateOnly = targetDate.Date;

            // Parse recurrence information
            RecurrenceInfo recurrence = null;
            if (!string.IsNullOrWhiteSpace(recurrenceJson))
            {
                try
                {
                    recurrence = JsonSerializer.Deserialize<RecurrenceInfo>(recurrenceJson);
                }
                catch
                {
                    // Invalid JSON, treat as non-recurring
                    recurrence = null;
                }
            }

            // If not recurring or no recurrence info, check if original booking falls on target date
            if (recurrence == null || !recurrence.IsRecurring)
            {
                if (originalStart.Date == targetDateOnly || originalEnd.Date == targetDateOnly)
                {
                    occurrences.Add((originalStart, originalEnd));
                }
                return occurrences;
            }

            // For recurring bookings, generate occurrences
            var pattern = recurrence.Pattern?.ToLowerInvariant();
            var interval = recurrence.Interval ?? 1;
            var recurrenceEndDate = recurrence.EndDate ?? originalStart.AddMonths(6);
            
            // Only generate occurrences up to target date + 1 day
            var maxDate = targetDateOnly.AddDays(1);
            if (recurrenceEndDate > maxDate)
                recurrenceEndDate = maxDate;

            var duration = originalEnd - originalStart;
            var currentStart = originalStart;
            int iterationCount = 0;
            const int maxIterations = 730; // 2 years max to prevent infinite loops

            while (currentStart.Date <= recurrenceEndDate && iterationCount < maxIterations)
            {
                iterationCount++;
                var currentEnd = currentStart + duration;

                // Check if this occurrence falls on the target date
                if (currentStart.Date == targetDateOnly || currentEnd.Date == targetDateOnly)
                {
                    occurrences.Add((currentStart, currentEnd));
                }

                // If we've passed the target date, stop generating
                if (currentStart.Date > targetDateOnly)
                    break;

                // Generate next occurrence based on pattern
                if (pattern == "daily")
                {
                    currentStart = currentStart.AddDays(interval);
                }
                else if (pattern == "weekly")
                {
                    if (recurrence.DaysOfWeek != null && recurrence.DaysOfWeek.Any())
                    {
                        // Find next occurrence on one of the specified days of week
                        var currentWeekStart = currentStart.Date.AddDays(-(int)currentStart.DayOfWeek);
                        var foundNext = false;

                        // Check remaining days in current week
                        foreach (var dayOfWeek in recurrence.DaysOfWeek.OrderBy(d => d))
                        {
                            var targetDayOfWeek = dayOfWeek % 7;
                            var daysToAdd = (targetDayOfWeek - (int)currentStart.DayOfWeek + 7) % 7;
                            
                            if (daysToAdd > 0) // Only future days in this week
                            {
                                currentStart = currentStart.AddDays(daysToAdd);
                                foundNext = true;
                                break;
                            }
                        }

                        // If no more days this week, move to next week interval
                        if (!foundNext)
                        {
                            var nextWeekStart = currentWeekStart.AddDays(7 * interval);
                            var firstDayOfWeek = recurrence.DaysOfWeek.OrderBy(d => d).First() % 7;
                            var daysFromWeekStart = (firstDayOfWeek - (int)nextWeekStart.DayOfWeek + 7) % 7;
                            currentStart = nextWeekStart.AddDays(daysFromWeekStart).Add(originalStart.TimeOfDay);
                        }
                    }
                    else
                    {
                        // No specific days, just add interval weeks
                        currentStart = currentStart.AddDays(7 * interval);
                    }
                }
                else if (pattern == "monthly")
                {
                    currentStart = currentStart.AddMonths(interval);
                }
                else
                {
                    // Unknown pattern, stop
                    break;
                }
            }

            return occurrences;
        }

        /// <summary>
        /// Internal class for deserializing recurrence JSON
        /// </summary>
        private class RecurrenceInfo
        {
            public bool IsRecurring { get; set; }
            public string Pattern { get; set; }
            public int? Interval { get; set; }
            public List<int> DaysOfWeek { get; set; }
            public DateTime? EndDate { get; set; }
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
                LastComputedAt = dailySummary?.LastComputedAt ?? DateTime.Now,  // Changed from DateTime.UtcNow
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
                LastComputedAt = summaries.Any() ? summaries.Max(s => s.LastComputedAt) : DateTime.Now,  // Changed from DateTime.UtcNow
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
                LastComputedAt = hourlyStats.Any() ? hourlyStats.Max(s => s.LastComputedAt) : DateTime.Now,  // Changed from DateTime.UtcNow
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
                Code = s.RoomCode,
                RoomName = s.RoomName,
                Hour = s.Hour,
                OccupancyRate = s.OccupancyRate
            }).ToList();
        }

        #endregion
    }
}
