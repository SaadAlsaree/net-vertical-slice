using __RootNamespace__.Data;
using __RootNamespace__.Data.Entities.__Category__;
using __RootNamespace__.Infrastructure.Common;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace __RootNamespace__.Features.__Entity__Feature.GetById__Entity__;

public sealed class GetById__Entity__Handler(__DbContext__ _db, IMapper _mapper)
{
    public async Task<Response<GetById__Entity__ViewModel>> Handle(
        GetById__Entity__Query request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.Set<__Entity__>()
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .ProjectToType<GetById__Entity__ViewModel>(_mapper.Config)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? Response<GetById__Entity__ViewModel>.Failure(
                Error.NotFound("__Entity__.NotFound", "__Entity__ not found"))
            : Response<GetById__Entity__ViewModel>.Success(entity);
    }
}
