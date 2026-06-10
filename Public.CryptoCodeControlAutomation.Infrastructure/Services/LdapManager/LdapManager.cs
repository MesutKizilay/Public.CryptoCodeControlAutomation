using CryptoCodeControlAutomation.Application.Services.LdapService;
using Microsoft.Extensions.Configuration;
using System.DirectoryServices.Protocols;
using System.Net;

namespace CryptoCodeControlAutomation.Infrastructure.Services.LdapManagerService
{
    public class LdapManager : ILdapService
    {
        private readonly LdapSettings _settings;

        public LdapManager(IConfiguration configuration)
        {
            _settings = configuration.GetSection("LdapSettings").Get<LdapSettings>()
                ?? throw new NullReferenceException("LdapSettings section cannot found in configuration");
        }

        public Task<LdapAuthResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host))
            {
                return Task.FromResult(new LdapAuthResult
                {
                    Success = false,
                    Message = "LDAP host bilgisi eksik."
                });
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult(new LdapAuthResult
                {
                    Success = false,
                    Message = "Kullanıcı adı veya şifre boş."
                });
            }

            if (string.IsNullOrWhiteSpace(_settings.BaseDn))
            {
                return Task.FromResult(new LdapAuthResult
                {
                    Success = false,
                    Message = "BaseDn ayari zorunludur."
                });
            }

            if (string.IsNullOrWhiteSpace(_settings.ServiceUser) ||
                string.IsNullOrWhiteSpace(_settings.ServicePassword))
            {
                return Task.FromResult(new LdapAuthResult
                {
                    Success = false,
                    Message = "ServiceUser veya ServicePassword eksik."
                });
            }

            try
            {
                var identifier = new LdapDirectoryIdentifier(_settings.Host, _settings.Port);

                using var connection = new LdapConnection(identifier)
                {
                    AuthType = AuthType.Negotiate,
                    Credential = new NetworkCredential(_settings.ServiceUser, _settings.ServicePassword, _settings.Domain)
                };

                connection.SessionOptions.ProtocolVersion = 3;
                if (_settings.UseSsl)
                    connection.SessionOptions.SecureSocketLayer = true;

                connection.Bind();

                var searchUser = NormalizeUserName(username);
                var searchFilter = $"(&(objectClass=user)(objectCategory=person)(sAMAccountName={EscapeLdapFilter(searchUser)}))";

                var request = new SearchRequest(
                    _settings.BaseDn,
                    searchFilter,
                    SearchScope.Subtree,
                    new[] { "distinguishedName", "displayName", "mail", "sAMAccountName" });

                var response = (SearchResponse)connection.SendRequest(request);
                if (response.Entries.Count == 0)
                {
                    return Task.FromResult(new LdapAuthResult
                    {
                        Success = false,
                        Message = "Kullanıcı LDAP üzerinde bulunamadi."
                    });
                }

                var entry = response.Entries[0];
                var userProfile = MapUserProfile(entry, searchUser);

                using var userConnection = new LdapConnection(identifier)
                {
                    AuthType = AuthType.Basic,
                    Credential = new NetworkCredential(userProfile.DistinguishedName, password)
                };

                userConnection.SessionOptions.ProtocolVersion = 3;
                if (_settings.UseSsl)
                    userConnection.SessionOptions.SecureSocketLayer = true;

                userConnection.Bind();

                return Task.FromResult(new LdapAuthResult
                {
                    Success = true,
                    Username = userProfile.Username,
                    FullName = userProfile.FullName,
                    Email = userProfile.Email
                });
            }
            catch (LdapException ex)
            {
                return Task.FromResult(new LdapAuthResult
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new LdapAuthResult
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        private static string NormalizeUserName(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return string.Empty;

            if (username.Contains("\\"))
                return username.Split('\\').Last();

            if (username.Contains("@"))
                return username.Split('@')[0];

            return username;
        }

        private static LdapUserProfile MapUserProfile(SearchResultEntry entry, string fallbackUserName)
        {
            return new LdapUserProfile
            {
                DistinguishedName = entry.DistinguishedName,
                Username = GetAttribute(entry, "sAMAccountName") ?? fallbackUserName,
                FullName = GetAttribute(entry, "displayName") ?? fallbackUserName,
                Email = GetAttribute(entry, "mail")
            };
        }

        private static string? GetAttribute(SearchResultEntry entry, string name)
        {
            return entry.Attributes.Contains(name)
                ? entry.Attributes[name][0]?.ToString()
                : null;
        }

        private static string EscapeLdapFilter(string input)
        {
            return input
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
        }
    }
}
