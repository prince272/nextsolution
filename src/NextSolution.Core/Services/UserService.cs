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
using NextSolution.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Services
{
    public interface IUserService : IDisposable, IAsyncDisposable
    {
    }

    public class UserService : IConversationService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IServiceProvider _validatorProvider;

        public UserService(
            ILogger<UserService> logger,
            IMapper mapper,
            IUserContext userContext,
            IUserRepository userRepository,
            IServiceProvider validatorProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _validatorProvider = validatorProvider ?? throw new ArgumentNullException(nameof(validatorProvider));
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
