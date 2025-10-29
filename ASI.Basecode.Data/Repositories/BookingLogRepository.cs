using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public async Task<List<BookingLog>> GetBookingLogsAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<BookingLog>().ToListAsync(cancellationToken);
        }

        public async Task<List<BookingLog>> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<BookingLog>().Where(b => b.BookingId == bookingId).ToListAsync(cancellationToken);
        }

        public async Task<BookingLog> GetByIdAsync(int bookingLogId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<BookingLog>().FindAsync(new object[] { bookingLogId }, cancellationToken).AsTask();
        }

        public async Task AddAsync(BookingLog entity, CancellationToken cancellationToken = default)
        {
            await GetDbSet<BookingLog>().AddAsync(entity, cancellationToken);
        }
    }
}
