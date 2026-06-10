using System.Threading;
using System.Threading.Tasks;

namespace CryptoCodeControlAutomation.Application.Services.LdapService
{
    public interface ILdapService
    {
        Task<LdapAuthResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    }

    public class LdapAuthResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
