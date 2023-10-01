using FluentValidation;

namespace NextSolution.Core.Models.Chats
{
    public class DeleteChatForm
    {
        public long Id { get; set; }
    }

    public class DeleteChatFormValidator : AbstractValidator<DeleteChatForm>
    {
        public DeleteChatFormValidator()
        {
        }
    }
}
