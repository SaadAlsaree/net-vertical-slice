using __RootNamespace__.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace __RootNamespace__.Features.__Entity__Feature.GetList__Entity__;

public static class GetList__Entity__Endpoint
{
    [WolverineGet("/api/__resource__")]
    [Tags("__Entity__")]
    [EndpointSummary("List __Entity__s")]
    [EndpointDescription("Get a paginated list of __Entity__s with filtering and sorting")]
    [ProducesResponseType(typeof(Response<PagedList<GetList__Entity__ViewModel>>), StatusCodes.Status200OK)]
    public static async Task<IResult> List(
        IMessageBus bus,
        CancellationToken cancellationToken,
        [FromQuery] int? page = 1,
        [FromQuery] int? pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] string? orderBy = "asc")
    {
        var query = new GetList__Entity__Query(page, pageSize, searchTerm, sortBy, orderBy);
        var result = await bus.InvokeAsync<Response<PagedList<GetList__Entity__ViewModel>>>(
            query, cancellationToken);

        return Results.Ok(result);
    }
}
