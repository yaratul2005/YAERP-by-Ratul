using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Identity;

public class AuditLog : Entity<AuditLogId>
{
    private AuditLog(AuditLogId id, TenantId? tenantId, string? userId, string entityName, string action, string? oldValuesJson, string? newValuesJson, DateTime timestampUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        EntityName = entityName;
        Action = action;
        OldValuesJson = oldValuesJson;
        NewValuesJson = newValuesJson;
        TimestampUtc = timestampUtc;
    }

    private AuditLog() { }

    public TenantId? TenantId { get; private set; }
    public string? UserId { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? OldValuesJson { get; private set; }
    public string? NewValuesJson { get; private set; }
    public DateTime TimestampUtc { get; private set; }

    public static AuditLog Create(TenantId? tenantId, string? userId, string entityName, string action, string? oldValuesJson, string? newValuesJson)
    {
        return new AuditLog(new AuditLogId(Guid.NewGuid()), tenantId, userId, entityName, action, oldValuesJson, newValuesJson, DateTime.UtcNow);
    }
}
