using Core.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfiguration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {

        public void Configure(EntityTypeBuilder<Role> builder)
        {
            //builder.HasQueryFilter(u => u.Status == true);
            builder.HasData(_seeds);
            //builder.HasMany(r => r.UserRoles).WithOne(ur=>ur.Role).HasForeignKey(ur=>ur.RoleId);
        }

        private IEnumerable<Role> _seeds
        {
            get
            {
                Role user = new Role()
                {
                    RoleId = 1,
                    Name = "Admin"
                };

                yield return user;
            }
        }
    }
}