using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeLookup;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using Public.CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustStatus;

namespace CryptoCodeControlAutomation.Application.Services.Repositories
{
    public interface ICodeRepository : IAsyncRepository<Code>
    {
        Task<int> UpdateScrapCodes(List<long> ids, CodeStatus status, CancellationToken cancellationToken = default);
        Task<int> UpdateRecoverCodes(List<long> ids, CodeStatus status, int shelfLifeValue, byte shelfLifeUnit, CancellationToken cancellationToken = default);
        Task<List<CodeAllocationResult>> AllocateAvailableCodes(long plannedOrderId, int quantity, CancellationToken cancellationToken = default);
        Task<int> ResetAllocatedCodesToAvailable(List<long> codeIds, long plannedOrderId, CancellationToken cancellationToken = default);
        Task<int> ResetProduction(long? salesOrderItemId, long? plannedOrderId, CancellationToken cancellationToken = default);
        Task<GetCodeLookupDto?> GetCodeLookup(string code, CancellationToken cancellationToken = default);
    }
}
