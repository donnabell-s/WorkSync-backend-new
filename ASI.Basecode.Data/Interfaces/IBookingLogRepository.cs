using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IBookingLogRepository
    {
        IQueryable<BookingLog> GetBookingLogs();
        BookingLog GetById(int bookingLogId);
        void Add(BookingLog entity);
        void Update(BookingLog entity);
        void Delete(BookingLog entity);
    }

}
