#if AUTH_IDENTITY
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VsaTemplate.Data;
using VsaTemplate.Data.Entities;
using VsaTemplate.Extensions;
using VsaTemplate.Infrastructure.Authentication;
using VsaTemplate.Infrastructure.Common;
using VsaTemplate.Models;

namespace VsaTemplate.Features.AuthFeature.Refresh;

public sealed class RefreshHandler(
    ITokenService _tokenService,
    UserManager<ApplicationUser> _userManager,
    AppDbContext _db,
    IOptions<JwtSettings> _jwtSettings)
{
    public async Task<Response<AuthResponse>> Handle(
        RefreshCommand request,
        CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            return Response<AuthResponse>.Failure(
                Error.Failure("Auth.InvalidToken", "Invalid access token"));
        }

        var userId = principal.FindFirst(AuthClaims.UserId)?.Value;
        if (userId is null || !Guid.TryParse(userId, out var userGuid))
        {
            return Response<AuthResponse>.Failure(
                Error.Failure("Auth.InvalidToken", "Invalid access token"));
        }

        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null || storedToken.UserId != userGuid || !storedToken.IsActive)
        {
            return Response<AuthResponse>.Failure(
                Error.Failure("Auth.InvalidRefreshToken", "Invalid or expired refresh token"));
        }

        var user = await _userManager.FindByIdAsync(userGuid.ToString());
        if (user is null)
        {
            return Response<AuthResponse>.Failure(
                Error.NotFound("Auth.UserNotFound", "User not found"));
        }

        // Rotate: revoke the old refresh token and issue a new one.
        storedToken.RevokedAt = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, refreshToken) = _tokenService.GenerateTokens(user, roles);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.Value.RefreshTokenExpirationDays)
        });
        await _db.SaveChangesAsync(cancellationToken);

        return Response<AuthResponse>.Success(new AuthResponse(
            accessToken,
            refreshToken,
            _jwtSettings.Value.AccessTokenExpirationMinutes * 60));
    }
}
#endif
