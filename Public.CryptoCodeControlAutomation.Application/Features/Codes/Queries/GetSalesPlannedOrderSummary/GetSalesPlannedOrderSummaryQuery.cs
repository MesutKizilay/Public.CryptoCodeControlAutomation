using Core.Application.Request;
using Core.Persistence.Paging;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;
using CryptoCodeControlAutomation.Persistence.Repositories;
using MediatR;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetSalesPlannedOrderSummary
{
    public class GetSalesPlannedOrderSummaryQuery : IRequest<Paginate<GetSalesPlannedOrderSummaryDto>>
    {
        public PageRequest PageRequest { get; set; } = new();
        public string? Search { get; set; }

        public class GetSalesPlannedOrderSummaryQueryHandler
            : IRequestHandler<GetSalesPlannedOrderSummaryQuery, Paginate<GetSalesPlannedOrderSummaryDto>>
        {
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IPlannedOrderSalesLinkRepository _plannedOrderSalesLinkRepository;
            private readonly IPlannedOrderRepository _plannedOrderRepository;
            private readonly IUploadJobRepository _uploadJobRepository;

            public GetSalesPlannedOrderSummaryQueryHandler(
                ISalesOrderItemRepository salesOrderItemRepository,
                IPlannedOrderSalesLinkRepository plannedOrderSalesLinkRepository,
                IPlannedOrderRepository plannedOrderRepository,
                IUploadJobRepository uploadJobRepository)
            {
                _salesOrderItemRepository = salesOrderItemRepository;
                _plannedOrderSalesLinkRepository = plannedOrderSalesLinkRepository;
                _plannedOrderRepository = plannedOrderRepository;
                _uploadJobRepository = uploadJobRepository;
            }

            public async Task<Paginate<GetSalesPlannedOrderSummaryDto>> Handle(
                GetSalesPlannedOrderSummaryQuery request,
                CancellationToken cancellationToken)
            {
                var salesOrderItems = _salesOrderItemRepository.Query();
                var links = _plannedOrderSalesLinkRepository.Query();
                var plannedOrders = _plannedOrderRepository.Query();
                var uploadJobs = _uploadJobRepository.Query();

                var query = from salesOrderItem in salesOrderItems
                            join link in links
                                on salesOrderItem.SalesOrderItemId equals link.SalesOrderItemId into linkGroup
                            from link in linkGroup.DefaultIfEmpty()
                            join plannedOrder in plannedOrders
                                on link.PlannedOrderId equals plannedOrder.PlannedOrderId into plannedOrderGroup
                            from plannedOrder in plannedOrderGroup.DefaultIfEmpty()
                            select new GetSalesPlannedOrderSummaryDto
                            {
                                SalesOrderItemId = salesOrderItem.SalesOrderItemId,
                                PlannedOrderId = link == null ? null : link.PlannedOrderId,
                                SalesOrderNo = salesOrderItem.SalesOrderNo,
                                SalesItemNo = salesOrderItem.SalesItemNo,
                                SapCaseQty = salesOrderItem.SapCaseQty,
                                SapPlannedUnitQty = salesOrderItem.SapPlannedUnitQty,
                                IsCodeUploaded = uploadJobs.Any(uploadJob =>
                                    uploadJob.SalesOrderItemId == salesOrderItem.SalesOrderItemId &&
                                    uploadJob.Status == UploadJobStatus.Done),
                                PlannedOrderNo = plannedOrder == null ? null : plannedOrder.PlannedOrderNo,
                                PlannedOrderUnitQty = plannedOrder == null ? null : plannedOrder.OriginalTotalUnitQty
                            };

                var search = request.Search?.Trim();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(item =>
                        item.SalesOrderNo.Contains(search) ||
                        item.SalesItemNo.Contains(search) ||
                        item.PlannedOrderNo != null && item.PlannedOrderNo.Contains(search));
                }

                query = query
                    .OrderByDescending(item => item.SalesOrderItemId)
                    .ThenByDescending(item => item.PlannedOrderId);

                return await query.ToPaginateAsync(
                    request.PageRequest.Index,
                    request.PageRequest.Size,
                    cancellationToken);
            }
        }
    }
}
