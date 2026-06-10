using CryptoCodeControlAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfiguration
{
    public class PlannedOrderSalesLinkConfiguration : IEntityTypeConfiguration<PlannedOrderSalesLink>
    {
        public void Configure(EntityTypeBuilder<PlannedOrderSalesLink> builder)
        {
            builder.ToTable("PlannedOrderSalesLinks", "cz");

            builder.HasKey(x => x.LinkId);

            builder.Property(x => x.PlannedOrderId).IsRequired();
            builder.Property(x => x.SalesOrderItemId).IsRequired();
            builder.Property(x => x.ReservedUnitQty).IsRequired();
            builder.Property(x => x.ConsumedUnitQty).HasDefaultValue(0).IsRequired();
            builder.Property(x => x.Status).HasDefaultValue(0).IsRequired();
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("sysdatetime()").IsRequired();
        }
    }
}
