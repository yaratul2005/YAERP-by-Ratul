using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.HR.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    string Department,
    decimal BaseSalary,
    string JobTitle = "Staff Member",
    DateTime DateOfBirth = default,
    DateTime HireDate = default,
    string PayFrequency = "Monthly") : ICommand<Guid>;
