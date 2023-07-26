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
        public BadRequestException(string? message = "The request object format is not valid.", Exception? innerException = null) 
            : base(400, message, innerException)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public BadRequestException(IDictionary<string, string[]> errors, string? message = "One or more validation errors occurred.", Exception? innerException = null)
            : base(400, message, innerException)
        {
            Errors = errors.AsReadOnly();
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}