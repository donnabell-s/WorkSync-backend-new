using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class BookingLogRepository : BaseRepository, IBookingLogRepository
    {
        public BookingLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<BookingLog> GetBookingLogs() => GetDbSet<BookingLog>();

        public BookingLog GetById(int bookingLogId) => Context.Set<BookingLog>().Find(bookingLogId);

        public void Add(BookingLog entity) => GetDbSet<BookingLog>().Add(entity);

        public void Update(BookingLog entity) => SetEntityState(entity, EntityState.Modified);

        public void Delete(BookingLog entity) => GetDbSet<BookingLog>().Remove(entity);
    }
}
