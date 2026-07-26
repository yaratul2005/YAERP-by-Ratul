using FluentValidation;

namespace YAERP.Application.Purchasing.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(v => v.VendorId).NotEmpty();
        RuleFor(v => v.WarehouseId).NotEmpty();
        RuleFor(v => v.OrderNumber).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Items).NotEmpty();
        RuleForEach(v => v.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductId).NotEmpty();
            items.RuleFor(i => i.Quantity).GreaterThan(0);
            items.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
