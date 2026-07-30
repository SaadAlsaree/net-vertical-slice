#if AUTH_IDENTITY
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using VsaTemplate.Data;
using VsaTemplate.Data.Entities;
using VsaTemplate.Extensions;
using VsaTemplate.Models;

namespace VsaTemplate.DependencyInjection;

public static class IdentityAuthenticationExtensions
{
    public static IServiceCollection AddIdentityAuthentication(
        this IServiceCollection services,
        JwtSettings jwtSettings)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Audience = jwtSettings.Audience;
                options.RequireHttpsMetadata = jwtSettings.Issuer.StartsWith("https://");
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = AuthClaims.Roles,
                    NameClaimType = AuthClaims.UserLogin,
                };

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
