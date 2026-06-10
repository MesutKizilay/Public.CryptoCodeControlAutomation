using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Logging;
using Core.Security.Entities;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;

namespace CryptoCodeControlAutomation.Application.Features.Users.Commands.Delete
{
    public class DeleteUserCommand : IRequest, ISecuredRequest, ILoggableRequest
    {
        public int Id { get; set; }
        public string[] Roles => new string[] { "Supervisor" };
        public string LogMessage => $"Kullanıcı silindi. Id:{Id}";


        public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
        {
            private readonly IUserRepository _userRepository;

            public DeleteUserCommandHandler(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }

            public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.Get(u => u.UserId == request.Id, cancellationToken: cancellationToken);
                user.IsEnabled = false;
                await _userRepository.Update(user, cancellationToken);
            }
        }
    }
}