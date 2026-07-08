#if AUTH_IDENTITY
namespace VsaTemplate.Features.AuthFeature.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string? FullName
);
#endif
