using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IRoomService
    {
        IQueryable<Room> GetRooms();
        Room GetById(string roomId);
        void Create(Room room);
        void Update(Room room);
        void Delete(string roomId);
    }
}
