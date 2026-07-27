using System;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Financials;

namespace YAERP.Infrastructure.Tests.Financials;

public class TestFinDbContext : DbContext, IApplicationDbContext
{
    public TestFinDbContext(DbContextOptions<TestFinDbContext> options) : base(options) { }

    public DbSet<FixedAsset> FixedAssets { get; set; }
    public DbSet<DepreciationScheduleEntry> DepreciationScheduleEntries { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    public DbSet<TaxRule> TaxRules { get; set; }

    public DbSet<YAERP.Domain.Sales.SalesOrder> SalesOrders { get; set; }
    public DbSet<YAERP.Domain.Finance.Invoice> Invoices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FixedAsset>().HasKey(x => x.Id);
        modelBuilder.Entity<DepreciationScheduleEntry>().HasKey(x => x.Id);
        modelBuilder.Entity<Currency>().HasKey(x => x.Code);
        modelBuilder.Entity<ExchangeRate>().HasKey(x => x.Id);
        modelBuilder.Entity<TaxRule>().HasKey(x => x.Id);
        modelBuilder.Entity<YAERP.Domain.Sales.SalesOrder>().HasKey(x => x.Id);
        modelBuilder.Entity<YAERP.Domain.Sales.SalesOrderItem>().HasKey(x => x.Id);
        modelBuilder.Entity<YAERP.Domain.Finance.Invoice>().HasKey(x => x.Id);
        modelBuilder.Entity<YAERP.Domain.Finance.Invoice>().Property(x => x.Id).HasConversion(id => id.Value, value => new YAERP.Domain.Finance.InvoiceId(value));
        modelBuilder.Entity<YAERP.Domain.Sales.SalesOrder>().Property(x => x.Id).HasConversion(id => id.Value, value => new YAERP.Domain.Sales.SalesOrderId(value));
        modelBuilder.Entity<YAERP.Domain.Sales.SalesOrderItem>().Property(x => x.Id).HasConversion(id => id.Value, value => new YAERP.Domain.Sales.SalesOrderItemId(value));

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
        modelBuilder.Ignore<YAERP.Domain.Finance.Account>();
        modelBuilder.Ignore<YAERP.Domain.Finance.JournalEntry>();
        modelBuilder.Ignore<YAERP.Domain.HR.Employee>();
        modelBuilder.Ignore<YAERP.Domain.HR.Payroll>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.BomHeader>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.BomItem>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.WorkCenter>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.RoutingStep>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Manufacturing.WorkOrder>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Warehouse.WarehouseZone>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Warehouse.WarehouseBin>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Warehouse.InventoryLot>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Warehouse.ProductSerialNumber>();
        modelBuilder.Ignore<YAERP.Domain.Entities.Warehouse.InventoryCostLayer>();


        modelBuilder.Ignore<YAERP.Domain.Identity.TenantId>();
        modelBuilder.Ignore<YAERP.Domain.Identity.UserId>();
        modelBuilder.Ignore<YAERP.Domain.Identity.RoleId>();
        modelBuilder.Ignore<YAERP.Domain.Inventory.ProductId>();
        modelBuilder.Ignore<YAERP.Domain.Inventory.WarehouseId>();
        modelBuilder.Ignore<YAERP.Domain.Purchasing.VendorId>();
        modelBuilder.Ignore<YAERP.Domain.Purchasing.PurchaseOrderId>();
        modelBuilder.Ignore<YAERP.Domain.Sales.CustomerId>();
        modelBuilder.Ignore<YAERP.Domain.Sales.SalesOrderId>();
        modelBuilder.Ignore<YAERP.Domain.Finance.InvoiceId>();
        modelBuilder.Ignore<YAERP.Domain.Finance.AccountId>();
        modelBuilder.Ignore<YAERP.Domain.Finance.JournalEntryId>();

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
    public DbSet<YAERP.Domain.Finance.Account> Accounts => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Finance.JournalEntry> JournalEntries => throw new NotImplementedException();
    public DbSet<YAERP.Domain.HR.Employee> Employees => throw new NotImplementedException();
    public DbSet<YAERP.Domain.HR.Payroll> Payrolls => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.BomHeader> BomHeaders => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.BomItem> BomItems => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.WorkCenter> WorkCenters => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.RoutingStep> RoutingSteps => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Manufacturing.WorkOrder> WorkOrders => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Warehouse.WarehouseZone> WarehouseZones => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Warehouse.WarehouseBin> WarehouseBins => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Warehouse.InventoryLot> InventoryLots => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Warehouse.ProductSerialNumber> ProductSerialNumbers => throw new NotImplementedException();
    public DbSet<YAERP.Domain.Entities.Warehouse.InventoryCostLayer> InventoryCostLayers => throw new NotImplementedException();
}
