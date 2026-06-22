using VsaTemplate.Data;
using VsaTemplate.Extensions;
using VsaTemplate.Infrastructure.Common;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.Features.SampleFeature.GetListSample;

public sealed class GetListSampleHandler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<PagedList<GetListSampleViewModel>>> Handle(
        GetListSampleQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.Samples.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term) ||
                (e.Description != null && e.Description.ToLower().Contains(term)));
        }

        query = query.ApplySort(request.SortBy, request.OrderBy, defaultSortBy: "Name");

        var projectedQuery = query
            .ProjectToType<GetListSampleViewModel>(_mapper.Config);

        var pagedList = await PagedList<GetListSampleViewModel>.CreateAsync(
            projectedQuery,
            request.Page ?? 1,
            request.PageSize ?? 10,
            cancellationToken);

        return Response<PagedList<GetListSampleViewModel>>.Success(pagedList);
    }
}
