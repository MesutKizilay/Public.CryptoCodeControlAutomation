using CryptoCodeControlAutomation.Domain.Entities;
using Core.Persistence.Repositories;

namespace CryptoCodeControlAutomation.Application.Services.Repositories
{
    public interface IUploadJobRepository : IAsyncRepository<UploadJob>
    {
    }
}