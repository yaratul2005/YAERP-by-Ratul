using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using YAERP.UI.Services;
using YAERP.UI.Workspace;

namespace YAERP.UI.Tests.Workspace;

public class TestTabViewModel : TabViewModelBase
{
    public bool WasActivated { get; private set; }
    public bool WasDeactivated { get; private set; }
    public bool AllowClose { get; set; } = true;

    public override Task OnTabActivatedAsync()
    {
        WasActivated = true;
        return base.OnTabActivatedAsync();
    }

    public override Task OnTabDeactivatedAsync()
    {
        WasDeactivated = true;
        return base.OnTabDeactivatedAsync();
    }

    public override Task<bool> OnTabClosingAsync()
    {
        return Task.FromResult(AllowClose);
    }
}

public class TabWorkspaceServiceTests
{
    private readonly ITabWorkspaceService _sut;
    private readonly IServiceProvider _serviceProvider;

    public TabWorkspaceServiceTests()
    {
        var services = new ServiceCollection();
        services.AddTransient<TestTabViewModel>();
        _serviceProvider = services.BuildServiceProvider();
        var svc = new YAERP.UI.Services.TabWorkspaceService(_serviceProvider);
        _sut = svc;
    }

    [Fact]
    public void OpenTab_ShouldAddTabAndSetActive()
    {
        _sut.OpenTab<TestTabViewModel>();

        Assert.Single(_sut.OpenTabs);
        Assert.NotNull(_sut.ActiveTab);
        Assert.IsType<TestTabViewModel>(_sut.ActiveTab);
    }

    [Fact]
    public void OpenTab_ShouldBeIdempotent_BasedOnTabId()
    {
        _sut.OpenTab<TestTabViewModel>(t => t.TabId = "Tab1");
        _sut.OpenTab<TestTabViewModel>(t => t.TabId = "Tab1");

        Assert.Single(_sut.OpenTabs);
    }

    [Fact]
    public async Task CloseTabAsync_ShouldRemoveTab_IfClosingAllowed()
    {
        _sut.OpenTab<TestTabViewModel>();
        var tab = _sut.OpenTabs.First();

        var result = await _sut.CloseTabAsync(tab);

        Assert.True(result);
        Assert.Empty(_sut.OpenTabs);
    }

    [Fact]
    public async Task CloseTabAsync_ShouldNotRemoveTab_IfClosingDenied()
    {
        _sut.OpenTab<TestTabViewModel>(t => t.AllowClose = false);
        var tab = _sut.OpenTabs.First();

        var result = await _sut.CloseTabAsync(tab);

        Assert.False(result);
        Assert.Single(_sut.OpenTabs);
    }
}
