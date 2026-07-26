using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.HR;

namespace YAERP.Infrastructure.Persistence.Configurations.HR;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasConversion(id => id.Value, value => new EmployeeId(value));
        builder.Property(e => e.TenantId).HasConversion(id => id.Value, value => new TenantId(value)).IsRequired();
        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Department).IsRequired().HasMaxLength(100);
        builder.Property(e => e.BaseSalary).HasPrecision(18, 4);

        builder.HasIndex(e => new { e.TenantId, e.EmployeeCode }).IsUnique();
    }
}
