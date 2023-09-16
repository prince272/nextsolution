using AutoMapper;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.FileStorage;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models.Medias;
using NextSolution.Core.Models.Users;
using NextSolution.Core.Repositories;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NextSolution.Core.Mappers
{
    public interface IUserMapper
    {
        Task<UserWithSessionModel> MapAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default);

        Task<UserModel> MapAsync(User user, CancellationToken cancellationToken = default);

        Task<UserPageModel> MapAsync(IPageable<User> users, CancellationToken cancellationToken = default);

        Task<UserListModel> MapAsync(IEnumerable<User> users, CancellationToken cancellationToken = default);
    }

    public class UserMapper : IUserMapper
    {
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IUserContext _userContext;
        private readonly IClientRepository _clientRepository;

        public UserMapper(IMapper mapper, IFileStorage fileStorage, IUserRepository userRepository, IMediaRepository mediaRepository, IUserContext userContext, IClientRepository clientRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        public async Task<UserWithSessionModel> MapAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default)
        {
            var model = _mapper.Map(session, _mapper.Map<UserWithSessionModel>(user));
            model.Online = await _clientRepository.AnyAsync(_ => _.Active && _.UserId == user.Id, cancellationToken);
            model.Roles = (await _userRepository.GetRolesAsync(user, cancellationToken)).Select(_ => _.Camelize()).ToArray();
            if ((user.Avatar = (user.AvatarId != null ? await _mediaRepository.GetByIdAsync(user.AvatarId.Value) : null)) != null)
            {
                model.AvatarUrl = await _fileStorage.GetPublicUrlAsync(user.Avatar.Path, cancellationToken);
            }
            return model;
        }

        public async Task<UserModel> MapAsync(User user, CancellationToken cancellationToken = default)
        {
            var model = _mapper.Map<UserModel>(user);

            model.Online = await _clientRepository.AnyAsync(_ => _.Active && _.UserId == user.Id, cancellationToken);
            model.Roles = (await _userRepository.GetRolesAsync(user, cancellationToken)).Select(_ => _.Camelize()).ToArray();

            if ((user.Avatar = (user.AvatarId != null ? await _mediaRepository.GetByIdAsync(user.AvatarId.Value) : null)) != null)
            {
                model.AvatarUrl = await _fileStorage.GetPublicUrlAsync(user.Avatar.Path, cancellationToken);
            }

            return model;
        }

        public async Task<UserPageModel> MapAsync(IPageable<User> users, CancellationToken cancellationToken = default)
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            var pageModel = await MapAsync<UserPageModel>(users, cancellationToken);
            pageModel.PageNumber = users.PageNumber;
            pageModel.PageSize = users.PageSize;
            pageModel.TotalPages = users.TotalPages;
            pageModel.TotalItems = users.TotalItems;
            return pageModel;
        }

        public Task<UserListModel> MapAsync(IEnumerable<User> users, CancellationToken cancellationToken = default)
        {
            return MapAsync<UserListModel>(users, cancellationToken);
        }

        public async Task<TListModel> MapAsync<TListModel>(IEnumerable<User> users, CancellationToken cancellationToken = default)
            where TListModel : UserListModel
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            var items = new List<UserModel>();

            foreach (var user in users)
            {
                var userModel = await MapAsync(user, cancellationToken);
                items.Add(userModel);
            }

            var profileListModel = Activator.CreateInstance<TListModel>();
            profileListModel.Items = items;
            return profileListModel;
        }
    }
}