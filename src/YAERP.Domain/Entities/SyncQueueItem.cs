using System;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Entities;

public class SyncQueueItem : Entity<Guid>
{
    private SyncQueueItem(
        Guid id,
        string entityName,
        string entityId,
        string operationType,
        string payloadJson,
        DateTime createdAtUtc) : base(id)
    {
        EntityName = entityName;
        EntityId = entityId;
        OperationType = operationType;
        PayloadJson = payloadJson;
        CreatedAtUtc = createdAtUtc;
        IsSynced = false;
        SyncRetryCount = 0;
    }

    private SyncQueueItem() { } // EF Core

    public string EntityName { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string OperationType { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsSynced { get; private set; }
    public DateTime? SyncedAtUtc { get; private set; }
    public int SyncRetryCount { get; private set; }
    public string? LastSyncError { get; private set; }

    public static SyncQueueItem Create(
        string entityName,
        string entityId,
        string operationType,
        string payloadJson)
    {
        return new SyncQueueItem(
            Guid.NewGuid(),
            entityName,
            entityId,
            operationType,
            payloadJson,
            DateTime.UtcNow);
    }

    public void MarkSynced(DateTime syncedAtUtc)
    {
        IsSynced = true;
        SyncedAtUtc = syncedAtUtc;
        LastSyncError = null;
    }

    public void RecordSyncFailure(string errorMessage)
    {
        SyncRetryCount++;
        LastSyncError = errorMessage;
    }
}
