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

                var sourceQuery = from log in logs
                                  join salesOrderItem in salesOrderItems on log.SalesOrderItemId equals salesOrderItem.SalesOrderItemId into salesOrderItemJoin
                                  from salesOrderItem in salesOrderItemJoin.DefaultIfEmpty()
                                  join plannedOrder in plannedOrders on log.PlannedOrderId equals plannedOrder.PlannedOrderId into plannedOrderJoin
                                  from plannedOrder in plannedOrderJoin.DefaultIfEmpty()
                                  select new
                                  {
                                      Log = log,
                                      SalesOrderItem = salesOrderItem,
                                      PlannedOrder = plannedOrder
                                  };

                var searchValue = request.DynamicQuery?.Filter?.Value?.Trim();
                if (!string.IsNullOrEmpty(searchValue))
                {
                    sourceQuery = sourceQuery.Where(x =>
                        x.Log.OperationType.Contains(searchValue) ||
                        (x.SalesOrderItem != null &&
                         x.SalesOrderItem.SalesOrderNo != null &&
                         x.SalesOrderItem.SalesOrderNo.Contains(searchValue)) ||
                        (x.SalesOrderItem != null &&
                         x.SalesOrderItem.SalesItemNo != null &&
                         x.SalesOrderItem.SalesItemNo.Contains(searchValue)) ||
                        (x.PlannedOrder != null &&
                         x.PlannedOrder.PlannedOrderNo != null &&
                         x.PlannedOrder.PlannedOrderNo.Contains(searchValue)) ||
                        (x.Log.CreatedBy != null && x.Log.CreatedBy.Contains(searchValue)) ||
                        (x.Log.Reason != null && x.Log.Reason.Contains(searchValue)));
                }

                var query = sourceQuery.Select(x => new GetListCodeAdjustmentLogDto
                {
                    CodeAdjustmentLogId = x.Log.CodeAdjustmentLogId,
                    OperationType = x.Log.OperationType,
                    SalesOrderNo = x.SalesOrderItem != null ? x.SalesOrderItem.SalesOrderNo : null,
                    SalesItemNo = x.SalesOrderItem != null ? x.SalesOrderItem.SalesItemNo : null,
                    PlannedOrderNo = x.PlannedOrder != null ? x.PlannedOrder.PlannedOrderNo : null,
                    FromStatus = x.Log.FromStatus,
                    ToStatus = x.Log.ToStatus,
                    FromShiftDate = x.Log.FromShiftDate,
                    ToShiftDate = x.Log.ToShiftDate,
                    Quantity = x.Log.Quantity,
                    Reason = x.Log.Reason,
                    CreatedBy = x.Log.CreatedBy,
                    CreatedAt = x.Log.CreatedAt
                });

                if (request.DynamicQuery?.Sort?.Any() == true)
                {
                    query = query.ToDynamic(new DynamicQuery
                    {
                        Sort = request.DynamicQuery.Sort
                    });
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
