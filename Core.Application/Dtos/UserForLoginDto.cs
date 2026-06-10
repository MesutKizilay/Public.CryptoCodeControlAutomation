using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Dtos
{
    public class UserForLoginDto
    {
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
    }
}