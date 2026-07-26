using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;

namespace YAERP.Domain.HR;

public class Payroll : AggregateRoot<PayrollId>
{
    private Payroll(PayrollId id, TenantId tenantId, EmployeeId employeeId, DateTime periodStart, DateTime periodEnd, decimal grossSalary, decimal taxDeductions, decimal netSalary, bool isProcessed) : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        GrossSalary = grossSalary;
        TaxDeductions = taxDeductions;
        NetSalary = netSalary;
        IsProcessed = isProcessed;
    }

    private Payroll() { }

    public TenantId TenantId { get; private set; } = default!;
    public EmployeeId EmployeeId { get; private set; } = default!;
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal TaxDeductions { get; private set; }
    public decimal NetSalary { get; private set; }
    public bool IsProcessed { get; private set; }

    public static Payroll Create(TenantId tenantId, EmployeeId employeeId, DateTime periodStart, DateTime periodEnd, decimal grossSalary, decimal taxDeductions)
    {
        var netSalary = grossSalary - taxDeductions;
        return new Payroll(new PayrollId(Guid.NewGuid()), tenantId, employeeId, periodStart, periodEnd, grossSalary, taxDeductions, netSalary, false);
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
    }
}
