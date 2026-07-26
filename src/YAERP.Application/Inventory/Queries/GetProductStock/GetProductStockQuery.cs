using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Inventory.Queries.GetProductStock;

public record GetProductStockQuery(Guid ProductId, Guid? WarehouseId) : IQuery<ProductStockResult>;

public record ProductStockResult(Guid ProductId, decimal TotalQuantityOnHand);
