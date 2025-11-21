using ASI.Basecode.Data;
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
        private readonly WorkSyncDbContext _dbContext;
        private readonly INotificationService _notificationService;

        public BookingService(IBookingRepository bookingRepository, IUnitOfWork unitOfWork, WorkSyncDbContext dbContext, INotificationService notificationService)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _notificationService = notificationService;//added

        }

        public IQueryable<Booking> GetBookings() => _bookingRepository.GetBookings();
        public Booking GetById(int bookingId) => _bookingRepository.GetById(bookingId);

        private void EnsureBookingLogsSchema()
        {
            try
            {
                _unitOfWork.Database.Database.ExecuteSqlRaw("IF COL_LENGTH('ws.BookingLogs','UserRefId') IS NULL ALTER TABLE [ws].[BookingLogs] ADD [UserRefId] INT NULL;");
            }
            catch { /* best-effort */ }
        }

        private void AddLog(int? bookingId, string bookingName, string changeType, string message, int? actorId)
        {
            string authorName = null;
            if (actorId.HasValue)
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == actorId.Value);
                authorName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null;
            }
            _dbContext.BookingLogs.Add(new BookingLog
            {
                BookingIdString = bookingId?.ToString(),
                BookingName = bookingName,
                ChangeType = changeType,
                Message = message,
                AuthorId = actorId,
                AuthorName = authorName,
                Timestamp = DateTime.UtcNow
            });
        }

        public void Create(Booking booking, int? actorId = null)
        {
            // Explicit PK assignment required (ValueGeneratedNever)
            if (booking.BookingId == 0)
            {
                var current = _bookingRepository.GetBookings();
                var next = current.Any() ? current.Max(b => b.BookingId) + 1 : 1;
                booking.BookingId = next;
            }
            _bookingRepository.Add(booking);
            _unitOfWork.SaveChanges();
            AddLog(booking.BookingId, booking.Title, "create", "created booking", actorId);
            _unitOfWork.SaveChanges();

            _notificationService.CreateNotification($"A new booking was created: {booking.Title}","booking"
             );//added notification for new booking
        }

        public void Update(Booking booking, int? actorId = null, string changeTypeOverride = null, string messageOverride = null)
        {
            var existing = _bookingRepository.GetById(booking.BookingId);
            if (existing == null) throw new InvalidOperationException("Booking does not exist.");

            existing.RoomId = booking.RoomId;
            existing.Title = booking.Title;
            existing.Description = booking.Description;
            existing.StartDatetime = booking.StartDatetime;
            existing.EndDatetime = booking.EndDatetime;
            existing.Recurrence = booking.Recurrence;
            existing.Status = booking.Status;
            existing.ExpectedAttendees = booking.ExpectedAttendees;
            existing.UpdatedAt = DateTime.UtcNow;

            var changeType = changeTypeOverride ?? "update";
            var message = messageOverride ?? "edited booking content";
            _unitOfWork.SaveChanges();
            AddLog(existing.BookingId, existing.Title, changeType, message, actorId);
            _unitOfWork.SaveChanges();

            _notificationService.CreateNotification( $"Booking updated: {existing.Title}","booking_update"
            ); //added notification for booking update
        }

        public void Delete(int bookingId, int? actorId = null)
        {
            var entity = _bookingRepository.GetById(bookingId);
            if (entity == null) return;
            _bookingRepository.Delete(entity);
            _unitOfWork.SaveChanges();
            AddLog(entity.BookingId, entity.Title, "delete", "deleted booking", actorId);
            _unitOfWork.SaveChanges();

            _notificationService.CreateNotification($"Booking cancelled: {entity.Title}","booking_cancel"
            ); //added notification for booking deletion
        }

        public async Task<List<Booking>> GetBookingsAsync(CancellationToken cancellationToken = default) => await _bookingRepository.GetBookingsAsync(cancellationToken);
        public async Task<Booking> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default) => await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

        public async Task CreateAsync(Booking booking, int? actorId = null, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[BookingService] CreateAsync invoked for BookingId={booking.BookingId}, ActorId={actorId}");
            // Explicit PK assignment required (ValueGeneratedNever)
            if (booking.BookingId == 0)
            {
                var list = await _bookingRepository.GetBookingsAsync(cancellationToken);
                var next = list.Any() ? list.Max(b => b.BookingId) + 1 : 1;
                booking.BookingId = next;
            }
            await _bookingRepository.AddAsync(booking, cancellationToken);
            Console.WriteLine($"[BookingService] Booking created successfully. Adding log.");
            AddLog(booking.BookingId, booking.Title, "create", "created booking", actorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // Ensure log is persisted
        }

        public async Task UpdateAsync(Booking booking, int? actorId = null, string changeTypeOverride = null, string messageOverride = null, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[BookingService] UpdateAsync invoked for BookingId={booking.BookingId}, ActorId={actorId}");
            var existing = await _bookingRepository.GetByIdAsync(booking.BookingId, cancellationToken);
            if (existing == null) throw new InvalidOperationException("Booking does not exist.");

            existing.RoomId = booking.RoomId;
            existing.Title = booking.Title;
            existing.Description = booking.Description;
            existing.StartDatetime = booking.StartDatetime;
            existing.EndDatetime = booking.EndDatetime;
            existing.Recurrence = booking.Recurrence;
            existing.Status = booking.Status;
            existing.ExpectedAttendees = booking.ExpectedAttendees;
            existing.UpdatedAt = DateTime.UtcNow;

            var changeType = changeTypeOverride ?? "update";
            var message = messageOverride ?? "edited booking content";
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"[BookingService] Booking updated successfully. Adding log.");
            AddLog(existing.BookingId, existing.Title, changeType, message, actorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // Ensure log is persisted
        }

        public async Task DeleteAsync(int bookingId, int? actorId = null, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[BookingService] DeleteAsync invoked for BookingId={bookingId}, ActorId={actorId}");
            var entity = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (entity == null) return;
            _bookingRepository.Delete(entity);
            Console.WriteLine($"[BookingService] Booking deleted successfully. Adding log.");
            AddLog(entity.BookingId, entity.Title, "delete", "deleted booking", actorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // Ensure log is persisted
        }

        public async Task<(bool IsValid, string Message)> ValidateBookingAsync(string roomId, DateTime start, DateTime end, string recurrenceJson = null, int? excludeBookingId = null, CancellationToken cancellationToken = default)
        {
            if (start >= end) return (false, "Start must be before End");
            RecurrenceDto rec = null;
            if (!string.IsNullOrWhiteSpace(recurrenceJson)) { try { rec = JsonSerializer.Deserialize<RecurrenceDto>(recurrenceJson); } catch { rec = null; } }
            var occurrences = new List<(DateTime start, DateTime end)>();
            if (rec == null || !rec.IsRecurring) occurrences.Add((start, end)); else {
                var pattern = rec.Pattern?.ToLowerInvariant();
                var interval = rec.Interval.GetValueOrDefault(1);
                var endDate = rec.EndDate ?? start.AddMonths(6);
                var currentStart = start; var currentEnd = end; int count = 0;
                while (currentStart <= endDate && count < 365)
                {
                    if (pattern == "daily") { occurrences.Add((currentStart, currentEnd)); currentStart = currentStart.AddDays(interval); currentEnd = currentEnd.AddDays(interval); }
                    else if (pattern == "weekly") {
                        var days = rec.DaysOfWeek ?? new List<int> { (int)currentStart.DayOfWeek };
                        var weekStart = currentStart.Date;
                        foreach (var d in days) { int target = d % 7; int cur = (int)weekStart.DayOfWeek; int diff = (target - cur + 7) % 7; var occStart = weekStart.AddDays(diff).Add(currentStart.TimeOfDay); var occEnd = occStart + (currentEnd - currentStart); if (occStart <= endDate) occurrences.Add((occStart, occEnd)); }
                        weekStart = weekStart.AddDays(7 * interval); currentStart = weekStart + currentStart.TimeOfDay; currentEnd = currentStart + (end - start);
                    }
                    else if (pattern == "monthly") { occurrences.Add((currentStart, currentEnd)); currentStart = currentStart.AddMonths(interval); currentEnd = currentEnd.AddMonths(interval); }
                    else { occurrences.Add((currentStart, currentEnd)); break; }
                    count++;
                }
            }
            var existing = (await _bookingRepository.GetBookingsAsync(cancellationToken)).Where(b => b.RoomId == roomId && (!excludeBookingId.HasValue || b.BookingId != excludeBookingId.Value)).ToList();
            foreach (var occ in occurrences)
                foreach (var b in existing)
                    if (b.StartDatetime != null && b.EndDatetime != null && !string.Equals(b.Status, "Declined", StringComparison.OrdinalIgnoreCase) && occ.start < b.EndDatetime && b.StartDatetime < occ.end)
                        return (false, "Requested time (or recurring series) conflicts with existing bookings");
            var room = await _unitOfWork.Database.Set<Room>().FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);
            if (room == null) return (false, "Room not found");
            if (!string.IsNullOrWhiteSpace(room.OperatingHours))
            {
                RoomOperatingHoursDto ops = null; try { ops = JsonSerializer.Deserialize<RoomOperatingHoursDto>(room.OperatingHours); } catch { ops = null; }
                if (ops != null)
                {
                    foreach (var occ in occurrences)
                    {
                        var day = occ.start.DayOfWeek; var dayHours = (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) ? ops.Weekends : ops.Weekdays;
                        if (dayHours == null || string.IsNullOrWhiteSpace(dayHours.Open) || string.IsNullOrWhiteSpace(dayHours.Close)) return (false, "Room operating hours are not configured for this day");
                        if (!TimeSpan.TryParse(dayHours.Open, CultureInfo.InvariantCulture, out var openTs) || !TimeSpan.TryParse(dayHours.Close, CultureInfo.InvariantCulture, out var closeTs)) return (false, "Room operating hours time format is invalid");
                        var occStartTs = occ.start.TimeOfDay; var occEndTs = occ.end.TimeOfDay; if (occStartTs < openTs || occEndTs > closeTs) return (false, $"Requested time {occ.start} - {occ.end} is outside room operating hours ({dayHours.Open} - {dayHours.Close})");
                    }
                }
            }
            return (true, null);
        }

        private class RecurrenceDto { public bool IsRecurring { get; set; } public string Pattern { get; set; } public int? Interval { get; set; } public List<int> DaysOfWeek { get; set; } public DateTime? EndDate { get; set; } }
        private class RoomOperatingHoursDto { public DayHoursDto Weekdays { get; set; } public DayHoursDto Weekends { get; set; } }
        private class DayHoursDto { public string Open { get; set; } public string Close { get; set; } }
    }
}
