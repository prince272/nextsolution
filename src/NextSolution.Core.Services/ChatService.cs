using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models;
using NextSolution.Core.Models.Chats;
using NextSolution.Core.Repositories;
using NextSolution.Core.Utilities;
using System;
using System.Reflection;
using OpenAI = OpenAI_API.Chat;

namespace NextSolution.Core.Services
{

    public class ChatService : IChatService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OpenAI.IChatEndpoint _chatEndpoint;
        private readonly IChatRepository _chatRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClientContext _clientContext;
        private readonly IModelBuilder _modelBuilder;

        public ChatService(IServiceProvider serviceProvider, OpenAI.IChatEndpoint chatEndpoint, IChatRepository chatRepository, IChatMessageRepository chatMessageRepository, IUserRepository userRepository, IClientContext clientContext, IModelBuilder modelBuilder)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _chatEndpoint = chatEndpoint ?? throw new ArgumentNullException(nameof(chatEndpoint));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _chatMessageRepository = chatMessageRepository ?? throw new ArgumentNullException(nameof(chatMessageRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _clientContext = clientContext ?? throw new ArgumentNullException(nameof(clientContext));
            _modelBuilder = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder));
        }

        public async IAsyncEnumerable<ChatStreamModel> AddChatAsync(AddChatForm form)
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
                Title = "New Chat"
            }) : (await _chatRepository.GetByIdAsync(form.ChatId.Value, cancellationToken)) ?? throw new BadRequestException(nameof(form.ChatId), $"Chat '{form.ChatId}' does not exist.");

            if (currentUserIsInMemeber && chat.UserId != currentUser.Id) throw new ForbiddenException();

            chat.UpdatedAt = DateTimeOffset.UtcNow;
            await _chatRepository.UpdateAsync(chat);

            var previousMessage = form.MessageId == null ? null : 
                await _chatMessageRepository.GetByIdAsync(form.MessageId.Value) ?? 
                throw new BadRequestException(nameof(form.MessageId), $"Previous chat message '{form.MessageId}' does not exist.");

            var messages = await _chatMessageRepository.GetManyAsync(predicate: _ => _.ChatId == chat.Id, cancellationToken: cancellationToken);

            var currentMessage = (await _chatMessageRepository.CreateAsync(new ChatMessage
            {
                ParentId = previousMessage?.Id,
                ChatId = chat.Id,
                Role = ChatMessageRole.User,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Content = form.Prompt
            }, cancellationToken));

            var nextMessage = (await _chatMessageRepository.CreateAsync(new ChatMessage
            {
                ParentId = currentMessage.Id,
                ChatId = chat.Id,
                Role = ChatMessageRole.Assistant,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Content = string.Empty
            }, cancellationToken));

            var chatResponse = _chatEndpoint.StreamChatEnumerableAsync(new OpenAI.ChatRequest
            {
                Messages = messages.Append(currentMessage).ToList()
                .Select(_ => new OpenAI.ChatMessage { Content = _.Content, Role = OpenAI.ChatMessageRole.FromString(_.Role.ToString().ToLower()) }).ToList()
            });

            await foreach (var chatResult in chatResponse)
            {
                nextMessage.Content += chatResult.Choices.FirstOrDefault()?.Delta?.Content;

                var model = new ChatStreamModel
                {
                    ChatId = chat.Id,
                    ChatTitle = chat.Title,
                    User = await _modelBuilder.BuildAsync(currentMessage),
                    Assistant = await _modelBuilder.BuildAsync(nextMessage),
                };

                yield return model;
            }

            nextMessage.UpdatedAt = DateTimeOffset.UtcNow;
            await _chatMessageRepository.UpdateAsync(nextMessage);
        }

        public async Task<ChatModel> EditChatAsync(long chatId, EditChatForm form)
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

            var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
            if (chat == null) throw new BadRequestException(nameof(chatId), $"Chat '{chatId}' does not exist.");

            if (currentUserIsInMemeber && chat.UserId != currentUser.Id) throw new ForbiddenException();

            chat.Title = form.Title;
            chat.UpdatedAt = DateTimeOffset.UtcNow;
            await _chatRepository.UpdateAsync(chat, cancellationToken);

            var chatModel = await _modelBuilder.BuildAsync(chat, cancellationToken);
            return chatModel;
        }

        public async Task DeleteChatAsync(long chatId)
        {
            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
            if (chat == null) throw new BadRequestException(nameof(chatId), $"Chat '{chatId}' does not exist.");

            if (currentUserIsInMemeber && chat.UserId != currentUser.Id) throw new ForbiddenException();

            // TODO: Wrap inside a transaction
            await _chatMessageRepository.DeleteManyAsync(_ => _.ChatId == chat.Id);
            await _chatRepository.DeleteAsync(chat, cancellationToken);
        }

        public async Task DeleteAllChatsAsync()
        {
            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            // TODO: Wrap inside a transaction
            await _chatMessageRepository.DeleteManyAsync(_ => _.Chat != null && _.Chat.UserId == currentUser.Id);
            await _chatRepository.DeleteManyAsync(_ => _.UserId == currentUser.Id, cancellationToken);
        }

        public async Task<ChatModel> GetChatAsync(long chatId)
        {
            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
            if (chat == null) throw new NotFoundException($"Chat '{chatId}' does not exist.");

            if (currentUserIsInMemeber && chat.UserId != currentUser.Id) throw new ForbiddenException();

            var model = await _modelBuilder.BuildAsync(chat, cancellationToken);
            return model;
        }

        public async Task<ChatPageModel> GetChatsAsync(ChatCriteria criteria, long offset, int limit)
        {
            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            if (criteria == null) throw new ArgumentNullException(nameof(criteria));
            var predicate = criteria.Build();
            predicate = predicate.And(chat => !currentUserIsInMemeber || chat.UserId == currentUser.Id);

            var page = (await _chatRepository.GetManyAsync(offset, limit, predicate: predicate, cancellationToken: cancellationToken));
            var pageModel = await _modelBuilder.BuildAsync(page, cancellationToken);
            return pageModel;
        }

        public async Task<ChatMessagePageModel> GetMessagesAsync(long chatId, ChatMessageCriteria criteria, long offset, int limit)
        {
            var currentUser = _clientContext.UserId != null ? await _userRepository.GetByIdAsync(_clientContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var currentUserIsInAdmin = await _userRepository.IsInRoleAsync(currentUser, Role.Admin, cancellationToken);
            var currentUserIsInMemeber = await _userRepository.IsInRoleAsync(currentUser, Role.Member, cancellationToken);
            if (!currentUserIsInAdmin || !currentUserIsInMemeber) throw new ForbiddenException();

            var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
            if (chat == null) throw new NotFoundException($"Chat '{chatId}' does not exist.");

            if (currentUserIsInMemeber && chat.UserId != currentUser.Id) throw new ForbiddenException();

            if (criteria == null) throw new ArgumentNullException(nameof(criteria));

            var predicate = criteria.Build();
            predicate = predicate.And(_ => _.ChatId == chatId);

            var page = (await _chatMessageRepository.GetManyAsync(offset, limit, predicate: predicate, cancellationToken: cancellationToken));
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
        IAsyncEnumerable<ChatStreamModel> AddChatAsync(AddChatForm form);

        Task<ChatModel> EditChatAsync(long chatId, EditChatForm form);

        Task DeleteChatAsync(long chatId);

        Task DeleteAllChatsAsync();

        Task<ChatModel> GetChatAsync(long chatId);

        Task<ChatPageModel> GetChatsAsync(ChatCriteria criteria, long offset, int limit);

        Task<ChatMessagePageModel> GetMessagesAsync(long chatId, ChatMessageCriteria criteria, long offset, int limit);
    }
}