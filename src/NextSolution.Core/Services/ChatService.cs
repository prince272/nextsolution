using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Constants;
using NextSolution.Core.Entities;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models;
using NextSolution.Core.Models.Chats;
using NextSolution.Core.Models.Users;
using NextSolution.Core.Models.Users.Accounts;
using NextSolution.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NextSolution.Core.Services
{
    public class ChatService : IChatService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;
        private readonly IModelBuilder _modelBuilder;

        public ChatService(IServiceProvider serviceProvider, IChatRepository chatRepository, IUserRepository userRepository, IUserContext userContext, IModelBuilder modelBuilder)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _modelBuilder = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder));
        }

        public async Task<ChatModel> CreateAsync(CreateChatForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<CreateChatFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _userContext.UserId != null ? await _userRepository.GetByIdAsync(_userContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var chat = new Chat
            {
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                UserId = currentUser.Id,
                Title = form.Title
            };

            await _chatRepository.CreateAsync(chat, cancellationToken);
            var chatModel = await _modelBuilder.BuildAsync(chat, cancellationToken);
            return chatModel;
        }

        public async Task<ChatModel> EditAsync(EditChatForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<EditChatFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _userContext.UserId != null ? await _userRepository.GetByIdAsync(_userContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var isCurrentUserAdmin = await _userRepository.IsInRoleAsync(currentUser, Roles.Admin);

            var chat = await _chatRepository.GetByIdAsync(form.Id, cancellationToken);
            if (chat == null) throw new NotFoundException();

            if (!isCurrentUserAdmin && chat.UserId != currentUser.Id)
                throw new ForbiddenException();

            chat.Title = form.Title;
            chat.UpdatedAt = DateTimeOffset.UtcNow;
            await _chatRepository.UpdateAsync(chat, cancellationToken);

            var chatModel = await _modelBuilder.BuildAsync(chat, cancellationToken);
            return chatModel;
        }

        public async Task DeleteAsync(DeleteChatForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<DeleteChatFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _userContext.UserId != null ? await _userRepository.GetByIdAsync(_userContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var isCurrentUserAdmin = await _userRepository.IsInRoleAsync(currentUser, Roles.Admin);

            var chat = await _chatRepository.GetByIdAsync(form.Id, cancellationToken);
            if (chat == null) throw new NotFoundException();

            if (!isCurrentUserAdmin && chat.UserId != currentUser.Id)
                throw new ForbiddenException();

            await _chatRepository.DeleteAsync(chat, cancellationToken);
        }

        public async Task<ChatModel> GetAsync(GetChatForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _serviceProvider.GetRequiredService<GetChatFormValidator>();
            var formValidationResult = await formValidator.ValidateAsync(form, cancellationToken);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var currentUser = _userContext.UserId != null ? await _userRepository.GetByIdAsync(_userContext.UserId.Value, cancellationToken) : null;
            if (currentUser == null) throw new UnauthorizedException();

            var isCurrentUserAdmin = await _userRepository.IsInRoleAsync(currentUser, Roles.Admin);

            var chat = await _chatRepository.GetByIdAsync(form.Id, cancellationToken);
            if (chat == null) throw new NotFoundException();

            if (!isCurrentUserAdmin && chat.UserId != currentUser.Id)
                throw new ForbiddenException();

            var model = await _modelBuilder.BuildAsync(chat, cancellationToken);
            return model;
        }

        public async Task<ChatPageModel> GetManyAsync(ChatSearchParams searchParams, long offset, int limit)
        {
            if (searchParams == null) throw new ArgumentNullException(nameof(searchParams));
            var predicate = searchParams.Build();

            var page = (await _chatRepository.GetManyAsync(offset, limit, predicate: predicate, cancellationToken: cancellationToken));
            var pageModel = await _modelBuilder.BuildAsync(page, cancellationToken);
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

    public interface IChatService : IDisposable, IAsyncDisposable
    {
        Task<ChatModel> CreateAsync(CreateChatForm form);

        Task<ChatModel> EditAsync(EditChatForm form);

        Task DeleteAsync(DeleteChatForm form);

        Task<ChatModel> GetAsync(GetChatForm form);

        Task<ChatPageModel> GetManyAsync(ChatSearchParams searchParams, long offset, int limit);
    }
}
