namespace VsaTemplate.Models;

public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public TokenBucketSettings Authenticated { get; set; } = new();
    public SlidingWindowSettings Anonymous { get; set; } = new()
    {
        PermitLimit = 120,
        WindowSeconds = 60,
        SegmentsPerWindow = 6
    };
    public SlidingWindowSettings Login { get; set; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 300,
        SegmentsPerWindow = 5
    };
    public SlidingWindowSettings Registration { get; set; } = new()
    {
        PermitLimit = 5,
        WindowSeconds = 900,
        SegmentsPerWindow = 5
    };
    public SlidingWindowSettings RefreshToken { get; set; } = new()
    {
        PermitLimit = 30,
        WindowSeconds = 60,
        SegmentsPerWindow = 6
    };

    public sealed class TokenBucketSettings
    {
        public int TokenLimit { get; set; } = 60;
        public int TokensPerPeriod { get; set; } = 10;
        public int ReplenishmentPeriodSeconds { get; set; } = 10;
    }

    public sealed class SlidingWindowSettings
    {
        public int PermitLimit { get; set; }
        public int WindowSeconds { get; set; }
        public int SegmentsPerWindow { get; set; }
    }
}
