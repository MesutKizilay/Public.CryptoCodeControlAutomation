using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using Core.Persistence.Repositories;

namespace CryptoCodeControlAutomation.Application.Services.Repositories
{
    public interface ICodeRepository : IAsyncRepository<Code>
    {
        Task<int> UpdateScrapCodes(List<long> ids, CodeStatus status, CancellationToken cancellationToken = default);
        Task<int> UpdateRecoverCodes(List<long> ids, CodeStatus status, CancellationToken cancellationToken = default);
    }
}
