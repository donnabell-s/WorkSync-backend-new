using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBookingService
    {
        IQueryable<Booking> GetBookings();
        Booking GetById(int bookingId);
        void Create(Booking booking);
        void Update(Booking booking);
        void Delete(int bookingId);
    }
}
