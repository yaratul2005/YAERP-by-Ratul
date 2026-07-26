using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Sales;

namespace YAERP.Infrastructure.Persistence.Configurations.Sales;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(id => id.Value, value => new CustomerId(value));
        builder.Property(c => c.TenantId).HasConversion(id => id.Value, value => new TenantId(value)).IsRequired();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(250);
        builder.Property(c => c.CreditLimit).HasPrecision(18, 4);
    }
}
