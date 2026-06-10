using Core.Application.Dtos;
using Core.Application.Pipelines.Logging;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Entities;
using Core.Security.JWT;
using CryptoCodeControlAutomation.Application.Features.Auth.Rules;
using CryptoCodeControlAutomation.Application.Services.AuthServices;
using CryptoCodeControlAutomation.Application.Services.LdapService;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoCodeControlAutomation.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<LoggedResponse>, ILoggableRequest
    {
        public UserForLoginDto UserForLoginDto { get; set; }
        public string LogMessage => $"Kullanıcı girişi yapıldı. Kullanıcı:{UserForLoginDto.UserName}";

        public class LoginCommandHandler : IRequestHandler<LoginCommand, LoggedResponse>
        {
            private readonly IUserRepository _userRepository;
            private readonly AuthBusinessRules _authBusinessRules;
            private readonly IAuthService _authService;

            public LoginCommandHandler(IUserRepository userRepository, AuthBusinessRules authBusinessRules, IAuthService authService)
            {
                _userRepository = userRepository;
                _authBusinessRules = authBusinessRules;
                _authService = authService;
            }

            public async Task<LoggedResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.Get(predicate: u => u.Username == request.UserForLoginDto.UserName,
                                                     cancellationToken: cancellationToken,
                                                     withDeleted: true);

                await _authBusinessRules.UserShouldBeExists(user);
                await _authBusinessRules.UserShouldBeActive(user!);
                await _authBusinessRules.UserPasswordShouldBeMatch(user!, request.UserForLoginDto.PasswordHash);

                AccessToken createdAccessToken = await _authService.CreateAccessToken(user!);

                return new LoggedResponse
                {
                    AccessToken = createdAccessToken
                };
            }
        }
    }
}
