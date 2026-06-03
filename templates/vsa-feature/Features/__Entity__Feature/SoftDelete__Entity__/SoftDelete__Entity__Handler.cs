using __RootNamespace__.Data;
using __RootNamespace__.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace __RootNamespace__.Features.__Entity__Feature.SoftDelete__Entity__;

public sealed class SoftDelete__Entity__Handler(__DbContext__ _db)
{
    public async Task<Response<Guid>> Handle(
        SoftDelete__Entity__Command request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.__Entity__s
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);

        if (entity is null)
            return Response<Guid>.Failure(
                Error.NotFound("__Entity__.NotFound", "__Entity__ not found"));

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}
