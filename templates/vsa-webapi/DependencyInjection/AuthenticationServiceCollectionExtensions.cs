#if AUTH_IDENTITY
using VsaTemplate.Infrastructure.Authentication;
#endif
#if AUTH_ENABLED
using VsaTemplate.Models;
#endif

namespace VsaTemplate.DependencyInjection;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        bool isCodeGeneration)
    {
#if AUTH_KEYCLOAK
        var section = configuration.GetRequiredSection("Keycloak");
        var settings = section.Get<KeycloakSettings>()
            ?? throw new InvalidOperationException(
                "Keycloak configuration section is missing or invalid.");

        var keycloakOptions = services.AddOptions<KeycloakSettings>()
            .Bind(section)
            .Validate(
                value =>
                    Uri.TryCreate(value.Issuer, UriKind.Absolute, out _) &&
                    Uri.TryCreate(value.BaseUrl, UriKind.Absolute, out _) &&
                    !string.IsNullOrWhiteSpace(value.Realm) &&
                    !string.IsNullOrWhiteSpace(value.Audience),
                "Keycloak BaseUrl, Issuer, Realm, and Audience must be configured.");

        if (!isCodeGeneration)
        {
            keycloakOptions.ValidateOnStart();
        }

        services.AddKeycloakAuthentication(settings, environment);
#elif AUTH_IDENTITY
        var section = configuration.GetRequiredSection("Jwt");
        var settings = section.Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "Jwt configuration section is missing or invalid.");

        var jwtOptions = services.AddOptions<JwtSettings>()
            .Bind(section)
            .Validate(
                value =>
                    Uri.TryCreate(value.Issuer, UriKind.Absolute, out _) &&
                    !string.IsNullOrWhiteSpace(value.Audience) &&
                    value.SecretKey.Length >= 32,
                "Jwt Issuer, Audience, and a SecretKey of at least 32 characters are required.");

        if (!isCodeGeneration)
        {
            jwtOptions.ValidateOnStart();
        }

        services.AddIdentityAuthentication(settings);
        services.AddSingleton<ITokenService, TokenService>();
#endif

        return services;
    }
}
