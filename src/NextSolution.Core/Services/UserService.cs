using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Extensions.FileStorage;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models.Accounts;
using NextSolution.Core.Models.Conversations;
using NextSolution.Core.Models.Users;
using NextSolution.Core.Repositories;
using NextSolution.Core.Shared;
using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Services
{
    public interface IUserService : IDisposable, IAsyncDisposable
    {
        Task<ProfilePageModel> GetProfilesAsync(int pageNumber, int pageSize, ProfileFilter filter);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IServiceProvider _validatorProvider;

        public UserService(
            ILogger<UserService> logger,
            IMapper mapper,
            IUserContext userContext,
            IUserRepository userRepository,
            IClientRepository clientRepository,
            IServiceProvider validatorProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _validatorProvider = validatorProvider ?? throw new ArgumentNullException(nameof(validatorProvider));
        }

        public async Task<ProfilePageModel> GetProfilesAsync(int pageNumber, int pageSize, ProfileFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            var predicate = BuildProfilePredicate(filter);

            var page = (await _userRepository.GetManyAsync(pageNumber, pageSize, predicate: predicate));
            var pageModel = await GetProfilePageModelAsync(page);
            return pageModel;

        }

        private Expression<Func<User, bool>> BuildProfilePredicate(ProfileFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            var predicate = PredicateBuilder.True<User>();
            return predicate;
        }

        private async Task<ProfilePageModel> GetProfilePageModelAsync(IPageable<User> users)
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            var pageModel = await GetProfileListModelAsync<ProfilePageModel>(users);
            pageModel.PageNumber = users.PageNumber;
            pageModel.PageSize = users.PageSize;
            pageModel.TotalPages = users.TotalPages;
            pageModel.TotalItems = users.TotalItems;
            return pageModel;
        }

        private Task<ProfileListModel> GetProfileListModelAsync(IEnumerable<User> users)
        {
            return GetProfileListModelAsync<ProfileListModel>(users);
        }

        private async Task<TListModel> GetProfileListModelAsync<TListModel>(IEnumerable<User> users)
            where TListModel : ProfileListModel
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            var items = new List<ProfileModel>();

            foreach (var user in users)
            {
                var profileModel = _mapper.Map<ProfileModel>(user);
                profileModel.Online = await _clientRepository.IsUserOnlineAsync(user.Id);
                items.Add(profileModel);
            }

            var profileListModel = Activator.CreateInstance<TListModel>();
            profileListModel.Items = items;
            return profileListModel;
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
