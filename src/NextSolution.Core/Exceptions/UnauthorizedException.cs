using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NextSolution.Core.Exceptions
{
    public class UnauthorizedException : StatusCodeException
    {
        private const int STATUS_CODE = 401;

        public UnauthorizedException() : base(STATUS_CODE)
        {
        }

        public UnauthorizedException(string? title) : base(STATUS_CODE, title)
        {
        }

        public UnauthorizedException(string? title, Exception? innerException) : base(STATUS_CODE, title, innerException)
        {
        }
    }
}
