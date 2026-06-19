using Core.Application.Rules;
using CryptoCodeControlAutomation.Application.Features.Users.Constants;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Rules
{
    public class SalesOrderItemBusinessRules : BaseBusinessRules
    {
        private readonly ISalesOrderItemRepository _salesOrderItemRepository;
        private readonly IUploadJobRepository _uploadJobRepository;
        private readonly ICodeRepository _codeRepository;

        public SalesOrderItemBusinessRules(ISalesOrderItemRepository salesOrderItemRepository, IUploadJobRepository uploadJobRepository, ICodeRepository codeRepository)
        {
            _salesOrderItemRepository = salesOrderItemRepository;
            _uploadJobRepository = uploadJobRepository;
            _codeRepository = codeRepository;
        }

        public async Task SalesOrderNoAndSalesItemNoShouldBeUnique(string salesOrderNo, string salesItemNo)
        {
            bool isExists = await _salesOrderItemRepository.Any(s => s.SalesItemNo == salesItemNo && s.SalesOrderNo == salesOrderNo && s.Status != SalesOrderItemStatus.Cancelled);

            if (isExists)
                await ThrowBusinessException(SalesOrderItemMessages.SalesOrderItemMessagesAlreadyExist);
        }

        public async Task WasUploadJobImported(long salesOrderItemId)
        {
            bool isExists = await _uploadJobRepository.Any(u => u.SalesOrderItemId == salesOrderItemId && u.Status == UploadJobStatus.Importing);

            if (isExists)
                await ThrowBusinessException(SalesOrderItemMessages.CanNotDeleteSalesOrderItemsWhenImporting);
        }

        public async Task ActiveSalesOrderItemShouldNotExist(long salesOrderItemId)
        {
            bool isExists = await _salesOrderItemRepository.Any(s => s.SalesOrderItemId != salesOrderItemId && s.Status == SalesOrderItemStatus.Active);

            if (isExists)
                await ThrowBusinessException(SalesOrderItemMessages.ActiveSalesOrderItemAlreadyExists);
        }

        public async Task SapPlannedUnitQtyShouldNotExceedImportedCodeCount(long salesOrderItemId, int sapPlannedUnitQty)
        {
            List<UploadJob> uploadJobs = await _uploadJobRepository.GetList(
                predicate: u => u.SalesOrderItemId == salesOrderItemId && u.Status == UploadJobStatus.Done
            );

            if (uploadJobs.Count == 0)
                return;

            int insertedRows = uploadJobs.Sum(u => u.InsertedRows ?? 0);
            int requiredCodeCount = (int)Math.Ceiling(sapPlannedUnitQty * 1.05m);

            if (requiredCodeCount > insertedRows)
                await ThrowBusinessException($"{SalesOrderItemMessages.SapPlannedUnitQtyExceedsImportedCodeCount} Planlanan: {sapPlannedUnitQty}, Yüklenen Kod: {insertedRows}, Beklenen: {requiredCodeCount}.");
        }

        public async Task AreThereProducedCodes(long salesOrderItemId)
        {
            bool isExists = await _codeRepository.Any(u => u.SalesOrderItemId == salesOrderItemId && u.Status == CodeStatus.ProducedOk);

            if (isExists)
                await ThrowBusinessException(SalesOrderItemMessages.CanNotDeleteSalesOrderItemsWhenItHasProducedCodes);
        }
    }
}
