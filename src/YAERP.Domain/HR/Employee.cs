using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;

namespace YAERP.Domain.HR;

public class Employee : AggregateRoot<EmployeeId>
{
    private Employee(EmployeeId id, TenantId tenantId, string employeeCode, string firstName, string lastName, string email, string department, decimal baseSalary, bool isActive) : base(id)
    {
        TenantId = tenantId;
        EmployeeCode = employeeCode;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Department = department;
        BaseSalary = baseSalary;
        IsActive = isActive;
    }

    private Employee() { }

    public TenantId TenantId { get; private set; } = default!;
    public string EmployeeCode { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public decimal BaseSalary { get; private set; }
    public bool IsActive { get; private set; }

    public static Employee Create(TenantId tenantId, string employeeCode, string firstName, string lastName, string email, string department, decimal baseSalary)
    {
        return new Employee(new EmployeeId(Guid.NewGuid()), tenantId, employeeCode, firstName, lastName, email, department, baseSalary, true);
    }
}
