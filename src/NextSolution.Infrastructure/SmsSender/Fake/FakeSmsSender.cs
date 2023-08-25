using NextSolution.Core.Extensions.SmsSender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.SmsSender.Fake
{
    public class FakeSmsSender : ISmsSender
    {
        public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
