#if AUTH_KEYCLOAK
using System.Security.Claims;
using VsaTemplate.Models;

namespace VsaTemplate.Extensions;

public static class CurrentUserExtensions
{
    public static CurrentUser? GetCurrentUser(this ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return null;

        return new CurrentUser
        {
            UserId = principal.FindFirst(AuthClaims.UserId)?.Value,
            Email = principal.FindFirst(AuthClaims.Email)?.Value,
            UserLogin = principal.FindFirst(AuthClaims.UserLogin)?.Value,
            FullName = principal.FindFirst("name")?.Value,
            Roles = principal.FindAll(AuthClaims.Roles).Select(c => c.Value).ToArray(),
            IsAuthenticated = true
        };
    }

    public static CurrentUser? GetCurrentUser(this HttpContext context)
    {
        return context.User.GetCurrentUser();
    }

    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(AuthClaims.UserId)?.Value;
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(AuthClaims.Email)?.Value;
    }

    public static string? GetUserLogin(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(AuthClaims.UserLogin)?.Value;
    }

    public static string? GetUserId(this HttpContext context)
    {
        return context.User.GetUserId();
    }

    public static string? GetEmail(this HttpContext context)
    {
        return context.User.GetEmail();
    }

    public static string? GetUserLogin(this HttpContext context)
    {
        return context.User.GetUserLogin();
    }
}
#endif
