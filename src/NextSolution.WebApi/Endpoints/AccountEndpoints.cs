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
using System.Security.Claims;
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

            endpoints.MapPost("/register", SignUpAsync);
            endpoints.MapPost("/authenticate", SignInAsync);
            endpoints.MapPost("/{provider}/authenticate", SignInWithAsync);
            endpoints.MapGet("/{provider}/authenticate", SignInWithRedirectAsync);
            endpoints.MapPost("/session/revoke", SignOutAsync).RequireAuthorization();
            endpoints.MapPost("/session/refresh", RefreshSessionAsync);

            endpoints.MapPost("/username/verify/send-code", SendUsernameTokenAsync);
            endpoints.MapPost("/username/verify", VerifyUsernameAsync);

            endpoints.MapPost("/password/reset/send-code", SendPasswordResetTokenAsync);
            endpoints.MapPost("/password/reset", ResetPasswordAsync);

            endpoints.MapGet("/protected", () => "Protected").RequireAuthorization();
        }

        public async Task<IResult> SignUpAsync([FromServices] IAccountService accountService, [FromBody] SignUpForm form)
        {
            await accountService.SignUpAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> SignInAsync([FromServices] IAccountService accountService, [FromBody] SignInForm form)
        {
            return Results.Ok(await accountService.SignInAsync(form));
        }

        public async Task<IResult> SignInWithAsync(
            [FromServices] IAccountService accountService, 
            [FromServices] SignInManager<User> signInManager, 
            [FromRoute] string provider)
        {
            if (provider == null)
                throw new BadRequestException(nameof(provider), $"'{nameof(provider)}' must not be empty.");

            provider = provider.Pascalize();

            var signInInfo = await signInManager.GetExternalLoginInfoAsync();

            if (signInInfo == null)
                throw new BadRequestException($"{provider} authentication failed.");

            var username =
                (signInInfo.Principal.FindFirstValue(ClaimTypes.Email) ??
                signInInfo.Principal.FindFirstValue(ClaimTypes.MobilePhone) ??
                signInInfo.Principal.FindFirstValue(ClaimTypes.OtherPhone) ??
                signInInfo.Principal.FindFirstValue(ClaimTypes.HomePhone))!;

            var firstName = signInInfo.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
            var lastName = signInInfo.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;

            var form = new SignUpWithForm
            {
                Username = username,
                FirstName = firstName,
                LastName = lastName,

                ProviderName = signInInfo.LoginProvider,
                ProviderDisplayName = signInInfo.ProviderDisplayName,
                ProviderKey = signInInfo.ProviderKey
            };

            return Results.Ok(await accountService.SignInWithAsync(form));
        }

        public IResult SignInWithRedirectAsync(
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

        public async Task<IResult> SignOutAsync([FromServices] IAccountService accountService, [FromBody] SignOutForm form)
        {
            await accountService.SignOutAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> RefreshSessionAsync([FromServices] IAccountService accountService, [FromBody] RefreshSessionForm form)
        {
            return Results.Ok(await accountService.RefreshSessionAsync(form));
        }

        public async Task<IResult> SendUsernameTokenAsync([FromServices] IAccountService accountService, [FromBody] SendUsernameTokenForm form)
        {
            await accountService.SendUsernameTokenAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> VerifyUsernameAsync([FromServices] IAccountService accountService, [FromBody] VerifyUsernameForm form)
        {
            await accountService.VerifyUsernameAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> SendPasswordResetTokenAsync([FromServices] IAccountService accountService, [FromBody] SendPasswordResetTokenForm form)
        {
            await accountService.SendPasswordResetTokenAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> ResetPasswordAsync([FromServices] IAccountService accountService, [FromBody] ResetPasswordForm form)
        {
            await accountService.ResetPasswordAsync(form);
            return Results.Ok();
        }
    }
}