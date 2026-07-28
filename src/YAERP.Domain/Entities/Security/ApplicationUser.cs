using System;
using System.Collections.Generic;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Entities.Security;

public record UserId(Guid Value);

public class ApplicationUser : Entity<UserId>
{
    private ApplicationUser() { }

    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public bool IsMfaEnabled { get; private set; }
    public bool IsActive { get; private set; }
    public Guid TenantId { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public string? ModifiedBy { get; set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    public static ApplicationUser Create(Guid tenantId, string username, string fullName, string email, string passwordHash, string passwordSalt, string department, bool isMfaEnabled)
    {
        return new ApplicationUser
        {
            Id = new UserId(Guid.NewGuid()),
            TenantId = tenantId,
            Username = username,
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Department = department,
            IsMfaEnabled = isMfaEnabled,
            IsActive = true
        };
    }

    public void UpdateProfile(string fullName, string email, string department, bool isMfaEnabled)
    {
        FullName = fullName;
        Email = email;
        Department = department;
        IsMfaEnabled = isMfaEnabled;
    }

    public void UpdatePassword(string passwordHash, string passwordSalt)
    {
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    public void ToggleStatus(bool isActive)
    {
        IsActive = isActive;
    }
}
