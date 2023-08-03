using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.SmsSender
{
    public interface ISmsSender
    {
        Task SendAsync(string phoneNumber,  string message);
    }
}
