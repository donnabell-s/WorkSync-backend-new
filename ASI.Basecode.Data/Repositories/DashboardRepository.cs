using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class DashboardRepository : BaseRepository, IDashboardRepository
    {
        public DashboardRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /// <summary>
        /// Get summary metrics for dashboard KPI cards
        /// </summary>
        public async Task<DashboardSummaryViewModel> GetDashboardSummaryAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var todayStart = date.Date;
            var todayEnd = date.Date.AddDays(1);
            var now = DateTime.Now;

            // Available rooms (status = 'active' or similar)
            var availableRoomsCount = await GetDbSet<Room>()
                .AsNoTracking()
                .CountAsync(r => r.Status != null && (
                    r.Status.ToLower() == "active" || 
                    r.Status.ToLower() == "available"
                ), cancellationToken);

            // Rooms under maintenance
            var maintenanceRoomsCount = await GetDbSet<Room>()
                .AsNoTracking()
                .CountAsync(r => r.Status != null && r.Status.ToLower() == "maintenance", cancellationToken);

            // Today's bookings
            var todaysBookingsCount = await GetDbSet<Booking>()
                .AsNoTracking()
                .CountAsync(b => b.StartDatetime.HasValue && b.StartDatetime.Value.Date == todayStart, cancellationToken);

            // Ongoing bookings
            var ongoingBookingsCount = await GetDbSet<Booking>()
                .AsNoTracking()
                .CountAsync(b => b.StartDatetime.HasValue && b.EndDatetime.HasValue &&
                                b.StartDatetime.Value <= now && b.EndDatetime.Value >= now, cancellationToken);

            // Bookings completed today
            var completedBookingsCount = await GetDbSet<Booking>()
                .AsNoTracking()
                .CountAsync(b => b.EndDatetime.HasValue && b.EndDatetime.Value.Date == todayStart, cancellationToken);

            // Utilization rate calculation
            // Calculate total booked minutes from booking logs for today
            var totalBookedMinutes = await GetDbSet<BookingLog>()
                .AsNoTracking()
                .Where(bl => bl.Timestamp.HasValue && bl.Timestamp.Value.Date == todayStart &&
                           bl.EventType != null && bl.EventType.ToLower().Contains("start"))
                .Join(GetDbSet<Booking>(),
                      bl => bl.BookingId,
                      b => b.BookingId,
                      (bl, b) => new { b.StartDatetime, b.EndDatetime })
                .Where(joined => joined.StartDatetime.HasValue && joined.EndDatetime.HasValue)
                .SumAsync(joined => EF.Functions.DateDiffMinute(joined.StartDatetime.Value, joined.EndDatetime.Value), cancellationToken);

            // Simplified calculation: assume 8 hours per room per day (480 minutes)
            var totalRooms = await GetDbSet<Room>()
                .AsNoTracking()
                .CountAsync(r => r.Status != null && r.Status.ToLower() != "maintenance", cancellationToken);

            var totalAvailableMinutes = totalRooms * 480; // 8 hours * 60 minutes
            var utilizationPercentage = totalAvailableMinutes > 0 
                ? (double)totalBookedMinutes / totalAvailableMinutes * 100 
                : 0.0;

            return new DashboardSummaryViewModel
            {
                AvailableRooms = availableRoomsCount,
                RoomsUnderMaintenance = maintenanceRoomsCount,
                TodaysBookings = todaysBookingsCount,
                OngoingBookings = ongoingBookingsCount,
                BookingsCompletedToday = completedBookingsCount,
                UtilizationRateToday = Math.Round(utilizationPercentage, 2)
            };
        }

        /// <summary>
        /// Get bookings trend data for specified date range
        /// </summary>
        public async Task<List<BookingsTrendViewModel>> GetBookingsTrendAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var result = new List<BookingsTrendViewModel>();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var nextDay = date.AddDays(1);

                // Count bookings for this date
                var bookingsCount = await GetDbSet<Booking>()
                    .AsNoTracking()
                    .CountAsync(b => b.StartDatetime.HasValue && 
                               b.StartDatetime.Value.Date == date, cancellationToken);

                // Calculate utilization for this date
                var totalBookedMinutes = await GetDbSet<Booking>()
                    .AsNoTracking()
                    .Where(b => b.StartDatetime.HasValue && b.EndDatetime.HasValue &&
                              b.StartDatetime.Value.Date == date)
                    .SumAsync(b => EF.Functions.DateDiffMinute(b.StartDatetime.Value, b.EndDatetime.Value), cancellationToken);

                var totalRooms = await GetDbSet<Room>()
                    .AsNoTracking()
                    .CountAsync(r => r.Status != null && r.Status.ToLower() != "maintenance", cancellationToken);

                var totalAvailableMinutes = totalRooms * 480; // 8 hours * 60 minutes
                var utilizationPercentage = totalAvailableMinutes > 0 
                    ? (double)totalBookedMinutes / totalAvailableMinutes * 100 
                    : 0.0;

                result.Add(new BookingsTrendViewModel
                {
                    Date = date,
                    BookingsCount = bookingsCount,
                    UtilizationPercentage = Math.Round(utilizationPercentage, 2)
                });
            }

            return result;
        }

        /// <summary>
        /// Get peak usage heatmap data for specified date
        /// </summary>
        public async Task<List<PeakUsageViewModel>> GetPeakUsageAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var dateStart = date.Date;
            var dateEnd = date.Date.AddDays(1);

            // Get all bookings for the specified date
            var bookings = await GetDbSet<Booking>()
                .AsNoTracking()
                .Include(b => b.Room)
                .Where(b => b.StartDatetime.HasValue && b.EndDatetime.HasValue &&
                          b.StartDatetime.Value.Date == dateStart &&
                          b.Room != null)
                .Select(b => new
                {
                    RoomName = b.Room.Name,
                    StartTime = b.StartDatetime.Value,
                    EndTime = b.EndDatetime.Value
                })
                .ToListAsync(cancellationToken);

            var result = new List<PeakUsageViewModel>();

            // Get all rooms
            var rooms = await GetDbSet<Room>()
                .AsNoTracking()
                .Where(r => r.Status != null && r.Status.ToLower() != "maintenance")
                .Select(r => r.Name)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Calculate occupancy for each room for each hour (0-23)
            foreach (var roomName in rooms)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    var hourStart = dateStart.AddHours(hour);
                    var hourEnd = hourStart.AddHours(1);

                    // Calculate total minutes booked in this hour for this room
                    var minutesBooked = bookings
                        .Where(b => b.RoomName == roomName)
                        .Sum(b =>
                        {
                            var overlapStart = b.StartTime < hourStart ? hourStart : b.StartTime;
                            var overlapEnd = b.EndTime > hourEnd ? hourEnd : b.EndTime;
                            
                            if (overlapStart >= overlapEnd)
                                return 0;
                                
                            return (int)(overlapEnd - overlapStart).TotalMinutes;
                        });

                    var occupancyRate = (double)minutesBooked / 60.0 * 100;

                    result.Add(new PeakUsageViewModel
                    {
                        RoomName = roomName,
                        Hour = hour,
                        OccupancyRate = Math.Round(occupancyRate, 2)
                    });
                }
            }

            return result;
        }
    }
}