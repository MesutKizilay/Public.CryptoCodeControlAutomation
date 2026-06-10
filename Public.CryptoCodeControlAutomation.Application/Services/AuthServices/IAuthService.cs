using Core.Security.Entities;
using Core.Security.JWT;

namespace CryptoCodeControlAutomation.Application.Services.AuthServices
{
    public interface IAuthService
    {
        public Task<AccessToken> CreateAccessToken(User user);
    }
}