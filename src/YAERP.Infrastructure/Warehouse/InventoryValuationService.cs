using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Warehouse;

namespace YAERP.Infrastructure.Warehouse;

public class InventoryValuationService : IInventoryValuationService
{
    private readonly IApplicationDbContext _dbContext;

    public InventoryValuationService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddCostLayerAsync(Guid productId, Guid warehouseId, decimal quantity, decimal unitCost, CancellationToken cancellationToken = default)
    {
        var layer = new InventoryCostLayer
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            LayerDateUtc = DateTime.UtcNow,
            OriginalQuantity = quantity,
            RemainingQuantity = quantity,
            UnitCost = unitCost
        };

        _dbContext.Set<InventoryCostLayer>().Add(layer);
        // Note: Callers should call SaveChangesAsync
    }

    public async Task<CostConsumptionResult> ConsumeInventoryAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantityToConsume,
        ValuationMethod method,
        decimal? standardCost = null,
        CancellationToken cancellationToken = default)
    {
        if (quantityToConsume <= 0) return new CostConsumptionResult(0, 0, 0);

        if (method == ValuationMethod.StandardCost)
        {
            if (!standardCost.HasValue) throw new ArgumentException("Standard cost must be provided for StandardCost valuation method.");

            // For standard costing, total cost consumed is always Qty * StandardCost.
            // PPV is typically recorded on receipt, not on consumption, but for symmetric return signature we leave PPV = 0 here or calculate if needed.
            return new CostConsumptionResult(quantityToConsume, quantityToConsume * standardCost.Value, standardCost.Value, 0m);
        }

        var layersQuery = _dbContext.Set<InventoryCostLayer>()
            .Where(l => l.ProductId == productId && l.WarehouseId == warehouseId && l.RemainingQuantity > 0);

        List<InventoryCostLayer> layers;

        if (method == ValuationMethod.FIFO)
        {
            layers = await layersQuery.OrderBy(l => l.LayerDateUtc).ToListAsync(cancellationToken);
        }
        else if (method == ValuationMethod.LIFO)
        {
            layers = await layersQuery.OrderByDescending(l => l.LayerDateUtc).ToListAsync(cancellationToken);
        }
        else if (method == ValuationMethod.AVCO)
        {
            // AVCO typically maintains a running average rather than consuming specific layers.
            // But if we must compute it from layers, it's total value / total quantity.
            var allLayers = await layersQuery.ToListAsync(cancellationToken);
            decimal totalValue = allLayers.Sum(l => l.RemainingQuantity * l.UnitCost);
            decimal totalQty = allLayers.Sum(l => l.RemainingQuantity);

            if (totalQty < quantityToConsume) throw new InvalidOperationException("Not enough inventory to consume.");

            decimal avgCost = totalQty > 0 ? totalValue / totalQty : 0m;

            // To consume, we still need to reduce quantities evenly or just decrement remaining layers
            decimal remaining = quantityToConsume;
            foreach (var layer in allLayers)
            {
                if (remaining <= 0) break;
                decimal take = Math.Min(remaining, layer.RemainingQuantity);
                layer.RemainingQuantity -= take;
                remaining -= take;
            }

            return new CostConsumptionResult(quantityToConsume, quantityToConsume * avgCost, avgCost);
        }
        else
        {
            throw new NotSupportedException($"Valuation method {method} is not supported.");
        }

        // Processing FIFO / LIFO
        decimal totalCostConsumed = 0m;
        decimal qtyConsumed = 0m;
        decimal remainingToConsume = quantityToConsume;

        foreach (var layer in layers)
        {
            if (remainingToConsume <= 0) break;

            decimal take = Math.Min(remainingToConsume, layer.RemainingQuantity);
            layer.RemainingQuantity -= take;

            totalCostConsumed += (take * layer.UnitCost);
            qtyConsumed += take;
            remainingToConsume -= take;
        }

        if (remainingToConsume > 0)
        {
            throw new InvalidOperationException($"Not enough inventory to consume. Short by {remainingToConsume}.");
        }

        decimal effectiveUnitCost = qtyConsumed > 0 ? totalCostConsumed / qtyConsumed : 0m;

        return new CostConsumptionResult(qtyConsumed, totalCostConsumed, effectiveUnitCost);
    }
}
