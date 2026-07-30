using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using VsaTemplate.Infrastructure.Common;
using VsaTemplate.Infrastructure.RateLimiting;
using VsaTemplate.Models;

namespace VsaTemplate.DependencyInjection;

public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(RateLimitingSettings.SectionName)
            .Get<RateLimitingSettings>() ?? new RateLimitingSettings();

        Validate(settings);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, cancellationToken) =>
                WriteRejectionAsync(context, settings, cancellationToken);

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var partitionKey = RateLimitPartitionKey.ForUserOrClient(context);

#if AUTH_ENABLED
                if (partitionKey.StartsWith("user:", StringComparison.Ordinal))
                {
                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey,
                        _ => CreateTokenBucketOptions(settings.Authenticated));
                }
#endif

                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey,
                    _ => CreateSlidingWindowOptions(settings.Anonymous));
            });

            options.AddPolicy(
                RateLimitPolicyNames.AuthLogin,
                context => CreateClientSlidingWindowPartition(
                    context,
                    "login",
                    settings.Login));

            options.AddPolicy(
                RateLimitPolicyNames.AuthRegistration,
                context => CreateClientSlidingWindowPartition(
                    context,
                    "registration",
                    settings.Registration));

            options.AddPolicy(
                RateLimitPolicyNames.AuthRefreshToken,
                context => CreateClientSlidingWindowPartition(
                    context,
                    "refresh-token",
                    settings.RefreshToken));
        });

        return services;
    }

    private static RateLimitPartition<string> CreateClientSlidingWindowPartition(
        HttpContext context,
        string policy,
        RateLimitingSettings.SlidingWindowSettings settings)
    {
        var partitionKey = $"{policy}:{RateLimitPartitionKey.ForClient(context)}";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey,
            _ => CreateSlidingWindowOptions(settings));
    }

    private static TokenBucketRateLimiterOptions CreateTokenBucketOptions(
        RateLimitingSettings.TokenBucketSettings settings)
    {
        return new TokenBucketRateLimiterOptions
        {
            TokenLimit = settings.TokenLimit,
            TokensPerPeriod = settings.TokensPerPeriod,
            ReplenishmentPeriod =
                TimeSpan.FromSeconds(settings.ReplenishmentPeriodSeconds),
            AutoReplenishment = true,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        };
    }

    private static SlidingWindowRateLimiterOptions CreateSlidingWindowOptions(
        RateLimitingSettings.SlidingWindowSettings settings)
    {
        return new SlidingWindowRateLimiterOptions
        {
            PermitLimit = settings.PermitLimit,
            Window = TimeSpan.FromSeconds(settings.WindowSeconds),
            SegmentsPerWindow = settings.SegmentsPerWindow,
            AutoReplenishment = true,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        };
    }

    private static async ValueTask WriteRejectionAsync(
        OnRejectedContext context,
        RateLimitingSettings settings,
        CancellationToken cancellationToken)
    {
        var retryAfterSeconds = GetFallbackRetryAfterSeconds(
            context.HttpContext,
            settings);

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            retryAfterSeconds =
                Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
        }

        context.HttpContext.Response.Headers.RetryAfter =
            retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
        context.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            Response<object>.Failure(
                Error.Problem(
                    "RateLimit.Exceeded",
                    "Too many requests. Please retry later.")),
            cancellationToken);
    }

    private static int GetFallbackRetryAfterSeconds(
        HttpContext context,
        RateLimitingSettings settings)
    {
        var endpointPolicy = context.GetEndpoint()?
            .Metadata.GetMetadata<EnableRateLimitingAttribute>()?
            .PolicyName;

        return endpointPolicy switch
        {
            RateLimitPolicyNames.AuthLogin => settings.Login.WindowSeconds,
            RateLimitPolicyNames.AuthRegistration =>
                settings.Registration.WindowSeconds,
            RateLimitPolicyNames.AuthRefreshToken =>
                settings.RefreshToken.WindowSeconds,
            _ when RateLimitPartitionKey
                .ForUserOrClient(context)
                .StartsWith("user:", StringComparison.Ordinal) =>
                settings.Authenticated.ReplenishmentPeriodSeconds,
            _ => settings.Anonymous.WindowSeconds
        };
    }

    private static void Validate(RateLimitingSettings settings)
    {
        ValidatePositive(
            "RateLimiting:Authenticated:TokenLimit",
            settings.Authenticated.TokenLimit);
        ValidatePositive(
            "RateLimiting:Authenticated:TokensPerPeriod",
            settings.Authenticated.TokensPerPeriod);
        ValidatePositive(
            "RateLimiting:Authenticated:ReplenishmentPeriodSeconds",
            settings.Authenticated.ReplenishmentPeriodSeconds);

        if (settings.Authenticated.TokensPerPeriod >
            settings.Authenticated.TokenLimit)
        {
            throw new InvalidOperationException(
                "RateLimiting:Authenticated:TokensPerPeriod cannot exceed TokenLimit.");
        }

        ValidateSlidingWindow("RateLimiting:Anonymous", settings.Anonymous);
        ValidateSlidingWindow("RateLimiting:Login", settings.Login);
        ValidateSlidingWindow("RateLimiting:Registration", settings.Registration);
        ValidateSlidingWindow("RateLimiting:RefreshToken", settings.RefreshToken);
    }

    private static void ValidateSlidingWindow(
        string path,
        RateLimitingSettings.SlidingWindowSettings settings)
    {
        ValidatePositive($"{path}:PermitLimit", settings.PermitLimit);
        ValidatePositive($"{path}:WindowSeconds", settings.WindowSeconds);
        ValidatePositive($"{path}:SegmentsPerWindow", settings.SegmentsPerWindow);

        if (settings.SegmentsPerWindow > settings.WindowSeconds)
        {
            throw new InvalidOperationException(
                $"{path}:SegmentsPerWindow cannot exceed WindowSeconds.");
        }
    }

    private static void ValidatePositive(string path, int value)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException(
                $"{path} must be greater than zero.");
        }
    }
}
