using FluentValidation;

namespace YAERP.Application.Inventory.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.SKU)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(v => v.UnitOfMeasureId)
            .NotEmpty();

        RuleFor(v => v.StandardCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(v => v.ListPrice)
            .GreaterThanOrEqualTo(0);
    }
}
