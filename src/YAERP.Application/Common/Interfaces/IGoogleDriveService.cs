using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record DriveFolderDto(
    string Id,
    string Name,
    string? ParentId,
    string CreatedTime);

public interface IGoogleDriveService
{
    Task<List<DriveFolderDto>> GetAppFoldersAsync(string? parentFolderId = null, CancellationToken ct = default);
    Task<DriveFolderDto> CreateFolderAsync(string folderName, string? parentFolderId = null, CancellationToken ct = default);
    Task<string> UploadFileAsync(string localFilePath, string fileName, string? targetFolderId = null, CancellationToken ct = default);
}
