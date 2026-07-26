using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.HR.Commands.ProcessPayroll;

public record ProcessPayrollCommand(
    Guid EmployeeId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal GrossSalary,
    decimal TaxDeductions,
    Guid PayrollExpenseAccountId,
    Guid PayrollLiabilityAccountId) : ICommand<Guid>;
