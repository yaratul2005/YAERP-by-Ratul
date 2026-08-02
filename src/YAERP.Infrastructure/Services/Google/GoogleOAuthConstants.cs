using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;

namespace YAERP.Infrastructure.Services.Google;

public static class GoogleOAuthConstants
{
    public static readonly string[] Scopes = new string[]
    {
        "openid",
        "https://www.googleapis.com/auth/userinfo.email",
        "https://www.googleapis.com/auth/userinfo.profile",
        DriveService.Scope.DriveFile,
        SheetsService.Scope.Spreadsheets
    };

    public const string DefaultClientId = "YAERP-ENTERPRISE-DESKTOP-CLIENT-ID.apps.googleusercontent.com";
    public const string DefaultClientSecret = "YAERP-ENTERPRISE-SECRET-KEY";
    public const string AppFolderName = "YAERP_Cloud_Exports";
}
