using MediatR;
using CryptoCodeControlAutomation.Application.Services.Validations;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.ValidateSalesOrderItem
{
    public class ValidateSalesOrderItemQuery : IRequest<ValidateSalesOrderItemDto>
    {
        public string SalesOrderNo { get; set; }
        public string SalesItemNo { get; set; }

        public class ValidateSalesOrderItemQueryHandler : IRequestHandler<ValidateSalesOrderItemQuery, ValidateSalesOrderItemDto>
        {
            private readonly ISalesOrderItemService _salesOrderItemService;

            public ValidateSalesOrderItemQueryHandler(ISalesOrderItemService salesOrderItemService)
            {
                _salesOrderItemService = salesOrderItemService;
            }

            public async Task<ValidateSalesOrderItemDto> Handle(ValidateSalesOrderItemQuery request, CancellationToken cancellationToken)
            {
                var result = await _salesOrderItemService.ValidateSalesOrderItem(request.SalesOrderNo, request.SalesItemNo, cancellationToken);
                return result;
            }
        }
    }
}