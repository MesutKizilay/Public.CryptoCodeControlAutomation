using CryptoCodeControlAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfigurations
{
    public class CodeAdjustmentLogConfiguration : IEntityTypeConfiguration<CodeAdjustmentLog>
    {
        public void Configure(EntityTypeBuilder<CodeAdjustmentLog> builder)
        {
            builder.ToTable("CodeAdjustmentLogs", "cz");
            builder.HasKey(x => x.CodeAdjustmentLogId);
            builder.Property(x => x.OperationType).HasMaxLength(40).IsRequired();
            builder.Property(x => x.SalesOrderItemId).IsRequired(false);
            builder.Property(x => x.PlannedOrderId).IsRequired(false);
            builder.Property(x => x.FromStatus).HasConversion<byte>().IsRequired(false);
            builder.Property(x => x.ToStatus).HasConversion<byte>().IsRequired(false);
            builder.Property(x => x.FromShiftDate).HasColumnType("date").IsRequired(false);
            builder.Property(x => x.ToShiftDate).HasColumnType("date").IsRequired(false);
            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            builder.Property(x => x.CreatedBy).HasMaxLength(256).IsRequired(false);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("sysdatetime()").IsRequired();
            builder.HasMany(x => x.Items)
                   .WithOne(x => x.CodeAdjustmentLog)
                   .HasForeignKey(x => x.CodeAdjustmentLogId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
