using ASI.Basecode.Data.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IBookingLogRepository
    {
        IQueryable<BookingLog> GetBookingLogs();
        BookingLog GetById(int bookingLogId);
        void Add(BookingLog entity);
        void Update(BookingLog entity);
        void Delete(BookingLog entity);

        Task<List<BookingLog>> GetBookingLogsAsync(CancellationToken cancellationToken = default);
        Task<List<BookingLog>> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);
        Task<BookingLog> GetByIdAsync(int bookingLogId, CancellationToken cancellationToken = default);
        Task AddAsync(BookingLog entity, CancellationToken cancellationToken = default);
    }

}
