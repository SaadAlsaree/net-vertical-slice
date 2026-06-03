namespace VsaTemplate.Infrastructure.Common;

public record Error(ErrorType Type, string Code, string Description)
{
    public static Error None => new(ErrorType.None, string.Empty, string.Empty);
    public static Error NullValue => new(ErrorType.Failure, "Error.NullValue", "Null value was provided");

    public static Error Failure(string code, string description) => new(ErrorType.Failure, code, description);
    public static Error NotFound(string code, string description) => new(ErrorType.NotFound, code, description);
    public static Error Problem(string code, string description) => new(ErrorType.Problem, code, description);
    public static Error Conflict(string code, string description) => new(ErrorType.Conflict, code, description);
    public static Error Validation(string code, string description) => new(ErrorType.Validation, code, description);
    public static Error Forbidden(string code, string description) => new(ErrorType.Forbidden, code, description);
}

public enum ErrorType
{
    None = 0,
    Failure = 1,
    Validation = 2,
    Problem = 3,
    NotFound = 4,
    Conflict = 5,
    Forbidden = 6
}
