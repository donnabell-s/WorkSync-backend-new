using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using System.Globalization;

namespace ASI.Basecode.Services.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<Booking> GetBookings()
        {
            return _bookingRepository.GetBookings();
        }

        public Booking GetById(int bookingId)
        {
            return _bookingRepository.GetById(bookingId);
        }

        public void Create(Booking booking)
        {
            // Ensure BookingId is set because database is configured with ValueGeneratedNever
            if (booking.BookingId == 0)
            {
                var current = _bookingRepository.GetBookings();
                var next = current.Any() ? current.Max(b => b.BookingId) + 1 : 1;
                booking.BookingId = next;
            }

            _bookingRepository.Add(booking);
            _unitOfWork.SaveChanges();
        }

        public void Update(Booking booking)
        {
            _bookingRepository.Update(booking);
            _unitOfWork.SaveChanges();
        }

        public void Delete(int bookingId)
        {
            var entity = _bookingRepository.GetById(bookingId);
            if (entity == null) return;
            _bookingRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }

        public async Task<List<Booking>> GetBookingsAsync(CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.GetBookingsAsync(cancellationToken);
        }

        public async Task<Booking> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        }

        public async Task CreateAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            // Ensure BookingId is set because database is configured with ValueGeneratedNever
            if (booking.BookingId == 0)
            {
                var list = await _bookingRepository.GetBookingsAsync(cancellationToken);
                var next = list.Any() ? list.Max(b => b.BookingId) + 1 : 1;
                booking.BookingId = next;
            }

            await _bookingRepository.AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            var entity = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (entity == null) return;
            _bookingRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Validate booking occurrences against existing bookings and room operating hours
        public async Task<(bool IsValid, string Message)> ValidateBookingAsync(string roomId, DateTime start, DateTime end, string recurrenceJson = null, int? excludeBookingId = null, CancellationToken cancellationToken = default)
        {
            if (start >= end) return (false, "Start must be before End");

            // Parse recurrence if provided
            RecurrenceDto rec = null;
            if (!string.IsNullOrWhiteSpace(recurrenceJson))
            {
                try { rec = JsonSerializer.Deserialize<RecurrenceDto>(recurrenceJson); } catch { rec = null; }
            }

            // Build occurrences
            var occurrences = new List<(DateTime start, DateTime end)>();
            if (rec == null || !rec.IsRecurring)
            {
                occurrences.Add((start, end));
            }
            else
            {
                var pattern = rec.Pattern?.ToLowerInvariant();
                var interval = rec.Interval.GetValueOrDefault(1);
                var endDate = rec.EndDate ?? start.AddMonths(6);
                var currentStart = start;
                var currentEnd = end;
                int occurrencesCount = 0;
                while (currentStart <= endDate && occurrencesCount < 365)
                {
                    if (pattern == "daily")
                    {
                        occurrences.Add((currentStart, currentEnd));
                        currentStart = currentStart.AddDays(interval);
                        currentEnd = currentEnd.AddDays(interval);
                    }
                    else if (pattern == "weekly")
                    {
                        var days = rec.DaysOfWeek ?? new List<int> { (int)currentStart.DayOfWeek };
                        var weekStart = currentStart.Date;
                        foreach (var d in days)
                        {
                            int targetDow = d % 7;
                            int currentDow = (int)weekStart.DayOfWeek;
                            int diff = (targetDow - currentDow + 7) % 7;
                            var occStart = weekStart.AddDays(diff).Add(currentStart.TimeOfDay);
                            var occEnd = occStart + (currentEnd - currentStart);
                            if (occStart <= endDate) occurrences.Add((occStart, occEnd));
                        }
                        weekStart = weekStart.AddDays(7 * interval);
                        currentStart = weekStart + currentStart.TimeOfDay;
                        currentEnd = currentStart + (end - start);
                    }
                    else if (pattern == "monthly")
                    {
                        occurrences.Add((currentStart, currentEnd));
                        currentStart = currentStart.AddMonths(interval);
                        currentEnd = currentEnd.AddMonths(interval);
                    }
                    else
                    {
                        occurrences.Add((currentStart, currentEnd));
                        break;
                    }

                    occurrencesCount++;
                }
            }

            // Load existing bookings for room
            var existing = (await _bookingRepository.GetBookingsAsync(cancellationToken)).Where(b => b.RoomId == roomId && (!excludeBookingId.HasValue || b.BookingId != excludeBookingId.Value)).ToList();

            bool Overlaps((DateTime s, DateTime e) a, (DateTime s, DateTime e) b) => a.s < b.e && b.s < a.e;

            foreach (var occ in occurrences)
            {
                foreach (var b in existing)
                {
                    if (b.StartDatetime == null || b.EndDatetime == null) continue;
                    if (!string.Equals(b.Status, "Declined", StringComparison.OrdinalIgnoreCase) && Overlaps(occ, (b.StartDatetime.Value, b.EndDatetime.Value)))
                        return (false, "Requested time (or recurring series) conflicts with existing bookings");
                }
            }

            // Validate against room operating hours
            var room = await _unitOfWork.Database.Set<Room>().FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);
            if (room == null) return (false, "Room not found");
            if (!string.IsNullOrWhiteSpace(room.OperatingHours))
            {
                RoomOperatingHoursDto ops = null;
                try { ops = JsonSerializer.Deserialize<RoomOperatingHoursDto>(room.OperatingHours); } catch { ops = null; }
                if (ops != null)
                {
                    foreach (var occ in occurrences)
                    {
                        var day = occ.start.DayOfWeek;
                        DayHoursDto dayHours = (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) ? ops.Weekends : ops.Weekdays;
                        if (dayHours == null || string.IsNullOrWhiteSpace(dayHours.Open) || string.IsNullOrWhiteSpace(dayHours.Close))
                            return (false, "Room operating hours are not configured for this day");

                        if (!TimeSpan.TryParse(dayHours.Open, CultureInfo.InvariantCulture, out var openTs) || !TimeSpan.TryParse(dayHours.Close, CultureInfo.InvariantCulture, out var closeTs))
                            return (false, "Room operating hours time format is invalid");

                        var occStartTs = occ.start.TimeOfDay;
                        var occEndTs = occ.end.TimeOfDay;
                        if (occStartTs < openTs || occEndTs > closeTs)
                            return (false, $"Requested time {occ.start} - {occ.end} is outside room operating hours ({dayHours.Open} - {dayHours.Close})");
                    }
                }
            }

            return (true, null);
        }

        // Local DTOs for recurrence and operating hours
        private class RecurrenceDto
        {
            public bool IsRecurring { get; set; }
            public string Pattern { get; set; }
            public int? Interval { get; set; }
            public List<int> DaysOfWeek { get; set; }
            public DateTime? EndDate { get; set; }
        }

        private class RoomOperatingHoursDto
        {
            public DayHoursDto Weekdays { get; set; }
            public DayHoursDto Weekends { get; set; }
        }

        private class DayHoursDto
        {
            public string Open { get; set; }
            public string Close { get; set; }
        }
    }
}
