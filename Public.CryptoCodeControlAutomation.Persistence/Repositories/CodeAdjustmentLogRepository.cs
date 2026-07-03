using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Persistence.Contexts;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class CodeAdjustmentLogRepository : EfRepositoryBase<CodeAdjustmentLog, CryptoContext>, ICodeAdjustmentLogRepository
    {
        public CodeAdjustmentLogRepository(CryptoContext context) : base(context)
        {
        }
    }
}
