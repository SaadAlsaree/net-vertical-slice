using __RootNamespace__.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace __RootNamespace__.Features.__Entity__Feature.Update__Entity__;

public static class Update__Entity__Endpoint
{
    [WolverinePut("/api/__resource__/{id:guid}")]
    [Tags("__Entity__")]
    [EndpointSummary("Update __Entity__")]
    [EndpointDescription("Update an existing __Entity__ by its unique identifier")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Update(
        Guid id,
        Update__Entity__Command command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var cmd = command with { Id = id };
        var result = await bus.InvokeAsync<Response<Guid>>(cmd, cancellationToken);

        return result.Succeeded
            ? Results.Ok(result)
            : Results.NotFound(result);
    }
}
