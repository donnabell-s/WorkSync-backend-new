using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class RoomLogService : IRoomLogService
    {
        private readonly IRoomLogRepository _roomLogRepository;

        public RoomLogService(IRoomLogRepository roomLogRepository)
        {
            _roomLogRepository = roomLogRepository;
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
            await _roomLogRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int roomLogId, CancellationToken cancellationToken = default)
        {
            var entity = await _roomLogRepository.GetByIdAsync(roomLogId, cancellationToken);
            if (entity == null) return;
            _roomLogRepository.Delete(entity);
            await _roomLogRepository.SaveChangesAsync(cancellationToken);
        }
    }
}