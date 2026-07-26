using System.Collections.Generic;
using System.Threading.Tasks;
using YAERP.Application.Inventory.DTOs;

namespace YAERP.Application.Common.Interfaces.Reporting;

public interface IExcelExporter
{
    Task<byte[]> ExportInventoryStockAsync(IEnumerable<StockItemDto> stockItems);
}
