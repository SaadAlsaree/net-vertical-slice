using __RootNamespace__.Data;
using __RootNamespace__.Data.Entities.__Category__;
using __RootNamespace__.Infrastructure.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace __RootNamespace__.Features.__Entity__Feature.Update__Entity__;

public sealed class Update__Entity__Handler(__DbContext__ _db, IMapper _mapper)
{
    public async Task<Response<Guid>> Handle(
        Update__Entity__Command request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.Set<__Entity__>()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity is null)
            return Response<Guid>.Failure(
                Error.NotFound("__Entity__.NotFound", "__Entity__ not found"));

        _mapper.Map(request, entity);
        entity.LastUpdateAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}
