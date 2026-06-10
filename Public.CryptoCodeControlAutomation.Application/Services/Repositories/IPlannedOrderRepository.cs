using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;

namespace CryptoCodeControlAutomation.Application.Services.Repositories
{
    public interface IPlannedOrderRepository : IAsyncRepository<PlannedOrder>
    {
    }
}
