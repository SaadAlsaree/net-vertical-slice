using VsaTemplate.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace VsaTemplate.Features.SampleFeature.UpdateSample;

public static class UpdateSampleEndpoint
{
    [WolverinePut("/api/samples/{id:guid}")]
    [Tags("Samples")]
    [EndpointSummary("Update Sample")]
    [EndpointDescription("Update an existing sample by its unique identifier")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Update(
        Guid id,
        UpdateSampleCommand command,
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
