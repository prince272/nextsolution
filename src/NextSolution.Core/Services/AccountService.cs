using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Constants;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Helpers;
using NextSolution.Core.Models.Accounts;
using NextSolution.Core.Repositories;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Services
{
    public class AccountService
    {
        private readonly IServiceProvider _validatorProvider;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public AccountService(IServiceProvider serviceProvider, IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _validatorProvider = serviceProvider;
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task CreateAsync(CreateAccountForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<IValidator<CreateAccountForm>>();
            var formValidationResult = (await formValidator.ValidateAsync(form));

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var formUsernameFormat = ValidationHelper.CheckFormat(form.Username);

            var user = formUsernameFormat switch
            {
                TextFormat.EmailAddress => await _userRepository.FindByEmailAsync(form.Username),
                TextFormat.PhoneNumber => await _userRepository.FindByPhoneNumberAsync(form.Username),
                _ => null
            };

            if (user != null) throw new BadRequestException(new Dictionary<string, string[]> {
                { nameof(form.Username), new[] { $"'{formUsernameFormat.Humanize()}' is already in use." } }
            });

            user = new User();
            user.FirstName = form.FirstName;
            user.LastName = form.LastName;
            user.Email = formUsernameFormat == TextFormat.EmailAddress ? form.Username : user.Email;
            user.PhoneNumber = formUsernameFormat == TextFormat.PhoneNumber ? form.Username : user.PhoneNumber;
            await _userRepository.GenerateUserNameAsync(user);
            await _userRepository.CreateAsync(user, form.Password);
            await AddUserToQualifiedRolesAsync(user);
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