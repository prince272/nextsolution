using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Exceptions
{
    public class ValidationException : InvalidOperationException
    {
        public ValidationException(string? message)
            : this(new Dictionary<string, string[]>(), message)
        {
        }

        public ValidationException(IDictionary<string, string[]> errors, string? message = "One or more validation errors occurred.") : base(message)
        {
            Errors = errors.AsReadOnly();
        }

        public IDictionary<string, string[]> Errors { get; set; }
    }
}