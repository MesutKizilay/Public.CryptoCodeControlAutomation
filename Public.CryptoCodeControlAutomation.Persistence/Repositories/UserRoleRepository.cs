using Core.Persistence.Repositories;
using Core.Security.Entities;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Persistence.Contexts;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class UserRoleRepository : EfRepositoryBase<UserRole, CryptoContext>, IUserRoleRepository
    {
        public UserRoleRepository(CryptoContext context) : base(context)
        {

        }
    }
}