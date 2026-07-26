using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities;
using YAERP.Infrastructure.Persistence;
using YAERP.Infrastructure.Persistence.Interceptors;
using YAERP.Infrastructure.Sync.Options;
using YAERP.Infrastructure.Sync.Protos;
using CloudSyncServiceImpl = YAERP.Infrastructure.Sync.Services.CloudSyncService;
using CloudSyncGrpcClient = YAERP.Infrastructure.Sync.Protos.CloudSyncService.CloudSyncServiceClient;

namespace YAERP.Infrastructure.Tests.Sync;

public class CloudSyncServiceTests
{
    private class TestTenantContext : ITenantContext
    {
        public YAERP.Domain.Identity.TenantId? CurrentTenantId => null;
        public YAERP.Domain.Identity.UserId? CurrentUserId => null;
    }

    [Fact]
    public async Task ProcessSyncBatchAsync_Should_MarkQueueItemsAsSynced_UponSuccessfulRpcResponse()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<YAERPDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TestTenantContext();
        var auditInterceptor = new AuditSaveInterceptor(tenantContext);
        var syncInterceptor = new SyncOutboxInterceptor();

        using var dbContext = new YAERPDbContext(dbOptions, tenantContext, auditInterceptor, syncInterceptor);

        var item1 = SyncQueueItem.Create("SalesOrder", "SO-1001", "INSERT", "{\"orderNumber\":\"SO-1001\"}");
        var item2 = SyncQueueItem.Create("Product", "PROD-2002", "UPDATE", "{\"sku\":\"PROD-2002\"}");

        dbContext.SyncQueueItems.Add(item1);
        dbContext.SyncQueueItems.Add(item2);
        await dbContext.SaveChangesAsync();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IApplicationDbContext)))
            .Returns(dbContext);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(scopeMock.Object);

        var syncOptions = Options.Create(new CloudSyncOptions
        {
            BranchId = "TEST-BRANCH-01",
            BatchSize = 50
        });

        var loggerMock = new Mock<ILogger<CloudSyncServiceImpl>>();

        var grpcClientMock = new Mock<CloudSyncGrpcClient>();

        var successResponse = new SyncBatchResponse
        {
            Success = true
        };
        successResponse.SyncedIds.Add(item1.Id.ToString());
        successResponse.SyncedIds.Add(item2.Id.ToString());

        var unaryCall = new AsyncUnaryCall<SyncBatchResponse>(
            Task.FromResult(successResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        grpcClientMock.Setup(c => c.SyncBatchAsync(
                It.IsAny<SyncBatchRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(unaryCall);

        var syncService = new CloudSyncServiceImpl(
            scopeFactoryMock.Object,
            grpcClientMock.Object,
            syncOptions,
            loggerMock.Object);

        // Act
        int syncedCount = await syncService.ProcessSyncBatchAsync(batchSize: 50);

        // Assert
        Assert.Equal(2, syncedCount);

        var updatedItem1 = await dbContext.SyncQueueItems.FindAsync(item1.Id);
        var updatedItem2 = await dbContext.SyncQueueItems.FindAsync(item2.Id);

        Assert.NotNull(updatedItem1);
        Assert.NotNull(updatedItem2);
        Assert.True(updatedItem1!.IsSynced);
        Assert.True(updatedItem2!.IsSynced);
        Assert.NotNull(updatedItem1.SyncedAtUtc);
        Assert.NotNull(updatedItem2.SyncedAtUtc);
        Assert.Null(updatedItem1.LastSyncError);
    }
}
