using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Domain.Entities.Manufacturing;

namespace YAERP.Application.Common.Interfaces;

public record ExplodedBomItem(Guid ComponentProductId, decimal EffectiveQuantity, int Level);

public interface IBomExplosionService
{
    Task<List<ExplodedBomItem>> ExplodeBomAsync(Guid assemblyProductId, decimal targetQuantity, CancellationToken cancellationToken = default);
    Task EnsureNoCyclesAsync(Guid newParentProductId, Guid newComponentProductId, CancellationToken cancellationToken = default);
}
