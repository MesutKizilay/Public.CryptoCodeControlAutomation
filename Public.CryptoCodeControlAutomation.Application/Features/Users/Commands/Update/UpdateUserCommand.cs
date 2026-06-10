using AutoMapper;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Logging;
using Core.Security.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CryptoCodeControlAutomation.Application.Features.Users.Rules;
using CryptoCodeControlAutomation.Application.Services.Repositories;


namespace CryptoCodeControlAutomation.Application.Features.Users.Commands.Update
{
    public class UpdateUserCommand : IRequest, ISecuredRequest, ILoggableRequest
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? FullName { get; set; }
        public bool IsEnabled { get; set; }
        public bool RequiresLdapAuthentication { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = null!;

        public string[] Roles => ["Supervisor"];
        public string LogMessage => $"Kullanıcı güncellendi. Id:{UserId}";


        public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
        {
            private readonly IUserRepository _userRepository;
            private readonly IMapper _mapper;
            private readonly UserBusinessRules _userBusinessRules;

            public UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper, UserBusinessRules userBusinessRule)
            {
                _userRepository = userRepository;
                _mapper = mapper;
                _userBusinessRules = userBusinessRule;
            }

            public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
            {
                await _userBusinessRules.UsernameShouldBeUnique(request.Username, request.UserId);

                var user = await _userRepository.Get(predicate: u => u.UserId == request.UserId,
                                                     include: u => u.Include(u => u.UserRoles));

                _mapper.Map(request, user);

                if (request.RequiresLdapAuthentication)
                {
                    user.PasswordHash = null;
                }

                await _userRepository.Update(user, cancellationToken);
            }
        }
    }
}
