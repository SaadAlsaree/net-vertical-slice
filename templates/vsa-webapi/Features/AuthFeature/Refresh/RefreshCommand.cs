#if AUTH_IDENTITY
namespace VsaTemplate.Features.AuthFeature.Refresh;

public record RefreshCommand(
    string AccessToken,
    string RefreshToken
);
#endif
