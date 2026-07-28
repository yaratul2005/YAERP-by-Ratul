using System;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Models;

namespace YAERP.Application.Common.Interfaces;

public interface IAutoUpdaterService
{
    Task<AppReleaseManifest?> CheckForUpdatesAsync(string currentVersion, CancellationToken cancellationToken = default);
    Task DownloadUpdateAsync(string downloadUrl, Action<int> progressCallback, CancellationToken cancellationToken = default);
    bool IsVersionNewer(string currentVersion, string remoteVersion);
}
