using NextSolution.Core.Extensions.SmsSender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.SmsSender
{
    public class FakeSmsSender : ISmsSender
    {
        public Task SendAsync(string phoneNumber, string message)
        {
            throw new NotImplementedException();
        }
    }
}
