using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Warehouse.Commands.ReceiveInventoryShipment;

public record ReceiveInventoryShipmentCommand(
    string SupplierOrderRef,
    Guid TargetWarehouseId,
    string ProductName,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalVolume,
    decimal TotalWeight,
    bool RequiresColdStorage,
    string AbcClassification) : ICommand<Guid>;
