using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfigurations
{
    public class CodesConfiguration : IEntityTypeConfiguration<Code>
    {
        public void Configure(EntityTypeBuilder<Code> builder)
        {
            builder.ToTable("Codes", "cz");
            builder.HasKey(c => c.CodeId);
            builder.Property(c => c.CodeValue).HasColumnName("Code").HasMaxLength(128).IsRequired();
            builder.Property(c => c.SalesOrderItemId).IsRequired();
            builder.Property(c => c.PlannedOrderId).IsRequired(false);
            builder.Property(c => c.StationId).IsRequired(false);
            builder.Property(c => c.PackagingLevel).IsRequired(false);
            builder.Property(c => c.Status)
                .HasConversion<byte>()
                .HasDefaultValue(CodeStatus.Available)
                .IsRequired();
            builder.Property(c => c.AllocatedAt).IsRequired(false);
            builder.Property(c => c.ProducedAt).IsRequired(false);
            builder.Property(c => c.ShiftDate).HasColumnType("date").IsRequired(false);
            builder.Property(c => c.ExpirationDate).HasColumnType("date").IsRequired(false);
            builder.Property(c => c.UpdatedAt).HasDefaultValueSql("sysdatetime()").IsRequired();
        }
    }
}
