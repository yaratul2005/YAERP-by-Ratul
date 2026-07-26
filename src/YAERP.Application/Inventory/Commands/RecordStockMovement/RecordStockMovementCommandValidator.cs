using FluentValidation;

namespace YAERP.Application.Inventory.Commands.RecordStockMovement;

public class RecordStockMovementCommandValidator : AbstractValidator<RecordStockMovementCommand>
{
    public RecordStockMovementCommandValidator()
    {
        RuleFor(v => v.ProductId).NotEmpty();
        RuleFor(v => v.WarehouseId).NotEmpty();
        RuleFor(v => v.Quantity).GreaterThan(0); // Assuming we always record positive magnitude and type determines direction
        RuleFor(v => v.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(v => v.MovementType).IsInEnum();
    }
}
