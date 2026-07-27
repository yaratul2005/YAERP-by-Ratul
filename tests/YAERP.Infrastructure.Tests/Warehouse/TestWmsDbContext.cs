using System;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Warehouse;

namespace YAERP.Infrastructure.Tests.Warehouse;

public class TestWmsDbContext : DbContext, IApplicationDbContext
{
    public TestWmsDbContext(DbContextOptions<TestWmsDbContext> options) : base(options) { }

    public DbSet<WarehouseZone> WarehouseZones { get; set; }
    public DbSet<WarehouseBin> WarehouseBins { get; set; }
    public DbSet<InventoryLot> InventoryLots { get; set; }
    public DbSet<ProductSerialNumber> ProductSerialNumbers { get; set; }
    public DbSet<InventoryCostLayer> InventoryCostLayers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WarehouseZone>().HasKey(x => x.Id);
        modelBuilder.Entity<WarehouseBin>().HasKey(x => x.Id);
        modelBuilder.Entity<InventoryLot>().HasKey(x => x.Id);
        modelBuilder.Entity<ProductSerialNumber>().HasKey(x => x.Id);
        modelBuilder.Entity<InventoryCostLayer>().HasKey(x => x.Id);

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
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.BomHeader>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.BomItem>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.WorkCenter>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.RoutingStep>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.WorkOrder>();
        base.OnModelCreating(modelBuilder);
    }

    // Implementing interface properties implicitly unused
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
    public DbSet<YAERP.Domain.HR.Employee> Employees => throw new NotImplementedException();
    public DbSet<YAERP.Domain.HR.Payroll> Payrolls => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.BomHeader> BomHeaders => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.BomItem> BomItems => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.WorkCenter> WorkCenters => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.RoutingStep> RoutingSteps => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.WorkOrder> WorkOrders => throw new NotImplementedException();
}
