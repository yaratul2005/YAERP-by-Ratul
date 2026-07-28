using System;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Entities.Sync;

public record SyncConflictLogId(Guid Value);

public class SyncConflictLog : Entity<SyncConflictLogId>
{
    private SyncConflictLog() { }

    public Guid OutboxMessageId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string LocalPayloadJson { get; private set; } = string.Empty;
    public string RemotePayloadJson { get; private set; } = string.Empty;
    public string ResolutionStrategy { get; private set; } = "Manual"; // LastWriteWins, ClientWins, ServerWins, Manual
    public bool IsResolved { get; private set; }
    public DateTime DetectedAtUtc { get; private set; }

    public static SyncConflictLog Create(Guid outboxMessageId, string entityType, Guid entityId, string localPayloadJson, string remotePayloadJson, string resolutionStrategy)
    {
        return new SyncConflictLog
        {
            Id = new SyncConflictLogId(Guid.NewGuid()),
            OutboxMessageId = outboxMessageId,
            EntityType = entityType,
            EntityId = entityId,
            LocalPayloadJson = localPayloadJson,
            RemotePayloadJson = remotePayloadJson,
            ResolutionStrategy = resolutionStrategy,
            IsResolved = false,
            DetectedAtUtc = DateTime.UtcNow
        };
    }

    public void Resolve()
    {
        IsResolved = true;
    }
}
