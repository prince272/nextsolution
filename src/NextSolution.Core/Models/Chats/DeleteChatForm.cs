using FluentValidation;
using NextSolution.Core.Models.Medias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
