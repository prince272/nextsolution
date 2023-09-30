using Humanizer;
using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NextSolution.Core.Exceptions
{
    public class BadRequestException : StatusCodeException
    {
        private const int STATUS_CODE = 400;

        public BadRequestException() : base(STATUS_CODE)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public BadRequestException(string? title) : base(STATUS_CODE, title)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public BadRequestException(IDictionary<string, string[]> errors, string? title = null, Exception? innerException = null)
            : base(STATUS_CODE, title ??= (errors.Count > 1 ? "One or more validation errors occurred." : errors.First().Value.Humanize()), innerException)
        {
            Errors = errors.AsReadOnly();
        }

        public BadRequestException(string propertyName, string propertyMessage, string? title = null, Exception? innerException = null)
            : base(STATUS_CODE, title ??= (propertyMessage), innerException)
        {
            Errors = new Dictionary<string, string[]> { { propertyName, new[] { propertyMessage } } };
        }

        public BadRequestException(string? title, Exception? innerException) : base(STATUS_CODE, title, innerException)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public IDictionary<string, string[]> Errors { get; }
    }

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

    public class NotFoundException : StatusCodeException
    {
        private const int STATUS_CODE = 404;

        public NotFoundException() : base(STATUS_CODE)
        {
        }

        public NotFoundException(string? title) : base(STATUS_CODE, title)
        {
        }

        public NotFoundException(string? title, Exception? innerException) : base(STATUS_CODE, title, innerException)
        {
        }
    }

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

    public class StatusCodeException : InvalidOperationException
    {
        public StatusCodeException(int statusCode, string? title = null, Exception? innerException = null) : base(GetMessage(statusCode), innerException)
        {
            StatusCode = statusCode;
            Title = GetTitle(statusCode, title);
        }

        public int StatusCode { get; }

        public string Title
        {
            get => GetDataValue<string>(this) ?? string.Empty;
            protected set => SetDataValue(this, value);
        }

        private static string GetTitle(int statusCode, string? title = null)
        {
            var titles = new Dictionary<int, string>()
            {
            };

            if (title == null) titles.TryGetValue(statusCode, out title);

            return title ?? statusCode switch
            {
                int sc when sc >= 400 && sc <= 499 => "A client-side error occurred.",
                int sc when sc >= 500 && sc <= 599 => "An internal server error occurred.",
                _ => "An unknown error occurred."
            };
        }


        private static readonly Dictionary<int, string> Messaegs = new()
        {
            // 1xx: Informational
            { 100, "The server received your request headers. Proceed to send the request body." },
            { 101, "The server is switching protocols as requested." },
            { 103, "The server hints early responses while preparing a full response." },

            // 2xx: Successful
            { 200, "Your request is successful." },
            { 201, "Your request has been fulfilled. A new resource is created." },
            { 202, "Your request accepted for processing, but not completed yet." },
            { 203, "Your request processed successfully, but response from another source." },
            { 204, "Your request processed successfully, but not returning any content." },
            { 205, "Your request processed successfully, and you need to reset the document view." },
            { 206, "The server is delivering only part of the resource due to a range header sent by you." },

            // 3xx: Redirection
            { 300, "Multiple choices. You can select a link to go to that location. Maximum five addresses." },
            { 301, "The requested resource has permanently moved to a new URL." },
            { 302, "The requested resource has temporarily moved to a different URL." },
            { 303, "The requested resource can be found under a different URL." },
            { 304, "The requested resource has not been modified since your last request." },
            { 307, "The requested resource has temporarily moved to a new URL." },
            { 308, "The requested resource has permanently moved to a new URL." },

            // 4xx: Client Error
            { 400, "The server cannot process your request due to bad syntax." },
            { 401, "You are unauthorized. Server refuses to respond without valid authentication credentials." },
            { 402, "Payment is required for accessing the resource (reserved for future use)." },
            { 403, "You are forbidden to access the requested resource." },
            { 404, "The requested resource could not be found." },
            { 405, "The server does not support the request method you used." },
            { 406, "The server can only generate a response that is not accepted by you." },
            { 407, "Proxy authentication is required for you to access the requested resource." },
            { 408, "The server timed out waiting for your request." },
            { 409, "Your request could not be completed due to a conflict." },
            { 410, "The requested resource is no longer available." },
            { 411, "The 'Content-Length' header is not defined. The server will not accept your request without it." },
            { 412, "The precondition given in your request evaluated to false by the server." },
            { 413, "The server will not accept your request because the request entity is too large." },
            { 414, "The server will not accept your request because the URI is too long." },
            { 415, "The server will not accept your request because the media type is not supported." },
            { 416, "The server cannot supply the requested portion of the file." },
            { 417, "The server cannot meet the requirements of the Expect request-header field." },

            // 5xx: Server Error
            { 500, "The server encountered an error while processing your request." },
            { 501, "The server does not recognize the request method or cannot fulfill your request." },
            { 502, "The server acting as a gateway received an invalid response from the upstream server." },
            { 503, "The server is currently unavailable (overloaded or down)." },
            { 504, "The server acting as a gateway did not receive a timely response from the upstream server." },
            { 505, "The server does not support the HTTP protocol version used in your request." },
            { 511, "Network authentication is required for you to gain network access." }
        };

        public static string GetMessage(int statusCode)
        {
            if (Messaegs.TryGetValue(statusCode, out var message))
                return message;

            return "The status code is not recognized.";
        }

        protected T? GetDataValue<T>(Exception exception, [CallerMemberName] string propertyName = "") => (T?)exception.Data[propertyName];

        protected void SetDataValue<T>(Exception exception, T value, [CallerMemberName] string propertyName = "") => exception.Data[propertyName] = value;
    }
}