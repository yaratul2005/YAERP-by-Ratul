using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using YAERP.Infrastructure.Security;

namespace YAERP.Infrastructure.Tests.Security;

public class IdentityServiceTests
{
    [Fact]
    public void HashPassword_ShouldGenerateHashAndSalt()
    {
        var identityService = new IdentityService();
        var password = "SuperSecretPassword123!";

        var hash = identityService.HashPassword(password, out string salt);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.False(string.IsNullOrWhiteSpace(salt));
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var identityService = new IdentityService();
        var password = "SuperSecretPassword123!";
        var hash = identityService.HashPassword(password, out string salt);

        var isVerified = identityService.VerifyPassword(password, hash, salt);

        Assert.True(isVerified);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var identityService = new IdentityService();
        var password = "SuperSecretPassword123!";
        var hash = identityService.HashPassword(password, out string salt);

        var isVerified = identityService.VerifyPassword("WrongPassword", hash, salt);

        Assert.False(isVerified);
    }

    [Fact]
    public async Task EvaluatePermissionAsync_ShouldReturnCachedResult()
    {
        var identityService = new IdentityService();
        var userId = Guid.NewGuid();
        var permissionCode = "Sales.View";

        identityService.SeedCacheForTest(userId, permissionCode);

        var hasPermission = await identityService.EvaluatePermissionAsync(userId, permissionCode, CancellationToken.None);
        var hasNoPermission = await identityService.EvaluatePermissionAsync(userId, "Finance.View", CancellationToken.None);

        Assert.True(hasPermission);
        Assert.False(hasNoPermission);
    }

    [Fact]
    public async Task InvalidatePermissionCache_ShouldRemoveUserPermissions()
    {
        var identityService = new IdentityService();
        var userId = Guid.NewGuid();
        var permissionCode = "Sales.View";

        identityService.SeedCacheForTest(userId, permissionCode);
        identityService.InvalidatePermissionCache(userId);

        var hasPermission = await identityService.EvaluatePermissionAsync(userId, permissionCode, CancellationToken.None);

        Assert.False(hasPermission);
    }
}
