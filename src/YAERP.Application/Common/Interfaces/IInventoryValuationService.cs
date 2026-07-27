using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public enum ValuationMethod
{
    FIFO,
    LIFO,
    AVCO,
    StandardCost
}

public record CostConsumptionResult(decimal QuantityConsumed, decimal TotalCost, decimal UnitCost, decimal PurchasePriceVariance = 0m);

public interface IInventoryValuationService
{
    Task AddCostLayerAsync(Guid productId, Guid warehouseId, decimal quantity, decimal unitCost, CancellationToken cancellationToken = default);
    Task<CostConsumptionResult> ConsumeInventoryAsync(Guid productId, Guid warehouseId, decimal quantityToConsume, ValuationMethod method, decimal? standardCost = null, CancellationToken cancellationToken = default);
}
