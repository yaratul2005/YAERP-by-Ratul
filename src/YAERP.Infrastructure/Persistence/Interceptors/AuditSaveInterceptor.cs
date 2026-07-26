using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Identity;
using System.Text.Json;

namespace YAERP.Infrastructure.Persistence.Interceptors;

public class AuditSaveInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;

    public AuditSaveInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AuditEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AuditEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AuditEntities(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog) continue;

            var entityName = entry.Entity.GetType().Name;
            var action = entry.State.ToString();
            string? oldValues = null;
            string? newValues = null;

            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                var originalValues = new Dictionary<string, object?>();
                foreach (var property in entry.OriginalValues.Properties)
                {
                    originalValues[property.Name] = entry.OriginalValues[property];
                }
                oldValues = JsonSerializer.Serialize(originalValues);
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                var currentValues = new Dictionary<string, object?>();
                foreach (var property in entry.CurrentValues.Properties)
                {
                    currentValues[property.Name] = entry.CurrentValues[property];
                }
                newValues = JsonSerializer.Serialize(currentValues);
            }

            var auditLog = AuditLog.Create(
                _tenantContext.CurrentTenantId,
                _tenantContext.CurrentUserId?.Value.ToString(),
                entityName,
                action,
                oldValues,
                newValues);

            auditLogs.Add(auditLog);
        }

        if (auditLogs.Any())
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }
}
