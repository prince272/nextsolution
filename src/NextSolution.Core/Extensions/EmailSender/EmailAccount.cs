using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.EmailSender
{
    public class EmailAccount
    {
        public string Username { get; set; } = default!;

        public string Password { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string DisplayName { get; set; } = default!;
    }
}
