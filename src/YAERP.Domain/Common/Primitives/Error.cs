namespace YAERP.Domain.Common.Primitives;

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict
}

public record Error(string Code, string Description, ErrorType ErrorType)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
}
