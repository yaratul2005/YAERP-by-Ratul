using FluentValidation;

namespace YAERP.Application.Finance.Commands.CreateInvoiceFromSalesOrder;

public class CreateInvoiceFromSalesOrderCommandValidator : AbstractValidator<CreateInvoiceFromSalesOrderCommand>
{
    public CreateInvoiceFromSalesOrderCommandValidator()
    {
        RuleFor(v => v.SalesOrderId).NotEmpty();
        RuleFor(v => v.InvoiceNumber).NotEmpty().MaximumLength(50);
        RuleFor(v => v.DueDateUtc).NotEmpty();
    }
}
