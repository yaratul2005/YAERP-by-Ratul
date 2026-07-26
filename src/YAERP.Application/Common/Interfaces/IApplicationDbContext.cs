using Microsoft.EntityFrameworkCore;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Purchasing;
using YAERP.Domain.Sales;
using YAERP.Domain.Finance;
using YAERP.Domain.HR;

namespace YAERP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<YAERP.Domain.Entities.SyncQueueItem> SyncQueueItems { get; }
    DbSet<YAERP.Domain.Entities.ApprovalRequest> ApprovalRequests { get; }
    DbSet<YAERP.Domain.Entities.ApprovalStepLog> ApprovalStepLogs { get; }

    DbSet<Product> Products { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<UnitOfMeasure> UnitsOfMeasure { get; }

    DbSet<Vendor> Vendors { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }

    DbSet<Customer> Customers { get; }
    DbSet<SalesOrder> SalesOrders { get; }

    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<Invoice> Invoices { get; }

    DbSet<Employee> Employees { get; }
    DbSet<Payroll> Payrolls { get; }

    Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default);
}
