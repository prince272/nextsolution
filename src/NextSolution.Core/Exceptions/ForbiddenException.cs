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

        public ForbiddenException(string? title) : base(STATUS_CODE, title)
        {
        }

        public ForbiddenException(string? title, Exception? innerException) : base(STATUS_CODE, title, innerException)
        {
        }
    }
}
