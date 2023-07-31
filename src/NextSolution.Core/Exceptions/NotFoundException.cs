using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NextSolution.Core.Exceptions
{
    public class NotFoundException : StatusCodeException
    {
        private const int STATUS_CODE = 404;

        public NotFoundException() : base(STATUS_CODE)
        {
        }

        public NotFoundException(string? reason) : base(STATUS_CODE, reason)
        {
        }

        public NotFoundException(string? reason, Exception? innerException) : base(STATUS_CODE, reason, innerException)
        {
        }
    }
}
