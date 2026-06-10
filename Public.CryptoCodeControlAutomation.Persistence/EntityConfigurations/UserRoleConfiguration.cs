using Core.Persistence.Repositories;
using Core.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfiguration
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {

        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });
            //builder.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
            //builder.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
        }
    }
}