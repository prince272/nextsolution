using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Constants;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Utilities;
using NextSolution.Core.Models.Accounts;
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

namespace NextSolution.Core.Services
{
    public class AccountService
    {
        private readonly IServiceProvider _validatorProvider;
        private readonly IMapper _mapper;
        private readonly IViewRenderer _viewRenderer;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public AccountService(IServiceProvider serviceProvider, IMapper mapper, IViewRenderer viewRenderer, IEmailSender emailSender, ISmsSender smsSender, IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _validatorProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task CreateAsync(CreateAccountForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<CreateAccountForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.FindByEmailAsync(form.Username),
                ContactType.PhoneNumber => await _userRepository.FindByPhoneNumberAsync(form.Username),
                _ => null
            };

            if (user != null)
                throw new BadRequestException(nameof(form.Username), $"The {form.UsernameType.Humanize(LetterCasing.LowerCase)} is already in use.");

            user = new User();
            user.FirstName = form.FirstName;
            user.LastName = form.LastName;
            user.Email = form.UsernameType == ContactType.EmailAddress ? form.Username : user.Email;
            user.PhoneNumber = form.UsernameType == ContactType.PhoneNumber ? form.Username : user.PhoneNumber;
            user.Active = true;
            user.ActiveAt = DateTimeOffset.UtcNow;
            await _userRepository.GenerateUserNameAsync(user);
            await _userRepository.CreateAsync(user, form.Password);
            await AddUserToQualifiedRolesAsync(user);
        }

        public async Task<UserSessionModel> GenerateSessionAsync(GenerateSessionForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<GenerateSessionForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.FindByEmailAsync(form.Username),
                ContactType.PhoneNumber => await _userRepository.FindByPhoneNumberAsync(form.Username),
                _ => null
            };

            if (user == null)
                throw new BadRequestException(nameof(form.Username), $"The {form.UsernameType.Humanize(LetterCasing.LowerCase)} is not found.");


            if (!user.EmailConfirmed && !user.PhoneNumberConfirmed) 
                throw new BadRequestException(nameof(form.Username), $"The {form.UsernameType.Humanize(LetterCasing.LowerCase)} is not verified.");


            if (!await _userRepository.CheckPasswordAsync(user, form.Password))
                throw new BadRequestException(nameof(form.Password), $"The {nameof(form.Password).Humanize(LetterCasing.LowerCase)} is not correct.");


            var session = await _userRepository.GenerateSessionAsync(user);
            await _userRepository.AddSessionAsync(user, session);

            var model = _mapper.Map(user, _mapper.Map<UserSessionModel>(session));
            model.Roles = await _userRepository.GetRolesAsync(user);
            return model;
        }

        public async Task<UserSessionModel> RefreshSessionAsync(RefreshSessionForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<RefreshSessionForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = await _userRepository.FindByRefreshTokenAsync(form.RefreshToken);

            if (user == null) throw new BadRequestException(nameof(form.RefreshToken), $"The {nameof(form.RefreshToken).Humanize(LetterCasing.LowerCase)} is not valid.");

            await _userRepository.RemoveSessionAsync(user, form.RefreshToken);

            var session = await _userRepository.GenerateSessionAsync(user);
            await _userRepository.AddSessionAsync(user, session);

            var model = _mapper.Map(user, _mapper.Map<UserSessionModel>(session));
            model.Roles = await _userRepository.GetRolesAsync(user);
            return model;
        }

        public async Task RevokeSessionAsync(RevokeSessionForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<RevokeSessionForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = await _userRepository.FindByRefreshTokenAsync(form.RefreshToken);

            if (user == null) throw new BadRequestException(nameof(form.RefreshToken), $"The {nameof(form.RefreshToken).Humanize(LetterCasing.LowerCase)} is not valid.");

            await _userRepository.RemoveSessionAsync(user, form.RefreshToken);
        }

        public async Task SendUsernameTokenAsync(SendUsernameTokenForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<SendUsernameTokenForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.FindByEmailAsync(form.Username),
                ContactType.PhoneNumber => await _userRepository.FindByPhoneNumberAsync(form.Username),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"The {form.UsernameType.Humanize(LetterCasing.LowerCase)} is not found.");


