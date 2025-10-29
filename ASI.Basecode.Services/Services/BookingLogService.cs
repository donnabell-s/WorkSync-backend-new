using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class BookingLogService : IBookingLogService
    {
        private readonly IBookingLogRepository _bookingLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingLogService(IBookingLogRepository bookingLogRepository, IUnitOfWork unitOfWork)
        {
            _bookingLogRepository = bookingLogRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<BookingLog> GetBookingLogs() => _bookingLogRepository.GetBookingLogs();

        public IQueryable<BookingLog> GetByBookingId(int bookingId) => _bookingLogRepository.GetBookingLogs().Where(b => b.BookingId == bookingId);

        public BookingLog GetById(int bookingLogId) => _bookingLogRepository.GetById(bookingLogId);

        public void Create(BookingLog log)
        {
            _bookingLogRepository.Add(log);
            _unitOfWork.SaveChanges();
        }

        public void Delete(int bookingLogId)
        {
            var entity = _bookingLogRepository.GetById(bookingLogId);
            if (entity == null) return;
            _bookingLogRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }
    }
}
