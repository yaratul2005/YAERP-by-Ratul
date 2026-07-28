using System;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Entities.Security;

public record UserSecurityAuditLogId(Guid Value);

public class UserSecurityAuditLog : Entity<UserSecurityAuditLogId>
{
    private UserSecurityAuditLog() { }

    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public DateTime TimestampUtc { get; private set; }

    public static UserSecurityAuditLog Create(Guid tenantId, Guid userId, string eventType, string severity, string details, string ipAddress)
    {
        return new UserSecurityAuditLog
        {
            Id = new UserSecurityAuditLogId(Guid.NewGuid()),
            TenantId = tenantId,
            UserId = userId,
            EventType = eventType,
            Severity = severity,
            Details = details,
            IpAddress = ipAddress,
            TimestampUtc = DateTime.UtcNow
        };
    }
}
