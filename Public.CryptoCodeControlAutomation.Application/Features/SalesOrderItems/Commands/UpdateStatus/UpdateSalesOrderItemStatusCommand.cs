using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Rules;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;
using MediatR;
using Microsoft.Data.SqlClient;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.UpdateStatus
{
    public class UpdateSalesOrderItemStatusCommand : IRequest
    {
        public long SalesOrderItemId { get; set; }

        public class UpdateSalesOrderItemStatusCommandHandler : IRequestHandler<UpdateSalesOrderItemStatusCommand>
        {
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly SalesOrderItemBusinessRules _salesOrderItemBusinessRules;


            public UpdateSalesOrderItemStatusCommandHandler(ISalesOrderItemRepository salesOrderItemRepository, SalesOrderItemBusinessRules salesOrderItemBusinessRules)
            {
                _salesOrderItemRepository = salesOrderItemRepository;
                _salesOrderItemBusinessRules = salesOrderItemBusinessRules;
            }

            public async Task Handle(UpdateSalesOrderItemStatusCommand request, CancellationToken cancellationToken)
            {
                await _salesOrderItemBusinessRules.WasUploadJobImported(request.SalesOrderItemId);                
                await _salesOrderItemBusinessRules.ActiveSalesOrderItemShouldNotExist(request.SalesOrderItemId);

                var salesOrderItem = await _salesOrderItemRepository.Get(predicate: item => item.SalesOrderItemId == request.SalesOrderItemId,
                                                                         withDeleted: false,
                                                                         cancellationToken: cancellationToken);

                if (salesOrderItem == null)
                    throw new BusinessException("Satış siparişi bulunamadı.");

                if (salesOrderItem.Status != SalesOrderItemStatus.Passive)
                    throw new BusinessException("Yalnızca pasif satış siparişleri aktifleştirilebilir.");


                try
                {
                    await _salesOrderItemRepository.ActivateAndStartPlannedOrder(salesOrderItem, cancellationToken: cancellationToken);
                }
                catch (SqlException exception) when (exception.Number is >= 61000 and < 62000)
                {
                    throw new BusinessException(exception.Message, exception);
                }
            }
        }
    }
}
