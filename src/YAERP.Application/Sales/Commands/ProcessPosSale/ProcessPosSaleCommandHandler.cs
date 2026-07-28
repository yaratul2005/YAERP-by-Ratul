using System;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Sales;

namespace YAERP.Application.Sales.Commands.ProcessPosSale;

public class ProcessPosSaleCommandHandler : ICommandHandler<ProcessPosSaleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ProcessPosSaleCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(ProcessPosSaleCommand request, CancellationToken cancellationToken)
    {
        if (request.LineItems == null || request.LineItems.Count == 0)
        {
            return Result.Failure<Guid>(new Error("POS.EmptyCart", "Cart items are required.", ErrorType.Validation));
        }

        var tenantId = _tenantContext.CurrentTenantId ?? new TenantId(Guid.NewGuid());
        var customerId = new CustomerId(Guid.NewGuid());
        var warehouseId = new WarehouseId(Guid.NewGuid());

        var so = SalesOrder.Create(tenantId, customerId, warehouseId, request.ReceiptNumber);

        foreach (var item in request.LineItems)
        {
            so.AddItem(new ProductId(item.ProductId), item.Quantity, item.UnitPrice);
        }

        so.Confirm();
        so.Fulfill();

        _context.SalesOrders.Add(so);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(so.Id.Value);
    }
}
