using CryptoCodeControlAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfigurations
{
    public class CodeAdjustmentLogItemConfiguration : IEntityTypeConfiguration<CodeAdjustmentLogItem>
    {
        public void Configure(EntityTypeBuilder<CodeAdjustmentLogItem> builder)
        {
            builder.ToTable("CodeAdjustmentLogItems", "cz");
            builder.HasKey(x => x.CodeAdjustmentLogItemId);
            builder.Property(x => x.CodeAdjustmentLogId).IsRequired();
            builder.Property(x => x.CodeId).IsRequired();
            builder.Property(x => x.CodeValue).HasMaxLength(128).IsRequired();
            builder.Property(x => x.OldStatus).HasConversion<byte>().IsRequired();
            builder.Property(x => x.NewStatus).HasConversion<byte>().IsRequired();
            builder.Property(x => x.OldShiftDate).HasColumnType("date").IsRequired(false);
            builder.Property(x => x.NewShiftDate).HasColumnType("date").IsRequired(false);
            builder.Property(x => x.OldProducedAt).IsRequired(false);
            builder.Property(x => x.NewProducedAt).IsRequired(false);
            builder.Property(x => x.OldExpirationDate).HasColumnType("date").IsRequired(false);
            builder.Property(x => x.NewExpirationDate).HasColumnType("date").IsRequired(false);
        }
    }
}
