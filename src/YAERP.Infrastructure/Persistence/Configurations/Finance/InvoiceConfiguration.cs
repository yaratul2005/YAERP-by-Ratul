using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Purchasing;
using YAERP.Domain.Sales;
using YAERP.Domain.Finance;

namespace YAERP.Infrastructure.Persistence.Configurations.Finance;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasConversion(id => id.Value, value => new InvoiceId(value));
        builder.Property(i => i.TenantId).HasConversion(id => id.Value, value => new TenantId(value)).IsRequired();
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);

        builder.Property(i => i.SalesOrderId).HasConversion(id => id == null ? (System.Guid?)null : id.Value, value => value.HasValue ? new SalesOrderId(value.Value) : null);
        builder.Property(i => i.PurchaseOrderId).HasConversion(id => id == null ? (System.Guid?)null : id.Value, value => value.HasValue ? new PurchaseOrderId(value.Value) : null);

        builder.Property(i => i.TotalAmount).HasPrecision(18, 4);

        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique();
    }
}
