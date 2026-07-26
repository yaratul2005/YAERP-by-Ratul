using FluentValidation;

namespace YAERP.Application.Inventory.Commands.CreateWarehouse;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(v => v.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(250);
    }
}
