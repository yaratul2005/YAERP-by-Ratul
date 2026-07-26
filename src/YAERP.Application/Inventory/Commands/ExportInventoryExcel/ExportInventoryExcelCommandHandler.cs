using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Interfaces.Reporting;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Application.Inventory.DTOs;

namespace YAERP.Application.Inventory.Commands.ExportInventoryExcel;

public class ExportInventoryExcelCommandHandler : ICommandHandler<ExportInventoryExcelCommand, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelExporter _excelExporter;

    public ExportInventoryExcelCommandHandler(IApplicationDbContext context, IExcelExporter excelExporter)
    {
        _context = context;
        _excelExporter = excelExporter;
    }

    public async Task<Result<byte[]>> Handle(ExportInventoryExcelCommand request, CancellationToken cancellationToken)
    {
        var products = await _context.Products.AsNoTracking().ToListAsync(cancellationToken);

        // This is a naive implementation since real quantity on hand is a sum of stock movements.
        var stockItems = products.Select(p => new StockItemDto(p.SKU, p.Name, 0m /* calculate real */, p.StandardCost)).ToList();

        var excelBytes = await _excelExporter.ExportInventoryStockAsync(stockItems);

        return Result.Success(excelBytes);
    }
}
