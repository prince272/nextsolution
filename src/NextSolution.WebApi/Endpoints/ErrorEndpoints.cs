using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Models.Accounts;
using NextSolution.Core.Services;
using NextSolution.WebApi.Shared;
using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace NextSolution.WebApi.Endpoints
{
    public class ErrorEndpoints : IEndpoints
    {
        public string Name => "Error";

        public void Map(IEndpointRouteBuilder endpoints)
        {
            endpoints = endpoints
                .MapGroup("/errors")
                .AllowAnonymous()
                .CacheOutput(_ => _.NoCache())
                .WithOpenApi();

            endpoints.Map("/{statusCode}", HandleStatusCode).WithName(nameof(HandleStatusCode));
            endpoints.Map("/throw", ThrowInvalidOperationException).WithName(nameof(ThrowInvalidOperationException));
        }


        public IResult HandleStatusCode(HttpContext httpContext)
        {
            IDictionary<string, TValue> ApplyDictionaryKeyPolicy<TValue>(IDictionary<string, TValue> dictionary)
            {
                var serializerOptions = httpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value?.SerializerOptions;
                dictionary = dictionary.ToDictionary(pair => serializerOptions?.DictionaryKeyPolicy?.ConvertName(pair.Key) ?? pair.Key, pair => pair.Value);
                return dictionary;
            }

            var environment = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var statusCodeFeature = httpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var exceptionFeature = httpContext.Features.Get<IExceptionHandlerFeature>();

            var statusCode = (exceptionFeature?.Error) switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                Exception => StatusCodes.Status500InternalServerError,
                _ => int.TryParse(httpContext.GetRouteValue("statusCode")?.ToString(), out int __) ? __ : StatusCodes.Status404NotFound
            };

            var instance = string.Join(":", new[] { environment.ApplicationName, environment.EnvironmentName, "Error", Activity.Current?.Id ?? httpContext?.TraceIdentifier }
            .Where(_ => !string.IsNullOrEmpty(_)));


            switch (statusCode)
            {
                case StatusCodes.Status400BadRequest:
                    {
                        if (exceptionFeature?.Error is ValidationException)
                        {
                            var exception = (ValidationException)exceptionFeature.Error;

                            var title = ReasonPhrases.GetReasonPhrase(statusCode);
                            var detail = exception.Message;
                            var errors = ApplyDictionaryKeyPolicy(exception.Errors);

                            return Results.ValidationProblem(
                                errors: errors,
                                title: title,
                                detail: detail,
                                instance: instance,
                                statusCode: statusCode);
                        }
                        else
                        {
                            var title = ReasonPhrases.GetReasonPhrase(statusCode);
                            var detail = "The request object format is not valid.";

                            return Results.Problem(title: title, detail: detail, instance: instance, statusCode: statusCode);
                        }
                    }

                case StatusCodes.Status404NotFound:
                    {
                        if (exceptionFeature?.Error is NotFoundException)
                        {
                            var exception = (NotFoundException)exceptionFeature.Error;
                            var title = ReasonPhrases.GetReasonPhrase(statusCode);
                            var detail = exception.Message;

                            return Results.Problem(title: title, detail: detail, instance: instance, statusCode: statusCode);
                        }
                        else
                        {
                            var title = ReasonPhrases.GetReasonPhrase(statusCode);
                            var detail = "The specified resource was not found.";

                            return Results.Problem(title: title, detail: detail, instance: instance, statusCode: statusCode);
                        }
                    }
            }

            return Results.Problem(title: ReasonPhrases.GetReasonPhrase(statusCode), detail: "Oops! Something went wrong.", instance: instance, statusCode: statusCode);
        }

        public IResult ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }
    }
}