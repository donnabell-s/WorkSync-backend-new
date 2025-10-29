using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IBookingRepository
    {
        IQueryable<Booking> GetBookings();
        Booking GetById(int bookingId);
        void Add(Booking entity);
        void Update(Booking entity);
        void Delete(Booking entity);

        // Async variants
        Task<List<Booking>> GetBookingsAsync(CancellationToken cancellationToken = default);
        Task<Booking> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default);
        Task AddAsync(Booking entity, CancellationToken cancellationToken = default);
    }

}
