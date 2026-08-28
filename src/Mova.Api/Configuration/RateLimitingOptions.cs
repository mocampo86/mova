namespace Mova.Api.Configuration;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; set; } = true;

    public RateLimitingPolicyOptions Search { get; set; } = new()
    {
        PermitLimit = 60,
        WindowSeconds = 60,
        QueueLimit = 0
    };

    public RateLimitingPolicyOptions Login { get; set; } = new()
    {
        PermitLimit = 20,
        WindowSeconds = 60,
        QueueLimit = 0
    };

    public RateLimitingPolicyOptions Reservation { get; set; } = new()
    {
        PermitLimit = 30,
        WindowSeconds = 60,
        QueueLimit = 0
    };
}

public sealed class RateLimitingPolicyOptions
{
    public int PermitLimit { get; set; } = 10;

    public int WindowSeconds { get; set; } = 60;

    public int QueueLimit { get; set; } = 0;
}
