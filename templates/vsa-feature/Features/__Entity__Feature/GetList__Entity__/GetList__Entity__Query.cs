namespace __RootNamespace__.Features.__Entity__Feature.GetList__Entity__;

public record GetList__Entity__Query(
    int? Page = 1,
    int? PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = "Id", // TODO(__Entity__): change to your default sort field, e.g. "NameAr"
    string? OrderBy = "asc"
);
