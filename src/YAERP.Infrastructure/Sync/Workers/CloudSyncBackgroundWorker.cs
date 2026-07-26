using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YAERP.Application.Common.Interfaces;
using YAERP.Infrastructure.Sync.Options;

namespace YAERP.Infrastructure.Sync.Workers;

/// <summary>
/// Hosted background service running a periodic timer loop to monitor network health
/// and push offline outbox sync records to the central cloud.
/// </summary>
public class CloudSyncBackgroundWorker : BackgroundService
{
    private readonly ICloudSyncService _syncService;
    private readonly CloudSyncOptions _options;
    private readonly ILogger<CloudSyncBackgroundWorker> _logger;

    public CloudSyncBackgroundWorker(
        ICloudSyncService syncService,
        IOptions<CloudSyncOptions> options,
        ILogger<CloudSyncBackgroundWorker> logger)
    {
        _syncService = syncService;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cloud Sync Background Worker initialized with {Interval}s interval.", _options.SyncIntervalSeconds);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(1, _options.SyncIntervalSeconds)));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                bool isConnected = await _syncService.CheckCloudHealthAsync(stoppingToken);
                if (isConnected)
                {
                    int synced = await _syncService.ProcessSyncBatchAsync(_options.BatchSize, stoppingToken);
                    if (synced > 0)
                    {
                        _logger.LogInformation("Background Sync Worker processed {Count} outbox records.", synced);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Cloud Sync Background Worker loop.");
            }
        }
    }
}
