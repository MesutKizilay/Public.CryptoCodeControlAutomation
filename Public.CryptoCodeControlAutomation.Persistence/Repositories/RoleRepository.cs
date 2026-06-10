using Core.Persistence.Repositories;
using Core.Security.Entities;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Persistence.Contexts;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class RoleRepository : EfRepositoryBase<Role, CryptoContext>, IRoleRepository
    {
        public RoleRepository(CryptoContext context) : base(context)
        {
            
        }
    }
}