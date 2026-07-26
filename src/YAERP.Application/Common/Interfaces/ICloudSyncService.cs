using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public interface ICloudSyncService
{
    bool IsCloudConnected { get; }
    Task<bool> CheckCloudHealthAsync(CancellationToken cancellationToken = default);
    Task<int> ProcessSyncBatchAsync(int batchSize = 50, CancellationToken cancellationToken = default);
}
