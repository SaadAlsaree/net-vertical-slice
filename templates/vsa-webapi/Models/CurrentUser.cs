#if AUTH_KEYCLOAK
namespace VsaTemplate.Models;

public sealed record CurrentUser
{
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string? UserLogin { get; init; }
    public string? FullName { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public bool IsAuthenticated { get; init; }
}
#endif
