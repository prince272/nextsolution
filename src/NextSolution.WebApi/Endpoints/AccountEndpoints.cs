using Microsoft.AspNetCore.Mvc;
using NextSolution.Core.Models.Accounts;
using NextSolution.Core.Services;
using NextSolution.WebApi.Shared;

namespace NextSolution.WebApi.Endpoints
{
    public class AccountEndpoints : IEndpoints
    {
        public string Name => "Account";

        public void Map(IEndpointRouteBuilder endpoints)
        {
            endpoints = endpoints
                .MapGroup("/accounts")
                .WithOpenApi();

            endpoints.MapPost("/", SignUpAsync).WithName(nameof(SignUpAsync));
        }

        public async Task<IResult> SignUpAsync([FromServices] AccountService accountService, [FromBody] CreateAccountForm form)
        {
            await accountService.CreateAsync(form);
            return Results.Ok();
        }
    }
}
