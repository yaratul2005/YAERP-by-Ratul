using FluentValidation;

namespace YAERP.Application.Purchasing.Commands.ReceivePurchaseOrder;

public class ReceivePurchaseOrderCommandValidator : AbstractValidator<ReceivePurchaseOrderCommand>
{
    public ReceivePurchaseOrderCommandValidator()
    {
        RuleFor(v => v.PurchaseOrderId).NotEmpty();
    }
}
