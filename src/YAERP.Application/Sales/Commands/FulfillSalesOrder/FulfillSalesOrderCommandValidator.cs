using FluentValidation;

namespace YAERP.Application.Sales.Commands.FulfillSalesOrder;

public class FulfillSalesOrderCommandValidator : AbstractValidator<FulfillSalesOrderCommand>
{
    public FulfillSalesOrderCommandValidator()
    {
        RuleFor(v => v.SalesOrderId).NotEmpty();
    }
}
