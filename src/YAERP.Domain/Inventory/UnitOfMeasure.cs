using System;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Inventory;

public class UnitOfMeasure : Entity<UnitOfMeasureId>
{
    private UnitOfMeasure(UnitOfMeasureId id, string code, string name, bool isDecimalAllowed) : base(id)
    {
        Code = code;
        Name = name;
        IsDecimalAllowed = isDecimalAllowed;
    }

    private UnitOfMeasure() { } // EF Core

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsDecimalAllowed { get; private set; }

    public static UnitOfMeasure Create(string code, string name, bool isDecimalAllowed)
    {
        return new UnitOfMeasure(new UnitOfMeasureId(Guid.NewGuid()), code, name, isDecimalAllowed);
    }
}
