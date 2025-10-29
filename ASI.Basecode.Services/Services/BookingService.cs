using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<Booking> GetBookings()
        {
            return _bookingRepository.GetBookings();
        }

        public Booking GetById(int bookingId)
        {
            return _bookingRepository.GetById(bookingId);
        }

        public void Create(Booking booking)
        {
            _bookingRepository.Add(booking);
            _unitOfWork.SaveChanges();
        }

        public void Update(Booking booking)
        {
            _bookingRepository.Update(booking);
            _unitOfWork.SaveChanges();
        }

        public void Delete(int bookingId)
        {
            var entity = _bookingRepository.GetById(bookingId);
            if (entity == null) return;
            _bookingRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }

        public async Task<List<Booking>> GetBookingsAsync(CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.GetBookingsAsync(cancellationToken);
        }

        public async Task<Booking> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        }

        public async Task CreateAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            await _bookingRepository.AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            var entity = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (entity == null) return;
            _bookingRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
