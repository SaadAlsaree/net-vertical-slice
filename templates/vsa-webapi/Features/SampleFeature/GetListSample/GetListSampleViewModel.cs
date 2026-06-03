namespace VsaTemplate.Features.SampleFeature.GetListSample;

public record GetListSampleViewModel(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt
);
