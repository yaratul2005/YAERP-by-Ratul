using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            if (_context.Database.IsRelational())
            {
                await _context.Database.MigrateAsync(cancellationToken);
            }

            // Seed Default Tenant
            var defaultTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Name == "Default System Tenant", cancellationToken);
            if (defaultTenant == null)
            {
                defaultTenant = Tenant.Create("Default System Tenant", null);
                _context.Tenants.Add(defaultTenant);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var tenantId = defaultTenant.Id;

            // Seed Default Admin User
            if (!await _context.Users.AnyAsync(u => u.Email == "admin@yaerp.local", cancellationToken))
            {
                var admin = User.Create(tenantId, "admin@yaerp.local", "HASHED_admin123", "System", "Administrator");
                _context.Users.Add(admin);
            }

            // Seed Chart of Accounts
            if (!await _context.Accounts.AnyAsync(a => a.TenantId == tenantId, cancellationToken))
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
            if (!await _context.Set<UnitOfMeasure>().AnyAsync(cancellationToken))
            {
                _context.Set<UnitOfMeasure>().AddRange(
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
