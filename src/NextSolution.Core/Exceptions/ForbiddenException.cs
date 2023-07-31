using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NextSolution.Core.Exceptions
{
    public class ForbiddenException : StatusCodeException
    {
        private const int STATUS_CODE = 403;

        public ForbiddenException() : base(STATUS_CODE)
        {
        }

        public ForbiddenException(string? reason) : base(STATUS_CODE, reason)
        {
        }

        public ForbiddenException(string? reason, Exception? innerException) : base(STATUS_CODE, reason, innerException)
        {
        }
    }
}
