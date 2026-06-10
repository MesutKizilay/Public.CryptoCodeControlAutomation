using Core.Security.Entities;
using Core.Security.JWT;
using Microsoft.EntityFrameworkCore;
using CryptoCodeControlAutomation.Application.Services.Repositories;

namespace CryptoCodeControlAutomation.Application.Services.AuthServices
{
    public class AuthManager : IAuthService
    {
        private readonly ITokenHelper _tokenHelper;
        private readonly IUserRoleRepository _userRoleRepository;

        public AuthManager(ITokenHelper tokenHelper, IUserRoleRepository userRoleRepository)
        {
            _tokenHelper = tokenHelper;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<AccessToken> CreateAccessToken(User user)
        {
            //IList<OperationClaim> operationClaims = await _userOperationClaimRepository
            //    .Query()
            //    .AsNoTracking()
            //    .Where(p => p.UserId == user.Id)
            //    .Select(p => new OperationClaim { Id = p.OperationClaimId, Name = p.OperationClaim.Name })
            //    .ToListAsync();

            IList<Role> operationClaims = (await _userRoleRepository.GetList(predicate: c => c.UserId == user.UserId, include: c => c.Include(c => c.Role)))
                                                                             .Select(c => new Role
                                                                             {
                                                                                 RoleId = c.Role.RoleId,
                                                                                 Name = c.Role.Name
                                                                             }).ToList();

            AccessToken accessToken = _tokenHelper.CreateToken(user, operationClaims);
            return accessToken;
        }
    }
}