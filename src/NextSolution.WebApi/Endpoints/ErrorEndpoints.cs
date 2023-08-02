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
    public class ErrorEndpoints : Shared.Endpoints
    {
        public ErrorEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
            : base(endpointRouteBuilder)
        {
            endpointRouteBuilder = endpointRouteBuilder.MapGroup("");
        }


        protected override RouteGroupBuilder MapGroup(string prefix)
        {
            var group = base.MapGroup(prefix)
                .AllowAnonymous()
                .CacheOutput(_ => _.NoCache());
                            
            return group;
        }

        public override void Configure()
        {
            var endpoints = MapGroup("/errors");

            endpoints.Map("/{statusCode}", HandleException)
                .WithName(nameof(HandleException));

            endpoints.MapGet("/throw", ThrowServerException)
                .WithName(nameof(ThrowServerException));

            endpoints.MapGet("/not-found", ThrowNotFoundException)
                .WithName(nameof(ThrowNotFoundException));
        }

        public IResult HandleException(HttpContext httpContext)
        {
            IDictionary<string, TValue> ApplyDictionaryKeyPolicy<TValue>(IDictionary<string, TValue> dictionary)
            {
                var serializerOptions = httpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value?.SerializerOptions;
                dictionary = dictionary.ToDictionary(pair => serializerOptions?.DictionaryKeyPolicy?.ConvertName(pair.Key) ?? pair.Key, pair => pair.Value);
                return dictionary;
            }

            int statusCode = Enum.TryParse<HttpStatusCode>(httpContext.GetRouteValue(nameof(statusCode))?.ToString(), out var status) ? (int)status : (int)HttpStatusCode.NotFound;
            var statusCodeFeature = httpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var exceptionFeature = httpContext.Features.Get<IExceptionHandlerFeature>();

            StatusCodeException? exception;
            string? instance;

            if (exceptionFeature is null)
            {
                instance = statusCodeFeature?.OriginalPath ?? httpContext.Request.Path;

                exception = statusCode switch
                {
                    StatusCodes.Status400BadRequest => new BadRequestException(),
                    StatusCodes.Status404NotFound => new NotFoundException(),
                    _ => new StatusCodeException(statusCode)
                };
            }
            else
            {
                instance = exceptionFeature.Path;

                if (exceptionFeature.Error is StatusCodeException statusCodeException)
                {
                    exception = statusCodeException;
                    statusCode = statusCodeException.StatusCode;
                }
                else
                {
                    exception = new StatusCodeException(statusCode, innerException: exceptionFeature?.Error);
                }
            }

            var title = exception.Title;
            var detail = exception.Message;
            var extensions = ApplyDictionaryKeyPolicy(exception.Data.Cast<DictionaryEntry>().ToDictionary(entry => entry.Key.ToString()!, entry => entry.Value));

            switch (exception)
            {
                case BadRequestException:
                    {
                        var errors = ApplyDictionaryKeyPolicy(((BadRequestException)exception).Errors);
                        return Results.ValidationProblem(errors: errors, title: title, detail: detail, instance: instance, statusCode: statusCode, extensions: extensions);
                    }

                default:
                    return Results.Problem(title: title, detail: detail, instance: instance, statusCode: statusCode, extensions: extensions);
            }
        }

        public IResult ThrowServerException()
        {
            throw new InvalidOperationException();
        }

        public IResult ThrowNotFoundException()
        {
            throw new NotFoundException();
        }
    }
}