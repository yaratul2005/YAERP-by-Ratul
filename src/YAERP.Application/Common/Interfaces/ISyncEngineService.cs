using System;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public interface ISyncEngineService
{
    Task PushAsync(CancellationToken cancellationToken = default);
    Task PullAsync(CancellationToken cancellationToken = default);
    Task ResolveConflictAsync(Guid conflictId, string resolutionStrategy, CancellationToken cancellationToken = default);
}
