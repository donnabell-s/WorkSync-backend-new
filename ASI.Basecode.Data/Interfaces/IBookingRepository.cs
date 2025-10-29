using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IBookingRepository
    {
        IQueryable<Booking> GetBookings();
        Booking GetById(int bookingId);
        void Add(Booking entity);
        void Update(Booking entity);
        void Delete(Booking entity);
    }

}
