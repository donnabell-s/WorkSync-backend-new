using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IRoomRepository roomRepository, IUnitOfWork unitOfWork)
        {
            _roomRepository = roomRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<Room> GetRooms() => _roomRepository.GetRooms();

        public Room GetById(string roomId) => _roomRepository.GetById(roomId);

        public void Create(Room room)
        {
            _roomRepository.Add(room);
            _unitOfWork.SaveChanges();
        }

        public void Update(Room room)
        {
            _roomRepository.Update(room);
            _unitOfWork.SaveChanges();
        }

        public void Delete(string roomId)
        {
            var entity = _roomRepository.GetById(roomId);
            if (entity == null) return;
            _roomRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }
    }
}
