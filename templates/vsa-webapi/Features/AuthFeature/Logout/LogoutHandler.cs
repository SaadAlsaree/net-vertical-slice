#if AUTH_IDENTITY
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Data;
using VsaTemplate.Infrastructure.Common;

namespace VsaTemplate.Features.AuthFeature.Logout;

public sealed class LogoutHandler(AppDbContext _db)
{
    public async Task<Response<object>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            // Idempotent: return success even if the token is already invalid or unknown.
            return Response<object>.Success("Logged out");
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Response<object>.Success("Logged out");
    }
}
#endif
