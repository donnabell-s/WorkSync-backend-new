using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBookingLogService
    {
        IQueryable<BookingLog> GetBookingLogs();
        IQueryable<BookingLog> GetByBookingId(int bookingId);
        BookingLog GetById(int bookingLogId);
        void Create(BookingLog log);
        void Delete(int bookingLogId);
    }
}
