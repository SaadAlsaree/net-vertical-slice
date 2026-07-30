#if AUTH_IDENTITY
using VsaTemplate.Infrastructure.Common;
using VsaTemplate.Infrastructure.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace VsaTemplate.Features.AuthFeature.Refresh;

public static class RefreshEndpoint
{
    [WolverinePost("/api/auth/refresh")]
    [EnableRateLimiting(RateLimitPolicyNames.AuthRefreshToken)]
    [Tags("Auth")]
    [EndpointSummary("Refresh Token")]
    [EndpointDescription("Exchange an expired access token and a valid refresh token for a new token pair")]
    [ProducesResponseType(typeof(Response<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status429TooManyRequests)]
    public static async Task<IResult> Refresh(
        RefreshCommand command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<AuthResponse>>(command, cancellationToken);

        return result.Succeeded
            ? Results.Ok(result)
            : Results.Unauthorized();
    }
}
#endif
