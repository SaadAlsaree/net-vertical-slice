namespace VsaTemplate.Infrastructure.Common;

public class Response<T>
{
    public bool Succeeded { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public List<ValidationErrorDetail> Errors { get; init; } = new();

    public static Response<T> Success(T data, string message = "Operation succeeded")
    {
        return new Response<T>
        {
            Succeeded = true,
            Data = data,
            Message = message
        };
    }

    public static Response<T> Success(string message = "Operation succeeded")
    {
        return new Response<T>
        {
            Succeeded = true,
            Message = message
        };
    }

    public static Response<T> Failure(Error error)
    {
        return new Response<T>
        {
            Succeeded = false,
            Code = error.Code,
            Message = error.Description
        };
    }

    public static Response<T> Failure(List<ValidationErrorDetail> errors)
    {
        return new Response<T>
        {
            Succeeded = false,
            Code = "Validation.Error",
            Message = "Validation failed",
            Errors = errors
        };
    }
}
