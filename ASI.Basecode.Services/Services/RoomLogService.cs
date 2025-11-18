using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Services
{
    public class RoomLogService : IRoomLogService
    {
        private readonly IRoomLogRepository _roomLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoomLogService(IRoomLogRepository roomLogRepository, IUnitOfWork unitOfWork)
        {
            _roomLogRepository = roomLogRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<RoomLog> GetRoomLogs() => _roomLogRepository.GetRoomLogs();

        public IQueryable<RoomLog> GetByRoomId(string roomId) 
            => _roomLogRepository.GetRoomLogs().Where(r => r.RoomId == roomId);

        public RoomLog GetById(int roomLogId) => _roomLogRepository.GetById(roomLogId);

        public void Create(RoomLog log)
        {
            _roomLogRepository.Add(log);
            _unitOfWork.SaveChanges();
        }

        public void Delete(int roomLogId)
        {
            var entity = _roomLogRepository.GetById(roomLogId);
            if (entity == null) return;
            _roomLogRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }

        public async Task<List<RoomLog>> GetRoomLogsAsync(CancellationToken cancellationToken = default)
        {
            return await _roomLogRepository.GetRoomLogsAsync(cancellationToken);
        }

        public async Task<List<RoomLog>> GetByRoomIdAsync(string roomId, CancellationToken cancellationToken = default)
        {
            return await _roomLogRepository.GetByRoomIdAsync(roomId, cancellationToken);
        }

        public async Task<RoomLog> GetByIdAsync(int roomLogId, CancellationToken cancellationToken = default)
        {
            return await _roomLogRepository.GetByIdAsync(roomLogId, cancellationToken);
        }

        public async Task CreateAsync(RoomLog log, CancellationToken cancellationToken = default)
        {
            await _roomLogRepository.AddAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int roomLogId, CancellationToken cancellationToken = default)
        {
            var entity = await _roomLogRepository.GetByIdAsync(roomLogId, cancellationToken);
            if (entity == null) return;
            _roomLogRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}