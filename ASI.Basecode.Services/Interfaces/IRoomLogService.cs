using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IRoomLogService
    {
        Task<List<RoomLog>> GetRoomLogsAsync(CancellationToken cancellationToken = default);
        Task<List<RoomLog>> GetByRoomIdAsync(string roomId, CancellationToken cancellationToken = default);
        Task<RoomLog> GetByIdAsync(int roomLogId, CancellationToken cancellationToken = default);
        Task CreateAsync(RoomLog log, CancellationToken cancellationToken = default);
        Task DeleteAsync(int roomLogId, CancellationToken cancellationToken = default);
    }
}