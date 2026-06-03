using VsaTemplate.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
#if CACHE_OUTPUT
using Microsoft.AspNetCore.OutputCaching;
#endif
using Wolverine;
using Wolverine.Http;

namespace VsaTemplate.Features.SampleFeature.GetListSample;

public static class GetListSampleEndpoint
{
#if CACHE_OUTPUT
    [OutputCache(PolicyName = "ApiGet")]
#endif
    [WolverineGet("/api/samples")]
    [Tags("Samples")]
    [EndpointSummary("List Samples")]
    [EndpointDescription("Get a paginated list of samples with filtering and sorting")]
    [ProducesResponseType(typeof(Response<PagedList<GetListSampleViewModel>>), StatusCodes.Status200OK)]
    public static async Task<IResult> List(
        IMessageBus bus,
        CancellationToken cancellationToken,
        [FromQuery] int? page = 1,
        [FromQuery] int? pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] string? orderBy = "asc")
    {
        var query = new GetListSampleQuery(page, pageSize, searchTerm, sortBy, orderBy);
        var result = await bus.InvokeAsync<Response<PagedList<GetListSampleViewModel>>>(
            query, cancellationToken);

        return Results.Ok(result);
    }
}
