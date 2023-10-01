using NextSolution.Core.Shared;

namespace NextSolution.Core.Entities
{
    public class ChatMessage : IEntity
    {
        public virtual Chat Chat { get; set; } = default!;

        public long ChatId { get; set; }

        public long Id { get; set; }

        public string Role { get; set; } = default!;

        public string Content { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public virtual ChatMessage? Previous { get; set; }

        public long? PreviousId { get; set; }

        public class Roles
        {

            public const string User = nameof(User);

            public const string Assistant = nameof(Assistant);

            public const string System = nameof(System);

            public static IEnumerable<string> All => new[] { User, Assistant, System };
        }
    }
}