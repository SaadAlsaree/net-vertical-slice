#if AUTH_IDENTITY
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VsaTemplate.Data;
using VsaTemplate.Data.Entities;
using VsaTemplate.Infrastructure.Authentication;
using VsaTemplate.Infrastructure.Common;
using VsaTemplate.Models;

namespace VsaTemplate.Features.AuthFeature.Register;

public sealed class RegisterHandler(
    UserManager<ApplicationUser> _userManager,
    ITokenService _tokenService,
    AppDbContext _db,
    IOptions<JwtSettings> _jwtSettings)
{
    public async Task<Response<AuthResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Response<AuthResponse>.Failure(
                Error.Conflict("Auth.UserExists", "A user with this email already exists"));
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var error = createResult.Errors.First();
            return Response<AuthResponse>.Failure(
                Error.Failure("Auth.CreateUserFailed", error.Description));
        }

        await _userManager.AddToRoleAsync(user, "User");

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
