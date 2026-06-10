using Core.Application.Rules;
using Core.Security.Entities;
using CryptoCodeControlAutomation.Application.Features.Auth.Constants;
using CryptoCodeControlAutomation.Application.Services.Repositories;

namespace CryptoCodeControlAutomation.Application.Features.Auth.Rules
{
    public class AuthBusinessRules : BaseBusinessRules
    {
        private readonly IUserRepository _userRepository;

        public AuthBusinessRules(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task UserShouldBeExists(User? user)
        {
            if (user == null)
                await ThrowBusinessException(AuthMessages.UserDoesntExist);
        }

        public async Task OperatorRoleShouldBeExist(Role role)
        {
            if (role == null)
                await ThrowBusinessException(AuthMessages.OperatorRoleShouldBeExist);
        }

        public async Task UserPasswordShouldBeMatch(User user, string password)
        {
            //User? user = await _userRepository.Get(filter: u => u.Id == id);
            //if (!HashingHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            //    throw new BusinessException(AuthMessages.PasswordDontMatch);
            if (string.IsNullOrWhiteSpace(user.PasswordHash) || user.PasswordHash != password)
            {
                await ThrowBusinessException(AuthMessages.PasswordDoesntMatch);
            }
        }

        public async Task UserShouldBeActive(User user)
        {
            if (!user.IsEnabled)
            {
                await ThrowBusinessException(AuthMessages.UserIsNotActive);
            }
        }
    }
}
