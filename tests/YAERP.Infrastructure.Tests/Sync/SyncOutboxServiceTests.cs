using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using YAERP.Infrastructure.Sync;
using YAERP.Domain.Entities.Sync;

namespace YAERP.Infrastructure.Tests.Sync;

public class SyncOutboxServiceTests
{
    [Fact]
    public async Task EnqueueMutationAsync_ShouldAddPendingMessage()
    {
        var service = new SyncOutboxService();
        await service.EnqueueMutationAsync("Customer", Guid.NewGuid(), "Create", "{}", CancellationToken.None);

        var pending = await service.GetPendingMessagesAsync(100, CancellationToken.None);
        Assert.Single(pending);

        var msg = pending.First() as OutboxMessage;
        Assert.NotNull(msg);
        Assert.Equal("Pending", msg!.Status);
        Assert.Equal("Customer", msg.EntityType);
    }
}
