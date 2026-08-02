using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YAERP.Application.Common.Interfaces;

namespace YAERP.Infrastructure.Services.Google;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly IGoogleAuthService _authService;
    private readonly ILogger<GoogleDriveService> _logger;

    private readonly List<DriveFolderDto> _mockFolders = new()
    {
        new DriveFolderDto("folder-root", "YAERP Root Folder", null, DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd HH:mm")),
        new DriveFolderDto("folder-invoices", "Invoices & Billing", "folder-root", DateTime.UtcNow.AddDays(-15).ToString("yyyy-MM-dd HH:mm")),
        new DriveFolderDto("folder-inventory", "Stock & Inventory Snapshots", "folder-root", DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd HH:mm")),
        new DriveFolderDto("folder-financials", "General Ledger & Financials", "folder-root", DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-dd HH:mm")),
        new DriveFolderDto("folder-sales", "Sales & CRM Reports", "folder-root", DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd HH:mm"))
    };

    public GoogleDriveService(IGoogleAuthService authService, ILogger<GoogleDriveService> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<List<DriveFolderDto>> GetAppFoldersAsync(string? parentFolderId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching Google Drive app-folder listing for parent '{ParentFolderId}'...", parentFolderId ?? "ROOT");
        await Task.Delay(200, ct);

        string effectiveParent = parentFolderId ?? "folder-root";
        return _mockFolders.Where(f => f.ParentId == effectiveParent || (parentFolderId == null && f.ParentId == null)).ToList();
    }

    public async Task<DriveFolderDto> CreateFolderAsync(string folderName, string? parentFolderId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating new Google Drive folder '{FolderName}' under '{ParentFolderId}'...", folderName, parentFolderId ?? "ROOT");
        await Task.Delay(200, ct);

        var newFolder = new DriveFolderDto(
            Id: $"folder-{Guid.NewGuid():N}",
            Name: folderName,
            ParentId: parentFolderId ?? "folder-root",
            CreatedTime: DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));

        _mockFolders.Add(newFolder);
        return newFolder;
    }

    public async Task<string> UploadFileAsync(string localFilePath, string fileName, string? targetFolderId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Uploading local file '{FileName}' to Google Drive target folder '{FolderId}'...", fileName, targetFolderId ?? "ROOT");
        await Task.Delay(300, ct);
        return $"https://drive.google.com/file/d/mock-file-id-{Guid.NewGuid():N}/view";
    }
}
