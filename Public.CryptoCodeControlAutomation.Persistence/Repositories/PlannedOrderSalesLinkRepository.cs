using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Persistence.Contexts;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class PlannedOrderSalesLinkRepository : EfRepositoryBase<PlannedOrderSalesLink, CryptoContext>, IPlannedOrderSalesLinkRepository
    {
        public PlannedOrderSalesLinkRepository(CryptoContext context) : base(context)
        {
        }
    }
}
