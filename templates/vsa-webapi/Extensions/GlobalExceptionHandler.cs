using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using FluentValidation;
using VsaTemplate.Infrastructure.Common;
using VsaTemplate.Infrastructure.Common.Exceptions;

namespace VsaTemplate.Extensions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();
        httpContext.Response.Headers["X-Correlation-ID"] = correlationId;

        Response<object> response;
        HttpStatusCode statusCode;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                response = Response<object>.Failure(
                    validationException.Errors
                        .Select(e => new ValidationErrorDetail
                        {
                            Property = e.PropertyName,
                            Message = e.ErrorMessage
                        }).ToList());
                _logger.LogInformation(exception, "Validation failed: {Message}", exception.Message);
                break;

            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                response = Response<object>.Failure(
                    Error.NotFound("Resource.NotFound", exception.Message));
                _logger.LogError(exception, "Resource not found: {Message}", exception.Message);
                break;

            case ForbiddenException:
                statusCode = HttpStatusCode.Forbidden;
                response = Response<object>.Failure(
                    Error.Forbidden("Access.Forbidden", exception.Message));
                _logger.LogError(exception, "Forbidden: {Message}", exception.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                response = Response<object>.Failure(
                    Error.Failure("Auth.Unauthorized", "Unauthorized access"));
                _logger.LogError(exception, "Unauthorized: {Message}", exception.Message);
                break;

            case DomainException domainEx:
                statusCode = MapDomainError(domainEx.ErrorType);
                response = Response<object>.Failure(
                    new Error(domainEx.ErrorType, domainEx.ErrorCode, domainEx.Message));
                _logger.LogError(exception, "Domain error: {Message}", exception.Message);
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                response = Response<object>.Failure(
                    Error.Failure("Error.Internal", "An unexpected error occurred"));
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        httpContext.Response.StatusCode = (int)statusCode;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static HttpStatusCode MapDomainError(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => HttpStatusCode.NotFound,
        ErrorType.Conflict => HttpStatusCode.Conflict,
        ErrorType.Forbidden => HttpStatusCode.Forbidden,
        ErrorType.Validation => HttpStatusCode.BadRequest,
        _ => HttpStatusCode.BadRequest
    };
}
