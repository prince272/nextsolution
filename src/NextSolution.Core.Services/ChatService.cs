using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models;
using NextSolution.Core.Models.Chats;
using NextSolution.Core.Repositories;
using NextSolution.Core.Utilities;

namespace NextSolution.Core.Services
{
    public class ChatService : IChatService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatRepository _chatRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClientContext _clientContext;
        private readonly IModelBuilder _modelBuilder;

        public ChatService(IServiceProvider serviceProvider, IChatRepository chatRepository, IChatMessageRepository chatMessageRepository, IUserRepository userRepository, IClientContext clientContext, IModelBuilder modelBuilder)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _chatMessageRepository = chatMessageRepository ?? throw new ArgumentNullException(nameof(chatMessageRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _clientContext = clientContext ?? throw new ArgumentNullException(nameof(clientContext));
            _modelBuilder = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder));
        }

        public async IAsyncEnumerable<ChatCompletionModel> CompleteChatAsync(ChatCompletionForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<ChatCompletionFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            var chat = form.ChatId == null ? await _chatRepository.CreateAsync(new Chat
            {
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                UserId = currentUser.Id,
                Title = form.Prompt // Generate from AI
            }) : (await _chatRepository.GetByIdAsync(form.ChatId.Value, cancellationToken)) ?? throw new BadRequestException(nameof(form.ChatId), $"Chat '{form.ChatId}' does not exist.");

            if (currentUserIsInMemeber && chat.UserId != currentUser.Id) throw new ForbiddenException();

            ChatMessage? previousMessage = null;

            if (form.MessageId != null)
            {
                previousMessage = await _chatMessageRepository.GetByIdAsync(form.MessageId.Value, cancellationToken);
                if (previousMessage == null) throw new BadRequestException(nameof(form.MessageId), $"Chat message '{form.MessageId}' does not exist.");

                if (previousMessage.ChatId != chat.Id || previousMessage.Role != ChatMessage.Roles.User)
                    throw new ForbiddenException();
            }

            var currentMessage = await _chatMessageRepository.CreateAsync(new ChatMessage
            {
                PreviousId = previousMessage?.Id,
                ChatId = chat.Id,
                Role = ChatMessage.Roles.User,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Content = form.Prompt
            });

            var messages = (await _chatMessageRepository.GetAncestorsAsync(currentMessage)).Append(currentMessage).ToArray();


            yield return new ChatCompletionModel();
        }

        public async Task<ChatModel> EditChatAsync(EditChatForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<EditChatFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            var chat = await _chatRepository.GetByIdAsync(form.Id, cancellationToken);
            if (chat == null) throw new BadRequestException(nameof(form.Id), $"Chat '{form.Id}' does not exist.");

            if (currentUserIsInMemeber && chat.UserId != currentUser.Id) throw new ForbiddenException();

            chat.Title = form.Title;
            chat.UpdatedAt = DateTimeOffset.UtcNow;
            await _chatRepository.UpdateAsync(chat, cancellationToken);

            var chatModel = await _modelBuilder.BuildAsync(chat, cancellationToken);
            return chatModel;
        }

        public async Task DeleteChatAsync(DeleteChatForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<DeleteChatFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            var chat = await _chatRepository.GetByIdAsync(form.Id, cancellationToken);
            if (chat == null) throw new BadRequestException(nameof(form.Id), $"Chat '{form.Id}' does not exist.");

            if (currentUserIsInMemeber && chat.UserId != currentUser.Id) throw new ForbiddenException();

            // TODO: Wrap inside a transaction
            await _chatMessageRepository.DeleteManyAsync(_ => _.ChatId == chat.Id);
            await _chatRepository.DeleteAsync(chat, cancellationToken);
        }

        public async Task<ChatModel> GetChatAsync(GetChatForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<GetChatFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            var chat = await _chatRepository.GetByIdAsync(form.Id, cancellationToken);
            if (chat == null) throw new BadRequestException(nameof(form.Id), $"Chat '{form.Id}' does not exist.");

            if (currentUserIsInMemeber && chat.UserId != currentUser.Id) throw new ForbiddenException();

            var model = await _modelBuilder.BuildAsync(chat, cancellationToken);
            return model;
        }

        public async Task<ChatPageModel> GetChatsAsync(ChatSearchCriteria searchCriteria, long offset, int limit)
        {
            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            if (searchCriteria == null) throw new ArgumentNullException(nameof(searchCriteria));
            var predicate = searchCriteria.Build();
            predicate = predicate.And(chat => !currentUserIsInMemeber || chat.UserId == currentUser.Id);

            var page = (await _chatRepository.GetManyAsync(offset, limit, predicate: predicate, cancellationToken: cancellationToken));
            var pageModel = await _modelBuilder.BuildAsync(page, cancellationToken);
            return pageModel;
        }


        // source: https://alistairevans.co.uk/2019/10/24/net-asynchronous-disposal-tips-for-implementing-iasyncdisposable-on-your-own-types/
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
                cancellationToken.ThrowIfCancellationRequested();
                //_disposableResource?.Dispose();
                //(_asyncDisposableResource as IDisposable)?.Dispose();
                //_disposableResource = null;
                //_asyncDisposableResource = null;
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
                cancellationToken.ThrowIfCancellationRequested();
                //await _disposableResource?.DisposeAsync();
                //await (_asyncDisposableResource as IDisposable)?.DisposeAsync();
                //_disposableResource = null;
                //_asyncDisposableResource = null;
            }

            return ValueTask.CompletedTask;
        }
    }

    public interface IChatService : IDisposable, IAsyncDisposable
    {
        IAsyncEnumerable<ChatCompletionModel> CompleteChatAsync(ChatCompletionForm form);

        Task<ChatModel> EditChatAsync(EditChatForm form);

        Task DeleteChatAsync(DeleteChatForm form);

        Task<ChatModel> GetChatAsync(GetChatForm form);

        Task<ChatPageModel> GetChatsAsync(ChatSearchCriteria searchCriteria, long offset, int limit);
    }
}
