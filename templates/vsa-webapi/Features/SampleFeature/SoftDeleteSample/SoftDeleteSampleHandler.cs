using VsaTemplate.Data;
using VsaTemplate.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.Features.SampleFeature.SoftDeleteSample;

public sealed class SoftDeleteSampleHandler(AppDbContext _db)
{
    public async Task<Response<Guid>> Handle(
        SoftDeleteSampleCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.Samples
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity is null)
            return Response<Guid>.Failure(
                Error.NotFound("Sample.NotFound", "Sample not found"));

        // SaveChangesAsync intercepts EntityState.Deleted and converts it to a soft-delete
        _db.Samples.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}
