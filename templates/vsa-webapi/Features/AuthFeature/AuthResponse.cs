#if AUTH_IDENTITY
namespace VsaTemplate.Features.AuthFeature;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);
#endif
