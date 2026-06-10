using Core.Persistence.Repositories;
using Core.Security.Entities;

namespace CryptoCodeControlAutomation.Application.Services.Repositories
{
    public interface IRoleRepository : IAsyncRepository<Role>
    {
    }
}