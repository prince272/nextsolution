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

namespace NextSolution.Core.Services
{
    public class AccountService
    {
        private readonly IServiceProvider _validatorProvider;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public AccountService(IServiceProvider serviceProvider, IMapper mapper, IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _validatorProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

            var formUsernameContactType = TextHelper.CheckContact(form.Username);

            var user = formUsernameContactType switch
            {
                ContactType.EmailAddress => await _userRepository.FindByEmailAsync(form.Username),
                ContactType.PhoneNumber => await _userRepository.FindByPhoneNumberAsync(form.Username),
                _ => null
            };

            if (user != null) throw new BadRequestException(new Dictionary<string, string[]> {
                { nameof(form.Username), new[] { $"'{formUsernameContactType.Humanize()}' is already in use." } }
            });

            user = new User();
            user.FirstName = form.FirstName;
            user.LastName = form.LastName;
            user.Email = formUsernameContactType == ContactType.EmailAddress ? form.Username : user.Email;
            user.PhoneNumber = formUsernameContactType == ContactType.PhoneNumber ? form.Username : user.PhoneNumber;
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

            var formUsernameContactType = TextHelper.CheckContact(form.Username);

            var user = formUsernameContactType switch
            {
                ContactType.EmailAddress => await _userRepository.FindByEmailAsync(form.Username),
                ContactType.PhoneNumber => await _userRepository.FindByPhoneNumberAsync(form.Username),
                _ => null
            };

            if (user == null) throw new BadRequestException(new Dictionary<string, string[]> {
                { nameof(form.Username), new[] { $"'{formUsernameContactType.Humanize()}' is not found." } }
            });


            if (!user.EmailConfirmed && !user.PhoneNumberConfirmed) throw new BadRequestException(new Dictionary<string, string[]> {
                { nameof(form.Username), new[] { $"'{formUsernameContactType.Humanize()}' is not verified." } }
            });


            if (!await _userRepository.CheckPasswordAsync(user, form.Password)) throw new BadRequestException(new Dictionary<string, string[]> {
                { nameof(form.Password), new[] { $"'{nameof(form.Password).Humanize()}' is not correct." } }
            });


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

            if (user == null) throw new BadRequestException(new Dictionary<string, string[]> {
                { nameof(form.RefreshToken), new[] { $"'{form.RefreshToken.Humanize()}' is not valid." } }
            });

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