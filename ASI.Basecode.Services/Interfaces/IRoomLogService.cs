using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IRoomLogService
    {
        IQueryable<RoomLog> GetRoomLogs();
        IQueryable<RoomLog> GetByRoomId(string roomId);
        RoomLog GetById(int roomLogId);
        void Create(RoomLog log);
        void Delete(int roomLogId);

        Task<List<RoomLog>> GetRoomLogsAsync(CancellationToken cancellationToken = default);
        Task<List<RoomLog>> GetByRoomIdAsync(string roomId, CancellationToken cancellationToken = default);
        Task<RoomLog> GetByIdAsync(int roomLogId, CancellationToken cancellationToken = default);
        Task CreateAsync(RoomLog log, CancellationToken cancellationToken = default);
        Task DeleteAsync(int roomLogId, CancellationToken cancellationToken = default);
    }
}