#if AUTH_IDENTITY
using VsaTemplate.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace VsaTemplate.Features.AuthFeature.Logout;

public static class LogoutEndpoint
{
    [WolverinePost("/api/auth/logout")]
    [Tags("Auth")]
    [EndpointSummary("Logout")]
    [EndpointDescription("Revoke a refresh token, invalidating the session")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    public static async Task<IResult> Logout(
        LogoutCommand command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<object>>(command, cancellationToken);

        return Results.Ok(result);
    }
}
#endif
