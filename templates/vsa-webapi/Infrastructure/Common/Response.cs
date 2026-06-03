namespace VsaTemplate.Infrastructure.Common;

public class Response<T>
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public List<ValidationErrorDetail> Errors { get; set; } = new();

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

public class ValidationErrorDetail
{
    public string Property { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
