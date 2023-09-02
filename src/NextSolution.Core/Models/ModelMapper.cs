using AutoMapper;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models.Users;
using NextSolution.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NextSolution.Core.Models
{
    public interface IModelMapper
    {
        Task<UserModel> MapAsync(User user, CancellationToken cancellationToken = default);

        Task<UserWithSessionModel> MapAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default);
    }

    public class ModelMapper : IModelMapper
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;
        private readonly IClientRepository _clientRepository;

        public ModelMapper(IMapper mapper, IUserRepository userRepository, IUserContext userContext, IClientRepository clientRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        public async Task<UserWithSessionModel> MapAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default)
        {
            var model = _mapper.Map(session, _mapper.Map<UserWithSessionModel>(user));
            model.Online = await _clientRepository.AnyAsync(_ => _.UserId == user.Id, cancellationToken);
            model.Roles = (await _userRepository.GetRolesAsync(user, cancellationToken)).Select(_ => _.Camelize()).ToArray();
            return model;
        }

        public async Task<UserModel> MapAsync(User user, CancellationToken cancellationToken = default)
        {
            var model = _mapper.Map<UserModel>(user);
            model.Online = await _clientRepository.AnyAsync(_ => _.UserId == user.Id, cancellationToken);
            model.Roles = (await _userRepository.GetRolesAsync(user, cancellationToken)).Select(_ => _.Camelize()).ToArray();
            return model;
        }
    }
}
