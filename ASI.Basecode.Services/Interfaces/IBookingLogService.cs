using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBookingLogService
    {
        IQueryable<BookingLog> GetBookingLogs();
        IQueryable<BookingLog> GetByBookingId(int bookingId);
        BookingLog GetById(int bookingLogId);
        void Create(BookingLog log);
        void Delete(int bookingLogId);

        Task<List<BookingLog>> GetBookingLogsAsync(CancellationToken cancellationToken = default);
        Task<List<BookingLog>> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);
        Task<BookingLog> GetByIdAsync(int bookingLogId, CancellationToken cancellationToken = default);
        Task CreateAsync(BookingLog log, CancellationToken cancellationToken = default);
        Task DeleteAsync(int bookingLogId, CancellationToken cancellationToken = default);
    }
}
