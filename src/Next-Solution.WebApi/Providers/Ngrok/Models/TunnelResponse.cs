using System.Text.Json.Serialization;

namespace Next_Solution.WebApi.Providers.Ngrok.Models
{
    public class TunnelListResponse
    {
        public TunnelResponse[] Tunnels { get; set; } = null!;
    }

    public class TunnelResponse
    {
        public TunnelConfig Config { get; set; } = null!;

        public string Name { get; set; } = null!;

        // <summary>
        // URL of the ephemeral tunnel's public endpoint
        // </summary>
        [JsonPropertyName("public_url")] public string PublicUrl { get; set; } = null!;
        // <summary>
        // tunnel protocol for ephemeral tunnels. one of <c>http</c>, <c>https</c>,
        // <c>tcp</c> or <c>tls</c>
        // </summary>
        [JsonPropertyName("proto")] public string Proto { get; set; } = null!;
    }

    public class TunnelConfig
    {
        [JsonPropertyName("addr")] public string Address { get; set; } = null!;
    }
}