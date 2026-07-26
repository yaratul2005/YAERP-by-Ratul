using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Inventory.Commands.CreateProduct;

public record CreateProductCommand(
    string SKU,
    string? Barcode,
    string Name,
    string? Description,
    Guid? ProductCategoryId,
    Guid UnitOfMeasureId,
    decimal StandardCost,
    decimal ListPrice,
    bool IsBatchTracked) : ICommand<Guid>;
