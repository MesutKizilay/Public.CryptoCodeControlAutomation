using Core.Persistence.Repositories;
using System.Text.Json.Serialization;

namespace Core.Security.Entities
{
    public class Role : Entity
    {
        public int RoleId { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserRole> UserRoles { get; set; } = null!;
    }
}