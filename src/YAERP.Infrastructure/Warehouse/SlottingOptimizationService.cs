using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;

namespace YAERP.Infrastructure.Warehouse;

public class SlottingOptimizationService : ISlottingOptimizationService
{
    private readonly IApplicationDbContext _dbContext;

    public SlottingOptimizationService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SlottingRecommendation>> RecommendBinsAsync(
        Guid warehouseId,
        decimal incomingVolume,
        decimal incomingWeight,
        bool requiresColdStorage,
        string abcClassification,
        CancellationToken cancellationToken = default)
    {
        // 1. Fetch zones for the given warehouse that match temperature constraints
        // Assuming we map ABC class to ZoneType logically or just general Picking/BulkStorage
        string preferredZoneType = abcClassification == "A" ? "Picking" : "BulkStorage";

        var validZonesQuery = _dbContext.Set<YAERP.Domain.Entities.Warehouse.WarehouseZone>()
            .Where(z => z.WarehouseId == warehouseId);

        if (requiresColdStorage)
        {
            validZonesQuery = validZonesQuery.Where(z => z.ZoneType == "ColdStorage");
        }

        var validZoneIds = await validZonesQuery.Select(z => z.Id).ToListAsync(cancellationToken);

        if (!validZoneIds.Any())
            return new List<SlottingRecommendation>();

        // 2. Fetch all unlocked bins in these zones matching the temperature control
        var validBins = await _dbContext.Set<YAERP.Domain.Entities.Warehouse.WarehouseBin>()
            .Where(b => validZoneIds.Contains(b.ZoneId) && !b.IsLocked)
            .Where(b => !requiresColdStorage || b.IsTemperatureControlled)
            .ToListAsync(cancellationToken);

        // 3. For each bin, calculate current used volume and weight
        // This is a naive implementation without grouping by bin, assume we can get it from inventory
        // In reality, we'd query current stock per bin and sum volume/weight.
        // For the sake of this implementation, assume bins are empty or we mock the check.
        // We'll calculate remaining capacity:

        var recommendations = new List<SlottingRecommendation>();

        // Mock current utilization = 0 for demonstration, as we lack full stock-by-bin tracking query right now
        // Or we assume `CurrentVolume` = 0 and `CurrentWeight` = 0
        foreach (var bin in validBins)
        {
            decimal currentVolume = 0m;
            decimal currentWeight = 0m;

            decimal availableVolume = bin.MaxVolumeCubicMeters - currentVolume;
            decimal availableWeight = bin.MaxWeightKg - currentWeight;

            if (availableVolume >= incomingVolume && availableWeight >= incomingWeight)
            {
                recommendations.Add(new SlottingRecommendation(bin.Id, bin.BinCode, availableVolume, availableWeight));
            }
        }

        // 4. Sort recommendations based on ABC logic:
        // If Class A, sort by lowest Level first (e.g. L01 is lower than L04) to minimize picking height
        // Otherwise, sort by available volume to maximize space usage
        if (abcClassification == "A")
        {
            return recommendations
                .OrderBy(r => {
                    var bin = validBins.First(b => b.Id == r.BinId);
                    return bin.Level; // Sort alphabetically: L01 < L02
                })
                .ToList();
        }
        else
        {
            return recommendations
                .OrderBy(r => r.AvailableVolume) // Tighter packing
                .ToList();
        }
    }
}
