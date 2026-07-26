using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Purchasing;

namespace YAERP.Infrastructure.Persistence.Configurations.Purchasing;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasConversion(id => id.Value, value => new VendorId(value));
        builder.Property(v => v.TenantId).HasConversion(id => id.Value, value => new TenantId(value)).IsRequired();
        builder.Property(v => v.Name).IsRequired().HasMaxLength(250);
    }
}
