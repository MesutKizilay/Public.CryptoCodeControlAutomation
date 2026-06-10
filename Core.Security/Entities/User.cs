using Core.Persistence.Repositories;

namespace Core.Security.Entities
{
    public class User : Entity
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? FullName { get; set; }
        public bool IsEnabled { get; set; }
        public bool RequiresLdapAuthentication { get; set; } = true;
        public DateTime CreatedUtc { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = null!;
    }
}
