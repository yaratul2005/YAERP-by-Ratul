using Xunit;
using YAERP.Domain.Identity;

namespace YAERP.Domain.Tests.Identity;

public class UserTests
{
    [Fact]
    public void Create_Should_ReturnUserWithCorrectProperties()
    {
        // Arrange
        var tenantId = new TenantId(Guid.NewGuid());
        var email = "test@example.com";
        var passwordHash = "HASHED_pass123";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = User.Create(tenantId, email, passwordHash, firstName, lastName);

        // Assert
        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id.Value);
        Assert.Equal(tenantId, user.TenantId);
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.True(user.IsActive);
        Assert.Empty(user.Roles);
    }

    [Fact]
    public void AddRole_Should_AddRoleToUser()
    {
        // Arrange
        var tenantId = new TenantId(Guid.NewGuid());
        var user = User.Create(tenantId, "a@b.com", "hash", "A", "B");
        var role = Role.Create(tenantId, "Admin", new List<string> { "all" });

        // Act
        user.AddRole(role);

        // Assert
        Assert.Single(user.Roles);
        Assert.Contains(role, user.Roles);
    }
}
