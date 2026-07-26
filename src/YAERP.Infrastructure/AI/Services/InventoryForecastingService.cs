using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Interfaces.AI;
using YAERP.Domain.Inventory;

namespace YAERP.Infrastructure.AI.Services;

/// <summary>Internal ML.NET input schema for SSA time-series training.</summary>
internal sealed class StockData
{
    public float Quantity { get; set; }
}

/// <summary>Internal ML.NET output schema holding predicted columns.</summary>
internal sealed class StockForecastOutput
{
    public float[]? ForecastedQuantity { get; set; }
    public float[]? LowerBound { get; set; }
    public float[]? UpperBound { get; set; }
}

/// <summary>
/// ML.NET Singular Spectrum Analysis (SSA) inventory demand forecasting engine.
/// Queries historical <see cref="StockMovement"/> records to train a per-product/warehouse
/// time-series model and projects demand for a configurable horizon.
/// </summary>
public sealed class InventoryForecastingService : IInventoryForecastingService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<InventoryForecastingService> _logger;

    /// <summary>Minimum daily data points required to train SSA meaningfully.</summary>
    private const int MinDataPoints = 10;

    public InventoryForecastingService(
        IApplicationDbContext context,
        ILogger<InventoryForecastingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ──────────────────────────────────────────────────────────────
    // Primary contract (rich DTO with confidence bounds)
    // ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<InventoryForecastResultDto> ForecastProductDemandAsync(
        Guid productId,
        Guid warehouseId,
        int horizonDays = 30,
        CancellationToken cancellationToken = default)
    {
        var pId = new ProductId(productId);
        var wId = new WarehouseId(warehouseId);

        _logger.LogInformation(
            "Generating {Horizon}-day SSA demand forecast for Product {ProductId} / Warehouse {WarehouseId}",
            horizonDays, productId, warehouseId);

        // ── 1. Fetch outbound demand history ──────────────────────
        var movements = await _context.StockMovements
            .AsNoTracking()
            .Where(m => m.ProductId == pId && m.WarehouseId == wId &&
                        (m.MovementType == StockMovementType.OutboundShipment ||
                         m.MovementType == StockMovementType.TransferOut))
            .OrderBy(m => m.MovementDateUtc)
            .ToListAsync(cancellationToken);

        // ── 2. Return empty forecast if insufficient data ─────────
        if (movements.Count < MinDataPoints)
        {
            _logger.LogWarning(
                "Only {Count} data points available for Product {ProductId}; returning zero forecast.",
                movements.Count, productId);

            return BuildEmptyForecast(productId, warehouseId, horizonDays);
        }

        // ── 3. Aggregate to daily demand series ───────────────────
        var dailySeries = movements
            .GroupBy(m => m.MovementDateUtc.Date)
            .OrderBy(g => g.Key)
            .Select(g => new StockData { Quantity = (float)g.Sum(x => x.Quantity) })
            .ToList();

        // ── 4. Train SSA model ────────────────────────────────────
        var mlContext = new MLContext(seed: 42);
        var seriesLength = dailySeries.Count;
        var windowSize = Math.Max(2, seriesLength / 2);

        var dataView = mlContext.Data.LoadFromEnumerable(dailySeries);

        var pipeline = mlContext.Forecasting.ForecastBySsa(
            outputColumnName: "ForecastedQuantity",
            inputColumnName: "Quantity",
            windowSize: windowSize,
            seriesLength: seriesLength,
            trainSize: seriesLength,
            horizon: horizonDays,
            confidenceLevel: 0.95f,
            confidenceLowerBoundColumn: "LowerBound",
            confidenceUpperBoundColumn: "UpperBound");

        var model = pipeline.Fit(dataView);
        var engine = model.CreateTimeSeriesEngine<StockData, StockForecastOutput>(mlContext);
        var forecast = engine.Predict();

        var predicted = NormalizeFloatArray(forecast.ForecastedQuantity, horizonDays);
        var lower = NormalizeFloatArray(forecast.LowerBound, horizonDays);
        var upper = NormalizeFloatArray(forecast.UpperBound, horizonDays);

        // ── 5. Compute current on-hand stock ──────────────────────
        var allMovements = await _context.StockMovements
            .AsNoTracking()
            .Where(m => m.ProductId == pId && m.WarehouseId == wId)
            .Select(m => new { m.MovementType, m.Quantity })
            .ToListAsync(cancellationToken);

        decimal onHandStock = 0m;
        foreach (var m in allMovements)
        {
            onHandStock += (m.MovementType == StockMovementType.InboundReceipt ||
                            m.MovementType == StockMovementType.TransferIn ||
                            m.MovementType == StockMovementType.InventoryAdjustment)
                ? m.Quantity
                : -m.Quantity;
        }

        onHandStock = Math.Max(0m, onHandStock);

        // ── 6. Evaluate stockout risk ─────────────────────────────
        // Walk the forecast day-by-day to find when cumulative demand exceeds stock.
        var (isStockoutRisk, daysUntilStockout) = CalculateStockoutRisk(
            onHandStock, predicted, horizonDays);

        _logger.LogInformation(
            "Forecast complete — StockoutRisk={Risk}, DaysUntilStockout={Days}",
            isStockoutRisk, daysUntilStockout);

        return new InventoryForecastResultDto(
            ProductId: productId,
            WarehouseId: warehouseId,
            ForecastedValues: predicted,
            LowerBoundConfidence: lower,
            UpperBoundConfidence: upper,
            IsStockoutRisk: isStockoutRisk,
            DaysUntilStockout: daysUntilStockout);
    }

    // ──────────────────────────────────────────────────────────────
    // Legacy overload (backward compat for existing callers)
    // ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ProductDemandForecastDto> GenerateForecastAsync(
        Guid productId,
        Guid warehouseId,
        int horizonDays = 30)
    {
        var result = await ForecastProductDemandAsync(
            productId, warehouseId, horizonDays, CancellationToken.None);

        var legacyDemand = result.ForecastedValues
            .Select(f => (decimal)Math.Max(0f, f))
            .ToArray();

        return new ProductDemandForecastDto(
            productId,
            warehouseId,
            legacyDemand,
            result.IsStockoutRisk);
    }

    // ──────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────

    private static IReadOnlyList<float> NormalizeFloatArray(float[]? source, int length)
    {
        if (source is null || source.Length == 0)
            return new float[length];

        // Clamp negative forecasted quantities to 0 (demand cannot be negative).
        var result = new float[length];
        for (int i = 0; i < length && i < source.Length; i++)
            result[i] = Math.Max(0f, source[i]);

        return result;
    }

    private static (bool IsRisk, int Days) CalculateStockoutRisk(
        decimal onHand, IReadOnlyList<float> dailyForecast, int horizon)
    {
        decimal running = onHand;
        for (int day = 0; day < horizon; day++)
        {
            running -= (decimal)dailyForecast[day];
            if (running <= 0m)
                return (true, day + 1);
        }
        return (false, horizon);
    }

    private static InventoryForecastResultDto BuildEmptyForecast(
        Guid productId, Guid warehouseId, int horizonDays)
    {
        var zeros = new float[horizonDays];
        return new InventoryForecastResultDto(
            ProductId: productId,
            WarehouseId: warehouseId,
            ForecastedValues: zeros,
            LowerBoundConfidence: zeros,
            UpperBoundConfidence: zeros,
            IsStockoutRisk: false,
            DaysUntilStockout: horizonDays);
    }
}
