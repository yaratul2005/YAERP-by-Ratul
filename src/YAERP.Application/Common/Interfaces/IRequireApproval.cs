namespace YAERP.Application.Common.Interfaces;

public interface IRequireApproval
{
    string EntityType { get; }
    string EntityId { get; }
    decimal TotalAmount { get; }
    string UserId { get; }
}
