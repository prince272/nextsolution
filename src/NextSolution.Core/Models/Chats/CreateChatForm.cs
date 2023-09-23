using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Chats
{
    public class CreateChatForm
    {
        public string Title { get; set; } = default!;
    }

    public class CreateChatFormValidator : CreateChatFormValidator<CreateChatForm>
    {
    }

    public class CreateChatFormValidator<TCreateChatForm> : AbstractValidator<TCreateChatForm> where TCreateChatForm : CreateChatForm
    {
        public CreateChatFormValidator()
        {
            RuleFor(_ => _.Title).NotEmpty();
        }
    }
}
