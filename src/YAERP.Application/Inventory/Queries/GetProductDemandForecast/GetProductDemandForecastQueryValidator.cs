using FluentValidation;

namespace YAERP.Application.Inventory.Queries.GetProductDemandForecast;

public class GetProductDemandForecastQueryValidator : AbstractValidator<GetProductDemandForecastQuery>
{
    public GetProductDemandForecastQueryValidator()
    {
        RuleFor(v => v.ProductId).NotEmpty();
        RuleFor(v => v.WarehouseId).NotEmpty();
        RuleFor(v => v.HorizonDays).GreaterThan(0).LessThanOrEqualTo(365);
    }
}
