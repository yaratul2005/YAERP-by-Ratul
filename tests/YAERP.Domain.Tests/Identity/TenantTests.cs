using Xunit;
using YAERP.Domain.Identity;

namespace YAERP.Domain.Tests.Identity;

public class TenantTests
{
    [Fact]
    public void Create_Should_ReturnTenantWithCorrectProperties()
    {
        // Arrange
        var name = "Acme Corp";
        var connectionString = "Server=localhost;Database=acme;";

        // Act
        var tenant = Tenant.Create(name, connectionString);

        // Assert
        Assert.NotNull(tenant);
        Assert.NotEqual(Guid.Empty, tenant.Id.Value);
        Assert.Equal(name, tenant.Name);
        Assert.Equal(connectionString, tenant.ConnectionString);
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Deactivate_Should_SetIsActiveToFalse()
    {
        // Arrange
        var tenant = Tenant.Create("Acme");

        // Act
        tenant.Deactivate();

        // Assert
        Assert.False(tenant.IsActive);
    }

    [Fact]
    public void Activate_Should_SetIsActiveToTrue()
    {
        // Arrange
        var tenant = Tenant.Create("Acme");
        tenant.Deactivate();

        // Act
        tenant.Activate();

        // Assert
        Assert.True(tenant.IsActive);
    }
}
