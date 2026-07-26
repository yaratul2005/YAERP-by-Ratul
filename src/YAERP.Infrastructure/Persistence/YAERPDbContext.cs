using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Identity;
using YAERP.Infrastructure.Persistence.Interceptors;

namespace YAERP.Infrastructure.Persistence;

public class YAERPDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly AuditSaveInterceptor _auditSaveInterceptor;

    public YAERPDbContext(
        DbContextOptions<YAERPDbContext> options,
        ITenantContext tenantContext,
        AuditSaveInterceptor auditSaveInterceptor) : base(options)
    {
        _tenantContext = tenantContext;
        _auditSaveInterceptor = auditSaveInterceptor;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(YAERPDbContext).Assembly);

        var tenantId = _tenantContext.CurrentTenantId;

        // Global Query Filters for Multi-Tenancy
        modelBuilder.Entity<User>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<Role>().HasQueryFilter(x => x.TenantId == tenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(x => x.TenantId == tenantId);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditSaveInterceptor);
        base.OnConfiguring(optionsBuilder);
    }
}
