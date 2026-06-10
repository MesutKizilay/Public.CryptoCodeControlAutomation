namespace CryptoCodeControlAutomation.Infrastructure.Services.LdapManagerService
{
    internal sealed class LdapUserProfile
    {
        public string DistinguishedName { get; set; } = "";
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
    }
}
