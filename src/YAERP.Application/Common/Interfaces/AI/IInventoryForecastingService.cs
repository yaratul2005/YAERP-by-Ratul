using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces.AI;

/// <summary>
/// Immutable DTO carrying SSA time-series forecast results for a product/warehouse pair.
/// </summary>
public sealed record InventoryForecastResultDto(
    Guid ProductId,
    Guid WarehouseId,
    IReadOnlyList<float> ForecastedValues,
    IReadOnlyList<float> LowerBoundConfidence,
    IReadOnlyList<float> UpperBoundConfidence,
    bool IsStockoutRisk,
    int DaysUntilStockout);

/// <summary>
/// Legacy DTO retained for backward compatibility.
/// </summary>
public record ProductDemandForecastDto(
    Guid ProductId,
    Guid WarehouseId,
    decimal[] ForecastedDemand30Days,
    bool StockoutWarning);

/// <summary>
/// ML.NET-powered inventory demand forecasting contract.
/// </summary>
public interface IInventoryForecastingService
{
    /// <summary>
    /// Generates an SSA-based time-series demand forecast for the given product/warehouse.
    /// </summary>
    Task<InventoryForecastResultDto> ForecastProductDemandAsync(
        Guid productId,
        Guid warehouseId,
        int horizonDays = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Legacy overload for backward compatibility.
    /// </summary>
    Task<ProductDemandForecastDto> GenerateForecastAsync(
        Guid productId,
        Guid warehouseId,
        int horizonDays = 30);
}
