using Core.Security.JWT;

namespace CryptoCodeControlAutomation.Application.Features.Auth.Commands.Login
{
    public class LoggedResponse
    {
        public AccessToken? AccessToken { get; set; }
    }
}