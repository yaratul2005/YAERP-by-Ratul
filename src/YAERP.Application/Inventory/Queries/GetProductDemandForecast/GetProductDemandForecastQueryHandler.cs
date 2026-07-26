using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces.AI;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Inventory.Queries.GetProductDemandForecast;

public class GetProductDemandForecastQueryHandler : IQueryHandler<GetProductDemandForecastQuery, ProductDemandForecastDto>
{
    private readonly IInventoryForecastingService _forecastingService;

    public GetProductDemandForecastQueryHandler(IInventoryForecastingService forecastingService)
    {
        _forecastingService = forecastingService;
    }

    public async Task<Result<ProductDemandForecastDto>> Handle(GetProductDemandForecastQuery request, CancellationToken cancellationToken)
    {
        var forecast = await _forecastingService.GenerateForecastAsync(request.ProductId, request.WarehouseId, request.HorizonDays);
        return Result.Success(forecast);
    }
}
