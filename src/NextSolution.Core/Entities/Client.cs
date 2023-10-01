using NextSolution.Core.Shared;

namespace NextSolution.Core.Entities
{
    public class Client : IEntity
    {
        public Client()
        {
        }

        public long Id { get; set; }

        public string ConnectionId { get; set; } = default!;

        public DateTimeOffset ConnectionTime { get; set; }

        public string? IpAddress { get; set; }

        public string? DeviceId { get; set; }

        public long? UserId { get; set; }

        public User? User { get; set; }

        public string? UserAgent { get; set; }

        public bool Active { get; set; }
    }
}
