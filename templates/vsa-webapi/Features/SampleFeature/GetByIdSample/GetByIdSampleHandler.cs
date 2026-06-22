using VsaTemplate.Data;
using VsaTemplate.Infrastructure.Common;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.Features.SampleFeature.GetByIdSample;

public sealed class GetByIdSampleHandler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<GetByIdSampleViewModel>> Handle(
        GetByIdSampleQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.Samples
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .ProjectToType<GetByIdSampleViewModel>(_mapper.Config)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? Response<GetByIdSampleViewModel>.Failure(
                Error.NotFound("Sample.NotFound", "Sample not found"))
            : Response<GetByIdSampleViewModel>.Success(entity);
    }
}
