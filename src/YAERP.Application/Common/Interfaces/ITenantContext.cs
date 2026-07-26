using YAERP.Domain.Identity;

namespace YAERP.Application.Common.Interfaces;

public interface ITenantContext
{
    TenantId? CurrentTenantId { get; }
    UserId? CurrentUserId { get; }
}
