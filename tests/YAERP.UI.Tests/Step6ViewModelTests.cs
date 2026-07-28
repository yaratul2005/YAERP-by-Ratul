using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using Xunit;
using YAERP.Application.Approvals.Commands.ProcessApprovalDecision;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Plugins;
using YAERP.Application.Manufacturing.Commands.CreateBomHeader;
using YAERP.Domain.Common.Primitives;
using YAERP.UI.ViewModels;
using YAERP.UI.ViewModels.Modals;

namespace YAERP.UI.Tests;

public class Step6ViewModelTests
{
    [Fact]
    public async Task RejectionReasonModal_RequiresReason_BeforeDispatchingCommand()
    {
        var mockMediator = new Mock<IMediator>();
        var modalVm = new RejectionReasonModalViewModel(mockMediator.Object, Guid.NewGuid(), "PO-991");

        modalVm.RejectionReason = "   "; // Empty/whitespace
        await modalVm.ConfirmRejectionAsync();

        mockMediator.Verify(m => m.Send(It.IsAny<ProcessApprovalDecisionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal("Rejection justification reason is mandatory.", modalVm.StatusMessage);

        mockMediator.Setup(m => m.Send(It.IsAny<ProcessApprovalDecisionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        modalVm.RejectionReason = "Exceeds quarterly budget allocation limit.";
        await modalVm.ConfirmRejectionAsync();

        mockMediator.Verify(m => m.Send(It.IsAny<ProcessApprovalDecisionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BomEditorModal_SaveBom_DispatchesMediatRCommand()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<CreateBomHeaderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        bool notificationDispatched = false;
        bool closeRequested = false;

        var modalVm = new BomEditorModalViewModel(
            mockMediator.Object,
            onSuccessNotification: msg => notificationDispatched = true,
            onCloseRequested: () => closeRequested = true);

        await modalVm.SaveBomAsync();

        mockMediator.Verify(m => m.Send(It.IsAny<CreateBomHeaderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(notificationDispatched);
        Assert.True(closeRequested);
    }

    [Fact]
    public void PluginHub_EnumeratesLoadedAssemblies_InSandboxContext()
    {
        var mockPluginManager = new Mock<IPluginManager>();
        var mockPlugin = new Mock<IYaerpPlugin>();
        mockPlugin.Setup(p => p.PluginId).Returns("YAERP.Plugin.Test");
        mockPlugin.Setup(p => p.Name).Returns("Test Plugin");
        mockPlugin.Setup(p => p.Version).Returns("1.0.0");
        mockPlugin.Setup(p => p.Author).Returns("Test Author");

        mockPluginManager.Setup(m => m.LoadedPlugins).Returns(new List<IYaerpPlugin> { mockPlugin.Object });

        var pluginHubVm = new PluginHubViewModel(mockPluginManager.Object);

        Assert.Single(pluginHubVm.ActivePlugins);
        Assert.Equal("YAERP.Plugin.Test", pluginHubVm.ActivePlugins[0].PluginId);
    }
}
