using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record PickRouteStep(Guid BinId, string BinCode, Guid LotId, string LotNumber, decimal QuantityToPick);

public record GenealogyTraceNode(string NodeType, string ReferenceId, DateTime Timestamp, string Action);

public interface IFefoPickingService
{
    Task<List<PickRouteStep>> GenerateFefoPickRouteAsync(Guid productId, decimal requiredQuantity, CancellationToken cancellationToken = default);
    Task<List<GenealogyTraceNode>> ForwardTraceAsync(string supplierLotRef, CancellationToken cancellationToken = default);
    Task<List<GenealogyTraceNode>> BackwardTraceAsync(string salesOrderNumber, CancellationToken cancellationToken = default);
}
