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
using NextSolution.Core.Models.Users;
using NextSolution.Core.Services;
using NextSolution.Core.Utilities;
using NextSolution.Infrastructure.Identity;
using Serilog.Sinks.File;
using System.Security.Policy;

namespace NextSolution.WebApi.Endpoints
{
    public class UserEndpoints : Shared.Endpoints
    {
        public UserEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
            : base(endpointRouteBuilder)
        {
        }

        public override void Configure()
        {
            var endpoints = MapGroup("/users");
            endpoints.MapGet("/", GetUsersAsync);
        }

        public async Task<IResult> GetUsersAsync([FromServices] IUserService userService, [AsParameters] UserSearch search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            return Results.Ok(await userService.GetUsersAsync(search, pageNumber, pageSize));
        }
    }
}