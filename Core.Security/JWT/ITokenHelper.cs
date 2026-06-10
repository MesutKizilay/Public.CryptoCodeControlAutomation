using Core.Security.Entities;

namespace Core.Security.JWT
{
    public interface ITokenHelper
    {
        AccessToken CreateToken(User user, IList<Role> roles);
        //RefreshToken CreateRefreshToken(User user, string ipAddress);
    }
}