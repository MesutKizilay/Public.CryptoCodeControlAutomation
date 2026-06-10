namespace CryptoCodeControlAutomation.Infrastructure.Services.LdapManagerService
{
    public class LdapSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 389;
        public string Domain { get; set; } = "";
        public string BaseDn { get; set; } = "";
        public string ServiceUser { get; set; } = "";
        public string ServicePassword { get; set; } = "";
        public bool UseSsl { get; set; } = false;
    }
}
