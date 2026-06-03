namespace VsaTemplate.Features.SampleFeature.GetListSample;

public record GetListSampleQuery(
    int? Page = 1,
    int? PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = "Name",
    string? OrderBy = "asc"
);
