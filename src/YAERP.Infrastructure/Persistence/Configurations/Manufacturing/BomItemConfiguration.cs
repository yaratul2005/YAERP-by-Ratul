using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Entities.Manufacturing;

namespace YAERP.Infrastructure.Persistence.Configurations.Manufacturing;

public class BomItemConfiguration : IEntityTypeConfiguration<BomItem>
{
    public void Configure(EntityTypeBuilder<BomItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.BomHeaderId);
        builder.HasIndex(x => x.ComponentProductId);

        builder.Property(x => x.QuantityPerAssembly).HasPrecision(18, 4);
        builder.Property(x => x.ScrapFactorPercent).HasPrecision(5, 2);
        builder.Property(x => x.YieldPercent).HasPrecision(5, 2);

        builder.HasOne<BomHeader>()
            .WithMany(b => b.Components)
            .HasForeignKey(x => x.BomHeaderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
