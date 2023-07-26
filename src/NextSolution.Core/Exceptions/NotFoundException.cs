using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Exceptions
{
    public class NotFoundException : StatusCodeException
    {
        public NotFoundException(string? message = "The specified resource was not found.", Exception? innerException = null) : base(404, message, innerException)
        {
        }

        public NotFoundException(string name, object key, Exception? innerException = null) : base(404, $"Entity \"{name}\" ({key}) was not found.", innerException)
        {
        }
    }
}
