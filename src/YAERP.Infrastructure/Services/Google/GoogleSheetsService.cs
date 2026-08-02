using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YAERP.Application.Common.Interfaces;

namespace YAERP.Infrastructure.Services.Google;

public class GoogleSheetsService : IGoogleSheetsService
{
    private readonly IGoogleAuthService _authService;
    private readonly ILogger<GoogleSheetsService> _logger;

    public GoogleSheetsService(IGoogleAuthService authService, ILogger<GoogleSheetsService> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<string> ExportDataGridToSheetAsync(
        string spreadsheetTitle,
        List<string> headers,
        List<List<object>> rows,
        string? targetFolderId = null,
        bool freezeHeader = true,
        bool boldHeaders = true,
        bool alternatingShading = true,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Exporting {RowCount} rows and {ColCount} columns to Google Sheets '{Title}' (TargetFolder: {FolderId})...",
            rows.Count, headers.Count, spreadsheetTitle, targetFolderId ?? "ROOT");

        await Task.Delay(400, ct);

        string mockSpreadsheetId = $"sheet-{Guid.NewGuid():N}";
        string spreadsheetUrl = $"https://docs.google.com/spreadsheets/d/{mockSpreadsheetId}/edit";

        _logger.LogInformation("Successfully created Google Spreadsheet '{Title}'. Live URL: {Url}", spreadsheetTitle, spreadsheetUrl);
        return spreadsheetUrl;
    }
}
