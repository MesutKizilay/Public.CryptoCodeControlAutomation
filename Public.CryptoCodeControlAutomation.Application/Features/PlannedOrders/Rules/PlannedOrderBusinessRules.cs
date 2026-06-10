using Core.Application.Rules;
using CryptoCodeControlAutomation.Application.Features.PlannedOrders.Constans;
using CryptoCodeControlAutomation.Domain.Entities;

namespace CryptoCodeControlAutomation.Application.Features.PlannedOrders.Rules
{
    public class PlannedOrderBusinessRules : BaseBusinessRules
    {
        public async Task PlannedOrderWasNotFound(PlannedOrder plannedOrder)
        {
            if (plannedOrder == null)
                await ThrowBusinessException(PlannedOrderMessages.PlannedOrderNotFound);
        }
    }
}
