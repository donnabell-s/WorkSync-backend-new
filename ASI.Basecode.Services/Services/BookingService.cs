using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;

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
    }
}
