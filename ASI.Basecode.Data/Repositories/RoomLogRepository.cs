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
    public class RoomLogRepository : BaseRepository, IRoomLogRepository
    {
        public RoomLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<RoomLog> GetRoomLogs() => GetDbSet<RoomLog>()
            .Include(rl => rl.Room)
            .Include(rl => rl.User);

        public IQueryable<RoomLog> GetByRoomId(string roomId) => GetDbSet<RoomLog>()
            .Include(rl => rl.Room)
            .Include(rl => rl.User)
            .Where(rl => rl.RoomId == roomId);

        public RoomLog GetById(int roomLogId) => Context.Set<RoomLog>()
            .Include(rl => rl.Room)
            .Include(rl => rl.User)
            .FirstOrDefault(rl => rl.RoomLogId == roomLogId);

        public void Add(RoomLog entity) => GetDbSet<RoomLog>().Add(entity);

        public void Update(RoomLog entity) => SetEntityState(entity, EntityState.Modified);

        public void Delete(RoomLog entity) => GetDbSet<RoomLog>().Remove(entity);

        public async Task<List<RoomLog>> GetRoomLogsAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<RoomLog>()
                .Include(rl => rl.Room)
                .Include(rl => rl.User)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RoomLog>> GetByRoomIdAsync(string roomId, CancellationToken cancellationToken = default)
        {
            return await GetDbSet<RoomLog>()
                .Include(rl => rl.Room)
                .Include(rl => rl.User)
                .Where(rl => rl.RoomId == roomId)
                .ToListAsync(cancellationToken);
        }

        public async Task<RoomLog> GetByIdAsync(int roomLogId, CancellationToken cancellationToken = default)
        {
            return await Context.Set<RoomLog>()
                .Include(rl => rl.Room)
                .Include(rl => rl.User)
                .FirstOrDefaultAsync(rl => rl.RoomLogId == roomLogId, cancellationToken);
        }

        public async Task AddAsync(RoomLog entity, CancellationToken cancellationToken = default)
        {
            await GetDbSet<RoomLog>().AddAsync(entity, cancellationToken);
        }
    }
}