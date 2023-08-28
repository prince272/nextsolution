using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http;
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
using NextSolution.WebApi.Models;
using Serilog.Sinks.File;
using System.Security.Policy;

namespace NextSolution.WebApi.Endpoints
{
    public class FileEndpoints : Shared.Endpoints
    {
        public FileEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
            : base(endpointRouteBuilder)
        {
        }

        public override void Configure()
        {
            var endpoints = MapGroup("/files");
            endpoints.MapPost("/", PrepareFileAsync);
            endpoints.MapPatch("/{fileId}", UploadFileAsync);
            endpoints.MapDelete("/{fileId}", DeleteFileAsync);
        }


        public async Task<IResult> PrepareFileAsync(
            [FromServices] IMediaService mediaService,
            [FromHeader(Name = "Upload-Name")] string fileName,
            [FromHeader(Name = "Upload-Length")] long fileSize,
            [FromHeader(Name = "Upload-Type")] string contentType,
            [FromHeader(Name = "Upload-Offset")] long offset,
            HttpContext httpContext)
        {
            var fileId = AlgorithmHelper.GenerateStamp();
            var fileIdHash = AlgorithmHelper.GenerateMD5Hash(httpContext.Request.Path.Add($"/{fileId}"));

            await mediaService.UploadAsync(new UploadMediaByFileChunkForm
            {
                FileId = fileIdHash,
                FileName = fileName,
                FileSize = fileSize,
                Content = Stream.Null,
                ContentType = contentType,
                Offset = offset
            });

            return Results.Content(fileId);
        }

        public async Task<IResult> UploadFileAsync(
            [FromServices] IMediaService mediaService,
            [FromRoute] string fileId,
            [FromHeader(Name = "Upload-Name")] string fileName,
            [FromHeader(Name = "Upload-Length")] long fileSize,
            [FromHeader(Name = "Upload-Type")] string contentType,
            [FromHeader(Name = "Upload-Offset")] long offset,
            HttpContext httpContext)
        {
            var fileIdHash = AlgorithmHelper.GenerateMD5Hash(httpContext.Request.Path);

            await mediaService.UploadAsync(new UploadMediaByFileChunkForm
            {
                FileId = fileIdHash,
                FileName = fileName,
                FileSize = fileSize,
                Content = await httpContext.Request.Body.ToMemoryStreamAsync(),
                ContentType = contentType,
                Offset = offset
            });

            return Results.Content(fileId);
        }

        public async Task<IResult> DeleteFileAsync([FromServices] IMediaService mediaService, [FromRoute] string fileId)
        {
            await mediaService.DeleteAsync(new DeleteMediaByFileIdForm { FileId = fileId });
            return Results.Ok(fileId);
        }
    }
}