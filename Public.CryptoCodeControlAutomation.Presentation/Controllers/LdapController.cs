using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    [AllowAnonymous]
    public class LdapController : Controller
    {
        private readonly LdapAuthenticationService _ldapAuthenticationService;

        public LdapController(LdapAuthenticationService ldapAuthenticationService)
        {
            _ldapAuthenticationService = ldapAuthenticationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var result = _ldapAuthenticationService.Authenticate(request.Email, request.Password);

            if (!result.Success)
                return Unauthorized(result.Message);

            // local user sync burada yapilabilir
            // sonra cookie/jwt uret
            return Ok(new
            {
                result.Username,
                result.DisplayName,
                result.Email
            });
        }

        [HttpGet]
        public IActionResult ListUsers(string? q = null, int max = 20)
        {
            var result = _ldapAuthenticationService.SearchUsers(q, max);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new
            {
                result.Count,
                result.Users
            });
        }

        public class LdapAuthResult
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public string? Username { get; set; }
            public string? DisplayName { get; set; }
            public string? Email { get; set; }
            public string[] Roles { get; set; } = System.Array.Empty<string>();
        }

        public class LdapUserListResult
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public int Count { get; set; }
            public List<LdapUserInfo> Users { get; set; } = new();
        }

        public class LdapUserInfo
        {
            public string? Username { get; set; }
            public string? DisplayName { get; set; }
            public string? Email { get; set; }
            public string? DistinguishedName { get; set; }
        }

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

        public class LdapAuthenticationService
        {
            private readonly LdapSettings? _settings;
            private readonly IConfiguration _configuration;

            public LdapAuthenticationService(LdapSettings settings, IConfiguration configuration)
            {
                _configuration = configuration;
                _settings = _configuration.GetSection("LdapSettings").Get<LdapSettings>();
            }

            public LdapAuthResult Authenticate(string username, string password)
            {
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

                    var searchFilter = $"(&(objectClass=user)(sAMAccountName={EscapeLdapFilter(username)}))";
                    var request = new SearchRequest(
                        _settings.BaseDn,
                        searchFilter,
                        SearchScope.Subtree,
                        new[] { "distinguishedName", "displayName", "mail", "memberOf", "sAMAccountName" });

                    var response = (SearchResponse)connection.SendRequest(request);

                    if (response.Entries.Count == 0)
                    {
                        return new LdapAuthResult
                        {
                            Success = false,
                            Message = "Kullanici LDAP uzerinde bulunamadi."
                        };
                    }

                    var entry = response.Entries[0];
                    var userDn = entry.DistinguishedName;

                    using var userConnection = new LdapConnection(identifier)
                    {
                        AuthType = AuthType.Basic,
                        Credential = new NetworkCredential(userDn, password)
                    };

                    userConnection.SessionOptions.ProtocolVersion = 3;
                    if (_settings.UseSsl)
                        userConnection.SessionOptions.SecureSocketLayer = true;

                    userConnection.Bind();

                    var roles = entry.Attributes["memberOf"]?
                        .GetValues(typeof(string))
                        .Cast<string>()
                        .ToArray() ?? System.Array.Empty<string>();

                    return new LdapAuthResult
                    {
                        Success = true,
                        Username = GetAttribute(entry, "sAMAccountName"),
                        DisplayName = GetAttribute(entry, "displayName"),
                        Email = GetAttribute(entry, "mail"),
                        Roles = roles
                    };
                }
                catch (LdapException ex)
                {
                    return new LdapAuthResult
                    {
                        Success = false,
                        Message = $"LDAP hatasi: {ex.Message}"
                    };
                }
                catch (System.Exception ex)
                {
                    return new LdapAuthResult
                    {
                        Success = false,
                        Message = $"Genel hata: {ex.Message}"
                    };
                }
            }

            public LdapUserListResult SearchUsers(string? query, int max)
            {
                try
                {
                    if (_settings == null)
                    {
                        return new LdapUserListResult
                        {
                            Success = false,
                            Message = "LDAP ayarlari bulunamadi."
                        };
                    }

                    if (string.IsNullOrWhiteSpace(_settings.BaseDn))
                    {
                        return new LdapUserListResult
                        {
                            Success = false,
                            Message = "BaseDn ayari zorunludur."
                        };
                    }

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

                    var cleanQuery = (query ?? string.Empty).Trim();

                    string filter;
                    if (string.IsNullOrWhiteSpace(cleanQuery))
                    {
                        filter = "(&(objectClass=user)(objectCategory=person))";
                    }
                    else
                    {
                        var escaped = EscapeLdapFilter(cleanQuery);
                        filter = "(&(objectClass=user)(objectCategory=person)(|(sAMAccountName=*"
                                 + escaped + "*)(displayName=*"
                                 + escaped + "*)(mail=*"
                                 + escaped + "*)))";
                    }

                    var request = new SearchRequest(
                        _settings.BaseDn,
                        filter,
                        SearchScope.Subtree,
                        new[] { "distinguishedName", "displayName", "mail", "sAMAccountName" });

                    if (max > 0)
                        request.SizeLimit = max;

                    var response = (SearchResponse)connection.SendRequest(request);
                    var users = new List<LdapUserInfo>();

                    foreach (SearchResultEntry entry in response.Entries)
                    {
                        users.Add(new LdapUserInfo
                        {
                            Username = GetAttribute(entry, "sAMAccountName"),
                            DisplayName = GetAttribute(entry, "displayName"),
                            Email = GetAttribute(entry, "mail"),
                            DistinguishedName = GetAttribute(entry, "distinguishedName")
                        });
                    }

                    return new LdapUserListResult
                    {
                        Success = true,
                        Count = users.Count,
                        Users = users
                    };
                }
                catch (LdapException ex)
                {
                    return new LdapUserListResult
                    {
                        Success = false,
                        Message = $"LDAP hatasi: {ex.Message}"
                    };
                }
                catch (System.Exception ex)
                {
                    return new LdapUserListResult
                    {
                        Success = false,
                        Message = $"Genel hata: {ex.Message}"
                    };
                }
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
}
