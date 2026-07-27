using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Entities.Manufacturing;

namespace YAERP.Infrastructure.Persistence.Configurations.Manufacturing;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.WorkOrderNumber).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PlannedStartDate);
        builder.HasIndex(x => x.BomHeaderId);

        builder.Property(x => x.TargetQuantity).HasPrecision(18, 4);
        builder.Property(x => x.CompletedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ScrappedQuantity).HasPrecision(18, 4);
    }
}
