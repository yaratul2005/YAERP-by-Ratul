using NetArchTest.Rules;

namespace YAERP.Architecture.Tests;

public class ArchitectureTests
{
    private const string DomainNamespace = "YAERP.Domain";
    private const string ApplicationNamespace = "YAERP.Application";
    private const string InfrastructureNamespace = "YAERP.Infrastructure";
    private const string UINamespace = "YAERP.UI";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        // Arrange
        var assembly = typeof(Domain.Common.Primitives.Entity<>).Assembly;

        var otherProjects = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            UINamespace
        };

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOnInfrastructureOrUI()
    {
        // Arrange
        var assembly = typeof(Application.DependencyInjection).Assembly;

        var otherProjects = new[]
        {
            InfrastructureNamespace,
            UINamespace
        };

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_Should_Not_HaveDependencyOnUI()
    {
        // Arrange
        // We don't have any types in Infrastructure yet. Let's create a dummy one for test or just reference the assembly.
        // Actually, we can reference the assembly by name.
        var assembly = System.Reflection.Assembly.Load("YAERP.Infrastructure");

        var otherProjects = new[]
        {
            UINamespace
        };

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }
}
