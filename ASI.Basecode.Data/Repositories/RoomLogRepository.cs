using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class RoomLogRepository : IRoomLogRepository
    {
        private readonly WorkSyncDbContext _dbContext;

        public RoomLogRepository(WorkSyncDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<RoomLog>> GetRoomLogsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.RoomLogs
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RoomLog>> GetByRoomIdAsync(string roomId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RoomLogs
                .Where(r => r.RoomIdString == roomId)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<RoomLog> GetByIdAsync(int roomLogId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RoomLogs
                .FirstOrDefaultAsync(r => r.RoomLogId == roomLogId, cancellationToken);
        }

        public async Task AddAsync(RoomLog entity, CancellationToken cancellationToken = default)
        {
            await _dbContext.RoomLogs.AddAsync(entity, cancellationToken);
        }

        public void Add(RoomLog entity)
        {
            _dbContext.RoomLogs.Add(entity);
        }

        public void Delete(RoomLog entity)
        {
            _dbContext.RoomLogs.Remove(entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}