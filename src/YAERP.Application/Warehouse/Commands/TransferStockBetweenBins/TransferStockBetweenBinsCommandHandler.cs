using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Entities.Warehouse;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;

namespace YAERP.Application.Warehouse.Commands.TransferStockBetweenBins;

public class TransferStockBetweenBinsCommandHandler : ICommandHandler<TransferStockBetweenBinsCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;

    public TransferStockBetweenBinsCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(TransferStockBetweenBinsCommand request, CancellationToken cancellationToken)
    {
        if (request.SourceBinId == request.DestinationBinId)
        {
            return Result.Failure<bool>(new Error("Transfer.InvalidBins", "Source and Destination bins cannot be identical.", ErrorType.Validation));
        }

        var sourceBin = await _dbContext.Set<WarehouseBin>()
            .FirstOrDefaultAsync(b => b.Id == request.SourceBinId, cancellationToken);

        if (sourceBin == null)
        {
            return Result.Failure<bool>(new Error("Transfer.SourceBinNotFound", "Source bin not found.", ErrorType.NotFound));
        }

        if (sourceBin.IsLocked)
        {
            return Result.Failure<bool>(new Error("Transfer.SourceBinLocked", $"Source bin '{sourceBin.BinCode}' is locked.", ErrorType.Conflict));
        }

        var destBin = await _dbContext.Set<WarehouseBin>()
            .FirstOrDefaultAsync(b => b.Id == request.DestinationBinId, cancellationToken);

        if (destBin == null)
        {
            return Result.Failure<bool>(new Error("Transfer.DestBinNotFound", "Destination bin not found.", ErrorType.NotFound));
        }

        if (destBin.IsLocked)
        {
            return Result.Failure<bool>(new Error("Transfer.DestBinLocked", $"Destination bin '{destBin.BinCode}' is locked.", ErrorType.Conflict));
        }

        // Record stock movement transfer log via domain factory method
        var movement = StockMovement.Create(
            new TenantId(Guid.NewGuid()),
            new ProductId(request.ProductId),
            new WarehouseId(destBin.ZoneId),
            StockMovementType.TransferIn,
            request.Quantity,
            0m,
            $"TRF-{sourceBin.BinCode}->{destBin.BinCode} | {request.Notes}",
            null);

        _dbContext.StockMovements.Add(movement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
