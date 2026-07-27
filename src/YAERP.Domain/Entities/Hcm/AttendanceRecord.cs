using System;

namespace YAERP.Domain.Entities.Hcm;

public class AttendanceRecord
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }

    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public string Status { get; set; } = "Present"; // Present, Late, Absent, Overtime
}
