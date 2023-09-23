using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
