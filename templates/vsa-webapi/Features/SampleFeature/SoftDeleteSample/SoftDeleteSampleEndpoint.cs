using VsaTemplate.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace VsaTemplate.Features.SampleFeature.SoftDeleteSample;

public static class SoftDeleteSampleEndpoint
{
    [WolverineDelete("/api/samples/{id:guid}")]
    [Tags("Samples")]
    [EndpointSummary("Delete Sample")]
    [EndpointDescription("Soft delete a sample by its unique identifier")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status404NotFound)]
    public static async Task<IResult> Delete(
        Guid id,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<Guid>>(
            new SoftDeleteSampleCommand(id), cancellationToken);

        return result.Succeeded
            ? Results.Ok(result)
            : Results.NotFound(result);
    }
}
