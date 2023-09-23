using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Chats
{
    public class EditChatForm : CreateChatForm
    {
        [JsonIgnore]
        public long Id { get; set; }
    }

    public class EditChatFormValidator : CreateChatFormValidator<EditChatForm>
    {
    }
}
