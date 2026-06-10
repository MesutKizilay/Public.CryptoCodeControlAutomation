using CryptoCodeControlAutomation.Application.Services.Repositories;
using MediatR;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetNextSalesOrderNo
{
    public class GetNextSalesOrderNoQuery : IRequest<string>
    {
        public class GetNextSalesOrderNoQueryHandler : IRequestHandler<GetNextSalesOrderNoQuery, string>
        {
            private readonly ISalesOrderItemRepository _salesOrderItemsRepository;

            public GetNextSalesOrderNoQueryHandler(ISalesOrderItemRepository repository)
            {
                _salesOrderItemsRepository = repository;
            }

            public async Task<string> Handle(GetNextSalesOrderNoQuery request, CancellationToken cancellationToken)
            {
                const string defaultNumber = "00000001";

                var lastSalesOrderItem = await _salesOrderItemsRepository.Get(predicate: s => !string.IsNullOrEmpty(s.SalesOrderNo),
                                                                              orderBy: query => query.OrderByDescending(s => Convert.ToInt64(s.SalesOrderNo)),
                                                                              withDeleted: true,
                                                                              cancellationToken: cancellationToken);

                if (lastSalesOrderItem == null || !long.TryParse(lastSalesOrderItem.SalesOrderNo, out var lastOrderNo))
                    return defaultNumber;

                return (lastOrderNo + 1).ToString("D8");
            }
        }
    }
}
