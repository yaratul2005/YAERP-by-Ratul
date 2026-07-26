using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Identity;

public class User : AggregateRoot<UserId>
{
    private readonly List<Role> _roles = new();

    private User(UserId id, TenantId tenantId, string email, string passwordHash, string firstName, string lastName, bool isActive)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsActive = isActive;
    }

    private User() { } // EF Core

    public TenantId TenantId { get; private set; } = default!;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    public static User Create(TenantId tenantId, string email, string passwordHash, string firstName, string lastName)
    {
        return new User(new UserId(Guid.NewGuid()), tenantId, email, passwordHash, firstName, lastName, true);
    }

    public void AddRole(Role role)
    {
        if (!_roles.Contains(role))
        {
            _roles.Add(role);
        }
    }
}
