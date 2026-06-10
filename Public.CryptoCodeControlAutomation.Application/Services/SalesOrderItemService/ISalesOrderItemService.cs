using System.Threading.Tasks;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.ValidateSalesOrderItem;

namespace CryptoCodeControlAutomation.Application.Services.Validations
{
    public interface ISalesOrderItemService
    {
        Task<ValidateSalesOrderItemDto> ValidateSalesOrderItem(string salesOrderNo, string salesItemNo, CancellationToken cancellationToken = default);
    }
}