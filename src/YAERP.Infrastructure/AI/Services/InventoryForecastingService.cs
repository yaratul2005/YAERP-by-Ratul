using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Interfaces.AI;
using YAERP.Domain.Inventory;

namespace YAERP.Infrastructure.AI.Services;

public class StockData
{
    public float Quantity { get; set; }
}

public class StockForecast
{
    public float[]? ForecastedQuantity { get; set; }
}

public class InventoryForecastingService : IInventoryForecastingService
{
    private readonly IApplicationDbContext _context;

    public InventoryForecastingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDemandForecastDto> GenerateForecastAsync(Guid productId, Guid warehouseId, int horizonDays = 30)
    {
        var mlContext = new MLContext();

        // Get historical outbound shipments (demand) grouped by day
        var pId = new ProductId(productId);
        var wId = new WarehouseId(warehouseId);

        var movements = await _context.StockMovements
            .AsNoTracking()
            .Where(m => m.ProductId == pId && m.WarehouseId == wId &&
                       (m.MovementType == StockMovementType.OutboundShipment || m.MovementType == StockMovementType.TransferOut))
            .OrderBy(m => m.MovementDateUtc)
            .ToListAsync();

        if (movements.Count < 10)
        {
            // Not enough data, return safe default
            return new ProductDemandForecastDto(productId, warehouseId, new decimal[horizonDays], false);
        }

        var dailyDemand = movements
            .GroupBy(m => m.MovementDateUtc.Date)
            .Select(g => new StockData { Quantity = (float)g.Sum(x => x.Quantity) })
            .ToList();

        // Ensure we have enough data points for SSA
        var seriesLength = dailyDemand.Count;
        var windowSize = Math.Max(2, seriesLength / 2);

        var dataView = mlContext.Data.LoadFromEnumerable(dailyDemand);

        var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
            outputColumnName: "ForecastedQuantity",
            inputColumnName: "Quantity",
            windowSize: windowSize,
            seriesLength: seriesLength,
            trainSize: seriesLength,
            horizon: horizonDays,
            confidenceLevel: 0.95f,
            confidenceLowerBoundColumn: "LowerBound",
            confidenceUpperBoundColumn: "UpperBound");

        var model = forecastingPipeline.Fit(dataView);
        var forecastingEngine = model.CreateTimeSeriesEngine<StockData, StockForecast>(mlContext);

        var forecast = forecastingEngine.Predict();

        var predictedDemand = forecast.ForecastedQuantity?.Select(f => (decimal)Math.Max(0, f)).ToArray() ?? new decimal[horizonDays];

        // Total predicted demand for next 30 days
        var totalPredictedDemand = predictedDemand.Sum();

        // Calculate current stock
        var currentStockMovements = await _context.StockMovements
            .AsNoTracking()
            .Where(m => m.ProductId == pId && m.WarehouseId == wId)
            .Select(sm => new { sm.MovementType, sm.Quantity })
            .ToListAsync();

        decimal currentQuantity = 0;
        foreach (var sm in currentStockMovements)
        {
            if (sm.MovementType == StockMovementType.InboundReceipt ||
                sm.MovementType == StockMovementType.TransferIn ||
                sm.MovementType == StockMovementType.InventoryAdjustment)
            {
                currentQuantity += sm.Quantity;
            }
            else
            {
                currentQuantity -= sm.Quantity;
            }
        }

        // Warn if stockout risk (e.g., current stock covers less than 14 days of predicted demand)
        var demandNext14Days = predictedDemand.Take(14).Sum();
        bool stockoutWarning = currentQuantity < demandNext14Days;

        return new ProductDemandForecastDto(productId, warehouseId, predictedDemand, stockoutWarning);
    }
}
