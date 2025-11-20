using ASI.Basecode.Data.Models;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(int id, string password, ref User user);
        LoginResult AuthenticateUserByEmail(string email, string password, ref User user);
    }
}
