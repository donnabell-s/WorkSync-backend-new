using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IRoomLogRepository
    {
        IQueryable<RoomLog> GetRoomLogs();
        RoomLog GetById(int roomLogId);
        void Add(RoomLog entity);
        void Update(RoomLog entity);
        void Delete(RoomLog entity);

        Task<List<RoomLog>> GetRoomLogsAsync(CancellationToken cancellationToken = default);
        Task<List<RoomLog>> GetByRoomIdAsync(string roomId, CancellationToken cancellationToken = default);
        Task<RoomLog> GetByIdAsync(int roomLogId, CancellationToken cancellationToken = default);
        Task AddAsync(RoomLog entity, CancellationToken cancellationToken = default);
    }
}