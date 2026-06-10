using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Persistence.Contexts;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class PlannedOrderRepository : EfRepositoryBase<PlannedOrder, CryptoContext>, IPlannedOrderRepository
    {
        public PlannedOrderRepository(CryptoContext context) : base(context)
        {
        }
    }
}
