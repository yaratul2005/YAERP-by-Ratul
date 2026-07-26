using Microsoft.EntityFrameworkCore;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;

namespace YAERP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Product> Products { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<StockMovement> StockMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
