#if AUTH_IDENTITY
using VsaTemplate.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace VsaTemplate.Features.AuthFeature.Login;

public static class LoginEndpoint
{
    [WolverinePost("/api/auth/login")]
    [Tags("Auth")]
    [EndpointSummary("Login")]
    [EndpointDescription("Authenticate a user and return access and refresh tokens")]
    [ProducesResponseType(typeof(Response<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response<AuthResponse>), StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> Login(
        LoginCommand command,
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
