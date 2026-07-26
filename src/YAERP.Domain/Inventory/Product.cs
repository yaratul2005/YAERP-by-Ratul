using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory.Events;

namespace YAERP.Domain.Inventory;

public class Product : AggregateRoot<ProductId>
{
    private Product(
        ProductId id,
        TenantId tenantId,
        string sku,
        string? barcode,
        string name,
        string? description,
        ProductCategoryId? productCategoryId,
        UnitOfMeasureId unitOfMeasureId,
        decimal standardCost,
        decimal listPrice,
        bool isBatchTracked,
        bool isActive) : base(id)
    {
        TenantId = tenantId;
        SKU = sku;
        Barcode = barcode;
        Name = name;
        Description = description;
        ProductCategoryId = productCategoryId;
        UnitOfMeasureId = unitOfMeasureId;
        StandardCost = standardCost;
        ListPrice = listPrice;
        IsBatchTracked = isBatchTracked;
        IsActive = isActive;
    }

    private Product() { } // EF Core

    public TenantId TenantId { get; private set; } = default!;
    public string SKU { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProductCategoryId? ProductCategoryId { get; private set; }
    public UnitOfMeasureId UnitOfMeasureId { get; private set; } = default!;
    public decimal StandardCost { get; private set; }
    public decimal ListPrice { get; private set; }
    public bool IsBatchTracked { get; private set; }
    public bool IsActive { get; private set; }

    public static Product Create(
        TenantId tenantId,
        string sku,
        string? barcode,
        string name,
        string? description,
        ProductCategoryId? productCategoryId,
        UnitOfMeasureId unitOfMeasureId,
        decimal standardCost,
        decimal listPrice,
        bool isBatchTracked)
    {
        return new Product(
            new ProductId(Guid.NewGuid()),
            tenantId,
            sku,
            barcode,
            name,
            description,
            productCategoryId,
            unitOfMeasureId,
            standardCost,
            listPrice,
            isBatchTracked,
            true);
    }

    public void UpdatePrice(decimal newCost, decimal newListPrice)
    {
        if (newCost < 0) throw new ArgumentException("Cost cannot be negative.");
        if (newListPrice < 0) throw new ArgumentException("Price cannot be negative.");

        StandardCost = newCost;
        ListPrice = newListPrice;

        AddDomainEvent(new ProductPriceUpdatedEvent(Id, newCost, newListPrice));
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
