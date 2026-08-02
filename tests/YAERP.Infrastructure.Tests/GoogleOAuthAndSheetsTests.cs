using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Infrastructure.Services.Google;

namespace YAERP.Infrastructure.Tests;

public class GoogleOAuthAndSheetsTests
{
    [Fact]
    public void GoogleOAuthConstants_ShouldHaveRestrictedScopes()
    {
        Assert.Contains("openid", GoogleOAuthConstants.Scopes);
        Assert.Contains("https://www.googleapis.com/auth/userinfo.email", GoogleOAuthConstants.Scopes);
        Assert.Contains("https://www.googleapis.com/auth/userinfo.profile", GoogleOAuthConstants.Scopes);
        Assert.Contains("https://www.googleapis.com/auth/drive.file", GoogleOAuthConstants.Scopes);
        Assert.Contains("https://www.googleapis.com/auth/spreadsheets", GoogleOAuthConstants.Scopes);
        Assert.Equal(5, GoogleOAuthConstants.Scopes.Length);
    }

    [Fact]
    public async Task GoogleAuthService_AuthenticateAsync_ShouldReturnValidUserInfo()
    {
        var logger = NullLogger<GoogleAuthService>.Instance;
        var service = new GoogleAuthService(logger);

        var user = await service.AuthenticateAsync();

        Assert.NotNull(user);
        Assert.True(user.IsConnected);
        Assert.Equal("ratul.architect@yaerp.local", user.Email);
        Assert.True(service.IsConnected);
    }

    [Fact]
    public async Task GoogleDriveService_GetAppFoldersAsync_ShouldReturnAppFolders()
    {
        var authLogger = NullLogger<GoogleAuthService>.Instance;
        var driveLogger = NullLogger<GoogleDriveService>.Instance;
        var authService = new GoogleAuthService(authLogger);
        var driveService = new GoogleDriveService(authService, driveLogger);

        var folders = await driveService.GetAppFoldersAsync();

        Assert.NotNull(folders);
        Assert.NotEmpty(folders);
    }

    [Fact]
    public async Task GoogleSheetsService_ExportDataGridToSheetAsync_ShouldReturnValidUrl()
    {
        var authLogger = NullLogger<GoogleAuthService>.Instance;
        var sheetsLogger = NullLogger<GoogleSheetsService>.Instance;
        var authService = new GoogleAuthService(authLogger);
        var sheetsService = new GoogleSheetsService(authService, sheetsLogger);

        var headers = new List<string> { "SKU", "Product Name", "Price" };
        var rows = new List<List<object>>
        {
            new() { "SKU-101", "Laptop Stand", 29.99m },
            new() { "SKU-102", "Ergonomic Mouse", 49.99m }
        };

        string url = await sheetsService.ExportDataGridToSheetAsync("Test Export", headers, rows);

        Assert.NotNull(url);
        Assert.StartsWith("https://docs.google.com/spreadsheets/d/", url);
    }
}
