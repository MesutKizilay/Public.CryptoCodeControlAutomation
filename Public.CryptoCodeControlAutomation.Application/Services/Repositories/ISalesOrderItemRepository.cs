using CryptoCodeControlAutomation.Domain.Entities;
using Core.Persistence.Repositories;

namespace CryptoCodeControlAutomation.Application.Services.Repositories
{
    public interface ISalesOrderItemRepository : IAsyncRepository<SalesOrderItem>
    {
        Task Delete2(long salesOrderItemId, CancellationToken cancellationToken);
        Task Delete3(long salesOrderItemId, CancellationToken cancellationToken);
        Task Delete4(long salesOrderItemId, CancellationToken cancellationToken);
        Task<long> ImportCodesBulkInsert(long salesOrderItemId, string filePath, int firstRow = 0, string fieldTerminator = ",", string rowTerminator = "0x0d0a", CancellationToken cancellationToken = default);
        Task<long> ActivateAndStartPlannedOrder(SalesOrderItem salesOrderItem, string lineCode = "HAT1", CancellationToken cancellationToken = default);
    }
}
