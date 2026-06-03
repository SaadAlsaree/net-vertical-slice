using VsaTemplate.Data;
using VsaTemplate.Data.Entities;
using VsaTemplate.Infrastructure.Common;
using MapsterMapper;

namespace VsaTemplate.Features.SampleFeature.CreateSample;

public sealed class CreateSampleHandler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<Guid>> Handle(
        CreateSampleCommand request,
        CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Sample>(request);

        _db.Samples.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}
