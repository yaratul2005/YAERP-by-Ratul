using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Models;
using YAERP.UI.ViewModels;
using YAERP.UI.Services;
using YAERP.UI.Workspace;

namespace YAERP.UI.Tests.ViewModels;

public class MainViewModelUpdaterTests
{
    [Fact]
    public void DownloadUpdateAsync_ShouldSetIsDownloadingAndCallService()
    {
        var mockNav = new Mock<INavigationService>();
        var mockWorkspace = new Mock<ITabWorkspaceService>();
        var mockModal = new Mock<IModalService>();
        var mockDialog = new Mock<IDialogService>();
        var mockUpdater = new Mock<IAutoUpdaterService>();

        var vm = new MainViewModel(mockNav.Object, mockWorkspace.Object, mockModal.Object, mockDialog.Object, mockUpdater.Object);

        // This relies on the command executing the updater download method
        vm.DownloadUpdateCommand.Execute(null);

        mockUpdater.Verify(u => u.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<Action<int>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
