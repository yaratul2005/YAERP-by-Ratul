using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;

namespace YAERP.Infrastructure.Persistence.Configurations.Inventory;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Id)
            .HasConversion(id => id.Value, value => new StockMovementId(value));

        builder.Property(sm => sm.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(sm => sm.ProductId)
            .HasConversion(id => id.Value, value => new ProductId(value))
            .IsRequired();

        builder.Property(sm => sm.WarehouseId)
            .HasConversion(id => id.Value, value => new WarehouseId(value))
            .IsRequired();

        builder.Property(sm => sm.MovementType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(sm => sm.Quantity)
            .HasPrecision(18, 4);

        builder.Property(sm => sm.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(sm => sm.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(sm => sm.CreatedByUserId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value.HasValue ? new UserId(value.Value) : null);

        // High-performance Index on (TenantId, ProductId, WarehouseId, MovementDateUtc)
        builder.HasIndex(sm => new { sm.TenantId, sm.ProductId, sm.WarehouseId, sm.MovementDateUtc });
    }
}
