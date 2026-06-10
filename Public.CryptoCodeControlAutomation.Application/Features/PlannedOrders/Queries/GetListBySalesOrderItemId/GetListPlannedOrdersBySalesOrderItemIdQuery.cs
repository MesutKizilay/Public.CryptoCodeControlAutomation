using MediatR;
using Microsoft.EntityFrameworkCore;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Persistence.Repositories;

namespace CryptoCodeControlAutomation.Application.Features.PlannedOrders.Queries.GetListBySalesOrderItemId
{
    public class GetListPlannedOrdersBySalesOrderItemIdQuery : IRequest<List<GetListPlannedOrdersBySalesOrderItemIdDto>>
    {
        public long? SalesOrderItemId { get; set; }

        public class GetListPlannedOrdersBySalesOrderItemIdQueryHandler
            : IRequestHandler<GetListPlannedOrdersBySalesOrderItemIdQuery, List<GetListPlannedOrdersBySalesOrderItemIdDto>>
        {
            private readonly IPlannedOrderSalesLinkRepository _linkRepository;
            private readonly IPlannedOrderRepository _plannedOrderRepository;

            public GetListPlannedOrdersBySalesOrderItemIdQueryHandler(IPlannedOrderSalesLinkRepository linkRepository, IPlannedOrderRepository plannedOrderRepository)
            {
                _linkRepository = linkRepository;
                _plannedOrderRepository = plannedOrderRepository;
            }

            public async Task<List<GetListPlannedOrdersBySalesOrderItemIdDto>> Handle(GetListPlannedOrdersBySalesOrderItemIdQuery request, CancellationToken cancellationToken)
            {
                var links = _linkRepository.Query();

                if (request.SalesOrderItemId > 0)
                {
                    links = links.Where(l => l.SalesOrderItemId == request.SalesOrderItemId);
                }

                var plannedOrders = _plannedOrderRepository.Query();

                var result = await (from l in links
                                    join p in plannedOrders on l.PlannedOrderId equals p.PlannedOrderId
                                    select new GetListPlannedOrdersBySalesOrderItemIdDto
                                    {
                                        PlannedOrderId = p.PlannedOrderId,
                                        PlannedOrderNo = p.PlannedOrderNo,
                                        MaterialNo = p.MaterialNo,
                                        LineCode = p.LineCode,
                                        TotalUnitQty = p.TotalUnitQty,
                                        Status = p.Status,
                                        CodeCount = l.ConsumedUnitQty,
                                        SalesOrderItemId = l.SalesOrderItemId
                                    }).ToListAsync(cancellationToken);
                return result;
            }
        }
    }
}
