using System;

namespace YAERP.Application.Common.Models;

public class AppReleaseManifest
{
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDateUtc { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public bool IsMandatoryUpdate { get; set; }
    public string MinimumSupportedVersion { get; set; } = string.Empty;
}
