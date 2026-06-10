using Core.Security.Encryption;
using Core.Security.Entities;
using Core.Security.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core.Security.JWT
{
    public class JwtHelper : ITokenHelper
    {
        private IConfiguration _configuration { get; }
        private readonly TokenOptions _tokenOptions;
        private DateTime _accessTokenExpiration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            const string configurationSection = "TokenOptions";
            _tokenOptions = _configuration.GetSection(configurationSection).Get<TokenOptions>()
                ?? throw new NullReferenceException($"{configurationSection} section cannot found in configuration");
        }

        //public RefreshToken CreateRefreshToken(User user, string ipAddress)
        //{
        //    RefreshToken refreshToken = new RefreshToken()
        //    {
        //        UserId = user.Id,
        //        Token=RandomRefreshToken(),
        //        Expires=DateTime.Now.AddDays(7),
        //        CreatedByIp=ipAddress
        //    };

        //    return refreshToken;
        //}

        public AccessToken CreateToken(User user, IList<Role> roles)
        {
            _accessTokenExpiration = DateTime.Now.AddSeconds(_tokenOptions.AccessTokenExpiration);
            SecurityKey securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            SigningCredentials signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
            JwtSecurityToken jwt = CreateJwtSecurityToken(_tokenOptions, user, signingCredentials, roles);
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            string? token = jwtSecurityTokenHandler.WriteToken(jwt);

            return new AccessToken { Token = token, Expiration = _accessTokenExpiration };
        }

        public JwtSecurityToken CreateJwtSecurityToken(TokenOptions tokenOptions, User user,
            SigningCredentials signingCredentials, IList<Role> roles)
        {
            JwtSecurityToken jwt = new JwtSecurityToken(issuer: tokenOptions.Issuer,
                                                        audience: tokenOptions.Audience,
                                                        expires: _accessTokenExpiration,
                                                        notBefore: DateTime.Now,
                                                        claims: SetClaims(user, roles),
                                                        signingCredentials: signingCredentials);
            return jwt;
        }

        private IEnumerable<Claim> SetClaims(User user, IList<Role> roles)
        {
            List<Claim> claims = new();
            claims.AddNameIdentifier(user.UserId.ToString());
            claims.AddEmail(user.FullName ?? user.Username);
            claims.AddName(user.Username);
            claims.AddRoles(roles.Select(c => c.Name).ToArray());
            return claims;
        }

        //private string RandomRefreshToken()
        //{
        //    byte[] numberByte = new byte[32];
        //    using var random = RandomNumberGenerator.Create();
        //    random.GetBytes(numberByte);
        //    return Convert.ToBase64String(numberByte);
        //}
    }
}