using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using YAERP.Domain.Entities;

namespace YAERP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChangesInterceptor that captures mutations to domain entities
/// (Product, SalesOrder, StockMovement, JournalEntry, etc.) and writes an outbox
/// <see cref="SyncQueueItem"/> record into the ChangeTracker before committing transactions to local SQLite.
/// </summary>
public class SyncOutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        IgnoreReadOnlyProperties = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var dbContext = eventData.Context;
        var entries = dbContext.ChangeTracker.Entries()
            .Where(e => e.Entity is not SyncQueueItem &&
                        e.Entity.GetType().Name != "AuditLog" &&
                        e.Entity.GetType().Name != "ApprovalStepLog" &&
                        (e.State == EntityState.Added ||
                         e.State == EntityState.Modified ||
                         e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;

            // Extract EntityId from Id property via EF reflection or property dictionary
            string entityId = string.Empty;
            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            if (idProperty != null && idProperty.CurrentValue != null)
            {
                entityId = idProperty.CurrentValue.ToString() ?? Guid.NewGuid().ToString();
            }
            else
            {
                entityId = Guid.NewGuid().ToString();
            }

            string operationType = entry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => "UNKNOWN"
            };

            string payloadJson;
            try
            {
                payloadJson = JsonSerializer.Serialize(entry.Entity, entry.Entity.GetType(), JsonOptions);
            }
            catch
            {
                payloadJson = $"{{\"entityName\":\"{entityName}\",\"entityId\":\"{entityId}\"}}";
            }

            var syncQueueItem = SyncQueueItem.Create(entityName, entityId, operationType, payloadJson);
            dbContext.Set<SyncQueueItem>().Add(syncQueueItem);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
