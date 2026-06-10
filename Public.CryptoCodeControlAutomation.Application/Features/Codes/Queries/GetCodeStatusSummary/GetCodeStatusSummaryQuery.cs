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

                var statusCounts = await query
                    .GroupBy(c => c.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                int GetCount(CodeStatus status) => statusCounts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

                var dto = new GetCodeStatusSummaryDto
                {
                    Available = GetCount(CodeStatus.Available),
                    Allocated = GetCount(CodeStatus.Allocated),
                    ProducedOk = GetCount(CodeStatus.ProducedOk),
                    Reject = GetCount(CodeStatus.Reject),
                    Scrap = GetCount(CodeStatus.Scrap),
                    Void = GetCount(CodeStatus.Void)
                };

                dto.Total = dto.Available + dto.Allocated + dto.ProducedOk + dto.Reject + dto.Scrap + dto.Void;
                return dto;
            }
        }
    }
}
