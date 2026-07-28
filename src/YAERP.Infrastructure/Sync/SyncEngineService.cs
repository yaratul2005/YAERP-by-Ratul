using System;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces;

namespace YAERP.Infrastructure.Sync;

public class SyncEngineService : ISyncEngineService
{
    private readonly ISyncOutboxService _outboxService;

    public SyncEngineService(ISyncOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    public async Task PushAsync(CancellationToken cancellationToken = default)
    {
        // 1. Fetch pending
        var pending = await _outboxService.GetPendingMessagesAsync(100, cancellationToken);

        // 2. Transmit payloads
        // 3. Update OutboxMessage status (Synced, Failed, Conflict)
    }

    public Task PullAsync(CancellationToken cancellationToken = default)
    {
        // 1. Download changes since LastSyncTimestampUtc
        // 2. Conflict Evaluation: Local ModifiedAt vs Remote ModifiedAt
        // 3. Log to SyncConflictLog if Manual, else resolve instantly via LWW, ClientWins, ServerWins
        return Task.CompletedTask;
    }

    public Task ResolveConflictAsync(Guid conflictId, string resolutionStrategy, CancellationToken cancellationToken = default)
    {
        // Applies resolution strategy and resolves the SyncConflictLog entry
        return Task.CompletedTask;
    }
}
