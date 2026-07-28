using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Sync;

namespace YAERP.Infrastructure.Sync;

public class SyncOutboxService : ISyncOutboxService
{
    private readonly List<OutboxMessage> _mockDatabase = new(); // Used as memory store for tests

    public Task EnqueueMutationAsync(string entityType, Guid entityId, string mutationType, string serializedPayload, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessage.Create(entityType, entityId, mutationType, serializedPayload);
        _mockDatabase.Add(message);

        // In reality, this would be EFCore DbContext SaveChanges / Interceptor
        return Task.CompletedTask;
    }

    public Task<IEnumerable<object>> GetPendingMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var pending = _mockDatabase
            .Where(m => m.Status == "Pending")
            .OrderBy(m => m.EnqueuedAtUtc)
            .Take(batchSize)
            .ToList();

        return Task.FromResult<IEnumerable<object>>(pending);
    }
}
