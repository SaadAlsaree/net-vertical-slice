#if AUTH_IDENTITY
using System.Security.Claims;
using VsaTemplate.Data.Entities;

namespace VsaTemplate.Infrastructure.Authentication;

public interface ITokenService
{
    (string AccessToken, string RefreshToken) GenerateTokens(ApplicationUser user, IList<string> roles);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
#endif
