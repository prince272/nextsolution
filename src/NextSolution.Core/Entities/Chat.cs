using NextSolution.Core.Shared;

namespace NextSolution.Core.Entities
{
    public class Chat : IEntity
    {
        public virtual User User { get; set; } = default!;

        public long UserId { get; set; }

        public long Id { get; set; }

        public string Title { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
