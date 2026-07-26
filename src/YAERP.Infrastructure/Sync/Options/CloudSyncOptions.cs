namespace YAERP.Infrastructure.Sync.Options;

public class CloudSyncOptions
{
    public const string SectionName = "CloudSync";

    public string CloudServerUrl { get; set; } = "https://cloud.yaerp.internal:5001";
    public string BranchId { get; set; } = "BRANCH-MAIN-01";
    public int SyncIntervalSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 50;
}
