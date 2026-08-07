using System.Linq;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.RecoverCodes
{
    public class RecoverCodesCommand : IRequest<RecoverCodesResponse>
    {
        public List<long> CodeIds { get; set; } = new();

        public class RecoverCodesCommandHandler : IRequestHandler<RecoverCodesCommand, RecoverCodesResponse>
        {
            private readonly ICodeRepository _codeRepository;
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;

            public RecoverCodesCommandHandler(ICodeRepository codeRepository, ISalesOrderItemRepository salesOrderItemRepository)
            {
                _codeRepository = codeRepository;
                _salesOrderItemRepository = salesOrderItemRepository;
            }

            public async Task<RecoverCodesResponse> Handle(RecoverCodesCommand request, CancellationToken cancellationToken)
            {
                if (request.CodeIds.Count == 0)
                    throw new BusinessException("Kod listesi boþ olamaz.");

                var firstCodeId = request.CodeIds[0];
                var code = await _codeRepository.Get(
                    predicate: c => c.CodeId == firstCodeId,
                    cancellationToken: cancellationToken);

                if (code == null)
                    throw new BusinessException("Kod bulunamadý.");

                var salesOrderItem = await _salesOrderItemRepository.Get(
                    predicate: s => s.SalesOrderItemId == code.SalesOrderItemId,
                    cancellationToken: cancellationToken);

                if (salesOrderItem == null)
                    throw new BusinessException("Satýþ sipariþi bulunamadý.");

                var updated = await _codeRepository.UpdateRecoverCodes(
                    request.CodeIds,
                    CodeStatus.ProducedOk,
                    salesOrderItem.ShelfLifeValue,
                    (byte)salesOrderItem.ShelfLifeUnit,
                    cancellationToken);

                return new RecoverCodesResponse { UpdatedCount = request.CodeIds.Count };
            }
        }
    }
}
