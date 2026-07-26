using FluentValidation;

namespace YAERP.Application.HR.Commands.ProcessPayroll;

public class ProcessPayrollCommandValidator : AbstractValidator<ProcessPayrollCommand>
{
    public ProcessPayrollCommandValidator()
    {
        RuleFor(v => v.EmployeeId).NotEmpty();
        RuleFor(v => v.PeriodStart).NotEmpty();
        RuleFor(v => v.PeriodEnd).NotEmpty().GreaterThan(v => v.PeriodStart);
        RuleFor(v => v.GrossSalary).GreaterThanOrEqualTo(0);
        RuleFor(v => v.TaxDeductions).GreaterThanOrEqualTo(0).LessThanOrEqualTo(v => v.GrossSalary);
        RuleFor(v => v.PayrollExpenseAccountId).NotEmpty();
        RuleFor(v => v.PayrollLiabilityAccountId).NotEmpty();
    }
}
