using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;

namespace YAERP.Domain.Inventory;

public class Warehouse : AggregateRoot<WarehouseId>
{
    private Warehouse(WarehouseId id, TenantId tenantId, string code, string name, string? address, bool isActive) : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Name = name;
        Address = address;
        IsActive = isActive;
    }

    private Warehouse() { }

    public TenantId TenantId { get; private set; } = default!;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }

    public static Warehouse Create(TenantId tenantId, string code, string name, string? address)
    {
        return new Warehouse(new WarehouseId(Guid.NewGuid()), tenantId, code, name, address, true);
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
