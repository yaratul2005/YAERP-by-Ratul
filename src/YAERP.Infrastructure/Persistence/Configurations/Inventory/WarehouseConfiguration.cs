using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;

namespace YAERP.Infrastructure.Persistence.Configurations.Inventory;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, value => new WarehouseId(value));

        builder.Property(w => w.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(w => w.Address)
            .HasMaxLength(500);

        // Unique Index on (TenantId, Code)
        builder.HasIndex(w => new { w.TenantId, w.Code }).IsUnique();
    }
}
