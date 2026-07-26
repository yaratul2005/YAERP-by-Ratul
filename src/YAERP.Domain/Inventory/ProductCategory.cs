using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;

namespace YAERP.Domain.Inventory;

public class ProductCategory : AggregateRoot<ProductCategoryId>
{
    private ProductCategory(ProductCategoryId id, TenantId tenantId, string name, string code, ProductCategoryId? parentCategoryId) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Code = code;
        ParentCategoryId = parentCategoryId;
    }

    private ProductCategory() { } // EF Core

    public TenantId TenantId { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public ProductCategoryId? ParentCategoryId { get; private set; }

    public static ProductCategory Create(TenantId tenantId, string name, string code, ProductCategoryId? parentCategoryId = null)
    {
        return new ProductCategory(new ProductCategoryId(Guid.NewGuid()), tenantId, name, code, parentCategoryId);
    }
}
