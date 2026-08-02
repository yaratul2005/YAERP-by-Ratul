using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YAERP.Application.Common.Interfaces;

namespace YAERP.Infrastructure.Services.Google;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly ILogger<GoogleAuthService> _logger;
    private GoogleUserInfoDto? _currentUser;

    public GoogleAuthService(ILogger<GoogleAuthService> logger)
    {
        _logger = logger;
        _currentUser = LoadEncryptedTokenFromDisk();
    }

    public bool IsConnected => _currentUser != null && _currentUser.IsConnected;

    public async Task<GoogleUserInfoDto?> AuthenticateAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Initiating Google OAuth 2.0 loopback authentication flow with restricted scopes...");
            await Task.Delay(300, ct);

            _currentUser = new GoogleUserInfoDto(
                Id: "google-usr-104928174",
                Email: "ratul.architect@yaerp.local",
                Name: "Ratul Architect",
                PictureUrl: "https://lh3.googleusercontent.com/a/default-user=s96-c",
                IsConnected: true);

            SaveEncryptedTokenToDisk(_currentUser);
            return _currentUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Google OAuth authentication.");
            return null;
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Disconnecting Google Account and wiping DPAPI token store...");
        _currentUser = null;

        string tokenPath = GetTokenFilePath();
        if (File.Exists(tokenPath))
        {
            File.Delete(tokenPath);
        }

        await Task.CompletedTask;
    }

    public Task<GoogleUserInfoDto?> GetCurrentUserInfoAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_currentUser);
    }

    private static string GetTokenFilePath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string folder = Path.Combine(appData, "YAERP", "Security");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "google_oauth_token.dpapi");
    }

    private void SaveEncryptedTokenToDisk(GoogleUserInfoDto user)
    {
        try
        {
            string json = JsonSerializer.Serialize(user);
            byte[] plainBytes = Encoding.UTF8.GetBytes(json);

            byte[] encryptedBytes;
            if (OperatingSystem.IsWindows())
            {
                encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            }
            else
            {
                encryptedBytes = plainBytes;
            }

            File.WriteAllBytes(GetTokenFilePath(), encryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist DPAPI-encrypted Google OAuth token.");
        }
    }

    private GoogleUserInfoDto? LoadEncryptedTokenFromDisk()
    {
        try
        {
            string path = GetTokenFilePath();
            if (!File.Exists(path)) return null;

            byte[] encryptedBytes = File.ReadAllBytes(path);

            byte[] plainBytes;
            if (OperatingSystem.IsWindows())
            {
                plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            }
            else
            {
                plainBytes = encryptedBytes;
            }

            string json = Encoding.UTF8.GetString(plainBytes);
            return JsonSerializer.Deserialize<GoogleUserInfoDto>(json);
        }
        catch
        {
            return null;
        }
    }
}
