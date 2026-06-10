using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    [AllowAnonymous]
    public class TestController : Controller
    {
        public IActionResult Test()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new { message = "Code boþ olamaz." });
            }

            return Ok(new { message = "Code alýndý.", code });
        }

        [HttpPost]
        public IActionResult TestLdap([FromBody] LdapTestRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Ldap isteði boþ olamaz." });
            }

            if (string.IsNullOrWhiteSpace(request.Server))
            {
                return BadRequest(new { message = "Server bilgisi zorunludur." });
            }

            if (string.IsNullOrWhiteSpace(request.User) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Kullanýcý adý ve þifre zorunludur." });
            }

            try
            {
                var port = 639;//request.Port ?? (request.UseSsl ? 636 : 389);
                var identifier = new LdapDirectoryIdentifier(request.Server, port, false, false);
                var credential = BuildCredential(request);

                using var connection = new LdapConnection(identifier, credential, AuthType.Negotiate);
                connection.SessionOptions.ProtocolVersion = 3;
                connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
                if (request.UseSsl)
                {
                    connection.SessionOptions.SecureSocketLayer = true;
                    if (!string.IsNullOrWhiteSpace(request.TargetHost))
                    {
                        connection.SessionOptions.HostName = request.TargetHost;
                    }
                }

                connection.Bind();

                var baseDn = string.IsNullOrWhiteSpace(request.SearchBase)
                    ? GetDefaultNamingContext(connection)
                    : request.SearchBase;

                if (string.IsNullOrWhiteSpace(baseDn))
                {
                    return BadRequest(new { message = "Base DN bulunamadý. SearchBase gönderin." });
                }

                var filter = string.IsNullOrWhiteSpace(request.Filter)
                    ? "(&(objectCategory=person)(objectClass=user))"
                    : request.Filter;

                var searchRequest = new SearchRequest(
                    baseDn,
                    filter,
                    SearchScope.Subtree,
                    "sAMAccountName",
                    "displayName",
                    "distinguishedName");

                if (request.MaxResults.HasValue)
                {
                    searchRequest.SizeLimit = request.MaxResults.Value;
                }

                if (request.TimeLimitSeconds.HasValue)
                {
                    searchRequest.TimeLimit = TimeSpan.FromSeconds(request.TimeLimitSeconds.Value);
                }

                var response = (SearchResponse)connection.SendRequest(searchRequest);

                var users = new List<object>();
                foreach (SearchResultEntry entry in response.Entries)
                {
                    var sam = GetAttributeValue(entry, "sAMAccountName");
                    var displayName = GetAttributeValue(entry, "displayName");
                    var dn = GetAttributeValue(entry, "distinguishedName");
                    users.Add(new { samAccountName = sam, displayName, distinguishedName = dn });
                }

                return Ok(new
                {
                    success = true,
                    baseDn,
                    count = users.Count,
                    users
                });
            }
            catch (LdapException ex)
            {
                return BadRequest(new { message = "LDAP hata: " + ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static NetworkCredential BuildCredential(LdapTestRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Domain) && !request.User.Contains("\\") && !request.User.Contains("@"))
            {
                return new NetworkCredential(request.User, request.Password, request.Domain);
            }

            return new NetworkCredential(request.User, request.Password);
        }

        private static string GetDefaultNamingContext(LdapConnection connection)
        {
            var rootRequest = new SearchRequest(null, "(objectClass=*)", SearchScope.Base, "defaultNamingContext");
            var rootResponse = (SearchResponse)connection.SendRequest(rootRequest);
            if (rootResponse.Entries.Count == 0)
            {
                return null;
            }

            var attr = rootResponse.Entries[0].Attributes["defaultNamingContext"];
            return attr != null && attr.Count > 0 ? attr[0]?.ToString() : null;
        }

        private static string GetAttributeValue(SearchResultEntry entry, string name)
        {
            var attr = entry.Attributes[name];
            return attr != null && attr.Count > 0 ? attr[0]?.ToString() : null;
        }








    }

    public class TestCodeRequest
    {
        public string Code { get; set; }
    }

    public class LdapTestRequest
    {
        public string Server { get; set; }
        public int? Port { get; set; }
        public bool UseSsl { get; set; }
        public string TargetHost { get; set; }
        public string Domain { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string SearchBase { get; set; }
        public string Filter { get; set; }
        public int? MaxResults { get; set; } = 10;
        public int? TimeLimitSeconds { get; set; } = 10;
    }






}
