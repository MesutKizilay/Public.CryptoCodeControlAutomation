using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;
using MediatR;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Complete
{
    public class CompleteSalesOrderItemCommand : IRequest
    {
        public long SalesOrderItemId { get; set; }

        public class CompleteSalesOrderItemCommandHandler : IRequestHandler<CompleteSalesOrderItemCommand>
        {
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;

            public CompleteSalesOrderItemCommandHandler(ISalesOrderItemRepository salesOrderItemRepository)
            {
                _salesOrderItemRepository = salesOrderItemRepository;
            }

            public async Task Handle(CompleteSalesOrderItemCommand request, CancellationToken cancellationToken)
            {
                var salesOrderItem = await _salesOrderItemRepository.Get(
                    predicate: item => item.SalesOrderItemId == request.SalesOrderItemId,
                    cancellationToken: cancellationToken);

                if (salesOrderItem == null)
                    throw new BusinessException("Satış siparişi bulunamadı.");

                if (salesOrderItem.Status != SalesOrderItemStatus.Active)
                    throw new BusinessException("Yalnızca aktif satış siparişleri tamamlandı olarak işaretlenebilir.");

                await _salesOrderItemRepository.CompleteWithPlannedOrders(salesOrderItem, cancellationToken);
            }
        }
    }
}
