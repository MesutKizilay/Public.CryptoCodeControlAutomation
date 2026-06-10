using Core.Application.Rules;
using CryptoCodeControlAutomation.Application.Features.Users.Constants;
using CryptoCodeControlAutomation.Application.Services.Repositories;

namespace CryptoCodeControlAutomation.Application.Features.Users.Rules
{
    public class UserBusinessRules : BaseBusinessRules
    {
        private readonly IUserRepository _userRepository;

        public UserBusinessRules(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task UsernameShouldBeUnique(string username, int userId)
        {
            bool isExists = await _userRepository.Any(u => u.Username == username && u.UserId != userId);

            if (isExists)
                await ThrowBusinessException(UserMessages.UsernameAlreadyExist);
        }
    }
}