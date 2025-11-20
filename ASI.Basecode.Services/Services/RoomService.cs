using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace ASI.Basecode.Services.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly WorkSyncDbContext _dbContext;

        public RoomService(IRoomRepository roomRepository, IUnitOfWork unitOfWork, WorkSyncDbContext dbContext)
        {
            _roomRepository = roomRepository;
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        public IQueryable<Room> GetRooms() => _roomRepository.GetRooms();
        public Room GetById(string roomId) => _roomRepository.GetById(roomId);

        private void EnsureRoomLogsSchema()
        {
            try
            {
                _unitOfWork.Database.Database.ExecuteSqlRaw(@"IF COL_LENGTH('ws.RoomLogs','UserRefId') IS NULL ALTER TABLE [ws].[RoomLogs] ADD [UserRefId] INT NULL;");
            }
            catch { /* best-effort */ }
        }

        private void AddLog(string roomId, string roomName, string changeType, string message, int? actorId)
        {
            string authorName = null;
            if (actorId.HasValue)
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == actorId.Value);
                authorName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null;
            }
            var finalMessage = string.Equals(changeType, "delete", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(message)
                ? $"{message} (RoomId={roomId})" : message;
            _dbContext.RoomLogs.Add(new RoomLog
            {
                RoomIdString = roomId,
                RoomName = roomName,
                AuthorId = actorId,
                AuthorName = authorName,
                ChangeType = changeType,
                Message = finalMessage,
                Timestamp = DateTime.UtcNow
            });
        }

        public void Create(Room room, int? actorId = null)
        {
            // Remove RoomAmenities from Room before persisting to avoid duplicate tracking
            var amenities = room.RoomAmenities?.Select(a => a.Amenity).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            room.RoomAmenities = null;
            _roomRepository.Add(room);
            _unitOfWork.SaveChanges();
            // Only add amenities after Room is saved
            PersistAmenities(new Room { RoomId = room.RoomId, RoomAmenities = amenities.Select(a => new RoomAmenity { RoomId = room.RoomId, Amenity = a }).ToList() });
            _unitOfWork.SaveChanges();
            AddLog(room.RoomId, room.Name, "create", "created room", actorId);
        }

        public void Update(Room room, int? actorId = null)
        {
            var existing = _roomRepository.GetById(room.RoomId);
            if (existing == null) throw new InvalidOperationException("Room does not exist.");
            var oldStatus = existing.Status;
            existing.Name = room.Name;
            existing.Code = room.Code;
            existing.Seats = room.Seats;
            existing.Location = room.Location;
            existing.Level = room.Level;
            existing.SizeLabel = room.SizeLabel;
            existing.Status = room.Status;
            existing.OperatingHours = room.OperatingHours;
            existing.ImageUrl = room.ImageUrl;
            existing.UpdatedAt = DateTime.UtcNow;
            var amenitySet = _unitOfWork.Database.Set<RoomAmenity>();
            var currentAmenities = amenitySet.Where(a => a.RoomId == existing.RoomId).ToList();
            foreach (var a in currentAmenities) amenitySet.Remove(a);
            if (room.RoomAmenities != null && room.RoomAmenities.Any())
            {
                foreach (var amenity in room.RoomAmenities.Where(x => !string.IsNullOrWhiteSpace(x.Amenity)).Select(x => x.Amenity.Trim()).Distinct())
                {
                    amenitySet.Add(new RoomAmenity { RoomId = existing.RoomId, Amenity = amenity });
                }
            }
            string changeType = "update"; string msg = "updated room contents";
            if (oldStatus != null && !string.Equals(oldStatus, existing.Status, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(existing.Status, "Available", StringComparison.OrdinalIgnoreCase) || string.Equals(existing.Status, "Active", StringComparison.OrdinalIgnoreCase)) { changeType = "activate"; msg = "activated room"; }
                else { changeType = "inactivate"; msg = "inactivated room"; }
            }
            _unitOfWork.SaveChanges();
            AddLog(existing.RoomId, existing.Name, changeType, msg, actorId);
        }

        public void Delete(string roomId, int? actorId = null)
        {
            var entity = _roomRepository.GetById(roomId);
            var context = _unitOfWork.Database;
            using (var tx = context.Database.BeginTransaction())
            {
                try
                {
                    context.Database.ExecuteSqlRaw(@"UPDATE [ws].[RoomLogs] SET RoomId = NULL WHERE RoomId = {0}", roomId);
                    context.Database.ExecuteSqlRaw(@"UPDATE bl SET bl.BookingId = NULL FROM [ws].[BookingLogs] bl WHERE bl.BookingId IN (SELECT BookingId FROM [ws].[Bookings] WHERE RoomId = {0})", roomId);
                    context.Database.ExecuteSqlRaw(@"DELETE FROM [ws].[Bookings] WHERE RoomId = {0}", roomId);
                    context.Database.ExecuteSqlRaw(@"DELETE FROM [ws].[RoomAmenities] WHERE RoomId = {0}", roomId);
                    context.Database.ExecuteSqlRaw(@"DELETE FROM [ws].[Rooms] WHERE RoomId = {0}", roomId);
                    context.SaveChanges();
                    AddLog(roomId, entity?.Name ?? "", "delete", "deleted room", actorId);
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public async Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken = default) => await _roomRepository.GetRoomsAsync(cancellationToken);
        public async Task<Room> GetByIdAsync(string roomId, CancellationToken cancellationToken = default) => await _roomRepository.GetByIdAsync(roomId, cancellationToken);

        public async Task CreateAsync(Room room, int? actorId = null, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[RoomService] CreateAsync invoked for RoomId={room.RoomId}, ActorId={actorId}");
            var amenities = room.RoomAmenities?.Select(a => a.Amenity).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            room.RoomAmenities = null;
            await _roomRepository.AddAsync(room, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            PersistAmenities(new Room { RoomId = room.RoomId, RoomAmenities = amenities.Select(a => new RoomAmenity { RoomId = room.RoomId, Amenity = a }).ToList() }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"[RoomService] Room created successfully. Adding log.");
            AddLog(room.RoomId, room.Name, "create", "created room", actorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // Ensure log is persisted
        }

        public async Task UpdateAsync(Room room, int? actorId = null, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[RoomService] UpdateAsync invoked for RoomId={room.RoomId}, ActorId={actorId}");
            var existing = await _roomRepository.GetByIdAsync(room.RoomId, cancellationToken);
            if (existing == null) throw new InvalidOperationException("Room does not exist.");
            var oldStatus = existing.Status;
            existing.Name = room.Name;
            existing.Code = room.Code;
            existing.Seats = room.Seats;
            existing.Location = room.Location;
            existing.Level = room.Level;
            existing.SizeLabel = room.SizeLabel;
            existing.Status = room.Status;
            existing.OperatingHours = room.OperatingHours;
            existing.ImageUrl = room.ImageUrl;
            existing.UpdatedAt = DateTime.UtcNow;
            var amenitySet = _unitOfWork.Database.Set<RoomAmenity>();
            var currentAmenities = await amenitySet.Where(a => a.RoomId == existing.RoomId).ToListAsync(cancellationToken);
            amenitySet.RemoveRange(currentAmenities);
            if (room.RoomAmenities != null && room.RoomAmenities.Any())
            {
                foreach (var amenity in room.RoomAmenities.Where(x => !string.IsNullOrWhiteSpace(x.Amenity)).Select(x => x.Amenity.Trim()).Distinct())
                {
                    await amenitySet.AddAsync(new RoomAmenity { RoomId = existing.RoomId, Amenity = amenity }, cancellationToken);
                }
            }
            string changeType = "update"; string msg = "updated room contents";
            if (oldStatus != null && !string.Equals(oldStatus, existing.Status, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(existing.Status, "Available", StringComparison.OrdinalIgnoreCase) || string.Equals(existing.Status, "Active", StringComparison.OrdinalIgnoreCase)) { changeType = "activate"; msg = "activated room"; }
                else { changeType = "inactivate"; msg = "inactivated room"; }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"[RoomService] Room updated successfully. Adding log.");
            AddLog(existing.RoomId, existing.Name, changeType, msg, actorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // Ensure log is persisted
        }

        public async Task DeleteAsync(string roomId, int? actorId = null, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[RoomService] DeleteAsync invoked for RoomId={roomId}, ActorId={actorId}");
            var entity = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
            if (entity == null) return;
            var context = _unitOfWork.Database;
            await using (var tx = await context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync(@"UPDATE [ws].[RoomLogs] SET RoomId = NULL WHERE RoomId = {0}", new object[] { roomId }, cancellationToken);
                    await context.Database.ExecuteSqlRawAsync(@"UPDATE bl SET bl.BookingId = NULL FROM [ws].[BookingLogs] bl WHERE bl.BookingId IN (SELECT BookingId FROM [ws].[Bookings] WHERE RoomId = {0})", new object[] { roomId }, cancellationToken);
                    await context.Database.ExecuteSqlRawAsync(@"DELETE FROM [ws].[Bookings] WHERE RoomId = {0}", new object[] { roomId }, cancellationToken);
                    await context.Database.ExecuteSqlRawAsync(@"DELETE FROM [ws].[RoomAmenities] WHERE RoomId = {0}", new object[] { roomId }, cancellationToken);
                    await context.Database.ExecuteSqlRawAsync(@"DELETE FROM [ws].[Rooms] WHERE RoomId = {0}", new object[] { roomId }, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                    Console.WriteLine($"[RoomService] Room deleted successfully. Adding log.");
                    AddLog(roomId, entity.Name, "delete", "deleted room", actorId);
                    await _unitOfWork.SaveChangesAsync(cancellationToken); // Ensure log is persisted
                    await tx.CommitAsync(cancellationToken);
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        private void PersistAmenities(Room room, CancellationToken cancellationToken = default)
        {
            if (room.RoomAmenities == null || !room.RoomAmenities.Any()) return;
            var set = _unitOfWork.Database.Set<RoomAmenity>();
            foreach (var amenity in room.RoomAmenities.Where(x => !string.IsNullOrWhiteSpace(x.Amenity)).Select(x => x.Amenity.Trim()).Distinct())
            {
                set.Add(new RoomAmenity { RoomId = room.RoomId, Amenity = amenity });
            }
        }
    }
}
