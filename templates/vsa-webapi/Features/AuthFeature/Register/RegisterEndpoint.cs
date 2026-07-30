#if AUTH_IDENTITY
using VsaTemplate.Infrastructure.Common;
using VsaTemplate.Infrastructure.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace VsaTemplate.Features.AuthFeature.Register;

public static class RegisterEndpoint
{
    [WolverinePost("/api/auth/register")]
    [EnableRateLimiting(RateLimitPolicyNames.AuthRegistration)]
    [Tags("Auth")]
    [EndpointSummary("Register")]
    [EndpointDescription("Register a new user and return access and refresh tokens")]
    [ProducesResponseType(typeof(Response<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response<AuthResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status429TooManyRequests)]
    public static async Task<IResult> Register(
        RegisterCommand command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<AuthResponse>>(command, cancellationToken);

        if (!result.Succeeded)
        {
            return result.Code == "Auth.UserExists"
                ? Results.Conflict(result)
                : Results.BadRequest(result);
        }

        return Results.Created("/api/auth/login", result);
    }
}
#endif
