using Core.Application.Request;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoCodeControlAutomation.Application.Features.CodeAdjustmentLogs.Queries.GetList
{
    public class GetListCodeAdjustmentLogWithPaginateQuery : IRequest<Paginate<GetListCodeAdjustmentLogDto>>
    {
        public PageRequest PageRequest { get; set; } = null!;
        public DynamicQuery? DynamicQuery { get; set; }

        public class GetListCodeAdjustmentLogWithPaginateQueryHandler : IRequestHandler<GetListCodeAdjustmentLogWithPaginateQuery, Paginate<GetListCodeAdjustmentLogDto>>
        {
            private readonly ICodeAdjustmentLogRepository _codeAdjustmentLogRepository;
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IPlannedOrderRepository _plannedOrderRepository;

            public GetListCodeAdjustmentLogWithPaginateQueryHandler(
                ICodeAdjustmentLogRepository codeAdjustmentLogRepository,
                ISalesOrderItemRepository salesOrderItemRepository,
                IPlannedOrderRepository plannedOrderRepository)
            {
                _codeAdjustmentLogRepository = codeAdjustmentLogRepository;
                _salesOrderItemRepository = salesOrderItemRepository;
                _plannedOrderRepository = plannedOrderRepository;
            }

            public async Task<Paginate<GetListCodeAdjustmentLogDto>> Handle(GetListCodeAdjustmentLogWithPaginateQuery request, CancellationToken cancellationToken)
            {
                var logs = _codeAdjustmentLogRepository.Query();
                var salesOrderItems = _salesOrderItemRepository.Query().IgnoreQueryFilters();
                var plannedOrders = _plannedOrderRepository.Query();

                var query = from log in logs
                            join salesOrderItem in salesOrderItems on log.SalesOrderItemId equals salesOrderItem.SalesOrderItemId into salesOrderItemJoin
                            from salesOrderItem in salesOrderItemJoin.DefaultIfEmpty()
                            join plannedOrder in plannedOrders on log.PlannedOrderId equals plannedOrder.PlannedOrderId into plannedOrderJoin
                            from plannedOrder in plannedOrderJoin.DefaultIfEmpty()
                            select new GetListCodeAdjustmentLogDto
                            {
                                CodeAdjustmentLogId = log.CodeAdjustmentLogId,
                                OperationType = log.OperationType,
                                SalesOrderNo = salesOrderItem != null ? salesOrderItem.SalesOrderNo : null,
                                SalesItemNo = salesOrderItem != null ? salesOrderItem.SalesItemNo : null,
                                PlannedOrderNo = plannedOrder != null ? plannedOrder.PlannedOrderNo : null,
                                FromStatus = log.FromStatus,
                                ToStatus = log.ToStatus,
                                FromShiftDate = log.FromShiftDate,
                                ToShiftDate = log.ToShiftDate,
                                Quantity = log.Quantity,
                                Reason = log.Reason,
                                CreatedBy = log.CreatedBy,
                                CreatedAt = log.CreatedAt
                            };

                if (request.DynamicQuery is not null)
                {
                    query = query.ToDynamic(request.DynamicQuery);
                }
                else
                {
                    query = query.OrderByDescending(x => x.CreatedAt);
                }

                return await query.ToPaginateAsync(
                    index: request.PageRequest.Index,
                    size: request.PageRequest.Size,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
