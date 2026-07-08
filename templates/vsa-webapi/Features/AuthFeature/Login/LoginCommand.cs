#if AUTH_IDENTITY
namespace VsaTemplate.Features.AuthFeature.Login;

public record LoginCommand(
    string Email,
    string Password
);
#endif
