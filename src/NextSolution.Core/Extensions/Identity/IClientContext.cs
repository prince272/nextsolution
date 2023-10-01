namespace NextSolution.Core.Extensions.Identity
{
    public interface IClientContext
    {
        string? DeviceId { get; }

        string? IpAddress { get; }

        long? UserId { get; }

        string? UserAgent { get; }

        string? Issuer { get; }

        string? Audience { get; }
    }
}
