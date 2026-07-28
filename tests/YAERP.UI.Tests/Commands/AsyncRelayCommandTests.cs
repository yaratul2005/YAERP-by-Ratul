using System.Threading.Tasks;
using Xunit;
using YAERP.UI.Commands;

namespace YAERP.UI.Tests.Commands;

public class AsyncRelayCommandTests
{
    [Fact]
    public async Task Execute_ShouldBlockDoubleExecution()
    {
        int executionCount = 0;
        var tcs = new TaskCompletionSource();
        var command = new AsyncRelayCommand(async () =>
        {
            executionCount++;
            await tcs.Task;
        });

        Assert.True(command.CanExecute(null));

        command.Execute(null); // Starts execution, sets IsExecuting to true
        Assert.False(command.CanExecute(null));

        command.Execute(null); // Should be blocked

        Assert.Equal(1, executionCount);

        tcs.SetResult(); // Finish first execution
        // We'd ideally await a tiny delay or use a dispatcher, but in this simple mock it synchronously completes the await
    }
}
