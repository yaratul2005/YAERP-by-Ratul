using System;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Entities.Sync;

public record OutboxMessageId(Guid Value);

public class OutboxMessage : Entity<OutboxMessageId>
{
    private OutboxMessage() { }

    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string MutationType { get; private set; } = string.Empty; // Create, Update, Delete
    public string SerializedPayloadJson { get; private set; } = string.Empty;
    public DateTime EnqueuedAtUtc { get; private set; }
    public int RetryCount { get; private set; }
    public string Status { get; private set; } = "Pending"; // Pending, Syncing, Synced, Failed, Conflict
    public string? ErrorMessage { get; private set; }

    public static OutboxMessage Create(string entityType, Guid entityId, string mutationType, string serializedPayloadJson)
    {
        return new OutboxMessage
        {
            Id = new OutboxMessageId(Guid.NewGuid()),
            EntityType = entityType,
            EntityId = entityId,
            MutationType = mutationType,
            SerializedPayloadJson = serializedPayloadJson,
            EnqueuedAtUtc = DateTime.UtcNow,
            RetryCount = 0,
            Status = "Pending"
        };
    }

    public void MarkAsSyncing()
    {
        Status = "Syncing";
    }

    public void MarkAsSynced()
    {
        Status = "Synced";
        ErrorMessage = null;
    }

    public void MarkAsFailed(string error)
    {
        Status = "Failed";
        ErrorMessage = error;
        RetryCount++;
    }

    public void MarkAsConflict()
    {
        Status = "Conflict";
    }
}
