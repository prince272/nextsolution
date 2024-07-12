using NextSolution.Server.Data.Entities.Identity;
using NextSolution.Server.Models.Identity;
using NextSolution.Server.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace NextSolution.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling identity operations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityController"/> class.
        /// </summary>
        /// <param name="identityService">The identity service.</param>
        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        }

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="form">The account creation form data.</param>
        /// <returns>The result of the account creation.</returns>
        [HttpPost("create")]
        public async Task<IResult> Create([FromBody] CreateAccountForm form)
        {
            return await _identityService.CreateAccountAsync(form);
        }

        /// <summary>
        /// Confirms an existing user account.
        /// </summary>
        /// <param name="form">The confirmation form data.</param>
        /// <returns>The result of confirming the existing user account.</returns>
        [HttpPost("confirm")]
        public async Task<IResult> Confirm([FromBody] ConfirmAccountForm form)
        {
            return await _identityService.ConfirmAccountAsync(form);
        }

        /// <summary>
        /// Changes the current user account.
        /// </summary>
        /// <param name="form">The new user account form data.</param>
        /// <returns>The result of changing the current user account.</returns>
        [Authorize]
        [HttpPost("change")]
        public async Task<IResult> Change([FromBody] ChangeAccountForm form)
        {
            return await _identityService.ChangeAccountAsync(form);
        }

        /// <summary>
        /// Changes the password for the current user account.
        /// </summary>
        /// <param name="form">The new password form data.</param>
        /// <returns>The result of changing the password for the current user account.</returns>
        [Authorize]
        [HttpPost("password/change")]
        public async Task<IResult> ChangePassword([FromBody] ChangePasswordForm form)
        {
            return await _identityService.ChangePasswordAsync(form);
        }

        /// <summary>
        /// Resets the password for the user account.
        /// </summary>
        /// <param name="form">The new password form data.</param>
        /// <returns>The result of resetting the password for the user account.</returns>
        [HttpPost("password/reset")]
        public async Task<IResult> ResetPassword([FromBody] ResetPasswordForm form)
        {
            return await _identityService.ResetPasswordAsync(form);
        }

        /// <summary>
        /// Signs into an existing user account.
        /// </summary>
        /// <param name="form">The sign-in form data.</param>
        /// <returns>The result of signing into an existing user account.</returns>
        [HttpPost("signin")]
        public async Task<IResult> SignIn([FromBody] SignInForm form)
        {
            return await _identityService.SignInAsync(form);
        }


        /// <summary>
        /// Requests a redirect to the external sign-in provider.
        /// </summary>
        /// <param name="signInManager">The sign-in manager service.</param>
        /// <param name="configuration">The configuration service.</param>
        /// <param name="providerValue">The name of the external sign-in provider.</param>
        /// <param name="origin">The origin URL from which the sign-in request was made.</param>
        /// <returns>The result of the sign-in request.</returns>
        [HttpGet("signin/{provider}")]
        public IResult SignInWithGet([FromServices] SignInManager<User> signInManager, [FromServices] IConfiguration configuration, [FromRoute(Name = "provider")] string providerValue, [FromQuery] string origin)
        {
            if (!Enum.TryParse<SignInProvider>(providerValue, ignoreCase: true, out var provider))
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>(), title: $"The sign-in provider specified is invalid. Supported providers: {Enum.GetValues<SignInProvider>().Humanize()}.");

            if (string.IsNullOrWhiteSpace(origin))
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>(), title: $"The origin must be specified.");

            var allowedOrigins = configuration.GetSection("AllowedOrigins")?.Get<string[]>() ?? Array.Empty<string>();
            if (!allowedOrigins.Any(o => Uri.Compare(new Uri(o, UriKind.Absolute), new Uri(origin), UriComponents.SchemeAndServer, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) == 0))
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>(), title: $"The origin specified is not allowed.");

            // Request a redirect to the external sign-in provider.
            var authenticationProperties = signInManager.ConfigureExternalAuthenticationProperties(provider.ToString(), origin);
            return TypedResults.Challenge(authenticationProperties, new[] { provider.ToString() });
        }

        /// <summary>
        /// Handles the post-back from the external sign-in provider and signs the user in.
        /// </summary>
        /// <param name="signInManager">The sign-in manager service.</param>
        /// <param name="providerValue">The name of the external sign-in provider.</param>
        /// <returns>The result of the sign-in process.</returns>
        [HttpPost("signin/{provider}")]
        public async Task<IResult> SignInWithPost([FromServices] SignInManager<User> signInManager, [FromRoute(Name = "provider")] string providerValue)
        {
            if (!Enum.TryParse<SignInProvider>(providerValue, ignoreCase: true, out var provider))
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>(), title: $"The sign-in provider specified is invalid. Supported providers: {Enum.GetValues<SignInProvider>().Humanize()}.");

            var authenticationResult = await signInManager.GetExternalLoginInfoAsync();
            if (authenticationResult is null)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>(), title: "No External sign-in information was found.");

            return await _identityService.SignInWithAsync(new SignInWithForm
            {
                Principal = authenticationResult.Principal,
                Provider = provider,
                ProviderKey = authenticationResult.ProviderKey,
                ProviderDisplayName = authenticationResult.ProviderDisplayName
            });
        }

        /// <summary>
        /// Refreshes the current user's access token.
        /// </summary>
        /// <param name="form">The refresh token form data.</param>
        /// <returns>The result of refreshing the current user's access token.</returns>
        [HttpPost("refresh-token")]
        public async Task<IResult> RefreshToken([FromBody] RefreshTokenForm form)
        {
            return await _identityService.RefreshTokenAsync(form);
        }

        /// <summary>
        /// Signs out the current user.
        /// </summary>
        /// <param name="form">The sign-out form data.</param>
        /// <returns>The result of signing out the current user.</returns>
        [Authorize]
        [HttpPost("signout")]
        public async Task<IResult> SignOut([FromBody] SignOutForm form)
        {
            return await _identityService.SignOutAsync(form);
        }

        /// <summary>
        /// Gets the current user's profile.
        /// </summary>
        /// <returns>The result of getting the current user's profile.</returns>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IResult> GetCurrentProfile()
        {
            return await _identityService.GetCurrentProfileAsync();
        }
    }
}
