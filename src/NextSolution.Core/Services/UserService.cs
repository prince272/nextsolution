using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Constants;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Utilities;
using NextSolution.Core.Repositories;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using AutoMapper;
using NextSolution.Core.Extensions.ViewRenderer;
using NextSolution.Core.Extensions.EmailSender;
using NextSolution.Core.Extensions.SmsSender;
using System.Security.Claims;
using MediatR;
using NextSolution.Core.Events.Users;
using NextSolution.Core.Models.Users;
using NextSolution.Core.Models.Users.Accounts;
using NextSolution.Core.Extensions.FileStorage;
using NextSolution.Core.Models.Medias;
using Microsoft.Extensions.Options;
using NextSolution.Core.Extensions.Identity;
using System.Reflection;
using NextSolution.Core.Models;

namespace NextSolution.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileStorage _fileStorage;
        private readonly IMapper _mapper;
        private readonly IModelBuilder _modelBuilder;
        private readonly IMediator _mediator;
        private readonly IViewRenderer _viewRenderer;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IUserContext _userContext;
        private readonly IOptions<MediaServiceOptions> _mediaServiceOptions;
        private readonly IMediaRepository _mediaRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(IServiceProvider serviceProvider, IFileStorage fileStorage, IMapper mapper, IModelBuilder modelBuilder,
                           IMediator mediator, IViewRenderer viewRenderer, IEmailSender emailSender,
                           ISmsSender smsSender, IUserContext userContext, IOptions<MediaServiceOptions> mediaServiceOptions, IMediaRepository mediaRepository, IUserRepository userRepository,
                           IRoleRepository roleRepository)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _modelBuilder = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _mediaServiceOptions = mediaServiceOptions ?? throw new ArgumentNullException(nameof(mediaServiceOptions));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task SignUpAsync(SignUpForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<SignUpFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            // Throws an exception if the username already exists.
            if (form.UsernameType switch
            {
                ContactType.Email => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            } != null) throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.Title)}' is already taken.");

            var user = new User();
            user.FirstName = form.FirstName;
            user.LastName = form.LastName;
            user.Email = form.UsernameType == ContactType.Email ? form.Username : user.Email;
            user.EmailRequired = form.UsernameType == ContactType.Email;
            user.PhoneNumber = form.UsernameType == ContactType.PhoneNumber ? form.Username : user.PhoneNumber;
            user.PhoneNumberRequired = form.UsernameType == ContactType.PhoneNumber;
            user.Active = true;
            user.LastActiveAt = DateTimeOffset.UtcNow;
            await _userRepository.GenerateUserNameAsync(user, cancellationToken);
            await _userRepository.CreateAsync(user, form.Password, cancellationToken);

            foreach (var roleName in Roles.All)
            {
                if (await _roleRepository.GetByNameAsync(roleName, cancellationToken) == null)
                    await _roleRepository.CreateAsync(new Role(roleName), cancellationToken);
            }

            var totalUsers = await _userRepository.CountAsync(cancellationToken);

            // Assign roles to the specified user based on the total user count.
            // If there is only one user, grant both Admin and Member roles.
            // Otherwise, assign only the Member role.
            await _userRepository.AddToRolesAsync(user, (totalUsers == 1) ? new[] { Roles.Admin, Roles.Member } : new[] { Roles.Member }, cancellationToken);

            await _mediator.Publish(new UserSignedUp(user), cancellationToken);
        }

        public async Task<UserWithSessionModel> SignInAsync(SignInForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<SignInFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.Email => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            if (user == null)
                throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.Title)}' does not exist.");

            if (!await _userRepository.CheckPasswordAsync(user, form.Password, cancellationToken))
                throw new BadRequestException(nameof(form.Password), $"'{nameof(form.Password).Humanize(LetterCasing.Title)}' is not correct.");

            var session = await _userRepository.GenerateSessionAsync(user, cancellationToken);
            await _userRepository.AddSessionAsync(user, session, cancellationToken);

            var model = await _modelBuilder.BuildAsync(user, session);
            await _mediator.Publish(new UserSignedIn(user));
            return model;
        }

        public async Task<UserWithSessionModel> SignInWithAsync(SignUpWithForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<SignUpWithFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.Email => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            var isNewUser = user == null;

            if (user == null)
            {
                user = new User();
                user.FirstName = form.FirstName;
                user.LastName = form.LastName;
                user.Email = form.UsernameType == ContactType.Email ? form.Username : user.Email;
                user.EmailRequired = form.UsernameType == ContactType.Email;
                user.PhoneNumber = form.UsernameType == ContactType.PhoneNumber ? form.Username : user.PhoneNumber;
                user.PhoneNumberRequired = form.UsernameType == ContactType.PhoneNumber;
                user.Active = true;
                user.LastActiveAt = DateTimeOffset.UtcNow;
                await _userRepository.GenerateUserNameAsync(user, cancellationToken);
                await _userRepository.CreateAsync(user, cancellationToken);

                foreach (var roleName in Roles.All)
                {
                    if (await _roleRepository.GetByNameAsync(roleName, cancellationToken) == null)
                        await _roleRepository.CreateAsync(new Role(roleName), cancellationToken);
                }

                var totalUsers = await _userRepository.CountAsync(cancellationToken);

                // Assign roles to the specified user based on the total user count.
                // If there is only one user, grant both Admin and Member roles.
                // Otherwise, assign only the Member role.
                await _userRepository.AddToRolesAsync(user, (totalUsers == 1) ? new[] { Roles.Admin, Roles.Member } : new[] { Roles.Member }, cancellationToken);
            }

            await _userRepository.RemoveLoginAsync(user, form.ProviderName, form.ProviderKey, cancellationToken);
            await _userRepository.AddLoginAsync(user, new UserLoginInfo(form.ProviderName, form.ProviderKey, form.ProviderDisplayName), cancellationToken);

            var session = await _userRepository.GenerateSessionAsync(user, cancellationToken);
            await _userRepository.AddSessionAsync(user, session, cancellationToken);

            var model = await _modelBuilder.BuildAsync(user, session);

            await _mediator.Publish(isNewUser ? new UserSignedUp(user) : new UserSignedIn(user), cancellationToken);
            return model;
        }

        public async Task SignOutAsync(SignOutForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<SignOutFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = await _userRepository.GetByRefreshTokenAsync(form.RefreshToken, cancellationToken);

            if (user == null) throw new BadRequestException(nameof(form.RefreshToken), $"'{nameof(form.RefreshToken).Humanize(LetterCasing.Title)}' is not valid.");

            await _userRepository.RemoveSessionAsync(user, form.RefreshToken, cancellationToken);

            await _mediator.Publish(new UserSignedOut(user), cancellationToken);
        }

        public async Task<UserWithSessionModel> RefreshSessionAsync(RefreshSessionForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<RefreshSessionFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = await _userRepository.GetByRefreshTokenAsync(form.RefreshToken, cancellationToken);

            if (user == null) throw new BadRequestException(nameof(form.RefreshToken), $"'{nameof(form.RefreshToken).Humanize(LetterCasing.Title)}' is not valid.");

            await _userRepository.RemoveSessionAsync(user, form.RefreshToken, cancellationToken);

            var session = await _userRepository.GenerateSessionAsync(user, cancellationToken);
            await _userRepository.AddSessionAsync(user, session, cancellationToken);

            var model = await _modelBuilder.BuildAsync(user, session);
            return model;
        }

        public async Task SendUsernameTokenAsync(SendUsernameTokenForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<SendUsernameTokenFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.Email => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.Title)}' does not exist.");


            if (form.UsernameType == ContactType.Email)
            {
                var code = await _userRepository.GenerateEmailTokenAsync(user, cancellationToken);

                var message = new EmailMessage()
                {
                    Recipients = new[] { form.Username },
                    Subject = $"Verify Your {form.UsernameType.Humanize(LetterCasing.Title)}",
                    Body = await _viewRenderer.RenderAsync("/Email/VerifyUsername", (user, new VerifyUsernameForm { Username = form.Username, Code = code }), cancellationToken)
                };

                await _emailSender.SendAsync(account: "Notification", message, cancellationToken);
            }
            else if (form.UsernameType == ContactType.PhoneNumber)
            {
                var code = await _userRepository.GeneratePasswordResetTokenAsync(user, cancellationToken);
                var message = await _viewRenderer.RenderAsync("/Text/VerifyUsername", (user, new VerifyUsernameForm { Username = form.Username, Code = code }), cancellationToken);
                await _smsSender.SendAsync(form.Username, message, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"The value '{form.UsernameType}' of enum '{nameof(ContactType)}' is not supported.");
            }
        }

        public async Task VerifyUsernameAsync(VerifyUsernameForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<VerifyUsernameFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.Email => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.LowerCase)}' does not exist.");

            try
            {
                if (form.UsernameType == ContactType.Email)
                    await _userRepository.VerifyEmailAsync(user, form.Code, cancellationToken);

                else if (form.UsernameType == ContactType.PhoneNumber)
                    await _userRepository.VerifyPhoneNumberTokenAsync(user, form.Code, cancellationToken);
                else
                    throw new InvalidOperationException($"The value '{form.UsernameType}' of enum '{nameof(ContactType)}' is not supported.");
            }
            catch (InvalidOperationException ex)
            {
                throw new BadRequestException(nameof(form.Code), $"'{nameof(form.Code).Humanize(LetterCasing.Title)}' is not valid.", innerException: ex);
            }
        }

        public async Task SendPasswordResetTokenAsync(SendPasswordResetTokenForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<SendPasswordResetTokenFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.Email => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.Title)}' does not exist.");

            if (form.UsernameType == ContactType.Email)
            {
                var code = await _userRepository.GeneratePasswordResetTokenAsync(user, cancellationToken);

                var message = new EmailMessage()
                {
                    Recipients = new[] { form.Username },
                    Subject = $"Reset Your Password",
                    Body = await _viewRenderer.RenderAsync("/Email/ResetPassword", (user, new ResetPasswordForm { Username = form.Username, Code = code }), cancellationToken)
                };

                await _emailSender.SendAsync(account: "Notification", message, cancellationToken);
            }
            else if (form.UsernameType == ContactType.PhoneNumber)
            {
                var code = await _userRepository.GeneratePasswordResetTokenAsync(user, cancellationToken);
                var message = await _viewRenderer.RenderAsync("/Text/ResetPassword", (user, new ResetPasswordForm { Username = form.Username, Code = code }), cancellationToken);
                await _smsSender.SendAsync(form.Username, message, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"The value '{form.UsernameType}' of enum '{nameof(ContactType)}' is not supported.");
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<ResetPasswordFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.Email => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.Title)}' does not exist.");

            try
            {
                await _userRepository.ResetPasswordAsync(user, form.Password, form.Code, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                throw new BadRequestException(nameof(form.Code), $"'{nameof(form.Code).Humanize(LetterCasing.Title)}' is not valid.", innerException: ex);
            }
        }

        public async Task ChangePasswordAsync(ChangePasswordForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<ChangePasswordFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _userContext.UserId != null ? await _userRepository.GetByIdAsync(_userContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            await _userRepository.ChangePasswordAsync(currentUser, form.CurrentPassword, form.NewPassword, cancellationToken);
        }

        public async Task<UserPageModel> GetUsersAsync(UserSearchParams searchParams, int pageNumber, int pageSize)
        {
            if (searchParams == null) throw new ArgumentNullException(nameof(searchParams));
            var predicate = searchParams.Build();

            var page = (await _userRepository.GetManyAsync(pageNumber, pageSize, predicate: predicate, cancellationToken: cancellationToken));
            var pageModel = await _modelBuilder.BuildAsync(page, cancellationToken);
            return pageModel;
        }

        public async Task<UserModel> GetCurrentUserAsync()
        {
            var currentUser = _userContext.UserId != null ? await _userRepository.GetByIdAsync(_userContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();
            var currentUserModel = await _modelBuilder.BuildAsync(currentUser, cancellationToken);
            return currentUserModel;
        }

        public async Task EditCurrentUserAsync(EditUserForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<EditUserFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _userContext.UserId != null ? await _userRepository.GetByIdAsync(_userContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            if (await _userRepository.IsUserNameTakenAsync(currentUser, form.UserName, cancellationToken))
                throw new BadRequestException(nameof(form.UserName), $"'{(nameof(form.UserName).ToLower()).Humanize(LetterCasing.Title)}' is already taken.");

            if (string.IsNullOrWhiteSpace(form.Email) && currentUser.EmailRequired)
                throw new BadRequestException(nameof(form.Email), $"'{(nameof(form.Email).ToLower()).Humanize(LetterCasing.Title)}' must not be empty.");

            if (string.IsNullOrWhiteSpace(form.PhoneNumber) && currentUser.PhoneNumberRequired)
                throw new BadRequestException(nameof(form.PhoneNumber), $"'{(nameof(form.PhoneNumber).ToLower()).Humanize(LetterCasing.Title)}' must not be empty.");

            await _userRepository.SetEmailAsync(currentUser, form.Email);
            await _userRepository.SetPhoneNumberAsync(currentUser, form.PhoneNumber);

            _mapper.Map(form, currentUser);
            await _userRepository.UpdateAsync(currentUser);
        }

        public async Task UploadCurrentUserAvatarAsync(UploadMediaChunkForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            form.Type = MediaType.Image;
            var formValidator = _serviceProvider.GetRequiredService<UploadMediaChunkFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());
            
            var status = await _fileStorage.WriteAsync(form.Path, form.Content, form.Size, form.Offset, cancellationToken);

            if (status == FileChunkStatus.Started)
            {
                var avatar = new Media
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Type = form.Type ?? _mediaServiceOptions.Value.GetMediaType(form.Path),
                    ContentType = form.ContentType ?? _mediaServiceOptions.Value.GetContentType(form.Path),
                    Path = form.Path,
                    Name = form.Name,
                    Size = form.Size
                };
                await _mediaRepository.CreateAsync(avatar, cancellationToken);

                form.Id = avatar.Id;
            }
            else if (status == FileChunkStatus.Completed)
            {
                var avatar = await _mediaRepository.GetByIdAsync(form.Id, cancellationToken);

                if (avatar != null)
                {
                    avatar.UpdatedAt = DateTimeOffset.UtcNow;
                    await _mediaRepository.UpdateAsync(avatar, cancellationToken);
                }
            }
        }

        public async Task<MediaModel> GetCurrentUserAvatarAsync()
        {
            var currentUser = _userContext.UserId != null ? await _userRepository.GetByIdAsync(_userContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var avatar = currentUser.AvatarId != null ? await _mediaRepository.GetByIdAsync(currentUser.AvatarId.Value, cancellationToken) : null;
            if (avatar == null) throw new NotFoundException();

            var content = await _fileStorage.ReadAsync(avatar.Path, cancellationToken);
            if (content == null) throw new NotFoundException();

            var model = _mapper.Map<MediaModel>(avatar);
            model.Content = content;
            model.Url = await _fileStorage.GetPublicUrlAsync(avatar.Path);
            return model;
        }


        private readonly CancellationToken cancellationToken = default;
        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // myResource.Dispose();
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!disposed)
            {
                disposed = true;
                await DisposeAsync(true);
                GC.SuppressFinalize(this);
            }
        }

        protected ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                //  await myResource.DisposeAsync();
                cancellationToken.ThrowIfCancellationRequested();
            }

            return ValueTask.CompletedTask;
        }
    }

    public interface IUserService : IDisposable, IAsyncDisposable
    {
        // Account
        Task<UserWithSessionModel> RefreshSessionAsync(RefreshSessionForm form);
        Task ChangePasswordAsync(ChangePasswordForm form);
        Task ResetPasswordAsync(ResetPasswordForm form);
        Task SendPasswordResetTokenAsync(SendPasswordResetTokenForm form);
        Task SendUsernameTokenAsync(SendUsernameTokenForm form);
        Task<UserWithSessionModel> SignInAsync(SignInForm form);
        Task<UserWithSessionModel> SignInWithAsync(SignUpWithForm form);
        Task SignOutAsync(SignOutForm form);
        Task SignUpAsync(SignUpForm form);
        Task VerifyUsernameAsync(VerifyUsernameForm form);

        // User
        Task<UserPageModel> GetUsersAsync(UserSearchParams searchParams, int pageNumber, int pageSize);
        Task<UserModel> GetCurrentUserAsync();
        Task EditCurrentUserAsync(EditUserForm form);
        Task UploadCurrentUserAvatarAsync(UploadMediaChunkForm form);
        Task<MediaModel> GetCurrentUserAvatarAsync();
    }
}