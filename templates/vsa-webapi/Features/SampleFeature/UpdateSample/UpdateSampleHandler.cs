using VsaTemplate.Data;
using VsaTemplate.Infrastructure.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.Features.SampleFeature.UpdateSample;

public sealed class UpdateSampleHandler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<Guid>> Handle(
        UpdateSampleCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.Samples
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);

        if (entity is null)
            return Response<Guid>.Failure(
                Error.NotFound("Sample.NotFound", "Sample not found"));

        _mapper.Map(request, entity);
        entity.LastUpdateAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}
