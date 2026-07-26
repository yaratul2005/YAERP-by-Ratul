using System;
using YAERP.Application.Common.Interfaces.AI;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Inventory.Queries.GetProductDemandForecast;

public record GetProductDemandForecastQuery(Guid ProductId, Guid WarehouseId, int HorizonDays = 30) : IQuery<ProductDemandForecastDto>;
