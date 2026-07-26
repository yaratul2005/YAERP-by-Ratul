using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;

namespace YAERP.Domain.Sales;

public class Customer : AggregateRoot<CustomerId>
{
    private Customer(CustomerId id, TenantId tenantId, string name, string? taxId, string? email, string? phone, string? shippingAddress, decimal creditLimit, bool isActive) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        TaxId = taxId;
        Email = email;
        Phone = phone;
        ShippingAddress = shippingAddress;
        CreditLimit = creditLimit;
        IsActive = isActive;
    }

    private Customer() { }

    public TenantId TenantId { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? TaxId { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? ShippingAddress { get; private set; }
    public decimal CreditLimit { get; private set; }
    public bool IsActive { get; private set; }

    public static Customer Create(TenantId tenantId, string name, string? taxId, string? email, string? phone, string? shippingAddress, decimal creditLimit)
    {
        return new Customer(new CustomerId(Guid.NewGuid()), tenantId, name, taxId, email, phone, shippingAddress, creditLimit, true);
    }
}
