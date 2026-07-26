using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Purchasing;

namespace YAERP.Infrastructure.Persistence.Configurations.Purchasing;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasConversion(id => id.Value, value => new PurchaseOrderId(value));
        builder.Property(p => p.TenantId).HasConversion(id => id.Value, value => new TenantId(value)).IsRequired();
        builder.Property(p => p.VendorId).HasConversion(id => id.Value, value => new VendorId(value)).IsRequired();
        builder.Property(p => p.WarehouseId).HasConversion(id => id.Value, value => new WarehouseId(value)).IsRequired();
        builder.Property(p => p.OrderNumber).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Status).HasConversion<string>().IsRequired();

        // Owns Many mapping for line items
        builder.OwnsMany(p => p.Items, ib =>
        {
            ib.ToTable("PurchaseOrderItems");
            ib.WithOwner().HasForeignKey("PurchaseOrderId");
            ib.HasKey(i => i.Id);
            ib.Property(i => i.Id).HasConversion(id => id.Value, value => new PurchaseOrderItemId(value));
            ib.Property(i => i.ProductId).HasConversion(id => id.Value, value => new ProductId(value)).IsRequired();
            ib.Property(i => i.Quantity).HasPrecision(18, 4);
            ib.Property(i => i.UnitPrice).HasPrecision(18, 4);
        });

        // Unique Index on (TenantId, OrderNumber)
        builder.HasIndex(p => new { p.TenantId, p.OrderNumber }).IsUnique();
    }
}
