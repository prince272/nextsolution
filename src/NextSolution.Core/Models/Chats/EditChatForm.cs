using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
