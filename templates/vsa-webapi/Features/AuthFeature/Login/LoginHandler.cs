#if AUTH_IDENTITY
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VsaTemplate.Data;
using VsaTemplate.Data.Entities;
using VsaTemplate.Infrastructure.Authentication;
using VsaTemplate.Infrastructure.Common;
using VsaTemplate.Models;

namespace VsaTemplate.Features.AuthFeature.Login;

public sealed class LoginHandler(
    UserManager<ApplicationUser> _userManager,
    ITokenService _tokenService,
    AppDbContext _db,
    IOptions<JwtSettings> _jwtSettings)
{
    public async Task<Response<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Response<AuthResponse>.Failure(
                Error.Failure("Auth.InvalidCredentials", "Invalid email or password"));
        }

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
