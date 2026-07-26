using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Entities;

namespace YAERP.Infrastructure.Persistence.Configurations;

public class ApprovalStepLogConfiguration : IEntityTypeConfiguration<ApprovalStepLog>
{
    public void Configure(EntityTypeBuilder<ApprovalStepLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ApprovalLevel).HasMaxLength(50);
        builder.Property(x => x.Action).HasMaxLength(50);
    }
}
