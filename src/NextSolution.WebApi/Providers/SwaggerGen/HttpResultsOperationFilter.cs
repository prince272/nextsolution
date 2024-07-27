using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NextSolution.WebApi.Providers.SwaggerGen
{
    public class HttpResultsOperationFilter : IOperationFilter
    {
        private readonly Lazy<string[]> _contentTypes;

        /// <summary>
        /// Constructor to inject services
        /// </summary>
        /// <param name="mvc">MVC options to define response content types</param>
        public HttpResultsOperationFilter(IOptions<MvcOptions> mvc)
        {
            _contentTypes = new Lazy<string[]>(() =>
            {
                var apiResponseTypes = new List<string>();
                if (mvc.Value == null)
                {
                    apiResponseTypes.Add("application/json");
                }
                else
                {
                    var jsonApplicationType = mvc.Value.FormatterMappings.GetMediaTypeMappingForFormat("json");
                    if (jsonApplicationType != null)
                        apiResponseTypes.Add(jsonApplicationType);
                    var xmlApplicationType = mvc.Value.FormatterMappings.GetMediaTypeMappingForFormat("xml");
                    if (xmlApplicationType != null)
                        apiResponseTypes.Add(xmlApplicationType);
                }
                return apiResponseTypes.ToArray();
            });
        }

        void IOperationFilter.Apply(OpenApiOperation operation, OperationFilterContext context)
        {

            if (!IsControllerAction(context)) return;

            var actionReturnType = UnwrapTask(context.MethodInfo.ReturnType);
            if (!IsHttpResults(actionReturnType)) return;

            if (typeof(IEndpointMetadataProvider).IsAssignableFrom(actionReturnType))
            {
                var populateMetadataMethod = actionReturnType.GetMethod("Microsoft.AspNetCore.Http.Metadata.IEndpointMetadataProvider.PopulateMetadata", BindingFlags.Static | BindingFlags.NonPublic);
                if (populateMetadataMethod == null) return;

                var endpointBuilder = new MetadataEndpointBuilder();
                populateMetadataMethod.Invoke(null, new object[] { context.MethodInfo, endpointBuilder });

                var responseTypes = endpointBuilder.Metadata.Cast<IProducesResponseTypeMetadata>().ToList();
                if (!responseTypes.Any()) return;
                operation.Responses.Clear();
                foreach (var responseType in responseTypes)
                {
                    var statusCode = responseType.StatusCode.ToString();
                    var oar = new OpenApiResponse { Description = GetResponseDescription(statusCode) };

                    if (responseType.Type != null && responseType.Type != typeof(void))
                    {
                        var schema = context.SchemaGenerator.GenerateSchema(responseType.Type, context.SchemaRepository);
                        foreach (var contentType in _contentTypes.Value)
                        {
                            oar.Content.Add(contentType, new OpenApiMediaType { Schema = schema });
                        }
                    }

                    operation.Responses.Add(statusCode, oar);
                }
            }
            else if (actionReturnType == typeof(UnauthorizedHttpResult))
            {
                operation.Responses.Clear();
                operation.Responses.Add("401", new OpenApiResponse { Description = ReasonPhrases.GetReasonPhrase(401) });

            }
        }

        private static bool IsControllerAction(OperationFilterContext context)
            => context.ApiDescription.ActionDescriptor is ControllerActionDescriptor;

        private static bool IsHttpResults(Type type)
            => type.Namespace == "Microsoft.AspNetCore.Http.HttpResults";

        private static Type UnwrapTask(Type type)
        {
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(Task<>) || genericType == typeof(ValueTask<>))
                {
                    return type.GetGenericArguments()[0];
                }
            }
            return type;
        }

        private static string? GetResponseDescription(string statusCode)
            => ResponseDescriptionMap
                .FirstOrDefault(entry => Regex.IsMatch(statusCode, entry.Key))
                .Value;

        private static readonly IReadOnlyCollection<KeyValuePair<string, string>> ResponseDescriptionMap = new[]
        {
        new KeyValuePair<string, string>("1\\d{2}", "Information"),

        new KeyValuePair<string, string>("201", "Created"),
        new KeyValuePair<string, string>("202", "Accepted"),
        new KeyValuePair<string, string>("204", "No Content"),
        new KeyValuePair<string, string>("2\\d{2}", "Success"),

        new KeyValuePair<string, string>("304", "Not Modified"),
        new KeyValuePair<string, string>("3\\d{2}", "Redirect"),

        new KeyValuePair<string, string>("400", "Bad Request"),
        new KeyValuePair<string, string>("401", "Unauthorized"),
        new KeyValuePair<string, string>("403", "Forbidden"),
        new KeyValuePair<string, string>("404", "Not Found"),
        new KeyValuePair<string, string>("405", "Method Not Allowed"),
        new KeyValuePair<string, string>("406", "Not Acceptable"),
        new KeyValuePair<string, string>("408", "Request Timeout"),
        new KeyValuePair<string, string>("409", "Conflict"),
        new KeyValuePair<string, string>("429", "Too Many Requests"),
        new KeyValuePair<string, string>("4\\d{2}", "Client Error"),

        new KeyValuePair<string, string>("5\\d{2}", "Server Error"),
        new KeyValuePair<string, string>("default", "Error")
    };

        private sealed class MetadataEndpointBuilder : EndpointBuilder
        {
            public override Endpoint Build() => throw new NotImplementedException();
        }
    }
}
