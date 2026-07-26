using FluentValidation;

namespace YAERP.Application.HR.Commands.CreateEmployee;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(v => v.EmployeeCode).NotEmpty().MaximumLength(50);
        RuleFor(v => v.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.LastName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(v => v.Department).NotEmpty().MaximumLength(100);
        RuleFor(v => v.BaseSalary).GreaterThanOrEqualTo(0);
    }
}
