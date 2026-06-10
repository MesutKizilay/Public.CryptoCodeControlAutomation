using Core.Persistence.Repositories;
using Core.Security.Entities;

namespace CryptoCodeControlAutomation.Application.Services.Repositories
{
    public interface IUserRoleRepository : IAsyncRepository<UserRole>
    {
    }
}