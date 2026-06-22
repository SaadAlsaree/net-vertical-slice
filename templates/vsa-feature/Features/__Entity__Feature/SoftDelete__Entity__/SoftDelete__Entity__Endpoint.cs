using __RootNamespace__.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace __RootNamespace__.Features.__Entity__Feature.SoftDelete__Entity__;

public static class SoftDelete__Entity__Endpoint
{
    [WolverineDelete("/api/__resource__/{id:guid}")]
    [Tags("__Entity__")]
    [EndpointSummary("Delete __Entity__")]
    [EndpointDescription("Soft delete an __Entity__ by its unique identifier")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status404NotFound)]
    public static async Task<IResult> Delete(
        Guid id,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<Guid>>(
            new SoftDelete__Entity__Command(id), cancellationToken);

        return result.Succeeded
            ? Results.Ok(result)
            : Results.NotFound(result);
    }
}
