using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.HR;

namespace YAERP.Infrastructure.Persistence.Configurations.HR;

public class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> builder)
    {
        builder.ToTable("Payrolls");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasConversion(id => id.Value, value => new PayrollId(value));
        builder.Property(p => p.TenantId).HasConversion(id => id.Value, value => new TenantId(value)).IsRequired();
        builder.Property(p => p.EmployeeId).HasConversion(id => id.Value, value => new EmployeeId(value)).IsRequired();
        builder.Property(p => p.GrossSalary).HasPrecision(18, 4);
        builder.Property(p => p.TaxDeductions).HasPrecision(18, 4);
        builder.Property(p => p.NetSalary).HasPrecision(18, 4);
    }
}
