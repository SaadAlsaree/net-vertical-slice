namespace VsaTemplate.Features.SampleFeature.GetByIdSample;

public record GetByIdSampleViewModel(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt
);
