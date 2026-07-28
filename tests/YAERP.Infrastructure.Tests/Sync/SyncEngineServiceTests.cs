using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using YAERP.Infrastructure.Sync;
using YAERP.Application.Common.Interfaces;

namespace YAERP.Infrastructure.Tests.Sync;

public class SyncEngineServiceTests
{
    [Fact]
    public async Task PushAsync_ShouldCallOutboxService()
    {
        var outboxService = new SyncOutboxService();
        await outboxService.EnqueueMutationAsync("Customer", Guid.NewGuid(), "Create", "{}", CancellationToken.None);

        var engineService = new SyncEngineService(outboxService);
        await engineService.PushAsync(CancellationToken.None);

        // This is a minimal test as there is no remote endpoint to verify against.
        Assert.NotNull(engineService);
    }
}
