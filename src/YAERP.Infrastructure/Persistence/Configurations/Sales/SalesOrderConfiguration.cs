using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Sales;

namespace YAERP.Infrastructure.Persistence.Configurations.Sales;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasConversion(id => id.Value, value => new SalesOrderId(value));
        builder.Property(s => s.TenantId).HasConversion(id => id.Value, value => new TenantId(value)).IsRequired();
        builder.Property(s => s.CustomerId).HasConversion(id => id.Value, value => new CustomerId(value)).IsRequired();
        builder.Property(s => s.WarehouseId).HasConversion(id => id.Value, value => new WarehouseId(value)).IsRequired();
        builder.Property(s => s.OrderNumber).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Status).HasConversion<string>().IsRequired();

        // Owns Many mapping for line items
        builder.OwnsMany(s => s.Items, ib =>
        {
            ib.ToTable("SalesOrderItems");
            ib.WithOwner().HasForeignKey("SalesOrderId");
            ib.HasKey(i => i.Id);
            ib.Property(i => i.Id).HasConversion(id => id.Value, value => new SalesOrderItemId(value));
            ib.Property(i => i.ProductId).HasConversion(id => id.Value, value => new ProductId(value)).IsRequired();
            ib.Property(i => i.Quantity).HasPrecision(18, 4);
            ib.Property(i => i.UnitPrice).HasPrecision(18, 4);
        });

        // Unique Index on (TenantId, OrderNumber)
        builder.HasIndex(s => new { s.TenantId, s.OrderNumber }).IsUnique();
    }
}
