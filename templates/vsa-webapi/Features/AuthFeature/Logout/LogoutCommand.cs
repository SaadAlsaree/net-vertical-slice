#if AUTH_IDENTITY
namespace VsaTemplate.Features.AuthFeature.Logout;

public record LogoutCommand(
    string RefreshToken
);
#endif
