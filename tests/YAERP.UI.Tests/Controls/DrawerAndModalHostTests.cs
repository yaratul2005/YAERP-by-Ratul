using Xunit;
using YAERP.UI.Workspace;

namespace YAERP.UI.Tests.Controls;

public class TestWorkspaceTab : TabViewModelBase
{
    public TestWorkspaceTab()
    {
        Title = "Test Tab";
        TabId = "TEST-TAB-01";
    }
}

public class DrawerAndModalHostTests
{
    [Fact]
    public void OpenDrawerCommand_ShouldUpdateDrawerState()
    {
        var tab = new TestWorkspaceTab();
        var dummyContent = new object();

        tab.OpenDrawer(dummyContent, "Edit Item Details");

        Assert.True(tab.IsDrawerOpen);
        Assert.Equal("Edit Item Details", tab.DrawerTitle);
        Assert.Same(dummyContent, tab.DrawerContent);
    }

    [Fact]
    public void CloseDrawerCommand_ShouldResetDrawerState()
    {
        var tab = new TestWorkspaceTab();
        tab.OpenDrawer(new object(), "Details");

        tab.CloseDrawer();

        Assert.False(tab.IsDrawerOpen);
        Assert.Null(tab.DrawerContent);
    }

    [Fact]
    public void OpenModalCommand_ShouldUpdateModalState()
    {
        var tab = new TestWorkspaceTab();
        var dummyForm = new object();

        tab.OpenModal(dummyForm, "Confirm Action");

        Assert.True(tab.IsModalOpen);
        Assert.Equal("Confirm Action", tab.ModalTitle);
        Assert.Same(dummyForm, tab.ModalContent);
    }

    [Fact]
    public void CloseModalCommand_ShouldResetModalState()
    {
        var tab = new TestWorkspaceTab();
        tab.OpenModal(new object(), "Title");

        tab.CloseModal();

        Assert.False(tab.IsModalOpen);
        Assert.Null(tab.ModalContent);
    }

    [Fact]
    public void ToastNotifications_ShouldSetBannerProperties()
    {
        var tab = new TestWorkspaceTab();

        tab.ShowSuccessToast("Operation completed successfully.");
        Assert.True(tab.IsToastVisible);
        Assert.True(tab.IsSuccessToast);
        Assert.Equal("Operation completed successfully.", tab.ToastMessage);

        tab.ShowErrorToast("Validation failed.");
        Assert.True(tab.IsToastVisible);
        Assert.False(tab.IsSuccessToast);
        Assert.Equal("Validation failed.", tab.ToastMessage);

        tab.HideToast();
        Assert.False(tab.IsToastVisible);
        Assert.Equal(string.Empty, tab.ToastMessage);
    }
}
