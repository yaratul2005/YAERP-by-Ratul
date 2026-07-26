using FluentValidation;

namespace YAERP.Application.Finance.Commands.CreateAccount;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(v => v.AccountNumber).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Name).NotEmpty().MaximumLength(250);
        RuleFor(v => v.Type).IsInEnum();
    }
}
