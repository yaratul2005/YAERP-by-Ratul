using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public interface IIdentityService
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string hash, string salt);
    Task<bool> EvaluatePermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default);
    Task LogSecurityEventAsync(Guid tenantId, Guid userId, string eventType, string severity, string details, string ipAddress, CancellationToken cancellationToken = default);
    void InvalidatePermissionCache(Guid userId);
}
