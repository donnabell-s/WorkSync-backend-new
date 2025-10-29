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
        void Create(Booking booking);
        void Update(Booking booking);
        void Delete(int bookingId);

        // Async variants
        Task<List<Booking>> GetBookingsAsync(CancellationToken cancellationToken = default);
        Task<Booking> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default);
        Task CreateAsync(Booking booking, CancellationToken cancellationToken = default);
        Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
        Task DeleteAsync(int bookingId, CancellationToken cancellationToken = default);
    }
}
