using __RootNamespace__.Infrastructure.Common;
using Wolverine;
using Wolverine.Http;

namespace __RootNamespace__.Features.__Entity__Feature.GetById__Entity__;

public static class GetById__Entity__Endpoint
{
    [WolverineGet("/api/__resource__/{id:guid}")]
    [Tags("__Entity__")]
    [EndpointSummary("Get __Entity__ by ID")]
    [EndpointDescription("Get a single __Entity__ by its unique identifier")]
    [ProducesResponseType(typeof(Response<GetById__Entity__ViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<GetById__Entity__ViewModel>), StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetById(
        Guid id,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<GetById__Entity__ViewModel>>(
            new GetById__Entity__Query(id), cancellationToken);

        return result.Succeeded
            ? Results.Ok(result)
            : Results.NotFound(result);
    }
}
