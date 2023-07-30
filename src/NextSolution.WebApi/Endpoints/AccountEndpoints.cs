using Microsoft.AspNetCore.Mvc;
using NextSolution.Core.Models.Accounts;
using NextSolution.Core.Services;

namespace NextSolution.WebApi.Endpoints
{
    public class AccountEndpoints : Shared.Endpoints
    {
        public AccountEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
            : base(endpointRouteBuilder)
        {
        }

        public override void Configure()
        {
            var endpoints = MapGroup("/accounts");

            endpoints.MapPost("/", CreateAccountAsync);
            endpoints.MapPost("/sessions/generate", GenerateSessionAsync);
            endpoints.MapPost("/sessions/refresh", RefreshSessionAsync);
            endpoints.MapPost("/sessions/revoke", RevokeSessionAsync);
            endpoints.MapPost("/authorize", () => "Authorized").RequireAuthorization();
        }

        public async Task<IResult> CreateAccountAsync([FromServices] AccountService accountService, [FromBody] CreateAccountForm form)
        {
            await accountService.CreateAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> GenerateSessionAsync([FromServices] AccountService accountService, [FromBody] GenerateSessionForm form)
        {
            return Results.Ok(await accountService.GenerateSessionAsync(form));
        }

        public async Task<IResult> RefreshSessionAsync([FromServices] AccountService accountService, [FromBody] RefreshSessionForm form)
        {
            return Results.Ok(await accountService.RefreshSessionAsync(form));
        }

        public async Task<IResult> RevokeSessionAsync([FromServices] AccountService accountService, [FromBody] RevokeSessionForm form)
        {
            await accountService.RevokeSessionAsync(form);
            return Results.Ok();
        }
    }
}