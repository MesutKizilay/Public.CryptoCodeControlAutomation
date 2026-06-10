using Core.Persistence.Repositories;
using Core.Security.Entities;

namespace CryptoCodeControlAutomation.Application.Services.Repositories
{
    public interface IUserRepository : IAsyncRepository<User>
    {
    }
}