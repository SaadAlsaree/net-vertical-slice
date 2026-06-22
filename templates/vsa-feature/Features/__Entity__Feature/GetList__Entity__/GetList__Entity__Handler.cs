using __RootNamespace__.Data;
using __RootNamespace__.Data.Entities.__Category__;
using __RootNamespace__.Extensions;
using __RootNamespace__.Infrastructure.Common;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace __RootNamespace__.Features.__Entity__Feature.GetList__Entity__;

public sealed class GetList__Entity__Handler(__DbContext__ _db, IMapper _mapper)
{
    public async Task<Response<PagedList<GetList__Entity__ViewModel>>> Handle(
        GetList__Entity__Query request,
        CancellationToken cancellationToken)
    {
        var query = _db.Set<__Entity__>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term) ||
                (e.Description != null && e.Description.ToLower().Contains(term)));
        }

        query = query.ApplySort(request.SortBy, request.OrderBy, defaultSortBy: "Name");

        var projectedQuery = query
            .ProjectToType<GetList__Entity__ViewModel>(_mapper.Config);

        var pagedList = await PagedList<GetList__Entity__ViewModel>.CreateAsync(
            projectedQuery,
            request.Page ?? 1,
            request.PageSize ?? 10,
            cancellationToken);

        return Response<PagedList<GetList__Entity__ViewModel>>.Success(pagedList);
    }
}
