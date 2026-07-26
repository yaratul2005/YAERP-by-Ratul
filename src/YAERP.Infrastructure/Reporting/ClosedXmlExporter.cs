using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using YAERP.Application.Common.Interfaces.Reporting;
using YAERP.Application.Inventory.DTOs;

namespace YAERP.Infrastructure.Reporting;

public class ClosedXmlExporter : IExcelExporter
{
    public Task<byte[]> ExportInventoryStockAsync(IEnumerable<StockItemDto> stockItems)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Inventory Stock");

        worksheet.Cell(1, 1).Value = "SKU";
        worksheet.Cell(1, 2).Value = "Product Name";
        worksheet.Cell(1, 3).Value = "Quantity On Hand";
        worksheet.Cell(1, 4).Value = "Unit Cost";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        var currentRow = 2;
        foreach (var item in stockItems)
        {
            worksheet.Cell(currentRow, 1).Value = item.SKU;
            worksheet.Cell(currentRow, 2).Value = item.ProductName;
            worksheet.Cell(currentRow, 3).Value = item.QuantityOnHand;
            worksheet.Cell(currentRow, 4).Value = item.UnitCost;
            currentRow++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
