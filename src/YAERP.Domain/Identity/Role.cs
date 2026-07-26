using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Identity;

public class Role : Entity<RoleId>
{
    private Role(RoleId id, TenantId tenantId, string name, List<string> permissions) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Permissions = permissions;
    }

    private Role() { }

    public TenantId TenantId { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public List<string> Permissions { get; private set; } = new();

    public static Role Create(TenantId tenantId, string name, List<string> permissions)
    {
        return new Role(new RoleId(Guid.NewGuid()), tenantId, name, permissions);
    }
}
