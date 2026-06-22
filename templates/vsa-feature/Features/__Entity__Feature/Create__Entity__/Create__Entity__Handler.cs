using __RootNamespace__.Data;
using __RootNamespace__.Data.Entities.__Category__;
using __RootNamespace__.Infrastructure.Common;
using MapsterMapper;

namespace __RootNamespace__.Features.__Entity__Feature.Create__Entity__;

public sealed class Create__Entity__Handler(__DbContext__ _db, IMapper _mapper)
{
    public async Task<Response<Guid>> Handle(
        Create__Entity__Command request,
        CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<__Entity__>(request);

        _db.Set<__Entity__>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}
