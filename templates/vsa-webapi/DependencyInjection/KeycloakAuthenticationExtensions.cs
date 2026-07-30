#if AUTH_KEYCLOAK
using VsaTemplate.Models;
using VsaTemplate.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace VsaTemplate.DependencyInjection;

public static class KeycloakAuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        KeycloakSettings keycloakSettings,
        IHostEnvironment environment)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakSettings.Issuer;
                options.MetadataAddress = $"{keycloakSettings.BaseUrl}/realms/{keycloakSettings.Realm}/.well-known/openid-configuration";
                options.Audience = keycloakSettings.Audience;
                options.RequireHttpsMetadata = keycloakSettings.Issuer.StartsWith("https://");
                options.RefreshOnIssuerKeyNotFound = true;

                if (environment.IsDevelopment() &&
                    keycloakSettings.BaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers =
                    [
                        keycloakSettings.Issuer,
                        keycloakSettings.InternalIssuer
                    ],
                    ValidateAudience = true,
                    ValidAudiences = [keycloakSettings.Audience, "account"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = AuthClaims.Roles,
                    NameClaimType = AuthClaims.UserLogin,
                };

                options.MapInboundClaims = false;

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var env = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
                        if (env.IsDevelopment())
                        {
                            context.Response.Headers["X-Auth-Failure"] = context.Exception.Message;
                        }

                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers["X-Token-Expired"] = "true";
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var env = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
                        if (env.IsDevelopment())
                        {
                            if (!string.IsNullOrWhiteSpace(context.Error))
                                context.Response.Headers["X-Auth-Error"] = context.Error;

                            if (!string.IsNullOrWhiteSpace(context.ErrorDescription))
                                context.Response.Headers["X-Auth-Error-Description"] = context.ErrorDescription;
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var userId = context.Principal?.FindFirst(AuthClaims.UserId)?.Value;
                        if (string.IsNullOrWhiteSpace(userId))
                        {
                            context.Fail("Missing user identifier in token.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder();

        return services;
    }
}
#endif
