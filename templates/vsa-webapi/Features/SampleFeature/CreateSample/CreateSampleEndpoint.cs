using VsaTemplate.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace VsaTemplate.Features.SampleFeature.CreateSample;

public static class CreateSampleEndpoint
{
    [WolverinePost("/api/samples")]
    [Tags("Samples")]
    [EndpointSummary("Create Sample")]
    [EndpointDescription("Create a new sample in the system")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Create(
        CreateSampleCommand command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<Guid>>(command, cancellationToken);

        return result.Succeeded
            ? Results.Created($"/api/samples/{result.Data}", result)
            : Results.BadRequest(result);
    }
}
