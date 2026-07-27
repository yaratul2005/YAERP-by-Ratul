using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Entities.Warehouse;

namespace YAERP.Infrastructure.Persistence.Configurations.Warehouse;

public class WarehouseZoneConfiguration : IEntityTypeConfiguration<WarehouseZone>
{
    public void Configure(EntityTypeBuilder<WarehouseZone> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.ZoneType);
    }
}

public class WarehouseBinConfiguration : IEntityTypeConfiguration<WarehouseBin>
{
    public void Configure(EntityTypeBuilder<WarehouseBin> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ZoneId);
        builder.HasIndex(x => x.BinCode).IsUnique();

        builder.Property(x => x.MaxVolumeCubicMeters).HasPrecision(18, 4);
        builder.Property(x => x.MaxWeightKg).HasPrecision(18, 4);
    }
}

public class InventoryLotConfiguration : IEntityTypeConfiguration<InventoryLot>
{
    public void Configure(EntityTypeBuilder<InventoryLot> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.LotNumber).IsUnique();
        builder.HasIndex(x => x.ExpirationDate);
    }
}

public class ProductSerialNumberConfiguration : IEntityTypeConfiguration<ProductSerialNumber>
{
    public void Configure(EntityTypeBuilder<ProductSerialNumber> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.SerialNumber).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}

public class InventoryCostLayerConfiguration : IEntityTypeConfiguration<InventoryCostLayer>
{
    public void Configure(EntityTypeBuilder<InventoryCostLayer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.LayerDateUtc);

        builder.Property(x => x.OriginalQuantity).HasPrecision(18, 4);
        builder.Property(x => x.RemainingQuantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
    }
}
