using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public interface IGoogleSheetsService
{
    Task<string> ExportDataGridToSheetAsync(
        string spreadsheetTitle,
        List<string> headers,
        List<List<object>> rows,
        string? targetFolderId = null,
        bool freezeHeader = true,
        bool boldHeaders = true,
        bool alternatingShading = true,
        CancellationToken ct = default);
}
