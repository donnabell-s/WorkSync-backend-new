using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IRoomLogRepository
    {
        Task<List<RoomLog>> GetRoomLogsAsync(CancellationToken cancellationToken = default);
        Task<List<RoomLog>> GetByRoomIdAsync(string roomId, CancellationToken cancellationToken = default);
        Task<RoomLog> GetByIdAsync(int roomLogId, CancellationToken cancellationToken = default);
        Task AddAsync(RoomLog entity, CancellationToken cancellationToken = default);
        void Add(RoomLog entity);
        void Delete(RoomLog entity);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}