using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces.AI;

public record ProductDemandForecastDto(Guid ProductId, Guid WarehouseId, decimal[] ForecastedDemand30Days, bool StockoutWarning);

public interface IInventoryForecastingService
{
    Task<ProductDemandForecastDto> GenerateForecastAsync(Guid productId, Guid warehouseId, int horizonDays = 30);
}
