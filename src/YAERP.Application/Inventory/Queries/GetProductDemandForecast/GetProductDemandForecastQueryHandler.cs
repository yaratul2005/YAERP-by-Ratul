using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Interfaces.AI;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;

namespace YAERP.Application.Inventory.Queries.GetProductDemandForecast;

/// <summary>
/// Validates entity existence then delegates to the ML.NET SSA forecasting engine.
/// Read path — uses <c>AsNoTracking</c> existence checks, no EF change-tracker overhead.
/// </summary>
public sealed class GetProductDemandForecastQueryHandler
    : IQueryHandler<GetProductDemandForecastQuery, InventoryForecastResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IInventoryForecastingService _forecastingService;

    public GetProductDemandForecastQueryHandler(
        IApplicationDbContext context,
        IInventoryForecastingService forecastingService)
    {
        _context = context;
        _forecastingService = forecastingService;
    }

    public async Task<Result<InventoryForecastResultDto>> Handle(
        GetProductDemandForecastQuery request,
        CancellationToken cancellationToken)
    {
        // ── Validate Product exists ────────────────────────────────
        var pId = new ProductId(request.ProductId);
        bool productExists = await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == pId, cancellationToken);

        if (!productExists)
            return Result.Failure<InventoryForecastResultDto>(
                new Error("Forecast.ProductNotFound",
                    $"Product '{request.ProductId}' does not exist.",
                    ErrorType.NotFound));

        // ── Validate Warehouse exists ──────────────────────────────
        var wId = new WarehouseId(request.WarehouseId);
        bool warehouseExists = await _context.Warehouses
            .AsNoTracking()
            .AnyAsync(w => w.Id == wId, cancellationToken);

        if (!warehouseExists)
            return Result.Failure<InventoryForecastResultDto>(
                new Error("Forecast.WarehouseNotFound",
                    $"Warehouse '{request.WarehouseId}' does not exist.",
                    ErrorType.NotFound));

        // ── Invoke SSA forecasting engine ──────────────────────────
        var forecast = await _forecastingService.ForecastProductDemandAsync(
            request.ProductId,
            request.WarehouseId,
            request.HorizonDays,
            cancellationToken);

        return Result.Success(forecast);
    }
}
