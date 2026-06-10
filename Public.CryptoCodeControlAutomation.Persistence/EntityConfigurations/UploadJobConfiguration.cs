using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoCodeControlAutomation.Persistence.EntityConfigurations
{
    public class UploadJobConfiguration : IEntityTypeConfiguration<UploadJob>
    {
        public void Configure(EntityTypeBuilder<UploadJob> builder)
        {
            builder.ToTable("UploadJobs", "cz");
            builder.HasKey(u => u.UploadJobId);
            builder.Property(u => u.SalesOrderItemId).IsRequired();
            builder.Property(u => u.FilePath).HasMaxLength(512).IsRequired();
            builder.Property(u => u.Status)
                .HasConversion<byte>()
                .IsRequired();
            builder.Property(u => u.TotalRows).IsRequired(false);
            builder.Property(u => u.InsertedRows).IsRequired(false);
            builder.Property(u => u.ErrorText).HasMaxLength(2000).IsRequired(false);
            builder.Property(u => u.CreatedAt).HasDefaultValueSql("sysdatetime()").IsRequired();
            builder.Property(u => u.StartedAt).IsRequired(false);
            builder.Property(u => u.FinishedAt).IsRequired(false);
        }
    }
}
