using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public interface ISyncOutboxService
{
    Task EnqueueMutationAsync(string entityType, Guid entityId, string mutationType, string serializedPayload, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetPendingMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default);
}
