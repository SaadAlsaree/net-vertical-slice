using __RootNamespace__.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace __RootNamespace__.Features.__Entity__Feature.Create__Entity__;

public static class Create__Entity__Endpoint
{
    [WolverinePost("/api/__resource__")]
    [Tags("__Entity__")]
    [EndpointSummary("Create __Entity__")]
    [EndpointDescription("Create a new __Entity__ in the system")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Create(
        Create__Entity__Command command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<Guid>>(command, cancellationToken);

        return result.Succeeded
            ? Results.Created($"/api/__resource__/{result.Data}", result)
            : Results.BadRequest(result);
    }
}
