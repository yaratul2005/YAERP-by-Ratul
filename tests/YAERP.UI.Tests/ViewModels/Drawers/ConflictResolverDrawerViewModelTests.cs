using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.ViewModels.Drawers;

namespace YAERP.UI.Tests.ViewModels.Drawers;

public class ConflictResolverDrawerViewModelTests
{
    [Fact]
    public void LoadConflicts_ShouldPopulateConflictsList()
    {
        var mockEngine = new Mock<ISyncEngineService>();
        var mockDialog = new Mock<IDialogService>();

        var vm = new ConflictResolverDrawerViewModel(mockEngine.Object, mockDialog.Object);

        Assert.Single(vm.Conflicts);
    }

    [Fact]
    public async Task KeepLocalVersionAsync_ShouldCallEngineAndRemoveFromList()
    {
        var mockEngine = new Mock<ISyncEngineService>();
        var mockDialog = new Mock<IDialogService>();

        var vm = new ConflictResolverDrawerViewModel(mockEngine.Object, mockDialog.Object);
        vm.SelectedConflict = vm.Conflicts.First();

        var expectedId = vm.SelectedConflict.Id;

        await vm.KeepLocalVersionCommand.ExecuteAsync(null);

        mockEngine.Verify(e => e.ResolveConflictAsync(expectedId, "ClientWins", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Empty(vm.Conflicts);
        Assert.Null(vm.SelectedConflict);
    }

    [Fact]
    public async Task AcceptCloudVersionAsync_ShouldCallEngineAndRemoveFromList()
    {
        var mockEngine = new Mock<ISyncEngineService>();
        var mockDialog = new Mock<IDialogService>();

        var vm = new ConflictResolverDrawerViewModel(mockEngine.Object, mockDialog.Object);
        vm.SelectedConflict = vm.Conflicts.First();

        var expectedId = vm.SelectedConflict.Id;

        await vm.AcceptCloudVersionCommand.ExecuteAsync(null);

        mockEngine.Verify(e => e.ResolveConflictAsync(expectedId, "ServerWins", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Empty(vm.Conflicts);
        Assert.Null(vm.SelectedConflict);
    }
}
