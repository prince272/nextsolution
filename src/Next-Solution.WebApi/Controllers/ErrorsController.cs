using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Next_Solution.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ErrorsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public ErrorsController(IWebHostEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        [Route("{statusCode}")]
        [SwaggerIgnore]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public IResult HandleError(int statusCode)
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            string title = GetStatusTitle(statusCode);

            if (exceptionFeature?.Error is not null)
            {
                var exceptionType = exceptionFeature.Error.GetType().FullName;
                string exceptionMessage = $"{exceptionFeature.Error.Message}\n\n{(_environment.IsDevelopment() ? $"Stack Trace:\n{exceptionFeature.Error.StackTrace}" : "")}";

                var instance = exceptionFeature.Path;
                var extensions = _environment.IsDevelopment() ? new Dictionary<string, object?>()
                    {
                        { nameof(exceptionType), exceptionType },
                        { nameof(exceptionMessage), exceptionMessage}
                    } : null;

                return Results.Problem(title: title, instance: instance, statusCode: statusCode, extensions: extensions);
            }
            else
            {
                var instance = statusCodeFeature?.OriginalPath ?? HttpContext.Request.Path;
                return Results.Problem(title: title, instance: instance, statusCode: statusCode);
            }
        }

        public static string GetStatusTitle(int statusCode)
        {
            if (statusCode >= 100 && statusCode < 200)
            {
                return "We've received your request and are processing it.";
            }
            else if (statusCode >= 200 && statusCode < 300)
            {
                return "Your request was successful.";
            }
            else if (statusCode >= 300 && statusCode < 400)
            {
                return "You'll be redirected to a new location shortly.";
            }
            else if (statusCode >= 400 && statusCode < 500)
            {
                return "There was an issue with your request.";
            }
            else if (statusCode >= 500 && statusCode < 600)
            {
                return "Oops! Something went wrong on our end.";
            }
            else
            {
                return "An unexpected server error occurred.";
            }
        }
    }
}