using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Next_Solution.WebApi.Data.Entities.Identity;
using Next_Solution.WebApi.Models.Identity;
using Next_Solution.WebApi.Providers.Messaging;
using Next_Solution.WebApi.Providers.Validation;
using Next_Solution.WebApi.Providers.JwtBearer;
using Next_Solution.WebApi.Helpers;
using Next_Solution.WebApi.Providers.RazorViewRender;
using Next_Solution.WebApi.Providers.ModelValidator;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using System.Text;
using IdentityAudit.Utilities;
using Microsoft.Extensions.Options;
using Next_Solution.WebApi.Options;
using Microsoft.AspNetCore.Identity;

namespace Next_Solution.WebApi.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IModelValidator _modelValidator;
        private readonly IMessageSender _messageSender;
        private readonly IRazorViewRenderer _razorViewRenderer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IDataProtector _formProtector;

        public IdentityService(
            IJwtProvider jwtBearerProvider,
            IModelValidator modelValidator,
            IMessageSender messageSender,
            IRazorViewRenderer razorViewRenderer,
            IHttpContextAccessor httpContextAccessor,
            IDataProtectionProvider dataProtectionProvider,
            IMapper mapper,
            IOptions<IdentityServiceOptions> identityServiceOptions,
            UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            _jwtProvider = jwtBearerProvider ?? throw new ArgumentNullException(nameof(jwtBearerProvider));
            _modelValidator = modelValidator ?? throw new ArgumentNullException(nameof(modelValidator));
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
            _razorViewRenderer = razorViewRenderer ?? throw new ArgumentNullException(nameof(razorViewRenderer));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _formProtector = dataProtectionProvider.CreateProtector(identityServiceOptions.Value.FormProtectorKey);
        }

        public async Task<Results<ValidationProblem, Ok>> CreateAccountAsync(CreateAccountForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);

            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var existingUser = await _userManager.GetByUsernameAsync(form.UsernameType, form.Username);

            if (existingUser is not null)
            {
                if (CheckIfEmailOrPhoneNumberRequiresConfirmation(existingUser, form.UsernameType))
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' is not confirmed."] }
                    };

                    return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
                }
                else
                {
                    var errors = new Dictionary<string, string[]>
                    { 
                        { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' is already taken."] }
                    };

                    return TypedResults.ValidationProblem(errors, 
                        title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
                }
            }

            var newUser = _mapper.Map<User>(form);
            newUser.FirstName = form.FirstName;
            newUser.LastName = form.LastName;
            newUser.UserName = await GenerateUserNameAsync(form.FirstName, form.LastName);
            newUser.Email = form.UsernameType == ContactType.Email ? form.Username : null;
            newUser.PhoneNumber = form.UsernameType == ContactType.PhoneNumber ? form.Username : null;
            newUser.CreatedAt = DateTimeOffset.UtcNow;
            newUser.LastActiveAt = DateTimeOffset.UtcNow;

            newUser.PasswordConfigured = true;
            var createUserResult = await _userManager.CreateAsync(newUser, form.Password);
            if (!createUserResult.Succeeded) throw new InvalidOperationException(createUserResult.Errors.GetMessage());

            await SetupUserRolesAsync(newUser);

            return TypedResults.Ok();
        }

        public async Task<Results<ValidationProblem, Ok>> SendConfirmAccountCodeAsync(SendConfirmAccountCodeForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);

            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var existingUser = await _userManager.GetByUsernameAsync(form.UsernameType, form.Username);

            if (existingUser is null)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' does not exist."] }
                };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var code = _userManager.GenerateConfirmUsernameCodeAsync(form.UsernameType, existingUser);

            var message = form.UsernameType switch
            {
                ContactType.Email => new Message
                {
                    Subject = "Confirm Your Email Address",
                    Body = await _razorViewRenderer.RenderAsync("/Templates/Email/ConfirmAccount", (existingUser, code)),
                    Recipients = new[] { existingUser.Email! },
                },
                ContactType.PhoneNumber => new Message
                {
                    Subject = "Confirm Your Phone Number",
                    Body = await _razorViewRenderer.RenderAsync("/Templates/Text/ConfirmAccount", (existingUser, code)),
                    Recipients = new[] { existingUser.PhoneNumber! },
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

            return TypedResults.Ok();

        }

        public async Task<Results<ValidationProblem, Ok>> ConfirmAccountAsync(ConfirmAccountForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);

            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var existingUser = await _userManager.GetByUsernameAsync(form.UsernameType, form.Username);

            if (existingUser is null)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' does not exist."] }
                };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var result = await _userManager.ConfirmUsernameAsync(form.UsernameType, existingUser, form.Code);

            if (!result.Succeeded)
            {
                var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.Code), [$"'{nameof(form.Code).Humanize(LetterCasing.Sentence)}' is not valid."] }
                    };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            return TypedResults.Ok();

        }

        public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> SendChangeAccountCodeAsync(SendChangeAccountCodeForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);

            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var currentUserId = _httpContextAccessor.HttpContext != null ? _userManager.GetUserId(_httpContextAccessor.HttpContext.User) : null;
            var currentUser = currentUserId != null ? await _userManager.FindByIdAsync(currentUserId) : null;
            if (currentUser is null) return TypedResults.Unauthorized();

            if (form.NewUsernameType == ContactType.Email)
            {
                if (!string.IsNullOrEmpty(currentUser.Email) && string.Equals(ValidationHelper.NormalizeEmail(form.NewUsername), currentUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.NewUsername), [$"'{form.NewUsernameType.Humanize(LetterCasing.Sentence)}' is the same as the current email."] }
                    };

                    return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
                }
            }
            else if (form.NewUsernameType == ContactType.PhoneNumber)
            {
                if (!string.IsNullOrEmpty(currentUser.PhoneNumber) && string.Equals(ValidationHelper.NormalizePhoneNumber(form.NewUsername), currentUser.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.NewUsername), [$"'{form.NewUsernameType.Humanize(LetterCasing.Sentence)}' is the same as the current phone number."] }
                    };

                    return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
                }
            }
            else throw new InvalidOperationException("Invalid new username type.");

            var code = _userManager.GenerateChangeUsernameCodeAsync(form.NewUsernameType, currentUser, form.NewUsername);

            var message = form.NewUsernameType switch
            {
                ContactType.Email => new Message
                {
                    Subject = "Change Your Email Address",
                    Body = await _razorViewRenderer.RenderAsync("/Templates/Email/ChangeAccount", (currentUser, code)),
                    Recipients = new[] { currentUser.Email! },
                },
                ContactType.PhoneNumber => new Message
                {
                    Subject = "Change Your Phone Number",
                    Body = await _razorViewRenderer.RenderAsync("/Templates/Text/ChangeAccount", (currentUser, code)),
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

        public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangeAccountAsync(ChangeAccountForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);

            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var currentUserId = _httpContextAccessor.HttpContext != null ? _userManager.GetUserId(_httpContextAccessor.HttpContext.User) : null;
            var currentUser = currentUserId != null ? await _userManager.FindByIdAsync(currentUserId) : null;
            if (currentUser is null) return TypedResults.Unauthorized();

            if (form.NewUsernameType == ContactType.Email)
            {
                if (!string.IsNullOrEmpty(currentUser.Email) && string.Equals(ValidationHelper.NormalizeEmail(form.NewUsername), currentUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.NewUsername), [$"'{form.NewUsernameType.Humanize(LetterCasing.Sentence)}' is the same as the current email."] }
                    };

                    return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
                }
            }
            else if (form.NewUsernameType == ContactType.PhoneNumber)
            {
                if (!string.IsNullOrEmpty(currentUser.PhoneNumber) && string.Equals(ValidationHelper.NormalizePhoneNumber(form.NewUsername), currentUser.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.NewUsername), [$"'{form.NewUsernameType.Humanize(LetterCasing.Sentence)}' is the same as the current phone number."] }
                    };

                    return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
                }
            }
            else throw new InvalidOperationException("Invalid new username type.");

            var result = await _userManager.ChangeUsernameAsync(form.NewUsernameType, currentUser, form.NewUsername, form.Code);

            if (!result.Succeeded)
            {
                var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.Code), [$"'{nameof(form.Code).Humanize(LetterCasing.Sentence)}' is not valid."] }
                    };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            return TypedResults.Ok();

        }

        public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangePasswordAsync(ChangePasswordForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);
            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var currentUserId = _httpContextAccessor.HttpContext != null ? _userManager.GetUserId(_httpContextAccessor.HttpContext.User) : null;
            var currentUser = currentUserId != null ? await _userManager.FindByIdAsync(currentUserId) : null;
            if (currentUser is null) return TypedResults.Unauthorized();

            if (currentUser.PasswordConfigured)
            {
                if (string.IsNullOrWhiteSpace(form.OldPassword))
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.OldPassword), [$"'{form.OldPassword.Humanize(LetterCasing.Sentence)}' must not be empty."] } 
                    };

                    return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
                }

                if (!await _userManager.CheckPasswordAsync(currentUser, form.OldPassword))
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.OldPassword), [$"'{form.OldPassword.Humanize(LetterCasing.Sentence)}' is incorrect."] }
                    };

                    return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
                }
                    
            }

            var removePasswordResult = await _userManager.RemovePasswordAsync(currentUser);
            if (!removePasswordResult.Succeeded) throw new InvalidOperationException(removePasswordResult.Errors.GetMessage());

            var result = await _userManager.AddPasswordAsync(currentUser, form.NewPassword);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());

            return TypedResults.Ok();
        }

        public async Task<Results<ValidationProblem, Ok>> SendResetPasswordCodeAsync(SendResetPasswordCodeForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);
            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var existingUser = await _userManager.GetByUsernameAsync(form.UsernameType, form.Username);

            if (existingUser is null)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' does not exist."] }
                };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }


            var code = await _userManager.GeneratePasswordResetTokenAsync(existingUser);

            var message = form.UsernameType switch
            {
                ContactType.Email => new Message
                {
                    Subject = "Reset Your Password",
                    Body = await _razorViewRenderer.RenderAsync("/Templates/Email/ResetPassword", (existingUser, code)),
                    Recipients = new[] { existingUser.Email! },
                },
                ContactType.PhoneNumber => new Message
                {
                    Subject = "Reset Your Password",
                    Body = await _razorViewRenderer.RenderAsync("/Templates/Text/ResetPassword", (existingUser, code)),
                    Recipients = new[] { existingUser.PhoneNumber! },
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

            return TypedResults.Ok();
        }

        public async Task<Results<ValidationProblem, Ok>> ResetPasswordAsync(ResetPasswordForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);
            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var existingUser = await _userManager.GetByUsernameAsync(form.UsernameType, form.Username);

            if (existingUser is null)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' does not exist."] }
                };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var resetPasswordResult = await _userManager.ResetPasswordAsync(existingUser, form.Code, form.NewPassword);

            if (!resetPasswordResult.Succeeded)
            {
                var errors = new Dictionary<string, string[]>
                    {
                        { nameof(form.Code), [$"'{nameof(form.Code).Humanize(LetterCasing.Sentence)}' is not valid."] }
                    };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            return TypedResults.Ok();
        }

        public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInAsync(SignInForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);
            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var existingUser = await _userManager.GetByUsernameAsync(form.UsernameType, form.Username);

            if (existingUser is null)
            {
                var errors = new Dictionary<string, string[]> {
                    { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' does not exist."] } 
                };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var checkPassword = await _userManager.CheckPasswordAsync(existingUser, form.Password);

            if (!checkPassword)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { nameof(form.Password), [$"'{nameof(form.Password).Humanize(LetterCasing.Sentence)}' is incorrect."] }
                };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            if (CheckIfEmailOrPhoneNumberRequiresConfirmation(existingUser, form.UsernameType))
            {
                var errors = new Dictionary<string, string[]>
                {
                    { nameof(form.Username), [$"'{form.UsernameType.Humanize(LetterCasing.Sentence)}' is not confirmed."] }
                };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            await _jwtProvider.InvalidateTokensAsync(existingUser, allowMultipleTokens: true);
            var userSession = await GenerateUserSessionAsync(existingUser);
            return TypedResults.Ok(userSession);
        }

        public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInAsync(SignInWithProviderForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);
            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var existingUser = await _userManager.GetByUsernameAsync(form.UsernameType, form.Username);

            if (existingUser is null)
            {
                existingUser = new User
                {
                    FirstName = form.FirstName,
                    LastName = form.LastName,
                    UserName = await GenerateUserNameAsync(form.FirstName, form.LastName),
                    Email = form.UsernameType == ContactType.Email ? form.Username : null,
                    EmailConfirmed = form.UsernameType == ContactType.Email,
                    PhoneNumber = form.UsernameType == ContactType.PhoneNumber ? form.Username : null,
                    PhoneNumberConfirmed = form.UsernameType == ContactType.PhoneNumber,
                    CreatedAt = DateTimeOffset.UtcNow,
                    LastActiveAt = DateTimeOffset.UtcNow,
                    PasswordConfigured = false
                };

                var createUserResult = await _userManager.CreateAsync(existingUser);

                if (!createUserResult.Succeeded)
                {
                    throw new InvalidOperationException(createUserResult.Errors.GetMessage());
                }

                await SetupUserRolesAsync(existingUser);
            }

            var providerName = form.Provider.ToString();
            var providerKey = form.ProviderKey;
            var providerDisplayName = form.Provider.GetEnumDisplayName();

            await _userManager.RemoveLoginAsync(existingUser, providerName, providerKey);
            await _userManager.AddLoginAsync(existingUser, new UserLoginInfo(providerName, providerKey, providerDisplayName));

            await _jwtProvider.InvalidateTokensAsync(existingUser, allowMultipleTokens: true);
            var userSession = await GenerateUserSessionAsync(existingUser);
            return TypedResults.Ok(userSession);
        }

        public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> RefreshTokenAsync(RefreshTokenForm form)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));

            var formValidation = await _modelValidator.ValidateAsync(form);
            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var existingUser = await _jwtProvider.FindUserByRefreshTokenAsync(form.RefreshToken);

            if (existingUser is null)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { nameof(form.RefreshToken), [$"'{form.RefreshToken.Humanize(LetterCasing.Sentence)}' is invalid."] }
                };

                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            await _jwtProvider.InvalidateTokensAsync(existingUser, form.RefreshToken);

            var userSession = await GenerateUserSessionAsync(existingUser);
            return TypedResults.Ok(userSession);
        }

        public async Task<Results<ValidationProblem, Ok>> SignOutAsync(SignOutForm form)
        {
            var formValidation = await _modelValidator.ValidateAsync(form);

            if (!formValidation.IsValid)
            {
                var errors = formValidation.Errors;
                return TypedResults.ValidationProblem(errors, title: errors.Count == 1 ? errors.First().Value.FirstOrDefault() : null);
            }

            var currentUserId = _httpContextAccessor.HttpContext != null ? _userManager.GetUserId(_httpContextAccessor.HttpContext.User) : null;
            var currentUser = currentUserId != null ? await _userManager.FindByIdAsync(currentUserId) : null;

            if (currentUser != null) await _jwtProvider.InvalidateTokensAsync(currentUser, form.RefreshToken, allowMultipleTokens: form.AllowMultipleTokens);

            return TypedResults.Ok();
        }

        public async Task<string> ProtectFormAsync<TForm>(TForm unprotectedForm) where TForm : class
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, unprotectedForm);
            stream.Position = 0;
            var protectedSession = _formProtector.Protect(Encoding.UTF8.GetString(stream.ToArray()));
            return protectedSession;
        }

        public async Task<TForm> UnprotectFormAsync<TForm>(string protectedForm) where TForm : class
        {
            var formData = _formProtector.Unprotect(protectedForm);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(formData));
            var form = await JsonSerializer.DeserializeAsync<TForm>(stream);
            return form!;
        }

        public async Task<Results<UnauthorizedHttpResult, Ok<UserProfileModel>>> GetUserProfileAsync()
        {
            var currentUserId = _httpContextAccessor.HttpContext != null ? _userManager.GetUserId(_httpContextAccessor.HttpContext.User) : null;
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
    }

    public interface IIdentityService
    {
        Task<Results<ValidationProblem, Ok>> CreateAccountAsync(CreateAccountForm form);

        Task<Results<ValidationProblem, Ok>> SendConfirmAccountCodeAsync(SendConfirmAccountCodeForm form);

        Task<Results<ValidationProblem, Ok>> ConfirmAccountAsync(ConfirmAccountForm form);

        Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> SendChangeAccountCodeAsync(SendChangeAccountCodeForm form);

        Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangeAccountAsync(ChangeAccountForm form);

        Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangePasswordAsync(ChangePasswordForm form);

        Task<Results<ValidationProblem, Ok>> SendResetPasswordCodeAsync(SendResetPasswordCodeForm form);

        Task<Results<ValidationProblem, Ok>> ResetPasswordAsync(ResetPasswordForm form);

        Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInAsync(SignInForm form);

        Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInAsync(SignInWithProviderForm form);

        Task<string> ProtectFormAsync<TForm>(TForm unprotectedForm) where TForm : class;

        Task<TForm> UnprotectFormAsync<TForm>(string protectedForm) where TForm : class;

        Task<Results<ValidationProblem, Ok<UserSessionModel>>> RefreshTokenAsync(RefreshTokenForm form);

        Task<Results<ValidationProblem, Ok>> SignOutAsync(SignOutForm form);

        Task<Results<UnauthorizedHttpResult, Ok<UserProfileModel>>> GetUserProfileAsync();
    }

    public static class IdentityExtensions
    {
        public static Task<TUser?> GetByEmailAsync<TUser>(this UserManager<TUser> userManager, string email)
           where TUser : IdentityUser<string>
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (email is null) throw new ArgumentNullException(nameof(email));

            email = ValidationHelper.NormalizeEmail(email);
            return userManager.FindByEmailAsync(email);
        }

        public static Task<TUser?> GetByPhoneNumberAsync<TUser>(this UserManager<TUser> userManager, string phoneNumber)
            where TUser : IdentityUser<string>
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (phoneNumber is null) throw new ArgumentNullException(nameof(phoneNumber));

            phoneNumber = ValidationHelper.NormalizePhoneNumber(phoneNumber);
            return userManager.Users.FirstOrDefaultAsync(_ => _.PhoneNumber == phoneNumber);
        }

        public static Task<TUser?> GetByUsernameAsync<TUser>(this UserManager<TUser> userManager, ContactType usernameType, string username)
            where TUser : IdentityUser<string>
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (username is null) throw new ArgumentNullException(nameof(username));

            return usernameType switch
            {
                ContactType.Email => userManager.GetByEmailAsync(username),
                ContactType.PhoneNumber => userManager.GetByPhoneNumberAsync(username),
                _ => throw new InvalidOperationException("Invalid username type.")
            };
        }

        public static async Task<string> GenerateConfirmUsernameCodeAsync<TUser>(this UserManager<TUser> userManager, ContactType usernameType, TUser user)
            where TUser : IdentityUser<string>
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (user is null) throw new ArgumentNullException(nameof(user));

            return usernameType switch
            {
                ContactType.Email => await userManager.GenerateChangeEmailTokenAsync(user, user.Email!),
                ContactType.PhoneNumber => await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber!),
                _ => throw new InvalidOperationException("Invalid username type.")
            };
        }

        public static async Task<IdentityResult> ConfirmUsernameAsync<TUser>(this UserManager<TUser> userManager, ContactType usernameType, TUser user, string code)
            where TUser : IdentityUser<string>
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (code is null) throw new ArgumentNullException(nameof(code));

            return usernameType switch
            {
                ContactType.Email => await userManager.ChangeEmailAsync(user, user.Email!, code),
                ContactType.PhoneNumber => await userManager.ChangePhoneNumberAsync(user, user.PhoneNumber!, code),
                _ => throw new InvalidOperationException("Invalid username type.")
            };
        }

        public static async Task<string> GenerateChangeUsernameCodeAsync<TUser>(this UserManager<TUser> userManager, ContactType newUsernameType, TUser user, string newUsername)
            where TUser : IdentityUser<string>
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (newUsername is null) throw new ArgumentNullException(nameof(newUsername));

            return newUsernameType switch
            {
                ContactType.Email => await userManager.GenerateChangeEmailTokenAsync(user, newUsername),
                ContactType.PhoneNumber => await userManager.GenerateChangePhoneNumberTokenAsync(user, newUsername),
                _ => throw new InvalidOperationException("Invalid username type.")
            };
        }

        public static async Task<IdentityResult> ChangeUsernameAsync<TUser>(this UserManager<TUser> userManager, ContactType newUsernameType, TUser user, string newUsername, string code)
            where TUser : IdentityUser<string>
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (newUsername is null) throw new ArgumentNullException(nameof(newUsername));
            if (code is null) throw new ArgumentNullException(nameof(code));

            return newUsernameType switch
            {
                ContactType.Email => await userManager.ChangeEmailAsync(user, newUsername, code),
                ContactType.PhoneNumber => await userManager.ChangePhoneNumberAsync(user, newUsername, code),
                _ => throw new InvalidOperationException("Invalid username type.")
            };
        }

        public static async Task<bool> VerifyResetPasswordAsync<TUser>(this UserManager<TUser> userManager, TUser user, string token)
            where TUser : IdentityUser<string>
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (user is null) throw new ArgumentNullException(nameof(user));
            return await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token);
        }

        public static string GetMessage(this IEnumerable<IdentityError> errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            return $"Identity operation failed:{string.Concat(errors.Select(x => $"{Environment.NewLine} -- {x.Code}: {x.Description}"))}";
        }
    }
}
