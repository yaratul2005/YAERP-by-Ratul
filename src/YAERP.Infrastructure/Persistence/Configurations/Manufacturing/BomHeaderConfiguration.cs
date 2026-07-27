using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Entities.Manufacturing;

namespace YAERP.Infrastructure.Persistence.Configurations.Manufacturing;

public class BomHeaderConfiguration : IEntityTypeConfiguration<BomHeader>
{
    public void Configure(EntityTypeBuilder<BomHeader> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AssemblyProductId);
        builder.HasIndex(x => new { x.AssemblyProductId, x.IsActive });

        builder.Property(x => x.BaseQuantity).HasPrecision(18, 4);
    }
}
