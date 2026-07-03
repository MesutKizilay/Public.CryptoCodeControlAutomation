using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetDailyProducedCodeSummary
{
    public class GetDailyProducedCodeSummaryQuery : IRequest<List<GetDailyProducedCodeSummaryDto>>
    {
        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }

        public class GetDailyProducedCodeSummaryQueryHandler
            : IRequestHandler<GetDailyProducedCodeSummaryQuery, List<GetDailyProducedCodeSummaryDto>>
        {
            private readonly ICodeRepository _codeRepository;

            public GetDailyProducedCodeSummaryQueryHandler(ICodeRepository codeRepository)
            {
                _codeRepository = codeRepository;
            }

            public async Task<List<GetDailyProducedCodeSummaryDto>> Handle(
                GetDailyProducedCodeSummaryQuery request,
                CancellationToken cancellationToken)
            {
                var query = _codeRepository.Query()
                    .AsNoTracking()
                    .Where(c => c.Status == CodeStatus.ProducedOk && c.ShiftDate.HasValue);

                if (request.SalesOrderItemId.HasValue && request.SalesOrderItemId.Value > 0)
                {
                    query = query.Where(c => c.SalesOrderItemId == request.SalesOrderItemId.Value);
                }

                if (request.PlannedOrderId.HasValue && request.PlannedOrderId.Value > 0)
                {
                    query = query.Where(c => c.PlannedOrderId == request.PlannedOrderId.Value);
                }

                return await query
                    .GroupBy(c => c.ShiftDate!.Value.Date)
                    .Select(group => new GetDailyProducedCodeSummaryDto
                    {
                        ShiftDate = group.Key,
                        ProducedCount = group.Count()
                    })
                    .OrderByDescending(item => item.ShiftDate)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
