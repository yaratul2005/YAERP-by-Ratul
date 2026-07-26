using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.HR.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    string Department,
    decimal BaseSalary) : ICommand<Guid>;
