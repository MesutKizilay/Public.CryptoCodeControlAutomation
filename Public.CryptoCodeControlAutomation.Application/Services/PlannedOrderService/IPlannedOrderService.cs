using System.Threading.Tasks;
using CryptoCodeControlAutomation.Application.Features.PlannedOrders.Queries.GetPlannedOrderByPalletNumber;

namespace CryptoCodeControlAutomation.Application.Services.Validations
{
    public interface IPlannedOrderService
    {
        Task<GetPlannedOrderByPalletNumberDto> GetPlannedOrderByPalletNumber(string tbNo, CancellationToken cancellationToken = default);
    }
}
