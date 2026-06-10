using CryptoCodeControlAutomation.Domain.Entities;
using Hangfire;

namespace CryptoCodeControlAutomation.Persistence.Services.Upload
{
    public interface IUploadJobService
    {
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        Task ProcessUpload(UploadJob uploadJob, SalesOrderItem salesOrderItem);
        //Task ProcessUpload2(UploadJob uploadJob);
    }
}