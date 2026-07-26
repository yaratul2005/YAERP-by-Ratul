using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;

namespace YAERP.Infrastructure.Persistence.Configurations.Inventory;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new ProductCategoryId(value));

        builder.Property(c => c.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ParentCategoryId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value.HasValue ? new ProductCategoryId(value.Value) : null);

        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();
    }
}
