using FluentValidation;

namespace NextSolution.Core.Models.Chats
{
    public class ChatCompletionForm
    {
        public long? ChatId { get; set; }

        public long? MessageId { get; set; }

        public string Prompt { get; set; } = default!;
    }

    public class ChatCompletionFormValidator : AbstractValidator<ChatCompletionForm>
    {
        public ChatCompletionFormValidator()
        {
            RuleFor(_ => _.Prompt).NotEmpty();
        }
    }
}
