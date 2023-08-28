using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Entities;
using NextSolution.Core.Events.Clients;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models.Accounts;
using NextSolution.Core.Models.Clients;
using NextSolution.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Services
{
    public interface IClientService : IDisposable, IAsyncDisposable
    {
        Task ConnectAsync(ConnectClientForm form);
        Task DisconnectAsync(DisconnectClientForm form);
        Task DisconnectAsync();
    }

    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;
        private readonly IUserContext _userContext;
        private readonly IServiceProvider _validatorProvider;

        public ClientService(IClientRepository clientRepository, IUserRepository userRepository, IMediator mediator, IUserContext userContext, IServiceProvider validatorProvider)
        {
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _validatorProvider = validatorProvider ?? throw new ArgumentNullException(nameof(validatorProvider));
        }

        private async Task<bool> IsUserOnlineAsync(long userId)
        {
            return await _clientRepository.AnyAsync(_ => _.UserId == userId, cancellationToken);
        }

        public async Task ConnectAsync(ConnectClientForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<ConnectClientForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var client = GenerateClient(form.ConnectionId);
            var userId = client.UserId;

            await _clientRepository.CreateAsync(client, cancellationToken);
            await _mediator.Publish(new ClientConnected(client), cancellationToken);

            if (userId.HasValue && await IsUserOnlineAsync(userId.Value))
            {
                await _mediator.Publish(new UserConnected((await _userRepository.FindByIdAsync(userId.Value, cancellationToken))!, client), cancellationToken);
            }
        }

        public async Task DisconnectAsync(DisconnectClientForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var formValidator = _validatorProvider.GetRequiredService<DisconnectClientForm.Validator>();
            var formValidationResult = await formValidator.ValidateAsync(form);

            if (!formValidationResult.IsValid)
                throw new BadRequestException(formValidationResult.ToDictionary());

            var client = await _clientRepository.FindAsync(predicate: _ => _.ConnectionId == form.ConnectionId, cancellationToken: cancellationToken);
            if (client == null) return;

            await _clientRepository.DeleteAsync(client, cancellationToken);

            var userId = client.UserId;

            if (userId.HasValue && !(await IsUserOnlineAsync(userId.Value)))
            {
                await _mediator.Publish(new UserDisconnected((await _userRepository.FindByIdAsync(userId.Value, cancellationToken))!, client), cancellationToken);
            }

            await _mediator.Publish(new ClientDisconnected(client), cancellationToken);
        }

        public async Task DisconnectAsync()
        {
            var connectionIds = await _clientRepository.FindAllAsync(selector: _ => _.ConnectionId, cancellationToken: cancellationToken);
            foreach (var connectionId in connectionIds)
                await DisconnectAsync(new DisconnectClientForm { ConnectionId = connectionId });
        }

        protected Client GenerateClient(string connectionId)
        {
            return new Client
            {
                ConnectionId = connectionId,
                IpAddress = _userContext.IpAddress,
                DeviceId = _userContext.DeviceId,
                UserId = _userContext.UserId,
                UserAgent = _userContext.UserAgent
            };
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