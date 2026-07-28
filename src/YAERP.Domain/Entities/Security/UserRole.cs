using System;

namespace YAERP.Domain.Entities.Security;

public class UserRole
{
    public UserId UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public RoleId RoleId { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;
}

public class RolePermission
{
    public RoleId RoleId { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;

    public PermissionId PermissionId { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
