using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Domain.Entities;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Purchasing;
using YAERP.Domain.Sales;
using YAERP.Domain.Finance;
using YAERP.Domain.HR;
using YAERP.Domain.Entities.Manufacturing;

namespace YAERP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<SyncQueueItem> SyncQueueItems { get; }
    DbSet<ApprovalRequest> ApprovalRequests { get; }
    DbSet<ApprovalStepLog> ApprovalStepLogs { get; }

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
    DbSet<BomHeader> BomHeaders { get; }
    DbSet<BomItem> BomItems { get; }
    DbSet<WorkCenter> WorkCenters { get; }
    DbSet<RoutingStep> RoutingSteps { get; }
    DbSet<WorkOrder> WorkOrders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}
