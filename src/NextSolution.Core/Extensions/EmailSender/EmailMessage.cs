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

        public IList<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();

        public IList<string> Recipients { get; set; } = new List<string>();
    }

}
