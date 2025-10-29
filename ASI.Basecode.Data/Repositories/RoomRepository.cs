using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
    }
}
