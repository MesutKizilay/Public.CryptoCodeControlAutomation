using MediatR;
using Microsoft.EntityFrameworkCore;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeStatusSummary
{
    public class GetCodeStatusSummaryQuery : IRequest<GetCodeStatusSummaryDto>
    {
        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }

        public class GetCodeStatusSummaryQueryHandler : IRequestHandler<GetCodeStatusSummaryQuery, GetCodeStatusSummaryDto>
        {
            private readonly ICodeRepository _codeRepository;

            public GetCodeStatusSummaryQueryHandler(ICodeRepository codeRepository)
            {
                _codeRepository = codeRepository;
            }

            public async Task<GetCodeStatusSummaryDto> Handle(GetCodeStatusSummaryQuery request, CancellationToken cancellationToken)
            {
                var query = _codeRepository.Query();

                if (request.SalesOrderItemId.HasValue)
                {
                    query = query.Where(c => c.SalesOrderItemId == request.SalesOrderItemId.Value);
                }

                if (request.PlannedOrderId.HasValue)
                {
                    query = query.Where(c => c.PlannedOrderId == request.PlannedOrderId.Value);
                }

                var counts = await query
                    .GroupBy(_ => 1)
                    .Select(group => new
                    {
                        Available = group.Count(c => c.Status == CodeStatus.Available),
                        Allocated = group.Count(c => c.Status == CodeStatus.Allocated),
                        ProducedTotal = group.Count(c => c.Status == CodeStatus.ProducedOk),
                        Reject = group.Count(c => c.Status == CodeStatus.ProducedOk && c.RecoverAt.HasValue),
                        Scrap = group.Count(c => c.Status == CodeStatus.Scrap),
                        Void = group.Count(c => c.Status == CodeStatus.Void)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var dto = new GetCodeStatusSummaryDto
                {
                    Available = counts?.Available ?? 0,
                    Allocated = counts?.Allocated ?? 0,
                    ProducedOk = (counts?.ProducedTotal ?? 0) - (counts?.Reject ?? 0),
                    Reject = counts?.Reject ?? 0,
                    Scrap = counts?.Scrap ?? 0,
                    Void = counts?.Void ?? 0
                };

                dto.Total = dto.Available + dto.Allocated + dto.ProducedOk + dto.Reject + dto.Scrap + dto.Void;
                return dto;
            }
        }
    }
}
