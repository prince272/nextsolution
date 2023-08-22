using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Models.Accounts;
using NextSolution.Core.Models.Medias;
using NextSolution.Core.Services;
using NextSolution.Core.Utilities;
using NextSolution.Infrastructure.Identity;
using System.Security.Policy;

namespace NextSolution.WebApi.Endpoints
{
    public class MediaEndpoints : Shared.Endpoints
    {
        public MediaEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
            : base(endpointRouteBuilder)
        {
        }

        public override void Configure()
        {
            var endpoints = MapGroup("/media");

            endpoints.MapPost("/upload", UploadMediaAsync);
            endpoints.MapPut("/upload", UploadMediaAsync);
        }

        private async Task<IResult> UploadMediaAsync([FromServices] MediaService mediaService,
            [FromHeader(Name = "Upload-Name")] string fileName,
            [FromHeader(Name = "Upload-Length")] long fileSize,
            [FromHeader(Name = "Upload-Offset")] long offset,
            [FromHeader(Name = "Upload-Type")] MediaType? mediaType,
            [FromHeader(Name = "Content-Type")] string? contentType, HttpContext httpContext)
        {
            var chunk = HttpMethods.IsPatch(httpContext.Request.Method) ? await httpContext.Request.Body.ToMemoryStreamAsync() : Stream.Null;

            await mediaService.UploadAsync(new UploadChunkForm
            {
                FileName = fileName,
                FileSize = fileSize,
                MediaType = mediaType,
                ContentType = contentType,
                Chunk = chunk,
                Offset = offset
            });
            return Results.Ok();
        }
    }
}