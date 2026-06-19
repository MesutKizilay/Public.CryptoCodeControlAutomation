using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Rules;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Update
{
    public class UpdateSalesOrderItemCommand : IRequest
    {
        public long SalesOrderItemId { get; set; }
        public string SalesOrderNo { get; set; }
        public string SalesItemNo { get; set; }
        public string MaterialNo { get; set; }
        public string? GTIN { get; set; }
        public int SapPlannedUnitQty { get; set; }
        public int? SapCaseQty { get; set; }
        public int? ShelfLifeValue { get; set; }
        public ShelfLifeUnit? ShelfLifeUnit { get; set; }
        public DateTime? SapValidatedAt { get; set; }

        public class UpdateSalesOrderItemCommandHandler : IRequestHandler<UpdateSalesOrderItemCommand>
        {
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly SalesOrderItemBusinessRules _salesOrderItemBusinessRules;
            private readonly IMapper _mapper;

            public UpdateSalesOrderItemCommandHandler(ISalesOrderItemRepository salesOrderItemRepository, SalesOrderItemBusinessRules salesOrderItemBusinessRules, IMapper mapper)
            {
                _salesOrderItemRepository = salesOrderItemRepository;
                _salesOrderItemBusinessRules = salesOrderItemBusinessRules;
                _mapper = mapper;
            }

            public async Task Handle(UpdateSalesOrderItemCommand request, CancellationToken cancellationToken)
            {
                await _salesOrderItemBusinessRules.SapPlannedUnitQtyShouldNotExceedImportedCodeCount(request.SalesOrderItemId, request.SapPlannedUnitQty);

                var salesOrderItem = await _salesOrderItemRepository.Get(b => b.SalesOrderItemId == request.SalesOrderItemId, cancellationToken: cancellationToken);
                if (salesOrderItem == null)
                    throw new BusinessException("Satış siparişi bulunamadı.");

                salesOrderItem.UpdatedAt = DateTime.Now;
                _mapper.Map(request, salesOrderItem);
                salesOrderItem.ShelfLifeValue = request.ShelfLifeValue!.Value;
                salesOrderItem.ShelfLifeUnit = request.ShelfLifeUnit!.Value;

                await _salesOrderItemRepository.UpdateWithPlannedOrders(salesOrderItem, cancellationToken);
            }
        }
    }
}
