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
        private readonly IRoomLogRepository _roomLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(
            IRoomRepository roomRepository,
            IRoomLogRepository roomLogRepository,
            IUnitOfWork unitOfWork)
        {
            _roomRepository = roomRepository;
            _roomLogRepository = roomLogRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<Room> GetRooms() => _roomRepository.GetRooms();

        public Room GetById(string roomId) => _roomRepository.GetById(roomId);

        public void Create(Room room)
        {
            _roomRepository.Add(room);

            // Explicitly add RoomAmenities to ensure they are tracked and persisted
            if (room.RoomAmenities != null && room.RoomAmenities.Any())
            {
                var set = _unitOfWork.Database.Set<RoomAmenity>();
                foreach (var amenity in room.RoomAmenities)
                {
                    // ensure RoomId is set
                    if (string.IsNullOrWhiteSpace(amenity.RoomId)) amenity.RoomId = room.RoomId;
                    set.Add(amenity);
                }
            }

            _unitOfWork.SaveChanges();
        }

        public void Update(Room room)
        {
            _roomRepository.Update(room);
            _unitOfWork.SaveChanges();
        }

        public void Delete(string roomId)
        {
            var context = _unitOfWork.Database;
            using (var tx = context.Database.BeginTransaction())
            {
                try
                {
                    // Delete booking logs for bookings in this room
                    context.Database.ExecuteSqlRaw(@"DELETE FROM [ws].[BookingLogs] WHERE BookingId IN (SELECT BookingId FROM [ws].[Bookings] WHERE RoomId = {0})", roomId);

                    // Delete bookings
                    context.Database.ExecuteSqlRaw(@"DELETE FROM [ws].[Bookings] WHERE RoomId = {0}", roomId);

                    // Delete room amenities
                    context.Database.ExecuteSqlRaw(@"DELETE FROM [ws].[RoomAmenities] WHERE RoomId = {0}", roomId);

                    // Delete room logs
                    context.Database.ExecuteSqlRaw(@"DELETE FROM [ws].[RoomLogs] WHERE RoomId = {0}", roomId);

                    // Delete the room
                    context.Database.ExecuteSqlRaw(@"DELETE FROM [ws].[Rooms] WHERE RoomId = {0}", roomId);

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public async Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken = default)
        {
            return await _roomRepository.GetRoomsAsync(cancellationToken);
        }

        public async Task<Room> GetByIdAsync(string roomId, CancellationToken cancellationToken = default)
        {
            return await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        }

        public async Task CreateAsync(Room room, int? userRefId = null, CancellationToken cancellationToken = default)
        {
            await _roomRepository.AddAsync(room, cancellationToken);

            if (room.RoomAmenities != null && room.RoomAmenities.Any())
            {
                var set = _unitOfWork.Database.Set<RoomAmenity>();
                foreach (var amenity in room.RoomAmenities)
                {
                    if (string.IsNullOrWhiteSpace(amenity.RoomId)) amenity.RoomId = room.RoomId;
                    await set.AddAsync(amenity, cancellationToken);
                }
            }

            // Create log entry for room creation
            await CreateRoomLogAsync(room.RoomId, userRefId, "Created", room.Status ?? "Available", cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Room room, int? userRefId = null, CancellationToken cancellationToken = default)
        {
            var existing = await _roomRepository.GetByIdAsync(room.RoomId, cancellationToken);
            var oldStatus = existing?.Status;
            var oldImageUrl = existing?.ImageUrl;

            _roomRepository.Update(room);

            // Determine event type based on what changed
            string eventType = "Updated";
            if (oldStatus != room.Status)
            {
                eventType = room.Status switch
                {
                    "Under Maintenance" => "MaintenanceStarted",
                    "Available" when oldStatus == "Under Maintenance" => "MaintenanceCompleted",
                    _ => "StatusChanged"
                };
            }
            else if (oldImageUrl != room.ImageUrl)
            {
                eventType = "ImageUpdated";
            }

            await CreateRoomLogAsync(room.RoomId, userRefId, eventType, room.Status, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(string roomId, int? userRefId = null, CancellationToken cancellationToken = default)
        {
            // Log deletion before actually deleting
            await CreateRoomLogAsync(roomId, userRefId, "Deleted", "Deleted", cancellationToken);

            var context = _unitOfWork.Database;
            await using (var tx = await context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync(@"DELETE FROM [ws].[BookingLogs] WHERE BookingId IN (SELECT BookingId FROM [ws].[Bookings] WHERE RoomId = {0})", new object[] { roomId }, cancellationToken);

                    await context.Database.ExecuteSqlRawAsync(@"DELETE FROM [ws].[Bookings] WHERE RoomId = {0}", new object[] { roomId }, cancellationToken);

                    await context.Database.ExecuteSqlRawAsync(@"DELETE FROM [ws].[RoomAmenities] WHERE RoomId = {0}", new object[] { roomId }, cancellationToken);

                    await context.Database.ExecuteSqlRawAsync(@"DELETE FROM [ws].[RoomLogs] WHERE RoomId = {0}", new object[] { roomId }, cancellationToken);

                    await context.Database.ExecuteSqlRawAsync(@"DELETE FROM [ws].[Rooms] WHERE RoomId = {0}", new object[] { roomId }, cancellationToken);

                    await tx.CommitAsync(cancellationToken);
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        // Helper method to create room log entries
        private async Task CreateRoomLogAsync(string roomId, int? userRefId, string eventType, string currentStatus, CancellationToken cancellationToken)
        {
            var logs = await _roomLogRepository.GetRoomLogsAsync(cancellationToken);
            var nextLogId = logs.Any() ? logs.Max(l => l.RoomLogId) + 1 : 1;

            var log = new RoomLog
            {
                RoomLogId = nextLogId,
                RoomId = roomId,
                UserRefId = userRefId,
                EventType = eventType,
                CurrentStatus = currentStatus,
                Timestamp = DateTime.UtcNow
            };

            await _roomLogRepository.AddAsync(log, cancellationToken);
        }
    }
}
