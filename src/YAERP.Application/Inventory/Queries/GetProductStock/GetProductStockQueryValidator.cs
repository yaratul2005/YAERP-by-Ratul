using FluentValidation;

namespace YAERP.Application.Inventory.Queries.GetProductStock;

public class GetProductStockQueryValidator : AbstractValidator<GetProductStockQuery>
{
    public GetProductStockQueryValidator()
    {
        RuleFor(v => v.ProductId).NotEmpty();
    }
}
