using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IRoomService
    {
        IQueryable<Room> GetRooms();
        Room GetById(string roomId);
        void Create(Room room, int? actorId = null);
        void Update(Room room, int? actorId = null);
        void Delete(string roomId, int? actorId = null);

        Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken = default);
        Task<Room> GetByIdAsync(string roomId, CancellationToken cancellationToken = default);
        Task CreateAsync(Room room, int? actorId = null, CancellationToken cancellationToken = default);
        Task UpdateAsync(Room room, int? actorId = null, CancellationToken cancellationToken = default);
        Task DeleteAsync(string roomId, int? actorId = null, CancellationToken cancellationToken = default);
    }
}
