using NextSolution.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Exceptions
{
    public class StatusCodeException : InvalidOperationException
    {
        public StatusCodeException(int statusCode) : base(GetMessage(statusCode))
        {
            StatusCode = statusCode;
        }

        public StatusCodeException(int statusCode, string? message) : base(GetMessage(statusCode, message))
        {
            StatusCode = statusCode;
        }

        public StatusCodeException(int statusCode, string? message, Exception? innerException) : base(GetMessage(statusCode, message), innerException)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }

        private static string GetMessage(int statusCode, string? message = null)
        {
            message ??= $"{statusCode switch
                {
                    int sc when sc >= 400 && sc <= 499 => "A client-side error occurred.",
                    int sc when sc >= 500 && sc <= 599 => "An internal server error occurred.",
                    _ => "An unknown error occurred."
                }}";

            return message;
        }

        protected T? GetDataValue<T>(Exception exception, [CallerMemberName] string propertyName = "") => (T?)exception.Data[propertyName];

        protected void SetDataValue<T>(Exception exception, T value, [CallerMemberName] string propertyName = "") => exception.Data[propertyName] = value;
    }
}
