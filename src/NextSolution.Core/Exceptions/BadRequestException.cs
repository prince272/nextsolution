using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Exceptions
{
    public class BadRequestException : StatusCodeException
    {
        private const int STATUS_CODE = 400;

        public BadRequestException() : base(STATUS_CODE)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public BadRequestException(string? reason) : base(STATUS_CODE, reason)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public BadRequestException(IDictionary<string, string[]> errors, string? reason = "One or more validation errors occurred.", Exception? innerException = null)
            : base(STATUS_CODE, reason, innerException)
        {
            Errors = errors.AsReadOnly();
        }

        public BadRequestException(string? reason, Exception? innerException) : base(STATUS_CODE, reason, innerException)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}