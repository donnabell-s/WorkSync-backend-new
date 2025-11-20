using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBookingService
    {
        IQueryable<Booking> GetBookings();
        Booking GetById(int bookingId);
        void Create(Booking booking, int? actorId = null);
        void Update(Booking booking, int? actorId = null, string changeTypeOverride = null, string messageOverride = null);
        void Delete(int bookingId, int? actorId = null);

        // Async variants
        Task<List<Booking>> GetBookingsAsync(CancellationToken cancellationToken = default);
        Task<Booking> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default);
        Task CreateAsync(Booking booking, int? actorId = null, CancellationToken cancellationToken = default);
        Task UpdateAsync(Booking booking, int? actorId = null, string changeTypeOverride = null, string messageOverride = null, CancellationToken cancellationToken = default);
        Task DeleteAsync(int bookingId, int? actorId = null, CancellationToken cancellationToken = default);
        Task<(bool IsValid, string Message)> ValidateBookingAsync(string roomId, System.DateTime start, System.DateTime end, string recurrenceJson = null, int? excludeBookingId = null, CancellationToken cancellationToken = default);
    }
}
