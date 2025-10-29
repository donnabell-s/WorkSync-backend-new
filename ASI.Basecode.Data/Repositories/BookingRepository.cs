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
    public class BookingRepository : BaseRepository, IBookingRepository
    {
        public BookingRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<Booking> GetBookings() => GetDbSet<Booking>();

        public Booking GetById(int bookingId) => Context.Set<Booking>().Find(bookingId);

        public void Add(Booking entity) => GetDbSet<Booking>().Add(entity);

        public void Update(Booking entity) => SetEntityState(entity, EntityState.Modified);

        public void Delete(Booking entity) => GetDbSet<Booking>().Remove(entity);

        public async Task<List<Booking>> GetBookingsAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<Booking>().ToListAsync(cancellationToken);
        }

        public async Task<Booking> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<Booking>().FindAsync(new object[] { bookingId }, cancellationToken).AsTask();
        }

        public async Task AddAsync(Booking entity, CancellationToken cancellationToken = default)
        {
            await GetDbSet<Booking>().AddAsync(entity, cancellationToken);
        }
    }
}
