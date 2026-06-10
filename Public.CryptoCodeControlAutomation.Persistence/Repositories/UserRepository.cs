using Core.Persistence.Repositories;
using Core.Security.Entities;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Persistence.Contexts;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class UserRepository : EfRepositoryBase<User, CryptoContext>, IUserRepository
    {
        public UserRepository(CryptoContext context) : base(context)
        {
            
        }
    }
}