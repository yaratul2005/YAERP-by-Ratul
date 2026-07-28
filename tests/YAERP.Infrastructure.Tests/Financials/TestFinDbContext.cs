using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities;
using YAERP.Domain.Entities.Financials;
using YAERP.Domain.Entities.Hcm;
using YAERP.Domain.Entities.Manufacturing;
using YAERP.Domain.Entities.Warehouse;
using YAERP.Domain.Finance;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Purchasing;
using YAERP.Domain.Sales;

namespace YAERP.Infrastructure.Tests.Financials;

public class TestFinDbContext : DbContext, IApplicationDbContext
{
    public TestFinDbContext(DbContextOptions<TestFinDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<SyncQueueItem> SyncQueueItems { get; set; }
    public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
    public DbSet<ApprovalStepLog> ApprovalStepLogs { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Domain.Inventory.Warehouse> Warehouses { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<BomHeader> BomHeaders { get; set; }
    public DbSet<BomItem> BomItems { get; set; }
    public DbSet<WorkCenter> WorkCenters { get; set; }
    public DbSet<RoutingStep> RoutingSteps { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WarehouseZone> WarehouseZones { get; set; }
    public DbSet<WarehouseBin> WarehouseBins { get; set; }
    public DbSet<InventoryLot> InventoryLots { get; set; }
    public DbSet<ProductSerialNumber> ProductSerialNumbers { get; set; }
    public DbSet<InventoryCostLayer> InventoryCostLayers { get; set; }
    public DbSet<FixedAsset> FixedAssets { get; set; }
    public DbSet<DepreciationScheduleEntry> DepreciationScheduleEntries { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    public DbSet<TaxRule> TaxRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(YAERP.Infrastructure.Persistence.YAERPDbContext).Assembly);
        modelBuilder.Entity<FixedAsset>().HasKey(x => x.Id);
        modelBuilder.Entity<DepreciationScheduleEntry>().HasKey(x => x.Id);
        modelBuilder.Entity<Currency>().HasKey(x => x.Code);
        modelBuilder.Entity<ExchangeRate>().HasKey(x => x.Id);
        modelBuilder.Entity<TaxRule>().HasKey(x => x.Id);
        modelBuilder.Entity<Invoice>().HasKey(x => x.Id);

        modelBuilder.Ignore<Tenant>();
        modelBuilder.Ignore<User>();
        modelBuilder.Ignore<Role>();
        modelBuilder.Ignore<AuditLog>();
        modelBuilder.Ignore<SyncQueueItem>();
        modelBuilder.Ignore<ApprovalRequest>();
        modelBuilder.Ignore<ApprovalStepLog>();
        modelBuilder.Ignore<Product>();
        modelBuilder.Ignore<Domain.Inventory.Warehouse>();
        modelBuilder.Ignore<ProductCategory>();
        modelBuilder.Ignore<StockMovement>();
        modelBuilder.Ignore<UnitOfMeasure>();
        modelBuilder.Ignore<Vendor>();
        modelBuilder.Ignore<PurchaseOrder>();
        modelBuilder.Ignore<Customer>();
        modelBuilder.Ignore<SalesOrder>();
        modelBuilder.Ignore<Account>();
        modelBuilder.Ignore<JournalEntry>();
        modelBuilder.Ignore<Employee>();
        modelBuilder.Ignore<AttendanceRecord>();
        modelBuilder.Ignore<BomHeader>();
        modelBuilder.Ignore<BomItem>();
        modelBuilder.Ignore<WorkCenter>();
        modelBuilder.Ignore<RoutingStep>();
        modelBuilder.Ignore<WorkOrder>();
        modelBuilder.Ignore<WarehouseZone>();
        modelBuilder.Ignore<WarehouseBin>();
        modelBuilder.Ignore<InventoryLot>();
        modelBuilder.Ignore<ProductSerialNumber>();
        modelBuilder.Ignore<InventoryCostLayer>();
        base.OnModelCreating(modelBuilder);
    }
}
