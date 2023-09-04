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
using NextSolution.Core.Mappers;
using NextSolution.Core.Models.Users.Accounts;

namespace NextSolution.Core.Services
{
    public interface IUserService : IDisposable, IAsyncDisposable
    {
        // Accounts
        Task<UserWithSessionModel> RefreshSessionAsync(RefreshSessionForm form);
        Task ResetPasswordAsync(ResetPasswordForm form);
        Task SendPasswordResetTokenAsync(SendPasswordResetTokenForm form);
        Task SendUsernameTokenAsync(SendUsernameTokenForm form);
        Task<UserWithSessionModel> SignInAsync(SignInForm form);
        Task<UserWithSessionModel> SignInWithAsync(SignUpWithForm form);
        Task SignOutAsync(SignOutForm form);
        Task SignUpAsync(SignUpForm form);
        Task VerifyUsernameAsync(VerifyUsernameForm form);

        // Users
        Task<UserPageModel> GetUsersAsync(UserSearch search, int pageNumber, int pageSize);
    }

    public class UserService : IUserService
    {
        private readonly IServiceProvider _validatorProvider;
        private readonly IUserMapper _modelMapper;
        private readonly IMediator _mediator;
        private readonly IViewRenderer _viewRenderer;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(IServiceProvider validatorProvider, IUserMapper modelMapper, IMediator mediator, IViewRenderer viewRenderer, IEmailSender emailSender, ISmsSender smsSender, IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _validatorProvider = validatorProvider ?? throw new ArgumentNullException(nameof(validatorProvider));
            _modelMapper = modelMapper ?? throw new ArgumentNullException(nameof(modelMapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task SignUpAsync(SignUpForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<SignUpFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            // Throws an exception if the username already exists.
            if (form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            } != null) throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.Title)}' is already in use.");

            var user = new User();
            user.FirstName = form.FirstName;
            user.LastName = form.LastName;
            user.Email = form.UsernameType == ContactType.EmailAddress ? form.Username : user.Email;
            user.PhoneNumber = form.UsernameType == ContactType.PhoneNumber ? form.Username : user.PhoneNumber;
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

            var formValidator = _validatorProvider.GetRequiredService<SignInFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            if (user == null)
                throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.Title)}' does not exist.");

            if (!await _userRepository.CheckPasswordAsync(user, form.Password, cancellationToken))
                throw new BadRequestException(nameof(form.Password), $"'{nameof(form.Password).Humanize(LetterCasing.Title)}' is not correct.");

            var session = await _userRepository.GenerateSessionAsync(user, cancellationToken);
            await _userRepository.AddSessionAsync(user, session, cancellationToken);

            var model = await _modelMapper.MapAsync(user, session);
            await _mediator.Publish(new UserSignedIn(user));
            return model;
        }

        public async Task<UserWithSessionModel> SignInWithAsync(SignUpWithForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<SignUpWithFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            var isNewUser = user == null;

            if (user == null)
            {
                user = new User();
                user.FirstName = form.FirstName;
                user.LastName = form.LastName;
                user.Email = form.UsernameType == ContactType.EmailAddress ? form.Username : user.Email;
                user.PhoneNumber = form.UsernameType == ContactType.PhoneNumber ? form.Username : user.PhoneNumber;
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

            var model = await _modelMapper.MapAsync(user, session);

            await _mediator.Publish(isNewUser ? new UserSignedUp(user) : new UserSignedIn(user), cancellationToken);
            return model;
        }

        public async Task SignOutAsync(SignOutForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<SignOutFormValidator>();
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

            var formValidator = _validatorProvider.GetRequiredService<RefreshSessionFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = await _userRepository.GetByRefreshTokenAsync(form.RefreshToken, cancellationToken);

            if (user == null) throw new BadRequestException(nameof(form.RefreshToken), $"'{nameof(form.RefreshToken).Humanize(LetterCasing.Title)}' is not valid.");

            await _userRepository.RemoveSessionAsync(user, form.RefreshToken, cancellationToken);

            var session = await _userRepository.GenerateSessionAsync(user, cancellationToken);
            await _userRepository.AddSessionAsync(user, session, cancellationToken);

            var model = await _modelMapper.MapAsync(user, session);
            return model;
        }

        public async Task SendUsernameTokenAsync(SendUsernameTokenForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<SendUsernameTokenFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.Title)}' does not exist.");


            if (form.UsernameType == ContactType.EmailAddress)
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

            var formValidator = _validatorProvider.GetRequiredService<VerifyUsernameFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.LowerCase)}' is not found.");

            try
            {
                if (form.UsernameType == ContactType.EmailAddress)
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

            var formValidator = _validatorProvider.GetRequiredService<SendPasswordResetTokenFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
                ContactType.PhoneNumber => await _userRepository.GetByPhoneNumberAsync(form.Username, cancellationToken),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"'{form.UsernameType.Humanize(LetterCasing.Title)}' does not exist.");

            if (form.UsernameType == ContactType.EmailAddress)
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

            var formValidator = _validatorProvider.GetRequiredService<ResetPasswordFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.GetByEmailAsync(form.Username, cancellationToken),
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

        public async Task<UserPageModel> GetUsersAsync(UserSearch search, int pageNumber, int pageSize)
        {
            if (search == null) throw new ArgumentNullException(nameof(search));
            var predicate = search.Build();

            var page = (await _userRepository.GetManyAsync(pageNumber, pageSize, predicate: predicate));
            var pageModel = await _modelMapper.MapAsync(page);
            return pageModel;
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
}