using Core.Application.Request;
using Core.Persistence.Paging;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeReportList
{
    public class GetCodeReportListQuery : IRequest<Paginate<GetCodeReportListDto>>
    {
        public PageRequest PageRequest { get; set; }
        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public CodeStatus? Status { get; set; }
        public string? CodeValue { get; set; }
        public DateTime? ShiftDateStart { get; set; }
        public DateTime? ShiftDateEnd { get; set; }

        public class GetCodeReportListQueryHandler : IRequestHandler<GetCodeReportListQuery, Paginate<GetCodeReportListDto>>
        {
            private readonly ICodeRepository _codeRepository;
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IPlannedOrderRepository _plannedOrderRepository;

            public GetCodeReportListQueryHandler(ICodeRepository codeRepository, ISalesOrderItemRepository salesOrderItemRepository, IPlannedOrderRepository plannedOrderRepository)
            {
                _codeRepository = codeRepository;
                _salesOrderItemRepository = salesOrderItemRepository;
                _plannedOrderRepository = plannedOrderRepository;
            }

            public async Task<Paginate<GetCodeReportListDto>> Handle(GetCodeReportListQuery request, CancellationToken cancellationToken)
            {
                var codes = _codeRepository.Query();
                var sales = _salesOrderItemRepository.Query().IgnoreQueryFilters();
                var planned = _plannedOrderRepository.Query();

                var codeQuery = codes.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.CodeValue))
                {
                    var codeValue = request.CodeValue.Trim();
                    codeQuery = codeQuery.Where(c => c.CodeValue == codeValue);
                }

                if (request.SalesOrderItemId.HasValue && request.SalesOrderItemId.Value > 0)
                {
                    codeQuery = codeQuery.Where(c => c.SalesOrderItemId == request.SalesOrderItemId.Value);
                }

                if (request.PlannedOrderId.HasValue && request.PlannedOrderId.Value > 0)
                {
                    codeQuery = codeQuery.Where(c => c.PlannedOrderId == request.PlannedOrderId.Value);
                }

                if (request.Status.HasValue)
                {
                    codeQuery = codeQuery.Where(c => c.Status == request.Status.Value);
                }

                var shiftDateStart = request.ShiftDateStart?.Date;
                var shiftDateEnd = request.ShiftDateEnd?.Date;

                if (shiftDateStart.HasValue && shiftDateEnd.HasValue && shiftDateStart > shiftDateEnd)
                {
                    (shiftDateStart, shiftDateEnd) = (shiftDateEnd, shiftDateStart);
                }

                if (shiftDateStart.HasValue)
                {
                    codeQuery = codeQuery.Where(c => c.ShiftDate.HasValue &&
                                                     c.ShiftDate.Value >= shiftDateStart.Value);
                }

                if (shiftDateEnd.HasValue)
                {
                    var shiftDateEndExclusive = shiftDateEnd.Value.AddDays(1);
                    codeQuery = codeQuery.Where(c => c.ShiftDate.HasValue &&
                                                     c.ShiftDate.Value < shiftDateEndExclusive);
                }

                var query = from c in codeQuery
                            join s in sales on c.SalesOrderItemId equals s.SalesOrderItemId
                            join p in planned on c.PlannedOrderId equals p.PlannedOrderId into pjoin
                            from p in pjoin.DefaultIfEmpty()
                            select new GetCodeReportListDto
                            {
                                CodeValue = c.CodeValue,
                                Status = c.Status,
                                SalesOrderNo = s.SalesOrderNo,
                                SalesItemNo = s.SalesItemNo,
                                PlannedOrderNo = p != null ? p.PlannedOrderNo : string.Empty,
                                ProducedAt = c.ShiftDate,
                                ExpirationDate = c.ExpirationDate,
                                RecoverAt = c.RecoverAt,
                                UpdatedAt = c.UpdatedAt
                            };

                //var totalCount = await query.CountAsync(cancellationToken);

                var items = await query.ToPaginateAsync(index: request.PageRequest.Index, size: request.PageRequest.Size);

                return items;
            }
        }
    }
}
