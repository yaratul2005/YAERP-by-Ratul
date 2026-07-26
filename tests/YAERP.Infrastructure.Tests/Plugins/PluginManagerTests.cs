using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using YAERP.Application.Common.Plugins;
using YAERP.Infrastructure.Plugins;

namespace YAERP.Infrastructure.Tests.Plugins;

public class TestSamplePlugin : IYaerpPlugin
{
    public string PluginId => "test-sample-plugin-01";
    public string Name => "Test Sample Plugin";
    public string Version => "1.0.0";
    public string Author => "Ratul Architect";

    public bool IsInitialized { get; private set; }
    public bool IsShutdown { get; private set; }

    public void Initialize(IPluginContext context)
    {
        IsInitialized = true;
    }

    public void Shutdown()
    {
        IsShutdown = true;
    }
}

public class PluginManagerTests
{
    [Fact]
    public void LoadPlugins_Should_EnsurePluginsDirectoryExists()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), "YAERP_Plugins_Test_" + Guid.NewGuid().ToString("N"));
        var servicesMock = new Mock<IServiceProvider>();
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<PluginManager>>();

        var manager = new PluginManager(servicesMock.Object, configMock.Object, loggerMock.Object);

        try
        {
            Assert.False(Directory.Exists(tempDir));

            // Act
            manager.LoadPlugins(tempDir);

            // Assert
            Assert.True(Directory.Exists(tempDir));
            Assert.Empty(manager.LoadedPlugins);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void DefaultPluginContext_Should_InvokeMenuItemRegisteredEvent()
    {
        // Arrange
        var servicesMock = new Mock<IServiceProvider>();
        var configMock = new Mock<IConfiguration>();
        var context = new DefaultPluginContext(servicesMock.Object, configMock.Object);

        string? registeredTitle = null;
        string? registeredIcon = null;
        Type? registeredType = null;

        context.MenuItemRegistered += (title, icon, type) =>
        {
            registeredTitle = title;
            registeredIcon = icon;
            registeredType = type;
        };

        // Act
        context.RegisterModuleMenuItem("Custom Analytics", "ChartIcon", typeof(PluginManagerTests));

        // Assert
        Assert.Equal("Custom Analytics", registeredTitle);
        Assert.Equal("ChartIcon", registeredIcon);
        Assert.Equal(typeof(PluginManagerTests), registeredType);
    }
}
