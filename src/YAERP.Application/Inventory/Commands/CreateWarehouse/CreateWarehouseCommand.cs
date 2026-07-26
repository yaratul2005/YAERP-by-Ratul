using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Inventory.Commands.CreateWarehouse;

public record CreateWarehouseCommand(
    string Code,
    string Name,
    string? Address) : ICommand<Guid>;
