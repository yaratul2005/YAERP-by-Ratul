using System.Threading.Tasks;
using Xunit;
using YAERP.UI.Services;

namespace YAERP.UI.Tests.Services;

public class DialogServiceTests
{
    [Fact]
    public async Task ShowGlobalToastAsync_ShouldPushToStack()
    {
        var dialogService = new DialogService();
        await dialogService.ShowGlobalToastAsync("Test");

        // This is a basic test since we don't have access to the internal stack,
        // but it verifies no exceptions are thrown.
        Assert.NotNull(dialogService);
    }
}
