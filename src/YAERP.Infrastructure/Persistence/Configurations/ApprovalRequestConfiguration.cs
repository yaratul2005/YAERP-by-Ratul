using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Entities;

namespace YAERP.Infrastructure.Persistence.Configurations;

public class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TransactionAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrentApprovalLevel).HasMaxLength(50);
        builder.Property(x => x.Status).HasMaxLength(50);

        builder.HasMany(x => x.StepLogs)
               .WithOne()
               .HasForeignKey(x => x.ApprovalRequestId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.StepLogs).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
