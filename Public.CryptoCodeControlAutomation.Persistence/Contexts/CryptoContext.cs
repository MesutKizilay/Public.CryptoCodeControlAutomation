using Core.Security.Entities;
using CryptoCodeControlAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using System.Reflection;

namespace CryptoCodeControlAutomation.Persistence.Contexts
{
    public class CryptoContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public virtual DbSet<SalesOrderItem> SalesOrderItems { get; set; }
        public virtual DbSet<Code> Codes { get; set; }
        public virtual DbSet<UploadJob> UploadJobs { get; set; }
        public virtual DbSet<PlannedOrder> PlannedOrders { get; set; }
        public virtual DbSet<PlannedOrderSalesLink> PlannedOrderSalesLinks { get; set; }

        public CryptoContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
            //Database.EnsureCreated();
            //Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
