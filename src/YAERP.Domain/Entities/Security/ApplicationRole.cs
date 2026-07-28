using System;
using System.Collections.Generic;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Entities.Security;

public record RoleId(Guid Value);

public class ApplicationRole : Entity<RoleId>
{
    private ApplicationRole() { }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public string? ModifiedBy { get; set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    public static ApplicationRole Create(Guid tenantId, string name, string description)
    {
        return new ApplicationRole
        {
            Id = new RoleId(Guid.NewGuid()),
            TenantId = tenantId,
            Name = name,
            Description = description
        };
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
