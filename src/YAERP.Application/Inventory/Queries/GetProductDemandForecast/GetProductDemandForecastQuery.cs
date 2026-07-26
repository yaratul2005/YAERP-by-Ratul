using System;
using System.Collections.Generic;
using YAERP.Application.Common.Interfaces.AI;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Inventory.Queries.GetProductDemandForecast;

/// <summary>
/// CQRS query to generate an SSA time-series demand forecast for a product/warehouse pair.
/// Returns the rich <see cref="InventoryForecastResultDto"/> with confidence bounds and stockout risk.
/// </summary>
public record GetProductDemandForecastQuery(
    Guid ProductId,
    Guid WarehouseId,
    int HorizonDays = 30) : IQuery<InventoryForecastResultDto>;
