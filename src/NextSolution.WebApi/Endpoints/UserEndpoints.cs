using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Models.Medias;
using NextSolution.Core.Models.Users;
using NextSolution.Core.Models.Users.Accounts;
using NextSolution.Core.Services;
using NextSolution.Core.Utilities;
using NextSolution.Infrastructure.Identity;
using System.Security.Claims;
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

            endpoints.MapPost("/register", SignUpAsync);
            endpoints.MapPost("/session/generate", SignInAsync);
            endpoints.MapPost("/{provider}/session/generate", SignInWithAsync);
            endpoints.MapGet("/{provider}/session/generate", SignInWithRedirectAsync);
            endpoints.MapPost("/session/revoke", SignOutAsync).RequireAuthorization();
            endpoints.MapPost("/session/refresh", RefreshSessionAsync);

            endpoints.MapPost("/username/verify/send-code", SendUsernameTokenAsync);
            endpoints.MapPost("/username/verify", VerifyUsernameAsync);

            endpoints.MapPost("/password/reset/send-code", SendPasswordResetTokenAsync);
            endpoints.MapPost("/password/reset", ResetPasswordAsync);
            endpoints.MapPost("/password/change", ChangePasswordAsync);

            endpoints.MapGet("/", GetUsersAsync);
            endpoints.MapGet("/current", GetCurrentUserAsync);
            endpoints.MapPut("/current", EditCurrentUserAsync);

            endpoints.MapPost("/current/avatar", UploadCurrentUserAvatarAsync);
            endpoints.MapPatch("/current/avatar/{avatarId}", UploadCurrentUserAvatarAsync);
            endpoints.MapGet("/current/avatar/{avatarId}", GetCurrentUserAvatarAsync);

            endpoints.MapGet("/protected", () => "Protected").RequireAuthorization();
        }

        public async Task<IResult> SignUpAsync([FromServices] IUserService userService, [FromBody] SignUpForm form)
        {
            await userService.SignUpAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> SignInAsync([FromServices] IUserService userService, [FromBody] SignInForm form)
        {
            return Results.Ok(await userService.SignInAsync(form));
        }

        public async Task<IResult> SignInWithAsync(
            [FromServices] IUserService userService,
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

            return Results.Ok(await userService.SignInWithAsync(form));
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

        public async Task<IResult> SignOutAsync([FromServices] IUserService userService, [FromBody] SignOutForm form)
        {
            await userService.SignOutAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> RefreshSessionAsync([FromServices] IUserService userService, [FromBody] RefreshSessionForm form)
        {
            return Results.Ok(await userService.RefreshSessionAsync(form));
        }

        public async Task<IResult> SendUsernameTokenAsync([FromServices] IUserService userService, [FromBody] SendUsernameTokenForm form)
        {
            await userService.SendUsernameTokenAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> VerifyUsernameAsync([FromServices] IUserService userService, [FromBody] VerifyUsernameForm form)
        {
            await userService.VerifyUsernameAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> SendPasswordResetTokenAsync([FromServices] IUserService userService, [FromBody] SendPasswordResetTokenForm form)
        {
            await userService.SendPasswordResetTokenAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> ResetPasswordAsync([FromServices] IUserService userService, [FromBody] ResetPasswordForm form)
        {
            await userService.ResetPasswordAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> ChangePasswordAsync([FromServices] IUserService userService, [FromBody] ChangePasswordForm form)
        {
            await userService.ChangePasswordAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> GetUsersAsync([FromServices] IUserService userService, [AsParameters] UserSearchCriteria searchCriteria, [FromQuery] long offset = 0, [FromQuery] int limit = 25)
        {
            return Results.Ok(await userService.GetUsersAsync(searchCriteria, offset, limit));
        }

        public async Task<IResult> GetCurrentUserAsync([FromServices] IUserService userService)
        {
            return Results.Ok(await userService.GetCurrentUserAsync());
        }

        public async Task<IResult> EditCurrentUserAsync([FromServices] IUserService userService, [FromBody] EditUserForm form)
        {
            await userService.EditCurrentUserAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> UploadCurrentUserAvatarAsync(
            [FromServices] IUserService userService,
            [FromHeader(Name = "Upload-Id")] string fileId,
            [FromHeader(Name = "Upload-Name")] string fileName,
            [FromHeader(Name = "Upload-Length")] long fileSize,
            [FromHeader(Name = "Upload-Type")] string contentType,
            [FromHeader(Name = "Upload-Offset")] long offset,
            HttpContext httpContext)
        {
            string? avatarId = HttpMethods.IsPost(httpContext.Request.Method) ? null :
                               HttpMethods.IsPatch(httpContext.Request.Method) ? httpContext.GetRouteValue(nameof(avatarId))?.ToString() 
                               ?? throw new InvalidOperationException($"'{nameof(avatarId)}' not found in route values.") : null;

            using var content = await httpContext.Request.Body.ToMemoryStreamAsync();
            var form = new UploadMediaChunkForm
            {
                Id = long.TryParse(avatarId, out long value) ? value : 0,
                Name = fileName,
                Size = fileSize,
                Content = content,
                ContentType = contentType,
                Path = $"/avatars/{AlgorithmHelper.GenerateMD5Hash(fileId)}/{Path.GetFileNameWithoutExtension(fileName)}{Path.GetExtension(fileName).ToLower()}",
                Offset = offset
            };

            await userService.UploadCurrentUserAvatarAsync(form);
            return Results.Content(form.Id.ToString());
        }

        public async Task<IResult> GetCurrentUserAvatarAsync([FromServices] IUserService userService)
        {
            var result = await userService.GetCurrentUserAvatarAsync();
            return Results.File(result.Content, result.ContentType, result.Name, result.UpdatedAt);
        }
    }
}