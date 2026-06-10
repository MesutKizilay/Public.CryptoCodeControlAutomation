using AutoMapper;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;

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
        public DateTime? SapValidatedAt { get; set; }

        public class UpdateSalesOrderItemCommandHandler : IRequestHandler<UpdateSalesOrderItemCommand>
        {
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IMapper _mapper;

            public UpdateSalesOrderItemCommandHandler(ISalesOrderItemRepository salesOrderItemRepository, IMapper mapper)
            {
                _salesOrderItemRepository = salesOrderItemRepository;
                _mapper = mapper;
            }

            public async Task Handle(UpdateSalesOrderItemCommand request, CancellationToken cancellationToken)
            {
                var salesOrderItem = await _salesOrderItemRepository.Get(b => b.SalesOrderItemId == request.SalesOrderItemId, cancellationToken: cancellationToken);
                salesOrderItem.UpdatedAt = DateTime.Now;
                _mapper.Map(request, salesOrderItem);

                await _salesOrderItemRepository.Update(salesOrderItem, cancellationToken);
            }
        }
    }
}