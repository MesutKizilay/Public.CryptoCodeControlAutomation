using Core.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasQueryFilter(u => u.IsEnabled);
            builder.Property(u => u.RequiresLdapAuthentication)
                .HasDefaultValue(true)
                .IsRequired();
            //builder.HasData(_seeds);
            //builder.HasMany(u => u.UserRoles).WithOne(ur => ur.User).HasForeignKey(ur => ur.UserId);
        }

        private IEnumerable<User> _seeds
        {
            get
            {
                User user = new User()
                {
                    Username = "Mesut",
                    FullName = "Mesut Kızılay",
                    PasswordHash = "1",
                    IsEnabled = true,
                    UserId = 10,
                };

                yield return user;
            }
        }
    }
}
