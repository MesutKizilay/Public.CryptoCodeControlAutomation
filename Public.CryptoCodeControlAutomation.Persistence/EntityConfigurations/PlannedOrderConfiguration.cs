using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfiguration
{
    public class PlannedOrderConfiguration : IEntityTypeConfiguration<PlannedOrder>
    {
        public void Configure(EntityTypeBuilder<PlannedOrder> builder)
        {
            builder.ToTable("PlannedOrders", "cz");

            builder.HasKey(p => p.PlannedOrderId);

            builder.Property(p => p.PlannedOrderNo).HasMaxLength(64).IsRequired();
            builder.Property(p => p.MaterialNo).HasMaxLength(64).IsRequired();
            builder.Property(p => p.LineCode).HasMaxLength(16).IsRequired();
            builder.Property(p => p.TotalCaseQty).IsRequired(false);
            builder.Property(p => p.TotalUnitQty).IsRequired();

            builder.Property(p => p.P1Enabled).IsRequired();
            builder.Property(p => p.P2Enabled).IsRequired();
            builder.Property(p => p.P3Enabled).IsRequired();
            builder.Property(p => p.P4Enabled).IsRequired();

            builder.Property(p => p.Status)
                .HasConversion<byte>()
                .IsRequired();

            builder.Property(p => p.StartedAt).IsRequired(false);
            builder.Property(p => p.CompletedAt).IsRequired(false);
            builder.Property(p => p.CreatedAt).HasDefaultValueSql("sysdatetime()").IsRequired();
        }
    }
}
