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
        void Create(Room room);
        void Update(Room room);
        void Delete(string roomId);

        Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken = default);
        Task<Room> GetByIdAsync(string roomId, CancellationToken cancellationToken = default);
        Task CreateAsync(Room room, CancellationToken cancellationToken = default);
        Task UpdateAsync(Room room, CancellationToken cancellationToken = default);
        Task DeleteAsync(string roomId, CancellationToken cancellationToken = default);
    }
}
