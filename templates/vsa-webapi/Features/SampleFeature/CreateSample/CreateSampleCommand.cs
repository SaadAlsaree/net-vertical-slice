namespace VsaTemplate.Features.SampleFeature.CreateSample;

public record CreateSampleCommand(
    string Name,
    string? Description
);
