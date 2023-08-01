using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.EmailSender
{
    public class EmailMessage
    {
        public string Subject { get; set; } = default!;

        public string Body { get; set; } = default!;

        public IEnumerable<EmailAttachment> Attachments { get; set; } = Array.Empty<EmailAttachment>();

        public IEnumerable<string> Recipients { get; set; } = Array.Empty<string>();
    }

}
