using System;

namespace YAERP.Application.Inventory.DTOs;

public record StockItemDto(string SKU, string ProductName, decimal QuantityOnHand, decimal UnitCost);
