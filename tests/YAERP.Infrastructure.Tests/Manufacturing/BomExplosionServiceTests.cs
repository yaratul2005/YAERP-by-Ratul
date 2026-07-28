using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Manufacturing;
using YAERP.Infrastructure.Manufacturing;

namespace YAERP.Infrastructure.Tests.Manufacturing;

public class TestBomDbContext : DbContext, IApplicationDbContext
{
    public TestBomDbContext(DbContextOptions<TestBomDbContext> options) : base(options) { }

    public DbSet<BomHeader> BomHeaders { get; set; }
    public DbSet<BomItem> BomItems { get; set; }
    public DbSet<WorkCenter> WorkCenters { get; set; }
    public DbSet<RoutingStep> RoutingSteps { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BomHeader>().HasKey(x => x.Id);
        modelBuilder.Entity<BomItem>().HasKey(x => x.Id);
        modelBuilder.Entity<WorkCenter>().HasKey(x => x.Id);
        modelBuilder.Entity<RoutingStep>().HasKey(x => x.Id);
        modelBuilder.Entity<WorkOrder>().HasKey(x => x.Id);
        modelBuilder.Ignore<YAERP.Domain.Identity.Tenant>();
        modelBuilder.Ignore<YAERP.Domain.Identity.User>();
        modelBuilder.Ignore<YAERP.Domain.Identity.Role>();
        modelBuilder.Ignore<YAERP.Domain.Identity.AuditLog>();
        modelBuilder.Ignore<YAERP.Domain.Entities.SyncQueueItem>();
        modelBuilder.Ignore<YAERP.Domain.Entities.ApprovalRequest>();
        modelBuilder.Ignore<YAERP.Domain.Entities.ApprovalStepLog>();
        modelBuilder.Ignore<YAERP.Domain.Inventory.Product>();
        modelBuilder.Ignore<YAERP.Domain.Inventory.Warehouse>();
        modelBuilder.Ignore<YAERP.Domain.Inventory.ProductCategory>();
        modelBuilder.Ignore<YAERP.Domain.Inventory.StockMovement>();
        modelBuilder.Ignore<YAERP.Domain.Inventory.UnitOfMeasure>();
        modelBuilder.Ignore<YAERP.Domain.Purchasing.Vendor>();
        modelBuilder.Ignore<YAERP.Domain.Purchasing.PurchaseOrder>();
        modelBuilder.Ignore<YAERP.Domain.Sales.Customer>();
        modelBuilder.Ignore<YAERP.Domain.Sales.SalesOrder>();
        modelBuilder.Ignore<YAERP.Domain.Sales.SalesOrderItem>();
        modelBuilder.Ignore<YAERP.Domain.Finance.Account>();
        modelBuilder.Ignore<YAERP.Domain.Finance.JournalEntry>();
        modelBuilder.Ignore<YAERP.Domain.Finance.Invoice>();
        modelBuilder.Ignore<YAERP.Domain.HR.Employee>();
        modelBuilder.Ignore<YAERP.Domain.HR.Payroll>();

        modelBuilder.Ignore<YAERP.Domain.Entities.Financials.FixedAsset>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Financials.DepreciationScheduleEntry>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Financials.Currency>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Financials.ExchangeRate>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Financials.TaxRule>();
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<YAERP.Domain.Identity.Tenant> Tenants => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Identity.User> Users => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Identity.Role> Roles => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Identity.AuditLog> AuditLogs => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.SyncQueueItem> SyncQueueItems => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.ApprovalRequest> ApprovalRequests => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.ApprovalStepLog> ApprovalStepLogs => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Inventory.Product> Products => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Inventory.Warehouse> Warehouses => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Inventory.ProductCategory> ProductCategories => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Inventory.StockMovement> StockMovements => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Inventory.UnitOfMeasure> UnitsOfMeasure => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Purchasing.Vendor> Vendors => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Purchasing.PurchaseOrder> PurchaseOrders => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Sales.Customer> Customers => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Sales.SalesOrder> SalesOrders => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Finance.Account> Accounts => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Finance.JournalEntry> JournalEntries => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Finance.Invoice> Invoices => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Hcm.Employee> Employees => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Hcm.AttendanceRecord> AttendanceRecords => throw new NotImplementedException();
    public DbSet<YAERP.Domain.HR.Payroll> Payrolls => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Financials.FixedAsset> FixedAssets => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Financials.DepreciationScheduleEntry> DepreciationScheduleEntries => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Financials.Currency> Currencies => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Financials.ExchangeRate> ExchangeRates => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Financials.TaxRule> TaxRules => throw new NotImplementedException();

    public DbSet<YAERP.Domain.Entities.Warehouse.WarehouseZone> WarehouseZones => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Warehouse.WarehouseBin> WarehouseBins => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Warehouse.InventoryLot> InventoryLots => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Warehouse.ProductSerialNumber> ProductSerialNumbers => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Warehouse.InventoryCostLayer> InventoryCostLayers => throw new NotImplementedException();
}

public class BomExplosionServiceTests : IDisposable
{
    private readonly TestBomDbContext _dbContext;
    private readonly BomExplosionService _sut;

    public BomExplosionServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestBomDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestBomDbContext(options);
        _sut = new BomExplosionService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task EnsureNoCyclesAsync_WithCycle_ShouldThrowException()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _dbContext.BomHeaders.Add(new BomHeader
        {
            Id = Guid.NewGuid(),
            AssemblyProductId = childId,
            IsActive = true,
            Components = new List<BomItem>
            {
                new BomItem { Id = Guid.NewGuid(), ComponentProductId = parentId }
            }
        });
        await _dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidBomCycleException>(() =>
            _sut.EnsureNoCyclesAsync(parentId, childId));
    }

    [Fact]
    public async Task EnsureNoCyclesAsync_WithoutCycle_ShouldCompleteSuccessfully()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var grandChildId = Guid.NewGuid();

        _dbContext.BomHeaders.Add(new BomHeader
        {
            Id = Guid.NewGuid(),
            AssemblyProductId = childId,
            IsActive = true,
            Components = new List<BomItem>
            {
                new BomItem { Id = Guid.NewGuid(), ComponentProductId = grandChildId }
            }
        });
        await _dbContext.SaveChangesAsync();

        await _sut.EnsureNoCyclesAsync(parentId, childId);
    }

    [Fact]
    public async Task ExplodeBomAsync_ShouldCalculateYieldAndScrapCorrectly()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _dbContext.BomHeaders.Add(new BomHeader
        {
            Id = Guid.NewGuid(),
            AssemblyProductId = parentId,
            IsActive = true,
            BaseQuantity = 1m,
            Components = new List<BomItem>
            {
                new BomItem
                {
                    Id = Guid.NewGuid(),
                    ComponentProductId = childId,
                    QuantityPerAssembly = 10m,
                    YieldPercent = 80m, // 80% yield means we need 10 / 0.8 = 12.5
                    ScrapFactorPercent = 10m // 10% scrap means we need 12.5 * 1.1 = 13.75
                }
            }
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.ExplodeBomAsync(parentId, 1m);

        Assert.Single(result);
        Assert.Equal(childId, result[0].ComponentProductId);
        Assert.Equal(13.75m, result[0].EffectiveQuantity);
    }
}
