using System.Threading.Tasks;
using MediatR;
using Moq;
using Xunit;
using YAERP.UI.ViewModels;

namespace YAERP.UI.Tests;

public class InventoryViewModelTests
{
    [Fact]
    public async Task LoadProductsAsync_Should_PopulateProductsCollection()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new InventoryViewModel(mediatorMock.Object);

        Assert.Empty(viewModel.Products);

        // Act
        await viewModel.LoadProductsAsyncCommand.ExecuteAsync(null);

        // Assert
        Assert.NotEmpty(viewModel.Products);
        Assert.Equal(2, viewModel.Products.Count);
        Assert.Equal("Sample Item A", viewModel.Products[0].Name);
    }
}
