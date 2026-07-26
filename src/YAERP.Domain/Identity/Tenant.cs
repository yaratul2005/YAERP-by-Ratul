using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Identity;

public class Tenant : AggregateRoot<TenantId>
{
    private Tenant(TenantId id, string name, string? connectionString, bool isActive, DateTime createdUtc)
        : base(id)
    {
        Name = name;
        ConnectionString = connectionString;
        IsActive = isActive;
        CreatedUtc = createdUtc;
    }

    private Tenant() { } // For EF Core

    public string Name { get; private set; } = string.Empty;
    public string? ConnectionString { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    public static Tenant Create(string name, string? connectionString = null)
    {
        return new Tenant(new TenantId(Guid.NewGuid()), name, connectionString, true, DateTime.UtcNow);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
