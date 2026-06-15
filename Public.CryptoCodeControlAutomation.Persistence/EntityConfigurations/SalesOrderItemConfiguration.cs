using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfiguration
{
    public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
    {
        public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
        {
            builder.ToTable("SalesOrderItems", "cz");

            builder.HasKey(s => s.SalesOrderItemId);

            builder.Property(s => s.SalesOrderNo).HasMaxLength(32).IsRequired();
            builder.Property(s => s.SalesItemNo).HasMaxLength(16).IsRequired();
            builder.Property(s => s.MaterialNo).HasMaxLength(64).IsRequired();
            builder.Property(s => s.GTIN).HasMaxLength(32).IsRequired(false);
            builder.Property(s => s.SapCaseQty).IsRequired(false);
            builder.Property(s => s.SapPlannedUnitQty).IsRequired();
            builder.Property(s => s.RemainingUnitQty).IsRequired();

            builder.Property(s => s.IsOpen).HasDefaultValue(true);
            builder.Property(s => s.ApprovalStatus)
                .HasConversion<byte>()
                .HasDefaultValue(SalesOrderItemApprovalStatus.PendingApproval)
                .IsRequired();
            builder.Property(s => s.ProductionApprovedByUsername).HasMaxLength(100).IsRequired(false);
            builder.Property(s => s.ProductionApprovedAt).IsRequired(false);
            builder.Property(s => s.ShipmentApprovedByUsername).HasMaxLength(100).IsRequired(false);
            builder.Property(s => s.ShipmentApprovedAt).IsRequired(false);
            builder.Property(s => s.ShelfLifeValue)
                .HasDefaultValue(0)
                .IsRequired();
            builder.Property(s => s.ShelfLifeUnit)
                .HasConversion<byte>()
                .HasDefaultValue(ShelfLifeUnit.Day)
                .IsRequired();

            builder.Property(s => s.SapValidatedAt).HasDefaultValueSql("sysdatetime()");
            builder.Property(s => s.CreatedAt).HasDefaultValueSql("sysdatetime()");
            builder.Property(s => s.UpdatedAt).HasDefaultValueSql("sysdatetime()");

            builder.HasQueryFilter(u => u.Status != SalesOrderItemStatus.Cancelled);
        }
    }
}
