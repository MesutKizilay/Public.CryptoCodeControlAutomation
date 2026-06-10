using Core.Persistence.Repositories;
using Core.Security.Entities;

namespace CryptoCodeControlAutomation.Application.Features.Users.Queries.GetList
{
    public class GetListUserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        //public string PasswordHash { get; set; }
        public string? FullName { get; set; }
        public bool IsEnabled { get; set; }
        public bool RequiresLdapAuthentication { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = null!;
    }
}
