using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IRoomRepository
    {
        IQueryable<Room> GetRooms();
        Room GetById(string roomId);
        void Add(Room entity);
        void Update(Room entity);
        void Delete(Room entity);
    }

}
