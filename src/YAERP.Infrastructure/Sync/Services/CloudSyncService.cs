using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YAERP.Application.Common.Interfaces;
using YAERP.Infrastructure.Sync.Options;
using YAERP.Infrastructure.Sync.Protos;
using CloudSyncServiceClient = YAERP.Infrastructure.Sync.Protos.CloudSyncService.CloudSyncServiceClient;

namespace YAERP.Infrastructure.Sync.Services;

/// <summary>
/// Cloud sync service responsible for network health checking via gRPC HealthCheck
/// and batching un-synced <see cref="Domain.Entities.SyncQueueItem"/> outbox items to the CloudSyncService server.
/// </summary>
public class CloudSyncService : ICloudSyncService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CloudSyncServiceClient _client;
    private readonly CloudSyncOptions _options;
    private readonly ILogger<CloudSyncService> _logger;

    public CloudSyncService(
        IServiceScopeFactory scopeFactory,
        CloudSyncServiceClient client,
        IOptions<CloudSyncOptions> options,
        ILogger<CloudSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsCloudConnected { get; private set; }

    public async Task<bool> CheckCloudHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HealthRequest { BranchId = _options.BranchId };
            var response = await _client.HealthCheckAsync(request, cancellationToken: cancellationToken);
            IsCloudConnected = response != null && response.IsOnline;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cloud health check failed for Branch {BranchId}", _options.BranchId);
            IsCloudConnected = false;
        }

        return IsCloudConnected;
    }

    public async Task<int> ProcessSyncBatchAsync(int batchSize = 50, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        List<YAERP.Domain.Entities.SyncQueueItem> unSyncedItems;
        try
        {
            unSyncedItems = await dbContext.SyncQueueItems
                .Where(x => !x.IsSynced)
                .OrderBy(x => x.CreatedAtUtc)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SyncQueueItems query failed. Triggering automatic database table auto-healing.");
            if (dbContext is DbContext efDbContext)
            {
                try
                {
                    var creator = efDbContext.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
                    creator.CreateTables();
                }
                catch { }
            }
            return 0;
        }

        if (unSyncedItems.Count == 0)
        {
            return 0;
        }

        var batchRequest = new SyncBatchRequest
        {
            BranchId = _options.BranchId
        };

        foreach (var item in unSyncedItems)
        {
            batchRequest.Items.Add(new SyncItemMessage
            {
                SyncId = item.Id.ToString(),
                EntityName = item.EntityName,
                EntityId = item.EntityId,
                OperationType = item.OperationType,
                PayloadJson = item.PayloadJson,
                CreatedAtUnix = new DateTimeOffset(item.CreatedAtUtc).ToUnixTimeSeconds()
            });
        }

        try
        {
            var response = await _client.SyncBatchAsync(batchRequest, cancellationToken: cancellationToken);

            if (response != null && response.Success)
            {
                var syncedIdSet = response.SyncedIds.ToHashSet();
                int syncedCount = 0;

                foreach (var item in unSyncedItems)
                {
                    if (syncedIdSet.Count == 0 || syncedIdSet.Contains(item.Id.ToString()))
                    {
                        item.MarkSynced(DateTime.UtcNow);
                        syncedCount++;
                    }
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully synced batch of {Count} items to cloud", syncedCount);
                return syncedCount;
            }
            else
            {
                string error = response?.ErrorMessage ?? "RPC returned failure status";
                foreach (var item in unSyncedItems)
                {
                    item.RecordSyncFailure(error);
                }
                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Sync batch failed: {Error}", error);
                return 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC SyncBatch call encountered an unexpected exception");
            foreach (var item in unSyncedItems)
            {
                item.RecordSyncFailure(ex.Message);
            }
            await dbContext.SaveChangesAsync(cancellationToken);
            return 0;
        }
    }
}
