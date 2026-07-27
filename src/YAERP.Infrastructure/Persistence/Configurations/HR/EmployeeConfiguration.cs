using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Entities.Hcm;

namespace YAERP.Infrastructure.Persistence.Configurations.Hcm;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.EmployeeCode).IsUnique();

        builder.Property(x => x.BaseSalary).HasPrecision(18, 4);
    }
}

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.EmployeeId);

        builder.Property(x => x.RegularHours).HasPrecision(5, 2);
        builder.Property(x => x.OvertimeHours).HasPrecision(5, 2);
    }
}