            if (form.UsernameType == ContactType.EmailAddress)
            {
                var code = await _userRepository.GenerateEmailTokenAsync(user);

                var message = new EmailMessage()
                {
                    Recipients = new[] { form.Username },
                    Subject = $"Verify Your {form.UsernameType.Humanize(LetterCasing.Title)}",
                    Body = await _viewRenderer.RenderAsync("/Email/VerifyUsername", (user, new VerifyUsernameForm { Username = form.Username, Code = code }))
                };

                await _emailSender.SendAsync(account: "Notification", message);
            }
            else if (form.UsernameType == ContactType.PhoneNumber)
            {
                var code = await _userRepository.GeneratePasswordResetTokenAsync(user);
                var message = await _viewRenderer.RenderAsync("/Text/VerifyUsername", (user, new VerifyUsernameForm { Username = form.Username, Code = code }));
                await _smsSender.SendAsync(form.Username, message);
            }
            else
            {
                throw new InvalidOperationException($"The value '{form.UsernameType}' of enum '{nameof(ContactType)}' is not supported.");
            }
        }

        public async Task VerifyUsernameAsync(VerifyUsernameForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<VerifyUsernameForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.FindByEmailAsync(form.Username),
                ContactType.PhoneNumber => await _userRepository.FindByPhoneNumberAsync(form.Username),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"The {form.UsernameType.Humanize(LetterCasing.LowerCase)} is not found.");

            try
            {
                if (form.UsernameType == ContactType.EmailAddress)
                    await _userRepository.VerifyEmailAsync(user, form.Code);

                else if (form.UsernameType == ContactType.PhoneNumber)
                    await _userRepository.VerifyPhoneNumberTokenAsync(user, form.Code);
                else
                    throw new InvalidOperationException($"The value '{form.UsernameType}' of enum '{nameof(ContactType)}' is not supported.");
            }
            catch (InvalidOperationException ex)
            {
                throw new BadRequestException(nameof(form.Code), $"The verification {nameof(form.Code).Humanize(LetterCasing.LowerCase)} is not valid.", innerException: ex);
            }
        }

        public async Task SendPasswordResetTokenAsync(SendPasswordResetTokenForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<SendPasswordResetTokenForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.FindByEmailAsync(form.Username),
                ContactType.PhoneNumber => await _userRepository.FindByPhoneNumberAsync(form.Username),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"The {form.UsernameType.Humanize(LetterCasing.LowerCase)} is not found.");

            if (form.UsernameType == ContactType.EmailAddress)
            {
                var code = await _userRepository.GeneratePasswordResetTokenAsync(user);

                var message = new EmailMessage()
                {
                    Recipients = new[] { form.Username },
                    Subject = $"Reset Your Password",
                    Body = await _viewRenderer.RenderAsync("/Email/ResetPassword", (user, new ResetPasswordForm { Username = form.Username, Code = code }))
                };

                await _emailSender.SendAsync(account: "Notification", message);
            }
            else if (form.UsernameType == ContactType.PhoneNumber)
            {
                var code = await _userRepository.GeneratePasswordResetTokenAsync(user);
                var message = await _viewRenderer.RenderAsync("/Text/ResetPassword", (user, new ResetPasswordForm { Username = form.Username, Code = code }));
                await _smsSender.SendAsync(form.Username, message);
            }
            else
            {
                throw new InvalidOperationException($"The value '{form.UsernameType}' of enum '{nameof(ContactType)}' is not supported.");
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<ResetPasswordForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var user = form.UsernameType switch
            {
                ContactType.EmailAddress => await _userRepository.FindByEmailAsync(form.Username),
                ContactType.PhoneNumber => await _userRepository.FindByPhoneNumberAsync(form.Username),
                _ => null
            };

            if (user == null) throw new BadRequestException(nameof(form.Username), $"The {form.UsernameType.Humanize(LetterCasing.LowerCase)} is not found.");

            try
            {
                await _userRepository.ResetPasswordAsync(user, form.Password, form.Code);
            }
            catch (InvalidOperationException ex)
            {
                throw new BadRequestException(nameof(form.Code), $"The verification {nameof(form.Code).Humanize(LetterCasing.LowerCase)} is not valid.", innerException: ex);
            }
        }

        private async Task AddUserToQualifiedRolesAsync(User user)
        {
            foreach (var roleName in Roles.All)
            {
                if (await _roleRepository.FindByNameAsync(roleName) is null)
                    await _roleRepository.CreateAsync(new Role(roleName));
            }

            // TODO: Determine which role to add the user to.
        }
    }
}