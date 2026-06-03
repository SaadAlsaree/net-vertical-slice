using VsaTemplate.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
#if CACHE_OUTPUT
using Microsoft.AspNetCore.OutputCaching;
#endif
using Wolverine;
using Wolverine.Http;

namespace VsaTemplate.Features.SampleFeature.GetByIdSample;

public static class GetByIdSampleEndpoint
{
#if CACHE_OUTPUT
    [OutputCache(PolicyName = "ApiGet")]
#endif
    [WolverineGet("/api/samples/{id:guid}")]
    [Tags("Samples")]
    [EndpointSummary("Get Sample by ID")]
    [EndpointDescription("Get a single sample by its unique identifier")]
    [ProducesResponseType(typeof(Response<GetByIdSampleViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<GetByIdSampleViewModel>), StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetById(
        Guid id,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<GetByIdSampleViewModel>>(
            new GetByIdSampleQuery(id), cancellationToken);

        return result.Succeeded
            ? Results.Ok(result)
            : Results.NotFound(result);
    }
}
