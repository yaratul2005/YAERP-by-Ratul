using System;
using System.Collections.Generic;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Entities.Security;

public record PermissionId(Guid Value);

public class Permission : Entity<PermissionId>
{
    private Permission() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string ModuleGroup { get; private set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    public static Permission Create(string code, string name, string moduleGroup)
    {
        return new Permission
        {
            Id = new PermissionId(Guid.NewGuid()),
            Code = code,
            Name = name,
            ModuleGroup = moduleGroup
        };
    }
}
