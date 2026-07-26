using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Application.Common.Security;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Sales;

namespace YAERP.Application.Sales.Commands.IngestExternalSalesOrder;

public sealed class IngestExternalSalesOrderCommandHandler
    : ICommandHandler<IngestExternalSalesOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IHmacValidator _hmacValidator;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<IngestExternalSalesOrderCommandHandler> _logger;

    public IngestExternalSalesOrderCommandHandler(
        IApplicationDbContext context,
        IHmacValidator hmacValidator,
        ITenantContext tenantContext,
        ILogger<IngestExternalSalesOrderCommandHandler> logger)
    {
        _context = context;
        _hmacValidator = hmacValidator;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        IngestExternalSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.WebhookPayload;

        // 1. Verify HMAC Signature
        if (!_hmacValidator.ValidateSignature(request.RawPayloadJson, request.SignatureHeader, request.SecretKey))
        {
            _logger.LogWarning("Invalid HMAC signature received for external order {ExternalOrderId}", dto.ExternalOrderId);
            return Result.Failure<Guid>(new Error(
                "Webhook.InvalidSignature",
                "HMAC signature verification failed.",
                ErrorType.Validation));
        }

        // 2. Idempotency Check
        var existingOrder = await _context.SalesOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == dto.ExternalOrderId, cancellationToken);

        if (existingOrder != null)
        {
            _logger.LogInformation("External order {ExternalOrderId} already processed (Idempotent).", dto.ExternalOrderId);
            return Result.Success(existingOrder.Id.Value);
        }

        var tenantId = _tenantContext.CurrentTenantId ?? new TenantId(Guid.NewGuid());

        // 3. Resolve Customer & Warehouse
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);

        var customerId = customer?.Id ?? new CustomerId(Guid.NewGuid());

        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.TenantId == tenantId, cancellationToken);

        var warehouseId = warehouse?.Id ?? new WarehouseId(Guid.NewGuid());

        // 4. Create SalesOrder Entity
        var salesOrder = SalesOrder.Create(tenantId, customerId, warehouseId, dto.ExternalOrderId);

        // 5. Match Product SKUs & Add Items
        foreach (var itemDto in dto.LineItems)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == itemDto.SKU, cancellationToken);

            var productId = product?.Id ?? new ProductId(Guid.NewGuid());

            salesOrder.AddItem(productId, itemDto.Quantity, itemDto.UnitPrice);

            // Record Outbound Stock Movement
            var movement = StockMovement.Create(
                tenantId,
                productId,
                warehouseId,
                StockMovementType.OutboundShipment,
                itemDto.Quantity,
                itemDto.UnitPrice,
                $"Webhook-{dto.PlatformSource}-{dto.ExternalOrderId}",
                _tenantContext.CurrentUserId);

            _context.StockMovements.Add(movement);
        }

        salesOrder.Confirm();
        salesOrder.Fulfill();

        _context.SalesOrders.Add(salesOrder);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully ingested external {Platform} order {ExternalOrderId} as SalesOrder {SalesOrderId}",
            dto.PlatformSource, dto.ExternalOrderId, salesOrder.Id.Value);

        return Result.Success(salesOrder.Id.Value);
    }
}
