using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.EmailSender
{
    public interface IEmailSender
    {
        Task SendAsync(EmailAccount account, EmailMessage message);

        Task SendAsync(string account, EmailMessage message);
    }
}
