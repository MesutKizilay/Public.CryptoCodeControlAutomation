using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Persistence.Contexts;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using Core.Persistence.Repositories;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class UploadJobRepository : EfRepositoryBase<UploadJob, CryptoContext>, IUploadJobRepository
    {
        public UploadJobRepository(CryptoContext context) : base(context)
        {
        }
    }
}
