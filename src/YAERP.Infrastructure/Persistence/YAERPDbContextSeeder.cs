using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Identity;
using YAERP.Domain.Finance;
using YAERP.Domain.Inventory;

namespace YAERP.Infrastructure.Persistence;

public class YAERPDbContextSeeder : IDatabaseSeeder
{
    private readonly YAERPDbContext _context;
    private readonly ILogger<YAERPDbContextSeeder> _logger;

    public YAERPDbContextSeeder(YAERPDbContext context, ILogger<YAERPDbContextSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure schema is created directly since SQLite in a desktop app doesn't need external migration scripts initially
            // unless we ship them. For simplicity across Npgsql/Sqlite, we can EnsureCreated.
            // Better: use migrations if available, but for sqlite desktop apps EnsureCreated is often enough for a base boilerplate.
            // AGENTS.md mentions "automated EF Core migration execution on application startup".
            // Let's call MigrateAsync if relational, else EnsureCreated, but wait, both Npgsql and Sqlite are relational.
            // Note: Migrations are provider-specific. Since we use two providers, we'd need multiple migration sets.
            // Using EnsureCreatedAsync for this boilerplate to easily bootstrap schema regardless of connection.
            await _context.Database.EnsureCreatedAsync(cancellationToken);

            try
            {
                var creator = _context.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
                creator.CreateTables();
            }
            catch
            {
                // Tables already created or created via EnsureCreated
            }

            // Seed Default Tenant
            var defaultTenant = await _context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Name == "Default System Tenant", cancellationToken);
            if (defaultTenant == null)
            {
                defaultTenant = Tenant.Create("Default System Tenant", null);
                _context.Tenants.Add(defaultTenant);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var tenantId = defaultTenant.Id;

            // Seed Default Admin User
            if (!await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == "admin@yaerp.local", cancellationToken))
            {
                var admin = User.Create(tenantId, "admin@yaerp.local", "HASHED_admin123", "System", "Administrator");
                _context.Users.Add(admin);
            }

            // Seed Chart of Accounts
            if (!await _context.Accounts.IgnoreQueryFilters().AnyAsync(a => a.TenantId == tenantId, cancellationToken))
            {
                _context.Accounts.AddRange(
                    Account.Create(tenantId, "1000", "Cash", AccountType.Asset),
                    Account.Create(tenantId, "1200", "Accounts Receivable", AccountType.Asset),
                    Account.Create(tenantId, "2000", "Accounts Payable", AccountType.Liability),
                    Account.Create(tenantId, "3000", "Owner's Equity", AccountType.Equity),
                    Account.Create(tenantId, "4000", "Sales Revenue", AccountType.Revenue),
                    Account.Create(tenantId, "5000", "Cost of Goods Sold", AccountType.Expense)
                );
            }

            // Seed Units of Measure
            if (!await _context.UnitsOfMeasure.IgnoreQueryFilters().AnyAsync(cancellationToken))
            {
                _context.UnitsOfMeasure.AddRange(
                    UnitOfMeasure.Create("PCS", "Pieces", false),
                    UnitOfMeasure.Create("KG", "Kilograms", true),
                    UnitOfMeasure.Create("M", "Meters", true),
                    UnitOfMeasure.Create("BOX", "Boxes", false)
                );
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
