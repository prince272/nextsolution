using NextSolution._1.Server.Data.Entities.Identity;
using NextSolution._1.Server.Helpers;
using NextSolution._1.Server.Models.Identity;
using NextSolution._1.Server.Providers.Identity;
using NextSolution._1.Server.Providers.JwtBearer;
using NextSolution._1.Server.Providers.Messaging;
using NextSolution._1.Server.Providers.Validation;
using NextSolution._1.Server.Providers.ViewRender;
using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace NextSolution._1.Server.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IValidationProvider _validationProvider;
        private readonly IMessageSender _messageSender;
        private readonly IViewRenderer _viewRenderer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public IdentityService(
            IJwtProvider jwtBearerProvider,
            IValidationProvider validationProvider,
            IMessageSender messageSender,
            IViewRenderer viewRenderer,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            _jwtProvider = jwtBearerProvider ?? throw new ArgumentNullException(nameof(jwtBearerProvider));
            _validationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
            _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<Results<ValidationProblem, Ok>> CreateAccountAsync(CreateAccountForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));
            var formValidation = await _validationProvider.ValidateAsync(form);
            if (!formValidation.IsValid) return TypedResults.ValidationProblem(formValidation.Errors);

            var existingUser = form.UsernameType switch
            {
                ContactType.Email => await _userManager.FindByEmailAsync(form.Username = ValidationHelper.NormalizeEmail(form.Username)),
                ContactType.PhoneNumber => await _userManager.FindByPhoneNumberAsync(form.Username = ValidationHelper.NormalizePhoneNumber(form.Username)),
                _ => throw new InvalidOperationException("Invalid username type.")
            };

            if (existingUser is not null)
            {
                if (CheckIfEmailOrPhoneNumberRequiresConfirmation(existingUser, form.UsernameType!.Value)) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' is not confirmed."] } },
                    extensions: new Dictionary<string, object?> { { "reason", IdentityErrorReason.UsernameNotConfirmed } });

                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' is already taken."] } },
                extensions: new Dictionary<string, object?> { { "reason", IdentityErrorReason.UsernameTaken } });
            }

            var newUser = _mapper.Map<User>(form);
            newUser.UserName = await GenerateUserNameAsync(form.FirstName, form.LastName);
            newUser.Email = form.UsernameType == ContactType.Email ? form.Username : null;
            newUser.PhoneNumber = form.UsernameType == ContactType.PhoneNumber ? form.Username : null;
            newUser.CreatedAt = DateTimeOffset.UtcNow;
            newUser.LastActiveAt = DateTimeOffset.UtcNow;

            newUser.PasswordConfigured = true;
            var createUserResult = await _userManager.CreateAsync(newUser, form.Password);
            if (!createUserResult.Succeeded) throw new InvalidOperationException(createUserResult.Errors.GetMessage());

            await SetupUserRolesAsync(newUser);

            if (CheckIfEmailOrPhoneNumberRequiresConfirmation(newUser, form.UsernameType!.Value)) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' is not confirmed."] } },
                extensions: new Dictionary<string, object?> { { "reason", IdentityErrorReason.UsernameNotConfirmed } });

            return TypedResults.Ok();
        }

        public async Task<Results<ValidationProblem, Ok>> ConfirmAccountAsync(ConfirmAccountForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));
            var formValidation = await _validationProvider.ValidateAsync(form);
            if (!formValidation.IsValid) return TypedResults.ValidationProblem(formValidation.Errors);

            var user = form.UsernameType switch
            {
                ContactType.Email => await _userManager.FindByEmailAsync(form.Username = ValidationHelper.NormalizeEmail(form.Username)),
                ContactType.PhoneNumber => await _userManager.FindByPhoneNumberAsync(form.Username = ValidationHelper.NormalizePhoneNumber(form.Username)),
                _ => throw new InvalidOperationException("Invalid username type.")
            };

            if (user is null) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            { { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' does not exist."] } },
            extensions: new Dictionary<string, object?> { { "reason", IdentityErrorReason.UsernameNotFound } });

            if (form.SendCode)
            {
                var code = form.UsernameType switch
                {
                    ContactType.Email => await _userManager.GenerateChangeEmailTokenAsync(user, user.Email!),
                    ContactType.PhoneNumber => await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber!),
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                var message = form.UsernameType switch
                {
                    ContactType.Email => new Message
                    {
                        Subject = "Confirm Your Email Address",
                        Body = await _viewRenderer.RenderAsync("/Templates/Email/ConfirmAccount", (user, code)),
                        Recipients = new[] { user.Email! },
                    },
                    ContactType.PhoneNumber => new Message
                    {
                        Subject = "Confirm Your Phone Number",
                        Body = await _viewRenderer.RenderAsync("/Templates/Text/ConfirmAccount", (user, code)),
                        Recipients = new[] { user.PhoneNumber! },
                    },
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                var messageChannel = form.UsernameType switch
                {
                    ContactType.Email => MessageChannel.Email,
                    ContactType.PhoneNumber => MessageChannel.Sms,
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                await _messageSender.SendAsync(messageChannel, message);
            }
            else
            {
                var result = form.UsernameType switch
                {
                    ContactType.Email => await _userManager.ChangeEmailAsync(user, user.Email!, form.Code),
                    ContactType.PhoneNumber => await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber!, form.Code),
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                if (!result.Succeeded) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { nameof(form.Code), [$"'{nameof(form.Code).Humanize(LetterCasing.Sentence)}' is not valid."] } });
            }

            return TypedResults.Ok();

        }

        public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangeAccountAsync(ChangeAccountForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));
            var formValidation = await _validationProvider.ValidateAsync(form);
            if (!formValidation.IsValid) return TypedResults.ValidationProblem(formValidation.Errors);

            var currentUserId = (_httpContextAccessor.HttpContext != null ? _userManager.GetUserId(_httpContextAccessor.HttpContext.User) : null);
            var currentUser = currentUserId != null ? await _userManager.FindByIdAsync(currentUserId) : null;
            if (currentUser is null) return TypedResults.Unauthorized();

            if (form.NewUsernameType == ContactType.Email)
            {
                if (!string.IsNullOrEmpty(currentUser.Email) && string.Equals(ValidationHelper.NormalizeEmail(form.NewUsername), currentUser.Email, StringComparison.OrdinalIgnoreCase))
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { nameof(form.NewUsername), [$"'{form.NewUsernameType.Humanize(LetterCasing.Sentence)}' is the same as the current email."] } });
            }
            else if (form.NewUsernameType == ContactType.PhoneNumber)
            {
                if (!string.IsNullOrEmpty(currentUser.PhoneNumber) && string.Equals(ValidationHelper.NormalizePhoneNumber(form.NewUsername), currentUser.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                    { { nameof(form.NewUsername), [$"'{form.NewUsernameType.Humanize(LetterCasing.Sentence)}' is the same as the current phone number."] } });
            }
            else throw new InvalidOperationException("Invalid new username type.");

            if (form.SendCode)
            {
                var code = form.NewUsernameType switch
                {
                    ContactType.Email => await _userManager.GenerateChangeEmailTokenAsync(currentUser, form.NewUsername),
                    ContactType.PhoneNumber => await _userManager.GenerateChangePhoneNumberTokenAsync(currentUser, form.NewUsername),
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                var message = form.NewUsernameType switch
                {
                    ContactType.Email => new Message
                    {
                        Subject = "Change Your Email Address",
                        Body = await _viewRenderer.RenderAsync("/Templates/Email/ChangeAccount", (currentUser, code)),
                        Recipients = new[] { currentUser.Email! },
                    },
                    ContactType.PhoneNumber => new Message
                    {
                        Subject = "Change Your Phone Number",
                        Body = await _viewRenderer.RenderAsync("/Templates/Text/ChangeAccount", (currentUser, code)),
                        Recipients = new[] { currentUser.PhoneNumber! },
                    },
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                var messageChannel = form.NewUsernameType switch
                {
                    ContactType.Email => MessageChannel.Email,
                    ContactType.PhoneNumber => MessageChannel.Sms,
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                await _messageSender.SendAsync(messageChannel, message);

                return TypedResults.Ok();
            }
            else
            {
                var result = form.NewUsernameType switch
                {
                    ContactType.Email => await _userManager.ChangeEmailAsync(currentUser, form.NewUsername, form.Code),
                    ContactType.PhoneNumber => await _userManager.ChangePhoneNumberAsync(currentUser, form.NewUsername, form.Code),
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                if (!result.Succeeded) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { nameof(form.Code), [$"'{nameof(form.Code).Humanize(LetterCasing.Sentence)}' is not valid."] } });

                return TypedResults.Ok();
            }
        }

        public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangePasswordAsync(ChangePasswordForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));
            var formValidation = await _validationProvider.ValidateAsync(form);
            if (!formValidation.IsValid) return TypedResults.ValidationProblem(formValidation.Errors);

            var currentUserId = (_httpContextAccessor.HttpContext != null ? _userManager.GetUserId(_httpContextAccessor.HttpContext.User) : null);
            var currentUser = currentUserId != null ? await _userManager.FindByIdAsync(currentUserId) : null;
            if (currentUser is null) return TypedResults.Unauthorized();

            if (currentUser.PasswordConfigured)
            {
                if (string.IsNullOrWhiteSpace(form.OldPassword))
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                    { { nameof(form.OldPassword), [$"'{form.OldPassword.Humanize(LetterCasing.Sentence)}' must not be empty."] } });

                if (!await _userManager.CheckPasswordAsync(currentUser, form.OldPassword))
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                    { { nameof(form.OldPassword), [$"'{form.OldPassword.Humanize(LetterCasing.Sentence)}' is incorrect."] } });
            }

            var removePasswordResult = await _userManager.RemovePasswordAsync(currentUser);
            if (!removePasswordResult.Succeeded) throw new InvalidOperationException(removePasswordResult.Errors.GetMessage());

            var result = await _userManager.AddPasswordAsync(currentUser, form.NewPassword);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());

            return TypedResults.Ok();
        }

        public async Task<Results<ValidationProblem, Ok>> ResetPasswordAsync(ResetPasswordForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));
            var formValidation = await _validationProvider.ValidateAsync(form);
            if (!formValidation.IsValid) return TypedResults.ValidationProblem(formValidation.Errors);

            var user = form.UsernameType switch
            {
                ContactType.Email => await _userManager.FindByEmailAsync(form.Username = ValidationHelper.NormalizeEmail(form.Username)),
                ContactType.PhoneNumber => await _userManager.FindByPhoneNumberAsync(form.Username = ValidationHelper.NormalizePhoneNumber(form.Username)),
                _ => throw new InvalidOperationException("Invalid username type.")
            };

            if (user is null) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            { { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' does not exist."] } },
                       extensions: new Dictionary<string, object?> { { "reason", IdentityErrorReason.UsernameNotFound } });

            if (form.SendCode)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                var message = form.UsernameType switch
                {
                    ContactType.Email => new Message
                    {
                        Subject = "Reset Your Password",
                        Body = await _viewRenderer.RenderAsync("/Templates/Email/ResetPassword", (user, code)),
                        Recipients = new[] { user.Email! },
                    },
                    ContactType.PhoneNumber => new Message
                    {
                        Subject = "Reset Your Password",
                        Body = await _viewRenderer.RenderAsync("/Templates/Text/ResetPassword", (user, code)),
                        Recipients = new[] { user.PhoneNumber! },
                    },
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                var messageChannel = form.UsernameType switch
                {
                    ContactType.Email => MessageChannel.Email,
                    ContactType.PhoneNumber => MessageChannel.Sms,
                    _ => throw new InvalidOperationException("Invalid username type.")
                };

                await _messageSender.SendAsync(messageChannel, message);
            }
            else
            {
                var result = await _userManager.ResetPasswordAsync(user, form.Code, form.NewPassword);

                if (!result.Succeeded) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { nameof(form.Code), [$"'{nameof(form.Code).Humanize(LetterCasing.Sentence)}' is not valid."] } });
            }

            return TypedResults.Ok();

        }

        public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInAsync(SignInForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));
            var formValidation = await _validationProvider.ValidateAsync(form);
            if (!formValidation.IsValid) return TypedResults.ValidationProblem(formValidation.Errors);

            var user = form.UsernameType switch
            {
                ContactType.Email => await _userManager.FindByEmailAsync(form.Username = ValidationHelper.NormalizeEmail(form.Username)),
                ContactType.PhoneNumber => await _userManager.FindByPhoneNumberAsync(form.Username = ValidationHelper.NormalizePhoneNumber(form.Username)),
                _ => throw new InvalidOperationException("Invalid username type.")
            };

            if (user is null) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            { { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' does not exist."] } },
            extensions: new Dictionary<string, object?> { { "reason", IdentityErrorReason.UsernameNotConfirmed } });

            var checkPassword = await _userManager.CheckPasswordAsync(user, form.Password);
            if (!checkPassword) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            { { nameof(form.Password), [$"'{form.Password.Humanize(LetterCasing.Sentence)}' is incorrect."] } },
            extensions: new Dictionary<string, object?> { { "reason", IdentityErrorReason.PasswordIncorrect } });

            if (CheckIfEmailOrPhoneNumberRequiresConfirmation(user, form.UsernameType!.Value)) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            { { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' is not confirmed."] } },
            extensions: new Dictionary<string, object?> { { "reason", IdentityErrorReason.UsernameNotConfirmed } });

            await _jwtProvider.InvalidateTokensAsync(user, allowMultipleTokens: true);
            var userSession = await GenerateUserSessionAsync(user);
            return TypedResults.Ok(userSession);
        }

        public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInWithAsync(SignInWithForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));
            var formValidation = await _validationProvider.ValidateAsync(form);
            if (!formValidation.IsValid) return TypedResults.ValidationProblem(formValidation.Errors);

            var firstName = form.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "Unknown";
            var lastName = form.Principal.FindFirstValue(ClaimTypes.Surname);

            var username =
                (form.Principal.FindFirstValue(ClaimTypes.Email) ??
                form.Principal.FindFirstValue(ClaimTypes.MobilePhone) ??
                form.Principal.FindFirstValue(ClaimTypes.OtherPhone) ??
                form.Principal.FindFirstValue(ClaimTypes.HomePhone))!;

            var usernameType = ValidationHelper.DetermineContactType(username);

            if (usernameType == ContactType.Email && !ValidationHelper.TryParseEmail(username, out var _))
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>(),
                    title: $"{form.Provider} authentication failed.", detail: $"The email associated with your {form.Provider} account is either not found or invalid..");

            else if (usernameType == ContactType.PhoneNumber && !ValidationHelper.TryParsePhoneNumber(username, out var _))
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>(),
                    title: $"{form.Provider} authentication failed.", detail: $"The phone number associated with your {form.Provider} account is either not found or invalid.");

            var user = usernameType switch
            {
                ContactType.Email => await _userManager.FindByEmailAsync(username = ValidationHelper.NormalizeEmail(username)),
                ContactType.PhoneNumber => await _userManager.FindByPhoneNumberAsync(username = ValidationHelper.NormalizePhoneNumber(username)),
                _ => throw new InvalidOperationException("Invalid username type.")
            };

            if (user is null)
            {
                if (CheckIfEmailOrPhoneNumberRequiresConfirmation(usernameType)) return TypedResults.ValidationProblem(new Dictionary<string, string[]>(),
                    title: $"{form.Provider} authentication failed.", detail: $"Only users with a confirmed account can sign in using {form.Provider}.");

                user = new User();
                user.UserName = await GenerateUserNameAsync(firstName, lastName);
                user.Email = usernameType == ContactType.Email ? username : null;
                user.PhoneNumber = usernameType == ContactType.PhoneNumber ? username : null;
                user.CreatedAt = DateTimeOffset.UtcNow;
                user.LastActiveAt = DateTimeOffset.UtcNow;

                user.PasswordConfigured = false;
                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded) throw new InvalidOperationException(createUserResult.Errors.GetMessage());

                await SetupUserRolesAsync(user);
            }
            else
            {
                if (CheckIfEmailOrPhoneNumberRequiresConfirmation(user, usernameType)) return TypedResults.ValidationProblem(new Dictionary<string, string[]>(),
                    title: $"{form.Provider} authentication failed.", detail: $"Only users with a confirmed account can sign in using {form.Provider}.");
            }

            await _userManager.RemoveLoginAsync(user, form.Provider.ToString(), form.ProviderKey);
            await _userManager.AddLoginAsync(user, new UserLoginInfo(form.Provider.ToString(), form.ProviderKey, form.ProviderDisplayName));

            await _jwtProvider.InvalidateTokensAsync(user, allowMultipleTokens: true);
            var userSession = await GenerateUserSessionAsync(user);
            return TypedResults.Ok(userSession);
        }

        public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> RefreshTokenAsync(RefreshTokenForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));
            var formValidation = await _validationProvider.ValidateAsync(form);
            if (!formValidation.IsValid) return TypedResults.ValidationProblem(formValidation.Errors);

            var user = await _jwtProvider.FindUserByRefreshTokenAsync(form.RefreshToken);
            if (user is null) return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            { { nameof(form.RefreshToken), [$"'{form.RefreshToken.Humanize(LetterCasing.Sentence)}' is invalid."] } });

            await _jwtProvider.InvalidateTokensAsync(user, form.RefreshToken);

            var userSession = await GenerateUserSessionAsync(user);
            return TypedResults.Ok(userSession);
        }

        public async Task<Results<ValidationProblem, Ok>> SignOutAsync(SignOutForm form)
        {
            var currentUserId = (_httpContextAccessor.HttpContext != null ? _userManager.GetUserId(_httpContextAccessor.HttpContext.User) : null);
            var currentUser = currentUserId != null ? await _userManager.FindByIdAsync(currentUserId) : null;

            if (currentUser != null) await _jwtProvider.InvalidateTokensAsync(currentUser, form.RefreshToken, allowMultipleTokens: form.AllowMultipleTokens);

            return TypedResults.Ok();
        }

        public async Task<Results<UnauthorizedHttpResult, Ok<UserProfileModel>>> GetProfileAsync()
        {
            var currentUserId = (_httpContextAccessor.HttpContext != null ? _userManager.GetUserId(_httpContextAccessor.HttpContext.User) : null);
            var currentUser = currentUserId != null ? await _userManager.FindByIdAsync(currentUserId) : null;
            if (currentUser is null) return TypedResults.Unauthorized();

            var userProfileModel = await GetUserProfileAsync(currentUser);
            return TypedResults.Ok(userProfileModel);
        }

        private async Task<string> GenerateUserNameAsync(string firstName, string? lastName = null)
        {
            if (firstName == null) throw new ArgumentNullException(nameof(firstName));

            string separator = "-";
            string userName;
            int count = 1;

            do
            {
                var namePart = lastName == null ? firstName : $"{firstName} {lastName}";
                var nameWithCount = count == 1 ? namePart : $"{namePart} {count}";
                userName = TextHelper.GenerateSlug(nameWithCount.Trim(), separator).ToLower();
                count++;
            } while (await _userManager.Users.AnyAsync(user => user.UserName == userName));

            return userName;
        }

        private async Task<UserProfileModel> GetUserProfileAsync(User user)
        {
            var userProfileModel = _mapper.Map<UserProfileModel>(user);
            userProfileModel.Roles = (await _userManager.GetRolesAsync(user)).ToArray();
            return userProfileModel;
        }

        private async Task<UserSessionModel> GenerateUserSessionAsync(User user)
        {
            var userProfileModel = await GetUserProfileAsync(user);
            var userSessionModel = _mapper.Map(await _jwtProvider.GenerateTokenAsync(user), _mapper.Map<UserSessionModel>(userProfileModel));
            return userSessionModel;
        }

        private async Task SetupUserRolesAsync(User user)
        {
            // Ensure all roles exist.
            foreach (var role in RoleNames.All)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var createRoleResult = await _roleManager.CreateAsync(new Role(role));
                    if (!createRoleResult.Succeeded) throw new InvalidOperationException(createRoleResult.Errors.GetMessage());
                }
            }

            var roles = new List<string> { RoleNames.Member };

            // If this is the first user, make that new user an administrator.
            if (await _userManager.Users.LongCountAsync() == 1) roles.Add(RoleNames.Administrator);

            // Add the new user to the roles.
            var addRolesResult = await _userManager.AddToRolesAsync(user, roles);
            if (!addRolesResult.Succeeded) throw new InvalidOperationException(addRolesResult.Errors.GetMessage());
        }

        private bool CheckIfEmailOrPhoneNumberRequiresConfirmation(User user, ContactType usernameType)
        {
            // Check if the user needs to confirm their email
            bool emailRequiresConfirmation = _userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed && usernameType == ContactType.Email;

            // Check if the user needs to confirm their phone number
            bool phoneRequiresConfirmation = _userManager.Options.SignIn.RequireConfirmedPhoneNumber && !user.PhoneNumberConfirmed && usernameType == ContactType.PhoneNumber;

            // Return true if any of the above conditions are met
            return emailRequiresConfirmation || phoneRequiresConfirmation;
        }

        private bool CheckIfEmailOrPhoneNumberRequiresConfirmation(ContactType usernameType)
        {
            // Check if the user needs to confirm their email
            bool emailRequiresConfirmation = _userManager.Options.SignIn.RequireConfirmedEmail && usernameType == ContactType.Email;

            // Check if the user needs to confirm their phone number
            bool phoneRequiresConfirmation = _userManager.Options.SignIn.RequireConfirmedPhoneNumber && usernameType == ContactType.PhoneNumber;

            // Return true if any of the above conditions are met
            return emailRequiresConfirmation || phoneRequiresConfirmation;
        }
    }

    public interface IIdentityService
    {
        Task<Results<ValidationProblem, Ok>> CreateAccountAsync(CreateAccountForm form);

        Task<Results<ValidationProblem, Ok>> ConfirmAccountAsync(ConfirmAccountForm form);

        Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangeAccountAsync(ChangeAccountForm form);

        Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangePasswordAsync(ChangePasswordForm form);

        Task<Results<ValidationProblem, Ok>> ResetPasswordAsync(ResetPasswordForm form);

        Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInAsync(SignInForm form);

        Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInWithAsync(SignInWithForm form);

        Task<Results<ValidationProblem, Ok<UserSessionModel>>> RefreshTokenAsync(RefreshTokenForm form);

        Task<Results<ValidationProblem, Ok>> SignOutAsync(SignOutForm form);

        Task<Results<UnauthorizedHttpResult, Ok<UserProfileModel>>> GetProfileAsync();
    }

    public enum IdentityErrorReason
    {
        UsernameTaken,
        UsernameNotConfirmed,
        UsernameNotFound,
        PasswordIncorrect,
    }
}
