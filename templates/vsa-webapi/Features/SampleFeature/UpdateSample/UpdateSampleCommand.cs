namespace VsaTemplate.Features.SampleFeature.UpdateSample;

public record UpdateSampleCommand(
    Guid Id,
    string Name,
    string? Description
);
