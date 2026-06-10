using Core.Persistence.Repositories;
using System.Text.Json.Serialization;

namespace Core.Security.Entities
{
    public class UserRole : Entity
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
        //[JsonIgnore]
        public virtual Role Role { get; set; }
    }
}