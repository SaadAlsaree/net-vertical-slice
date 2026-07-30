using Microsoft.AspNetCore.Http;
#if AUTH_ENABLED
using VsaTemplate.Extensions;
#endif

namespace VsaTemplate.Infrastructure.RateLimiting;

public static class RateLimitPartitionKey
{
    public static string ForUserOrClient(HttpContext context)
    {
#if AUTH_ENABLED
        var userId = context.User.FindFirst(AuthClaims.UserId)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return $"user:{userId}";
        }
#endif

        return ForClient(context);
    }

    public static string ForClient(HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress;
        if (address is not null)
        {
            if (address.IsIPv4MappedToIPv6)
            {
                address = address.MapToIPv4();
            }

            return $"ip:{address}";
        }

        return $"connection:{context.Connection.Id}";
    }
}
