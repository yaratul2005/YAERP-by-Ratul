using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.ViewModels.Security;

namespace YAERP.UI.Tests.ViewModels.Security;

public class UserAndRolesViewModelTests
{
    [Fact]
    public async Task OpenCreateUserModalAsync_ShouldCallDialogService()
    {
        var mockDialogService = new Mock<IDialogService>();
        var mockIdentityService = new Mock<IIdentityService>();

        var viewModel = new UserAndRolesViewModel(mockDialogService.Object, mockIdentityService.Object);

        await viewModel.OpenCreateUserModalCommand.ExecuteAsync(null);

        mockDialogService.Verify(d => d.ShowModalAsync("Create User", "UserEditorModalViewModel"), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldLogSecurityEvent()
    {
        var mockDialogService = new Mock<IDialogService>();
        var mockIdentityService = new Mock<IIdentityService>();

        var viewModel = new UserAndRolesViewModel(mockDialogService.Object, mockIdentityService.Object);
        var userId = Guid.NewGuid();

        await viewModel.ResetPasswordCommand.ExecuteAsync(userId);

        mockIdentityService.Verify(i => i.LogSecurityEventAsync(
            Guid.Empty,
            userId,
            "PasswordReset",
            "High",
            "Password reset initiated",
            "127.0.0.1",
            It.IsAny<CancellationToken>()), Times.Once);

        mockDialogService.Verify(d => d.ShowGlobalToastAsync("Password reset initiated"), Times.Once);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_ShouldToggleStatusAndLogEvent()
    {
        var mockDialogService = new Mock<IDialogService>();
        var mockIdentityService = new Mock<IIdentityService>();

        var viewModel = new UserAndRolesViewModel(mockDialogService.Object, mockIdentityService.Object);
        var user = new UserDto { Id = Guid.NewGuid(), IsActive = true };

        await viewModel.ToggleUserStatusCommand.ExecuteAsync(user);

        Assert.False(user.IsActive);
        mockIdentityService.Verify(i => i.LogSecurityEventAsync(
            Guid.Empty,
            user.Id,
            "StatusToggle",
            "Medium",
            "User status changed to False",
            "127.0.0.1",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
