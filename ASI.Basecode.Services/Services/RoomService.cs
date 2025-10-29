using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public async Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken = default)
        {
            return await _roomRepository.GetRoomsAsync(cancellationToken);
        }

        public async Task<Room> GetByIdAsync(string roomId, CancellationToken cancellationToken = default)
        {
            return await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        }

        public async Task CreateAsync(Room room, CancellationToken cancellationToken = default)
        {
            await _roomRepository.AddAsync(room, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
        {
            _roomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(string roomId, CancellationToken cancellationToken = default)
        {
            var entity = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
            if (entity == null) return;
            _roomRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
