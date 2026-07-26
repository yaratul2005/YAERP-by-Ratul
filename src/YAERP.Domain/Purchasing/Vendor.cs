using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;

namespace YAERP.Domain.Purchasing;

public class Vendor : AggregateRoot<VendorId>
{
    private Vendor(VendorId id, TenantId tenantId, string name, string? taxId, string? email, string? phone, string? address, bool isActive) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        TaxId = taxId;
        Email = email;
        Phone = phone;
        Address = address;
        IsActive = isActive;
    }

    private Vendor() { }

    public TenantId TenantId { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? TaxId { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }

    public static Vendor Create(TenantId tenantId, string name, string? taxId, string? email, string? phone, string? address)
    {
        return new Vendor(new VendorId(Guid.NewGuid()), tenantId, name, taxId, email, phone, address, true);
    }
}
