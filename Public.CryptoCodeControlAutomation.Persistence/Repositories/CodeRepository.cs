using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using CryptoCodeControlAutomation.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class CodeRepository : EfRepositoryBase<Code, CryptoContext>, ICodeRepository
    {
        public CodeRepository(CryptoContext context) : base(context)
        {

        }

        public async Task<int> UpdateScrapCodes(List<long> ids, CodeStatus status, CancellationToken cancellationToken = default)
        {
            return await Context.Codes.Where(c => ids.Contains(c.CodeId))
                                      .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Status, status)
                                                                            .SetProperty(c => c.UpdatedAt, DateTime.Now),
                                                                             cancellationToken);
        }

        public async Task<int> UpdateRecoverCodes(List<long> ids, CodeStatus status, CancellationToken cancellationToken = default)
        {
            return await Context.Codes.Where(c => ids.Contains(c.CodeId))
                                      .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Status, status)
                                                                            .SetProperty(c => c.UpdatedAt, DateTime.Now)
                                                                            .SetProperty(c => c.RecoverAt, DateTime.Now)
                                                                            .SetProperty(c => c.ShiftDate, c => c.AllocatedAt),
                                                                            cancellationToken);
        }
    }
}
