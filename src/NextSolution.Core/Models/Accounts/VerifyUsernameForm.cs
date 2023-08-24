using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Accounts
{
    public class VerifyUsernameForm
    {
        public string Username { get; set; } = default!;

        [JsonIgnore]
        public ContactType UsernameType => ValidationHelper.GetContactType(Username);

        public string Code { get; set; } = default!;

        public class Validator : AbstractValidator<VerifyUsernameForm>
        {
            public Validator()
            {
                RuleFor(_ => _.Username).NotEmpty().Username();
            }
        }
    }
}
