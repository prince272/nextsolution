using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Next_Solution.WebApi.Data.Entities.Identity;
using Next_Solution.WebApi.Models.Identity;
using Next_Solution.WebApi.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Next_Solution.WebApi.Controllers
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
        private readonly ILogger<IdentityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityController"/> class.
        /// </summary>
        /// <param name="identityService">The identity service.</param>
        /// <param name="logger">The logger service.</param>
        public IdentityController(IIdentityService identityService, ILogger<IdentityController> logger)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="form">The data required to create the new user account.</param>
        /// <returns>The result of the account creation operation.</returns>
        [HttpPost("create")]
        public async Task<Results<ValidationProblem, Ok>> CreateAccount([FromBody] CreateAccountForm form)
        {
            return await _identityService.CreateAccountAsync(form);
        }

        /// <summary>
        /// Sends a confirmation code to the user for account verification.
        /// </summary>
        /// <param name="form">The data required to send the confirmation code.</param>
        /// <returns>The result of sending the confirmation code.</returns>
        [HttpPost("confirm/send-code")]
        public async Task<Results<ValidationProblem, Ok>> SendConfirmAccountCode([FromBody] SendConfirmAccountCodeForm form)
        {
            return await _identityService.SendConfirmAccountCodeAsync(form);
        }

        /// <summary>
        /// Confirms an existing user account using a confirmation code.
        /// </summary>
        /// <param name="form">The data required to confirm the account using the confirmation code.</param>
        /// <returns>The result of confirming the user account.</returns>
        [HttpPost("confirm")]
        public async Task<Results<ValidationProblem, Ok>> ConfirmAccount([FromBody] ConfirmAccountForm form)
        {
            return await _identityService.ConfirmAccountAsync(form);
        }

        /// <summary>
        /// Requests a code to change user account details.
        /// </summary>
        /// <param name="form">The data required to request the code for changing account details.</param>
        /// <returns>The result of requesting the change code.</returns>
        [Authorize]
        [HttpPost("change/send-code")]
        public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> SendChangeAccountCode([FromBody] SendChangeAccountCodeForm form)
        {
            return await _identityService.SendChangeAccountCodeAsync(form);
        }

        /// <summary>
        /// Changes the current user account details using a confirmation code.
        /// </summary>
        /// <param name="form">The data required to change the account details using the confirmation code.</param>
        /// <returns>The result of changing the account details.</returns>
        [Authorize]
        [HttpPost("change")]
        public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangeAccount([FromBody] ChangeAccountForm form)
        {
            return await _identityService.ChangeAccountAsync(form);
        }

        /// <summary>
        /// Changes the password for the current user account.
        /// </summary>
        /// <param name="form">The data required to change the password.</param>
        /// <returns>The result of changing the password.</returns>
        [Authorize]
        [HttpPost("password/change")]
        public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangePassword([FromBody] ChangePasswordForm form)
        {
            return await _identityService.ChangePasswordAsync(form);
        }

        /// <summary>
        /// Sends a code to reset the user account password.
        /// </summary>
        /// <param name="form">The data required to request the password reset code.</param>
        /// <returns>The result of sending the reset code.</returns>
        [HttpPost("password/reset/send-code")]
        public async Task<Results<ValidationProblem, Ok>> ResetPassword([FromBody] SendResetPasswordCodeForm form)
        {
            return await _identityService.SendResetPasswordCodeAsync(form);
        }

        /// <summary>
        /// Resets the password for the user account using a confirmation code.
        /// </summary>
        /// <param name="form">The data required to reset the password using the confirmation code.</param>
        /// <returns>The result of resetting the password.</returns>
        [HttpPost("password/reset")]
        public async Task<Results<ValidationProblem, Ok>> ResetPassword([FromBody] ResetPasswordForm form)
        {
            return await _identityService.ResetPasswordAsync(form);
        }

        /// <summary>
        /// Signs into an existing user account.
        /// </summary>
        /// <param name="form">The data required to sign into the user account.</param>
        /// <returns>The result of the sign-in operation.</returns>
        [HttpPost("sign-in")]
        public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignIn([FromBody] SignInForm form)
        {
            return await _identityService.SignInAsync(form);
        }

        /// <summary>
        /// Redirects the user to the external sign-in provider.
        /// </summary>
        /// <param name="signInManager">The sign-in manager service.</param>
        /// <param name="provider">The name of the external sign-in provider.</param>
        /// <param name="callbackUrl">The URL to redirect to after sign-in.</param>
        /// <returns>The result of the redirect operation.</returns>
        [HttpGet("sign-in/{provider}")]
        public Results<ValidationProblem, ChallengeHttpResult, Ok> SignInWithRedirect([FromServices] SignInManager<User> signInManager, SignInWithProvider provider, [FromQuery] string callbackUrl)
        {
            var redirectUrl = Url.ActionLink(nameof(SignInWithCallback), values: new { provider, callbackUrl });
            var authenticationProperties = signInManager.ConfigureExternalAuthenticationProperties(provider.ToString(), redirectUrl: redirectUrl);
            return TypedResults.Challenge(authenticationProperties, new[] { provider.ToString() });
        }

        [SwaggerIgnore]
        [HttpGet("sign-in/{provider}/callback")]
        public async Task<Results<RedirectHttpResult, Ok>> SignInWithCallback([FromServices] SignInManager<User> signInManager, SignInWithProvider provider, [FromQuery] string callbackUrl)
        {
            var authenticationResult = await signInManager.GetExternalLoginInfoAsync();

            if (authenticationResult is null)
            {
                return TypedResults.Redirect(callbackUrl, permanent: true);
            }

            var protectedForm = await _identityService.ProtectFormAsync(new SignInWithProviderForm
            {
                Provider = provider,
                ProviderKey = authenticationResult.ProviderKey,
                FirstName = authenticationResult.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "User",
                LastName = authenticationResult.Principal.FindFirstValue(ClaimTypes.Surname),
                Username = (authenticationResult.Principal.FindFirstValue(ClaimTypes.Email) ??
                           authenticationResult.Principal.FindFirstValue(ClaimTypes.MobilePhone) ??
                           authenticationResult.Principal.FindFirstValue(ClaimTypes.OtherPhone) ??
                           authenticationResult.Principal.FindFirstValue(ClaimTypes.HomePhone))!
            });

            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, new Dictionary<string, StringValues>
        {
            { "provider", provider.ToString() },
            { "token", protectedForm },
            { "requestId", Guid.NewGuid().ToString("N") }
        });

            return TypedResults.Redirect(callbackUrl);
        }

        /// <summary>
        /// Signs in with an external provider using a token.
        /// </summary>
        /// <param name="provider">The external sign-in provider.</param>
        /// <param name="token">The token to use for sign-in.</param>
        /// <returns>The result of the sign-in operation.</returns>
        [HttpPost("sign-in/{provider}/{token}")]
        public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInWithToken([FromRoute] SignInWithProvider provider, [FromRoute] string token)
        {
            SignInWithProviderForm? form;

            try
            {
                form = await _identityService.UnprotectFormAsync<SignInWithProviderForm>(token);
            }
            catch (Exception ex)
            {
                // Include provider in error
                _logger.LogError(ex, "Failed to unprotect form for provider {Provider}.", provider);
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>(), title: $"{provider} Authentication failed.");
            }

            return await _identityService.SignInWithAsync(form);
        }

        /// <summary>
        /// Refreshes the current user's access token.
        /// </summary>
        /// <param name="form">The data required to refresh the access token.</param>
        /// <returns>The result of refreshing the access token.</returns>
        [HttpPost("refresh-token")]
        public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> RefreshToken([FromBody] RefreshTokenForm form)
        {
            return await _identityService.RefreshTokenAsync(form);
        }

        /// <summary>
        /// Signs out the current user.
        /// </summary>
        /// <param name="form">The data required to sign out the user.</param>
        /// <returns>The result of the sign-out operation.</returns>
        [Authorize]
        [HttpPost("sign-out")]
        public async Task<Results<ValidationProblem, Ok>> SignOut([FromBody] SignOutForm form)
        {
            return await _identityService.SignOutAsync(form);
        }

        /// <summary>
        /// Retrieves the current user's profile.
        /// </summary>
        /// <returns>The result of retrieving the user's profile.</returns>
        [Authorize]
        [HttpGet("profile")]
        public async Task<Results<UnauthorizedHttpResult, Ok<UserProfileModel>>> GetUserProfile()
        {
            return await _identityService.GetUserProfileAsync();
        }
    }
}
