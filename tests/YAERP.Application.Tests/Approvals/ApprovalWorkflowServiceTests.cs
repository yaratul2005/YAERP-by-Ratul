using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Identity;
using YAERP.Infrastructure.Persistence;
using YAERP.Infrastructure.Persistence.Interceptors;
using YAERP.Infrastructure.Workflows;

namespace YAERP.Application.Tests.Approvals;

public class ApprovalWorkflowServiceTests
{
    private class TestTenantContext : ITenantContext
    {
        private readonly TenantId _tenantId = new(Guid.NewGuid());
        private readonly UserId _userId = new(Guid.NewGuid());

        public TenantId? CurrentTenantId => _tenantId;
        public UserId? CurrentUserId => _userId;
    }

    private YAERPDbContext CreateDbContext()
    {
        var dbOptions = new DbContextOptionsBuilder<YAERPDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TestTenantContext();
        var auditInterceptor = new AuditSaveInterceptor(tenantContext);
        var syncInterceptor = new SyncOutboxInterceptor();

        return new YAERPDbContext(dbOptions, tenantContext, auditInterceptor, syncInterceptor);
    }

    [Fact]
    public async Task InitiateApprovalAsync_Should_ReturnNull_WhenAmountIsLessThan5000()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var loggerMock = new Mock<ILogger<ApprovalWorkflowService>>();
        var service = new ApprovalWorkflowService(dbContext, loggerMock.Object);

        // Act
        var result = await service.InitiateApprovalAsync("PurchaseOrder", "PO-101", 3500m, "User-1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InitiateApprovalAsync_Should_RequireTier1Supervisor_WhenAmountIsBetween5000And25000()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var loggerMock = new Mock<ILogger<ApprovalWorkflowService>>();
        var service = new ApprovalWorkflowService(dbContext, loggerMock.Object);

        // Act
        var result = await service.InitiateApprovalAsync("PurchaseOrder", "PO-102", 15000m, "User-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Tier1_Supervisor", result!.CurrentApprovalLevel);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task ProcessApprovalStepAsync_Should_EscalateToTier2_WhenAmountIs25000OrGreater()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var loggerMock = new Mock<ILogger<ApprovalWorkflowService>>();
        var service = new ApprovalWorkflowService(dbContext, loggerMock.Object);

        // Act 1: Initiate $30,000 approval
        var requestDto = await service.InitiateApprovalAsync("PurchaseOrder", "PO-103", 30000m, "User-1");
        Assert.NotNull(requestDto);

        // Act 2: Process Tier 1 Approval
        bool tier1Success = await service.ProcessApprovalStepAsync(requestDto!.Id, "Supervisor-1", true, "Tier 1 Looks Good");
        Assert.True(tier1Success);

        var updatedRequest1 = await dbContext.ApprovalRequests.FindAsync(requestDto.Id);
        Assert.NotNull(updatedRequest1);
        Assert.Equal("Pending", updatedRequest1!.Status);
        Assert.Equal("Tier2_FinanceManager", updatedRequest1.CurrentApprovalLevel);

        // Act 3: Process Tier 2 Approval
        bool tier2Success = await service.ProcessApprovalStepAsync(requestDto.Id, "FinanceMgr-1", true, "Tier 2 Approved");
        Assert.True(tier2Success);

        var updatedRequest2 = await dbContext.ApprovalRequests.FindAsync(requestDto.Id);
        Assert.NotNull(updatedRequest2);
        Assert.Equal("Approved", updatedRequest2!.Status);
        Assert.NotNull(updatedRequest2.CompletedAtUtc);
    }

    [Fact]
    public async Task ProcessApprovalStepAsync_Should_SetStatusToRejected_WhenIsApprovedIsFalse()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var loggerMock = new Mock<ILogger<ApprovalWorkflowService>>();
        var service = new ApprovalWorkflowService(dbContext, loggerMock.Object);

        var requestDto = await service.InitiateApprovalAsync("PurchaseOrder", "PO-104", 10000m, "User-1");
        Assert.NotNull(requestDto);

        // Act: Reject request
        bool success = await service.ProcessApprovalStepAsync(requestDto!.Id, "Supervisor-1", false, "Over budget ceiling");

        // Assert
        Assert.True(success);
        var rejectedRequest = await dbContext.ApprovalRequests.FindAsync(requestDto.Id);
        Assert.NotNull(rejectedRequest);
        Assert.Equal("Rejected", rejectedRequest!.Status);
        Assert.Equal("Over budget ceiling", rejectedRequest.RejectionReason);
        Assert.NotNull(rejectedRequest.CompletedAtUtc);
    }
}
