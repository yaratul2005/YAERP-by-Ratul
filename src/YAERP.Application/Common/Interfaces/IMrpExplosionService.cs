using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record PlannedOrder(
    Guid ProductId,
    decimal Quantity,
    DateTime ReleaseDate,
    DateTime ReceiptDate,
    string OrderType); // "PurchaseOrder" or "WorkOrder"

public record MrpDemandResult(
    Guid ProductId,
    decimal GrossRequirement,
    decimal OnHandStock,
    decimal ScheduledReceipts,
    decimal SafetyStock,
    decimal NetRequirement,
    List<PlannedOrder> PlannedOrders);

public interface IMrpExplosionService
{
    Task<List<MrpDemandResult>> RunMrpAsync(DateTime targetDate, CancellationToken cancellationToken = default);
}
