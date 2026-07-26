using System.Threading.Tasks;
using MediatR;
using Moq;
using Xunit;
using YAERP.UI.ViewModels;

namespace YAERP.UI.Tests;

public class DashboardViewModelTests
{
    [Fact]
    public async Task LoadMetricsAsync_Should_SetPropertiesAndManageIsBusyState()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new DashboardViewModel(mediatorMock.Object);

        Assert.False(viewModel.IsBusy);
        Assert.Equal(0, viewModel.TotalSales);

        // Act
        var loadTask = viewModel.LoadMetricsAsyncCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsBusy);

        await loadTask;

        // Assert
        Assert.False(viewModel.IsBusy);
        Assert.Equal(120, viewModel.TotalSales);
        Assert.Equal(4500, viewModel.StockCount);
        Assert.Equal(15, viewModel.ActiveInvoices);
    }
}
