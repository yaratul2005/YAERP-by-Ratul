using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Inventory.Commands.ExportInventoryExcel;

public record ExportInventoryExcelCommand() : ICommand<byte[]>;
