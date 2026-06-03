using System.Net;
using System.Text.Json;
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
        var (statusCode, error) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                Error.Validation("Validation.Error", "Validation failed")),

            NotFoundException => (
                HttpStatusCode.NotFound,
                Error.NotFound("Resource.NotFound", exception.Message)),

            ForbiddenException => (
                HttpStatusCode.Forbidden,
                Error.Forbidden("Access.Forbidden", exception.Message)),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                Error.Failure("Auth.Unauthorized", "Unauthorized access")),

            DomainException domainEx => (
                MapDomainError(domainEx.ErrorType),
                new Error(domainEx.ErrorType, domainEx.ErrorCode, domainEx.Message)),

            _ => (
                HttpStatusCode.InternalServerError,
                Error.Failure("Error.Internal", "An unexpected error occurred"))
        };

        var logLevel = exception is ValidationException
            ? LogLevel.Information
            : LogLevel.Error;

        _logger.Log(logLevel, exception, "Exception handled: {Message}", exception.Message);

        var response = Response<object>.Failure(error);

        if (exception is ValidationException validationException)
        {
            response.Errors = validationException.Errors
                .Select(e => new ValidationErrorDetail
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList();
        }

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            cancellationToken);

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
