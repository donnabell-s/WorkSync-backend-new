using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Repositories
{
    public class RoomRepository : BaseRepository, IRoomRepository
    {
        public RoomRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<Room> GetRooms() => GetDbSet<Room>();

        public Room GetById(string roomId) => Context.Set<Room>().Find(roomId);

        public void Add(Room entity) => GetDbSet<Room>().Add(entity);

        public void Update(Room entity) => SetEntityState(entity, EntityState.Modified);

        public void Delete(Room entity) => GetDbSet<Room>().Remove(entity);

        public async Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<Room>().ToListAsync(cancellationToken);
        }

        public async Task<Room> GetByIdAsync(string roomId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<Room>().FindAsync(new object[] { roomId }, cancellationToken).AsTask();
        }

        public async Task AddAsync(Room entity, CancellationToken cancellationToken = default)
        {
            await GetDbSet<Room>().AddAsync(entity, cancellationToken);
        }
    }
}
