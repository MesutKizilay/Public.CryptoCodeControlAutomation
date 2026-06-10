using AutoMapper;
using Core.Application.Pipelines.Authorization;
using Core.Security.Entities;
using MediatR;
using CryptoCodeControlAutomation.Application.Features.Users.Rules;
using CryptoCodeControlAutomation.Application.Services.Repositories;

namespace CryptoCodeControlAutomation.Application.Features.Users.Commands.Create
{
    public class CreateUserCommand : IRequest, ISecuredRequest
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? FullName { get; set; }
        public bool IsEnabled { get; set; }
        public bool RequiresLdapAuthentication { get; set; } = true;

        public virtual ICollection<UserRole> UserRoles { get; set; } = null!;

        public string[] Roles => new string[] { "Supervisor" };
        public string LogMessage => $"Yeni kullanıcı oluşturuldu. Id:{UserId}";


        public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
        {
            private readonly IUserRepository _userRepository;
            private readonly IMapper _mapper;
            private readonly UserBusinessRules _userBusinessRules;

            public CreateUserCommandHandler(IMapper mapper, IUserRepository userRepository, UserBusinessRules userBusinessRule)
            {
                _mapper = mapper;
                _userRepository = userRepository;
                _userBusinessRules = userBusinessRule;
            }

            public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
            {
                await _userBusinessRules.UsernameShouldBeUnique(request.Username, request.UserId);

                var user = _mapper.Map<User>(request);
                user.CreatedUtc = DateTime.UtcNow;
                await _userRepository.Add(user, cancellationToken);
                request.UserId = user.UserId;
            }
        }
    }
}
