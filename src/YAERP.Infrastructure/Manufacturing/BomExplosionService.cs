using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Manufacturing;

namespace YAERP.Infrastructure.Manufacturing;

public class InvalidBomCycleException : Exception
{
    public InvalidBomCycleException(string message) : base(message) { }
}

public class BomExplosionService : IBomExplosionService
{
    private readonly IApplicationDbContext _dbContext;

    public BomExplosionService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ExplodedBomItem>> ExplodeBomAsync(Guid assemblyProductId, decimal targetQuantity, CancellationToken cancellationToken = default)
    {
        var result = new List<ExplodedBomItem>();
        await ExplodeRecursiveAsync(assemblyProductId, targetQuantity, 1, result, cancellationToken);

        // Group by component ID to aggregate duplicate components across different branches
        var aggregated = result
            .GroupBy(x => x.ComponentProductId)
            .Select(g => new ExplodedBomItem(
                g.Key,
                g.Sum(x => x.EffectiveQuantity),
                g.Min(x => x.Level)))
            .ToList();

        return aggregated;
    }

    private async Task ExplodeRecursiveAsync(Guid productId, decimal requiredQuantity, int level, List<ExplodedBomItem> result, CancellationToken cancellationToken)
    {
        // For testing we will simulate fetching from a real repository, but we need the DB context to fetch real headers if available.
        // Assuming there's a DbSet<BomHeader> and DbSet<BomItem> in DB context, but wait, we need to add them to YAERPDbContext.cs first.
        // I will do this in the next bash script, for now let's write the query assuming they are exposed (or we can inject a mechanism to get them).
        // Since IApplicationDbContext doesn't have them yet, let's use Set<T> method or similar if available, or just we will add them.

        // Find the active BOM for the product
        // Using raw EF Core for now. Note: We will add DbSet to IApplicationDbContext soon.
        var boms = await _dbContext.Set<BomHeader>()
            .Include(b => b.Components)
            .Where(b => b.AssemblyProductId == productId && b.IsActive)
            .ToListAsync(cancellationToken);

        var bom = boms.FirstOrDefault();

        if (bom == null)
        {
            // It's a leaf node (raw material)
            result.Add(new ExplodedBomItem(productId, requiredQuantity, level));
            return;
        }

        // It has a BOM, recurse into components
        foreach (var component in bom.Components)
        {
            // Safe division for yield
            decimal yieldFactor = component.YieldPercent > 0 ? component.YieldPercent / 100m : 1m;
            decimal scrapFactor = 1m + (component.ScrapFactorPercent / 100m);

            decimal effectiveQtyPerAssembly = (component.QuantityPerAssembly / yieldFactor) * scrapFactor;
            // Also divide by parent's BaseQuantity
            decimal normalizedQtyPerAssembly = effectiveQtyPerAssembly / (bom.BaseQuantity > 0 ? bom.BaseQuantity : 1m);

            decimal totalRequired = requiredQuantity * normalizedQtyPerAssembly;

            if (component.IsPhantomAssembly)
            {
                // Phantom assembly means we don't stock this, just blow through it at the same level
                await ExplodeRecursiveAsync(component.ComponentProductId, totalRequired, level, result, cancellationToken);
            }
            else
            {
                await ExplodeRecursiveAsync(component.ComponentProductId, totalRequired, level + 1, result, cancellationToken);
            }
        }
    }

    public async Task EnsureNoCyclesAsync(Guid newParentProductId, Guid newComponentProductId, CancellationToken cancellationToken = default)
    {
        // Simple DFS to check if newComponentProductId eventually requires newParentProductId
        var visited = new HashSet<Guid>();
        var stack = new Stack<Guid>();

        stack.Push(newComponentProductId);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current == newParentProductId)
                throw new InvalidBomCycleException($"Cycle detected: adding {newComponentProductId} to {newParentProductId} creates a circular dependency.");

            if (visited.Add(current))
            {
                var childrenIds = await _dbContext.Set<BomHeader>()
                    .Where(b => b.AssemblyProductId == current && b.IsActive)
                    .SelectMany(b => b.Components.Select(c => c.ComponentProductId))
                    .ToListAsync(cancellationToken);

                foreach (var child in childrenIds)
                {
                    stack.Push(child);
                }
            }
        }
    }
}
