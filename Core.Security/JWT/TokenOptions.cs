using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.Security.JWT
{
    public class TokenOptions
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public long AccessTokenExpiration { get; set; }
        public string SecurityKey { get; set; }        
    }
}