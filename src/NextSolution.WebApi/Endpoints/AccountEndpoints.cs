using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Models.Accounts;
using NextSolution.Core.Services;
using NextSolution.Infrastructure.Identity;
using System.Security.Policy;

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
            endpoints.MapPost("/sessions", CreateSessionAsync);
            endpoints.MapPost("/sessions/{provider}", CreateExternalSessionAsync);

            endpoints.MapGet("/sessions/{provider}", ConnectSession);

            endpoints.MapPost("/sessions/refresh", RefreshSessionAsync);
            endpoints.MapPost("/sessions/revoke", RevokeSessionAsync);

            endpoints.MapPost("/username/verify/send-code", SendUsernameTokenAsync);
            endpoints.MapPost("/username/verify", VerifyUsernameAsync);

            endpoints.MapPost("/password/reset/send-code", SendPasswordResetTokenAsync);
            endpoints.MapPost("/password/reset", ResetPasswordAsync);

            endpoints.MapGet("/authorize", () => "Authorized").RequireAuthorization();
        }

        public async Task<IResult> CreateAccountAsync([FromServices] AccountService accountService, [FromBody] CreateAccountForm form)
        {
            await accountService.CreateAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> CreateSessionAsync([FromServices] AccountService accountService, [FromBody] CreateSessionForm form)
        {
            return Results.Ok(await accountService.CreateSessionAsync(form));
        }

        public async Task<IResult> CreateExternalSessionAsync(
            [FromServices] AccountService accountService, 
            [FromServices] SignInManager<User> signInManager, 
            [FromRoute] string provider)
        {
            if (provider == null)
                throw new BadRequestException(nameof(provider), $"'{nameof(provider)}' must not be empty.");

            provider = provider.Pascalize();

            var signInInfo = await signInManager.GetExternalLoginInfoAsync();

            if (signInInfo == null)
                throw new BadRequestException($"{provider} authentication failed.");

            var form = new CreateExternalSessionForm
            {
                Principal = signInInfo.Principal,
                ProviderName = signInInfo.LoginProvider,
                ProviderDisplayName = signInInfo.ProviderDisplayName,
                ProviderKey = signInInfo.ProviderKey
            };

            return Results.Ok(await accountService.CreateExternalSessionAsync(form));
        }

        public IResult ConnectSession(
            [FromServices] AccountService accountService,
            [FromServices] SignInManager<User> signInManager,
            [FromServices] IConfiguration configuration,
            [FromRoute] string provider,
            [FromQuery] string returnUrl)
        {

            if (string.IsNullOrEmpty(provider))
                throw new BadRequestException(nameof(provider), $"'{nameof(provider)}' must not be empty.");

            if (string.IsNullOrEmpty(returnUrl))
                throw new BadRequestException(nameof(returnUrl), $"'{nameof(returnUrl)}' must not be empty.");

            provider = provider.Pascalize();

            var allowedOrigins = configuration.GetSection("AllowedOrigins")?.Get<string[]>() ?? Array.Empty<string>();

            if (!allowedOrigins.Any(origin => Uri.Compare(
                new Uri(origin, UriKind.Absolute),
                new Uri(origin), UriComponents.SchemeAndServer, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) == 0))
                throw new BadRequestException(nameof(returnUrl), $"'{nameof(returnUrl)}' is not allowed.");

            // Request a redirect to the external sign-in provider.
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
            return Results.Challenge(properties, new[] { provider });
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

        public async Task<IResult> SendUsernameTokenAsync([FromServices] AccountService accountService, [FromBody] SendUsernameTokenForm form)
        {
            await accountService.SendUsernameTokenAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> VerifyUsernameAsync([FromServices] AccountService accountService, [FromBody] VerifyUsernameForm form)
        {
            await accountService.VerifyUsernameAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> SendPasswordResetTokenAsync([FromServices] AccountService accountService, [FromBody] SendPasswordResetTokenForm form)
        {
            await accountService.SendPasswordResetTokenAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> ResetPasswordAsync([FromServices] AccountService accountService, [FromBody] ResetPasswordForm form)
        {
            await accountService.ResetPasswordAsync(form);
            return Results.Ok();
        }
    }
}