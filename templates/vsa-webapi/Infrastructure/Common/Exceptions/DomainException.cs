namespace VsaTemplate.Infrastructure.Common.Exceptions;

public class DomainException : Exception
{
    public ErrorType ErrorType { get; }
    public string ErrorCode { get; }

    public DomainException(string message, ErrorType errorType = ErrorType.Problem, string errorCode = "Domain.Error")
        : base(message)
    {
        ErrorType = errorType;
        ErrorCode = errorCode;
    }
}
