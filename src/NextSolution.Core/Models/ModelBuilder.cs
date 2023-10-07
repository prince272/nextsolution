using AutoMapper;
using Humanizer;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.FileStorage;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models.Chats;
using NextSolution.Core.Models.Users;
using NextSolution.Core.Repositories;
using NextSolution.Core.Utilities;

namespace NextSolution.Core.Models
{
    public interface IModelBuilder
    {
        // User
        Task<UserWithSessionModel> BuildAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default);

        Task<UserModel> BuildAsync(User user, CancellationToken cancellationToken = default);

        Task<UserPageModel> BuildAsync(IPageable<User> users, CancellationToken cancellationToken = default);

        // Chat
        Task<ChatModel> BuildAsync(Chat chat, CancellationToken cancellationToken = default);

        Task<ChatPageModel> BuildAsync(IPageable<Chat> chats, CancellationToken cancellationToken = default);

        Task<ChatMessageModel> BuildAsync(ChatMessage message, CancellationToken cancellationToken = default);

        Task<ChatMessagePageModel> BuildAsync(IPageable<ChatMessage> messages, CancellationToken cancellationToken = default);
    }

    public class ModelBuilder : IModelBuilder
    {
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IClientRepository _clientRepository;

        public ModelBuilder(IMapper mapper, IFileStorage fileStorage, IUserRepository userRepository, IMediaRepository mediaRepository, IClientRepository clientRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        public async Task<UserWithSessionModel> BuildAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (session == null) throw new ArgumentNullException(nameof(session));

            var model = _mapper.Map(session, _mapper.Map<UserWithSessionModel>(user));
            model.Online = await _clientRepository.AnyAsync(_ => _.Active && _.UserId == user.Id, cancellationToken);
            model.Roles = (await _userRepository.GetRolesAsync(user, cancellationToken)).Select(_ => _.Camelize()).ToList();
            if ((user.Avatar = user.AvatarId != null ? await _mediaRepository.GetByIdAsync(user.AvatarId.Value) : null) != null)
            {
                model.AvatarUrl = await _fileStorage.GetPublicUrlAsync(user.Avatar.Path, cancellationToken);
            }
            return model;
        }

        public async Task<UserModel> BuildAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var model = _mapper.Map<UserModel>(user);

            model.Online = await _clientRepository.AnyAsync(_ => _.Active && _.UserId == user.Id, cancellationToken);
            model.Roles = (await _userRepository.GetRolesAsync(user, cancellationToken)).Select(_ => _.Camelize()).ToList();

            if ((user.Avatar = user.AvatarId != null ? await _mediaRepository.GetByIdAsync(user.AvatarId.Value) : null) != null)
            {
                model.AvatarUrl = await _fileStorage.GetPublicUrlAsync(user.Avatar.Path, cancellationToken);
            }

            return model;
        }

        public async Task<UserPageModel> BuildAsync(IPageable<User> users, CancellationToken cancellationToken = default)
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            var items = new List<UserModel>();

            foreach (var user in users)
            {
                items.Add(await BuildAsync(user, cancellationToken));
            }

            var listModel = new UserPageModel
            {
                Items = items,
                Offset = users.Offset,
                Limit = users.Limit,
                Length = users.Length,
                Previous = users.Previous,
                Next = users.Next
            };
            return listModel;
        }

        public Task<ChatModel> BuildAsync(Chat chat, CancellationToken cancellationToken = default)
        {
            var model = _mapper.Map<ChatModel>(chat);
            return Task.FromResult(model);
        }

        public async Task<ChatPageModel> BuildAsync(IPageable<Chat> chats, CancellationToken cancellationToken = default)
        {
            if (chats == null) throw new ArgumentNullException(nameof(chats));

            var items = new List<ChatModel>();

            foreach (var chat in chats)
            {
                var chatModel = await BuildAsync(chat, cancellationToken);
                items.Add(chatModel);
            }

            var listModel = new ChatPageModel();
            listModel.Items = items;
            listModel.Offset = chats.Offset;
            listModel.Limit = chats.Limit;
            listModel.Length = chats.Length;
            listModel.Previous = chats.Previous;
            listModel.Next = chats.Next;
            return listModel;
        }

        public Task<ChatMessageModel> BuildAsync(ChatMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var model = _mapper.Map<ChatMessageModel>(message);
            return Task.FromResult(model);
        }

        public async Task<ChatMessagePageModel> BuildAsync(IPageable<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            var items = new List<ChatMessageModel>();

            foreach (var message in messages)
            {
                var messageModel = await BuildAsync(message, cancellationToken);
                items.Add(messageModel);
            }

            var listModel = new ChatMessagePageModel();
            listModel.Items = items;
            listModel.Offset = messages.Offset;
            listModel.Limit = messages.Limit;
            listModel.Length = messages.Length;
            listModel.Previous = messages.Previous;
            listModel.Next = messages.Next;
            return listModel;
        }
    }
}