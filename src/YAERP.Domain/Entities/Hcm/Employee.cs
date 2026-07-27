using System;

namespace YAERP.Domain.Entities.Hcm;

public class Employee
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string PayFrequency { get; set; } = "Monthly"; // Monthly, BiWeekly, Weekly

    public string BankName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankRoutingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active, OnLeave, Terminated
}
