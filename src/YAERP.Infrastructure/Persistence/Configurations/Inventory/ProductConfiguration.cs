using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;

namespace YAERP.Infrastructure.Persistence.Configurations.Inventory;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new ProductId(value));

        builder.Property(p => p.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Barcode)
            .HasMaxLength(100);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.ProductCategoryId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value.HasValue ? new ProductCategoryId(value.Value) : null);

        builder.Property(p => p.UnitOfMeasureId)
            .HasConversion(id => id.Value, value => new UnitOfMeasureId(value))
            .IsRequired();

        builder.Property(p => p.StandardCost)
            .HasPrecision(18, 4);

        builder.Property(p => p.ListPrice)
            .HasPrecision(18, 4);

        // Unique Index on (TenantId, SKU)
        builder.HasIndex(p => new { p.TenantId, p.SKU }).IsUnique();
    }
}
