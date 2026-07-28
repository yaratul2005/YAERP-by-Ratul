using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Models;

namespace YAERP.Infrastructure.Services;

public class AutoUpdaterService : IAutoUpdaterService
{
    private readonly HttpClient _httpClient;
    private readonly string _manifestUrl;

    public AutoUpdaterService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _manifestUrl = "https://raw.githubusercontent.com/ratul/yaerp/main/version.json"; // Default remote url
    }

    public AutoUpdaterService() : this(new HttpClient()) { }

    public async Task<AppReleaseManifest?> CheckForUpdatesAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(_manifestUrl, cancellationToken);
            var manifest = JsonSerializer.Deserialize<AppReleaseManifest>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (manifest != null && IsVersionNewer(currentVersion, manifest.Version))
            {
                return manifest;
            }
        }
        catch
        {
            // Suppress errors (network issues, parsing errors) and just return null indicating no update found
        }

        return null;
    }

    public async Task DownloadUpdateAsync(string downloadUrl, Action<int> progressCallback, CancellationToken cancellationToken = default)
    {
        // Mocking a download with progress reporting
        for (int i = 0; i <= 100; i += 10)
        {
            if (cancellationToken.IsCancellationRequested) break;

            progressCallback?.Invoke(i);
            await Task.Delay(100, cancellationToken);
        }
    }

    public bool IsVersionNewer(string currentVersion, string remoteVersion)
    {
        if (Version.TryParse(currentVersion.Replace("v", ""), out var current) &&
            Version.TryParse(remoteVersion.Replace("v", ""), out var remote))
        {
            return remote > current;
        }
        return false;
    }
}
