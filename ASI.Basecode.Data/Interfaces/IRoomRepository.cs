using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IRoomRepository
    {
        IQueryable<Room> GetRooms();
        Room GetById(string roomId);
        void Add(Room entity);
        void Update(Room entity);
        void Delete(Room entity);

        // Async variants
        Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken = default);
        Task<Room> GetByIdAsync(string roomId, CancellationToken cancellationToken = default);
        Task AddAsync(Room entity, CancellationToken cancellationToken = default);
    }

}
