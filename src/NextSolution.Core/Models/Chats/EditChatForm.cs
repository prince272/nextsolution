using FluentValidation;
using System.Text.Json.Serialization;

namespace NextSolution.Core.Models.Chats
{
    public class EditChatForm
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string Title { get; set; } = default!;
    }

    public class EditChatFormValidator : AbstractValidator<EditChatForm>
    {
        public EditChatFormValidator()
        {
            RuleFor(_ => _.Title).NotEmpty();
        }
    }
}
