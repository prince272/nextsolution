using FluentValidation;

namespace NextSolution.Core.Models.Chats
{
    public class GetChatForm
    {
        public long Id { get; set; }
    }

    public class GetChatFormValidator : AbstractValidator<GetChatForm>
    {
        public GetChatFormValidator()
        {
        }
    }
}
