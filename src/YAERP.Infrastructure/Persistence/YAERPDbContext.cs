using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Purchasing;
using YAERP.Domain.Sales;
using YAERP.Domain.Finance;
using YAERP.Domain.HR;
using YAERP.Domain.Entities.Manufacturing;
using YAERP.Domain.Entities.Warehouse;
using YAERP.Infrastructure.Persistence.Interceptors;

namespace YAERP.Infrastructure.Persistence;

public class YAERPDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly AuditSaveInterceptor _auditSaveInterceptor;
    private readonly SyncOutboxInterceptor _syncOutboxInterceptor;

    public YAERPDbContext(
        DbContextOptions<YAERPDbContext> options,
        ITenantContext tenantContext,
        AuditSaveInterceptor auditSaveInterceptor,
        SyncOutboxInterceptor syncOutboxInterceptor) : base(options)
    {
        _tenantContext = tenantContext;
        _auditSaveInterceptor = auditSaveInterceptor;
        _syncOutboxInterceptor = syncOutboxInterceptor;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SyncQueueItem> SyncQueueItems => Set<SyncQueueItem>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalStepLog> ApprovalStepLogs => Set<ApprovalStepLog>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<YAERP.Domain.Inventory.Warehouse> Warehouses => Set<YAERP.Domain.Inventory.Warehouse>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<BomHeader> BomHeaders => Set<BomHeader>();
    public DbSet<BomItem> BomItems => Set<BomItem>();
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<RoutingStep> RoutingSteps => Set<RoutingStep>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WarehouseZone> WarehouseZones => Set<WarehouseZone>();
    public DbSet<WarehouseBin> WarehouseBins => Set<WarehouseBin>();
    public DbSet<InventoryLot> InventoryLots => Set<InventoryLot>();
    public DbSet<ProductSerialNumber> ProductSerialNumbers => Set<ProductSerialNumber>();
    public DbSet<InventoryCostLayer> InventoryCostLayers => Set<InventoryCostLayer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(YAERPDbContext).Assembly);

        var tenantId = _tenantContext.CurrentTenantId;

        // Global Query Filters for Multi-Tenancy
        modelBuilder.Entity<User>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<Role>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(x => x.TenantId == tenantId);

        modelBuilder.Entity<Product>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<YAERP.Domain.Inventory.Warehouse>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<ProductCategory>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<StockMovement>().HasQueryFilter(x => x.TenantId == tenantId);

        modelBuilder.Entity<Vendor>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(x => x.TenantId == tenantId);

        modelBuilder.Entity<Customer>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<SalesOrder>().HasQueryFilter(x => x.TenantId == tenantId);

        modelBuilder.Entity<Account>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<JournalEntry>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<Invoice>().HasQueryFilter(x => x.TenantId == tenantId);

        modelBuilder.Entity<Employee>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<Payroll>().HasQueryFilter(x => x.TenantId == tenantId);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditSaveInterceptor, _syncOutboxInterceptor);
        base.OnConfiguring(optionsBuilder);
    }
}
