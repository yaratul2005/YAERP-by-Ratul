using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record SlottingRecommendation(Guid BinId, string BinCode, decimal AvailableVolume, decimal AvailableWeight);

public interface ISlottingOptimizationService
{
    Task<List<SlottingRecommendation>> RecommendBinsAsync(
        Guid warehouseId,
        decimal incomingVolume,
        decimal incomingWeight,
        bool requiresColdStorage,
        string abcClassification,
        CancellationToken cancellationToken = default);
}
